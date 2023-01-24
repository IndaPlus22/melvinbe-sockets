using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using SocketLeague;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace Server
{
    class ServerMain
    {
        private static byte[] recievedBuffer = new byte[4096];
        private static Dictionary<int, Socket> clientSockets = new Dictionary<int, Socket>();
        private static Socket serverSocket = new Socket(
            AddressFamily.InterNetwork,
            SocketType.Stream,
            ProtocolType.Tcp);

        private static bool connecting;

        public static World localGame;

        static void Main(string[] args)
        {
            Console.Title = "Server";
            localGame = new World();

            SetupServer();

            Stopwatch sw = new Stopwatch();

            float deltaTime = 0.0f;

            float time = 0.0f;

            while (true)
            {
                time += deltaTime;

                if (time > 3.0f)
                {
                    time -= 3.0f;
                    foreach (Player player in localGame.players)
                    {
                        Console.WriteLine("Player " + player.ID);
                        Console.WriteLine("Active: " + player.isActive);
                        Console.WriteLine("X: " + player.position.X + " Y: " + player.position.Y);
                    }
                    Console.WriteLine("Ball");
                    Console.WriteLine("X: " + localGame.ball.position.X + " Y: " + localGame.ball.position.Y);
                }

                localGame.Update(deltaTime);

                if (localGame.ball.position.X < -262 || localGame.ball.position.X > 262)
                {
                    localGame.Reset();

                    foreach (var client in clientSockets)
                    {
                        SendMessage(client.Key, MsgTypes.ResetGame, BitConverter.GetBytes(true));
                    }
                }

                foreach (var client in clientSockets)
                {
                    SendMessage(client.Key, MsgTypes.SetBall, localGame.ball.GetData());
                }

                if (!connecting && clientSockets.Count() < 4)
                {
                    connecting = true;
                    serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
                }

                Thread.Sleep(10);

                deltaTime = (float)sw.Elapsed.TotalSeconds;
                sw.Restart();
            }
        }

        private static void SetupServer()
        {
            Console.WriteLine("Setting up server...");

            serverSocket.Bind(new IPEndPoint(IPAddress.Any, 100));
            serverSocket.Listen(5);
        }

        private static void AcceptCallback(IAsyncResult ar)
        {
            Socket socket = serverSocket.EndAccept(ar);

            Console.WriteLine("Client accepted");

            int newID = -1;

            for (int i = 0; i < 4; i++)
            {
                if (!localGame.players[i].isActive)
                {
                    newID = i;
                    localGame.players[i].isActive = true;
                    localGame.players[i].ID = newID;

                    break;
                }
            }

            clientSockets.Add(newID, socket);

            SendMessage(newID, MsgTypes.AssignID, BitConverter.GetBytes(newID));

            List<byte> data = new List<byte>();
            data.AddRange(BitConverter.GetBytes(newID));
            data.AddRange(localGame.players[newID].GetData());
            SendMessage(newID, MsgTypes.SetPlayer, data.ToArray());

            socket.BeginReceive(recievedBuffer, 0, recievedBuffer.Length, SocketFlags.None, new AsyncCallback(RecieveCallback), socket);
            connecting = false;
        }

        private static void RecieveMessage(int clientID, MsgTypes msgType, byte[] data)
        {
            switch (msgType)
            {
                case MsgTypes.SetPlayer:
                {
                    if (!clientSockets.ContainsKey(clientID)) return;

                    Debug.WriteLine("Updating player: " + clientID);

                    localGame.players[clientID].SetData(data);

                    List<byte> buffer = new List<byte>();
                    buffer.AddRange(BitConverter.GetBytes(clientID));
                    buffer.AddRange(data);

                    for (int i = 0; i < 4; i++)
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

        private static void RecieveCallback(IAsyncResult ar)
        {
            Socket socket = (Socket)ar.AsyncState;

            int recievedSize = 0;

            try
            {
                recievedSize = socket.EndReceive(ar);
            }
            catch
            {
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

                localGame.players[removedID].isActive = false;
                localGame.players[removedID].position = World.startingPositions[removedID];
                localGame.players[removedID].rotation = World.startingRotations[removedID];
                localGame.players[removedID].velocity = Vector2.Zero;

                foreach (var s in clientSockets)
                {
                    List<byte> buffer = new List<byte>();
                    buffer.AddRange(BitConverter.GetBytes(removedID));
                    buffer.AddRange(localGame.players[removedID].GetData());
                    SendMessage(s.Key, MsgTypes.SetPlayer, buffer.ToArray());
                }

                return;
            }

            byte[] data = new byte[recievedSize];
            Array.Copy(recievedBuffer, data, recievedSize);

            while (recievedSize > 0)
            {
                int clientID = BitConverter.ToInt32(data, 0);
                int dataSize = BitConverter.ToInt32(data, 4);
                MsgTypes msgType = (MsgTypes)BitConverter.ToInt32(data, 8);
                data = data.Skip(12).ToArray();

                byte[] msgData = data.Take(dataSize).ToArray();
                data = data.Skip(dataSize).ToArray();

                recievedSize -= 12 + dataSize;

                RecieveMessage(clientID, msgType, msgData);
            }

            socket.BeginReceive(recievedBuffer, 0, recievedBuffer.Length, SocketFlags.None, new AsyncCallback(RecieveCallback), socket);
        }

        private static void SendMessage(int clientID, MsgTypes msgType, byte[] data)
        {
            Socket socket = clientSockets[clientID];

            List<byte> buffer = new List<byte>();
            buffer.AddRange(BitConverter.GetBytes(data.Length));
            buffer.AddRange(BitConverter.GetBytes((int)msgType));
            buffer.AddRange(data);

            socket.BeginSend(buffer.ToArray(), 0, buffer.Count, SocketFlags.None, new AsyncCallback(SendCallback), socket);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket socket = (Socket)ar.AsyncState;
                socket.EndSend(ar);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
