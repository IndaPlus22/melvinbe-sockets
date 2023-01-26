using SocketLeague;

using Microsoft.Xna.Framework;

using System;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace Server
{
    class ServerMain
    {
        // This is where recieved messages end up when retrieved from socket
        private static byte[] recievedBuffer = new byte[4096];

        private static Dictionary<int, Socket> clientSockets = new Dictionary<int, Socket>();
        private static Socket serverSocket = new Socket(
            AddressFamily.InterNetwork,
            SocketType.Stream,
            ProtocolType.Tcp);

        // true while looking for more clients, up to 4
        private static bool connecting;

        // This would be turned into a list if I ever implement support for multiple matches at once
        public static World localGame = new World();

        static void Main(string[] args)
        {
            Console.Title = "Server";

            SetupServer();

            // Calculate time between each frame
            Stopwatch sw = new Stopwatch();
            float deltaTime = 0.0f;
            float time = 0.0f;

            // Game loop that runs way too fast
            while (true)
            {
                time += deltaTime;

                // Display state of the servers local World every 3 seconds
                if (time > 3.0f)
                {
                    time -= 3.0f;
                    Console.WriteLine("");
                    foreach (Player player in localGame.players)
                    {
                        Console.WriteLine("Player " + player.ID);
                        Console.WriteLine("Active: " + player.isActive);
                        Console.WriteLine("X: " + player.position.X + " Y: " + player.position.Y);
                    }
                    Console.WriteLine("Ball");
                    Console.WriteLine("X: " + localGame.ball.position.X + " Y: " + localGame.ball.position.Y);
                }

                // Update server World
                localGame.Update(deltaTime);

                // Check for goals
                if (localGame.ball.position.X < -262 || localGame.ball.position.X > 262)
                {
                    // Reset server world and tell clients to do the same
                    localGame.Reset();
                    foreach (var client in clientSockets)
                    {
                        // This message has no extra data, the enum is enough information
                        SendMessage(client.Key, MsgTypes.ResetGame, new byte[0]); 
                    }
                }

                // Update ball for all clients
                foreach (var client in clientSockets)
                {
                    SendMessage(client.Key, MsgTypes.SetBall, localGame.ball.GetData());
                }

                // Begin accepting connections if there is room for more clients
                if (!connecting && clientSockets.Count() < 4)
                {
                    connecting = true;
                    serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
                }

                // Slow the loop down a little
                Thread.Sleep(10);

                deltaTime = (float)sw.Elapsed.TotalSeconds;
                sw.Restart();
            }
        }

        private static void SetupServer()
        {
            // Start listening for connections from any ip
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, 100));
            serverSocket.Listen(5);

            Console.WriteLine("Server is ready");
        }

        // Called when a new client is accepted
        private static void AcceptCallback(IAsyncResult ar)
        {
            Socket socket = serverSocket.EndAccept(ar);

            Console.WriteLine("Client accepted");

            int newID = -1;

            // Find empty spot for player and get ID of that spot
            for (int i = 0; i < 4; i++)
            {
                if (!localGame.players[i].isActive)
                {
                    newID = i;

                    // Activate player in that spot
                    localGame.players[i].isActive = true;

                    break;
                }
            }

            // Add new socket with that ID
            clientSockets.Add(newID, socket);

            // I was originally intending for there to be multiple games going on at once
            // which is why some of this assignment code may be redundant...

            // But alas I do not have time to touch something that works

            // Inform client that it has a player
            SendMessage(newID, MsgTypes.AssignID, BitConverter.GetBytes(newID));

            List<byte> data = new List<byte>();
            data.AddRange(BitConverter.GetBytes(newID));
            data.AddRange(localGame.players[newID].GetData());
            SendMessage(newID, MsgTypes.SetPlayer, data.ToArray());

            // Start recieving messages
            socket.BeginReceive(recievedBuffer, 0, recievedBuffer.Length, SocketFlags.None, new AsyncCallback(RecieveCallback), socket);
            connecting = false;
        }

        // Decides what to do with recieved and formatted message
        private static void RecieveMessage(int clientID, MsgTypes msgType, byte[] data)
        {
            switch (msgType)
            {
                case MsgTypes.SetPlayer:
                {
                    // Ignore any post-mortem messages
                    if (!clientSockets.ContainsKey(clientID)) return;

                    // Sync local player
                    localGame.players[clientID].SetData(data);

                    // Share update with other clients
                    List<byte> buffer = new List<byte>();
                    buffer.AddRange(BitConverter.GetBytes(clientID)); // Also send ID for client to know which player to update
                    buffer.AddRange(data);

                    for (int i = 0; i < 4; i++) // Using a foreach loop here is not thread safe
                        {
                        if (clientSockets.ContainsKey(i))
                        {
                            if (i != clientID)
                            {
                                SendMessage(i, MsgTypes.SetPlayer, buffer.ToArray());
                            }
                        }
                    }

                    break;
                }
            }
        }

        // Callback for recieving messages
        private static void RecieveCallback(IAsyncResult ar)
        {
            if (ar.AsyncState == null) return;
            Socket socket = (Socket)ar.AsyncState;

            // Size of all messages together in socket
            int recievedSize = 0;
            try
            {
                recievedSize = socket.EndReceive(ar);
            }
            catch
            {
                OnClientDisconnect(socket);

                return;
            }

            // Copy recieved buffer into data buffer
            byte[] data = new byte[recievedSize];
            Array.Copy(recievedBuffer, data, recievedSize);

            // While there still are messages to read
            while (recievedSize > 0)
            {
                // Extract message information:
                int clientID = BitConverter.ToInt32(data, 0);
                int dataSize = BitConverter.ToInt32(data, 4);
                MsgTypes msgType = (MsgTypes)BitConverter.ToInt32(data, 8);
                data = data.Skip(12).ToArray();

                byte[] msgData = data.Take(dataSize).ToArray();
                data = data.Skip(dataSize).ToArray();

                // There is now this much less message to read
                recievedSize -= 12 + dataSize;

                // Decide what to do with message
                RecieveMessage(clientID, msgType, msgData);
            }

            // Look for more messages to recieve
            socket.BeginReceive(recievedBuffer, 0, recievedBuffer.Length, SocketFlags.None, new AsyncCallback(RecieveCallback), socket);
        }

        // Function for constructing and sending messages
        private static void SendMessage(int clientID, MsgTypes msgType, byte[] data)
        {
            Socket socket = clientSockets[clientID];

            // Add "meta" data to front of message
            List<byte> buffer = new List<byte>();
            buffer.AddRange(BitConverter.GetBytes(data.Length));
            buffer.AddRange(BitConverter.GetBytes((int)msgType));
            buffer.AddRange(data);

            socket.BeginSend(buffer.ToArray(), 0, buffer.Count, SocketFlags.None, new AsyncCallback(SendCallback), socket);
        }

        // Callback for sent messages
        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // The send worked
                if (ar.AsyncState == null) return;
                Socket socket = (Socket)ar.AsyncState;
                socket.EndSend(ar);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        // Called when client connection is lost to remove player
        private static void OnClientDisconnect(Socket socket)
        {
            // Find ID of client that disconnected based on socket:
            int removedID = -1;

            foreach (var s in clientSockets)
            {
                if (s.Value == socket)
                {
                    removedID = s.Key;
                    socket.Disconnect(true);
                }
            }

            clientSockets.Remove(removedID);

            // Reset and deactivate player
            localGame.players[removedID].isActive = false;
            localGame.players[removedID].position = World.startingPositions[removedID];
            localGame.players[removedID].rotation = World.startingRotations[removedID];
            localGame.players[removedID].velocity = Vector2.Zero;
            localGame.players[removedID].boostAmount = 1.0f / 3.0f;

            // Update removed player for other connected clients
            foreach (var s in clientSockets) // This foreach has not caused any problems yet
            {
                List<byte> buffer = new List<byte>();
                buffer.AddRange(BitConverter.GetBytes(removedID));
                buffer.AddRange(localGame.players[removedID].GetData());
                SendMessage(s.Key, MsgTypes.SetPlayer, buffer.ToArray());
            }
        }
    }
}
