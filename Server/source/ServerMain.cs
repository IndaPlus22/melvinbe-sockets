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
using Microsoft.Xna.Framework.Input;

namespace Server
{
    class ServerMain
    {
        private static byte[] recievedBuffer = new byte[1024];
        private static Dictionary<int, Socket> clientSockets = new Dictionary<int, Socket>();
        private static Socket serverSocket = new Socket(
            AddressFamily.InterNetwork,
            SocketType.Stream,
            ProtocolType.Tcp);

        public static List<World> games = new List<World>();

        static void Main(string[] args)
        {
            Console.Title = "Server";
            games.Add(new World());
            games.Add(new World());
            games.Add(new World());

            SetupServer();
            while (true)
            {
                foreach (World game in games)
                {
                    game.Update(1.0f / 60.0f);
                }

                if (Console.ReadKey(true).Key == ConsoleKey.M)
                {
                    List<byte> buffer = new List<byte>();
                    buffer.AddRange(BitConverter.GetBytes(300.0f));
                    buffer.AddRange(BitConverter.GetBytes(300.0f));

                    SendMessageToAllOther(0, 0, buffer.ToArray());
                }
            }
        }

        private static void SetupServer()
        {
            Console.WriteLine("Setting up server...");

            serverSocket.Bind(new IPEndPoint(IPAddress.Any, 100));
            serverSocket.Listen(5);
            serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
        }

        private static void AcceptCallback(IAsyncResult ar)
        {
            Socket socket = serverSocket.EndAccept(ar);

            int newID = 0;
            foreach (World game in games)
            {
                foreach (Player player in game.players)
                {
                    if (!player.isActive)
                    {
                        player.isActive = true;
                        player.ID = newID;
                        goto PlayerAdded;
                    }
                    newID++;
                }
            }

            PlayerAdded:

            clientSockets.Add(newID, socket);

            socket.BeginReceive(recievedBuffer, 0, recievedBuffer.Length, SocketFlags.None, new AsyncCallback(RecieveCallback), socket);
            serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);

            SendMessage(newID, MsgTypes.AssignID, BitConverter.GetBytes(newID));
        }

        private static void RecieveMessage(int clientID, MsgTypes msgType, byte[] data)
        {
            if (msgType == MsgTypes.SetPlayer)
            {
                Vector2 position = new Vector2
                (
                BitConverter.ToSingle(data, 0),
                BitConverter.ToSingle(data, 4)
                );

                Console.WriteLine("Text recieved: " + position);
            }
        }

        private static void RecieveCallback(IAsyncResult ar)
        {
            Socket socket = (Socket)ar.AsyncState;
            int recieved = socket.EndReceive(ar);
            byte[] dataBuffer = new byte[recieved];
            Array.Copy(recievedBuffer, dataBuffer, recieved);

            int clientID;
            int msgType;

            clientID = BitConverter.ToInt32(dataBuffer, 0);
            msgType = BitConverter.ToInt32(dataBuffer, 4);
            dataBuffer = dataBuffer.Skip(8).ToArray();

            RecieveMessage(clientID, (MsgTypes)msgType, dataBuffer);

            socket.BeginReceive(recievedBuffer, 0, recievedBuffer.Length, SocketFlags.None, new AsyncCallback(RecieveCallback), socket);
        }

        private static void SendMessage(int clientID, MsgTypes msgType, byte[] data)
        {
            List<byte> buffer = new List<byte>();
            buffer.AddRange(BitConverter.GetBytes((int)msgType));
            buffer.AddRange(data);

            Socket socket = clientSockets[clientID];

            socket.BeginSend(buffer.ToArray(), 0, buffer.Count, SocketFlags.None, new AsyncCallback(SendCallback), socket);
        }

        private static void SendMessageToAllOther(int excludeID, MsgTypes msgType, byte[] data)
        {
            List<byte> buffer = new List<byte>();
            buffer.AddRange(BitConverter.GetBytes((int)msgType));
            buffer.AddRange(data);

            foreach (var socket in clientSockets)
            {
                if (socket.Key == excludeID) continue;
                socket.Value.BeginSend(buffer.ToArray(), 0, buffer.Count, SocketFlags.None, new AsyncCallback(SendCallback), socket.Value);
            }
        }

        private static void SendCallback(IAsyncResult ar)
        {
            Socket socket = (Socket)ar.AsyncState;
            socket.EndSend(ar);
        }
    }
}
