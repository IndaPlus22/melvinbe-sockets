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

        public static World game;

        static void Main(string[] args)
        {
            Console.Title = "Server";
            game = new World();

            SetupServer();

            while (true)
            {
                game.Update(1.0f / 60.0f);

                /*
                if (Console.ReadKey(true).Key == ConsoleKey.M)
                {
                    List<byte> buffer = new List<byte>();
                    buffer.AddRange(BitConverter.GetBytes(300.0f));
                    buffer.AddRange(BitConverter.GetBytes(300.0f));

                    SendMessageToAllOther(0, 0, buffer.ToArray());
                }
                */
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

            Console.WriteLine("Client accepted");

            int newID = -1;

            for (int i = 0; i < 4; i++)
            {
                if (!game.players[i].isActive)
                {
                    newID = i;
                    game.players[i].isActive = true;
                    game.players[i].ID = newID;

                    break;
                }
            }

            clientSockets.Add(newID, socket);

            SendMessage(newID, MsgTypes.AssignID, BitConverter.GetBytes(newID));

            for (int i = 0; i < 4; i++)
            {
                List<byte> data = new List<byte>();
                data.AddRange(BitConverter.GetBytes(i));
                data.AddRange(game.players[i].GetData());
                SendMessage(newID, MsgTypes.SetPlayer, data.ToArray());
            }

            socket.BeginReceive(recievedBuffer, 0, recievedBuffer.Length, SocketFlags.None, new AsyncCallback(RecieveCallback), socket);
            serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
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
            Socket socket = clientSockets[clientID];

            List<byte> buffer = new List<byte>();
            buffer.AddRange(BitConverter.GetBytes(data.Length));
            buffer.AddRange(BitConverter.GetBytes((int)msgType));
            buffer.AddRange(data);

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
