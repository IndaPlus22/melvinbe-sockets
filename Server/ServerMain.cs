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
        private static List<Socket> clientSockets = new List<Socket>();
        private static Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        static void Main(string[] args)
        {
            Console.Title = "Server";
            SetupServer();
            while (true)
            {
                if (Console.ReadKey(true).Key == ConsoleKey.M)
                {
                    List<byte> buffer = new List<byte>();
                    buffer.AddRange(BitConverter.GetBytes(300.0f));
                    buffer.AddRange(BitConverter.GetBytes(300.0f));

                    SendMessageToAll(0, buffer.ToArray());
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

        private static void AcceptCallback(IAsyncResult AR)
        {
            Socket socket = serverSocket.EndAccept(AR);
            clientSockets.Add(socket);
            Console.WriteLine("Client Connected");
            socket.BeginReceive(recievedBuffer, 0, recievedBuffer.Length, SocketFlags.None, new AsyncCallback(RecieveCallback), socket);
            serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
        }

        private static void RecieveMessage(int clientID, int msgType, byte[] data)
        {
            if (msgType == 0)
            {
                Vector2 position = new Vector2
                (
                BitConverter.ToSingle(data, 0),
                BitConverter.ToSingle(data, 4)
                );

                Console.WriteLine("Text recieved: " + position);
            }
        }

        private static void RecieveCallback(IAsyncResult AR)
        {
            Socket socket = (Socket)AR.AsyncState;
            int recieved = socket.EndReceive(AR);
            byte[] dataBuffer = new byte[recieved];
            Array.Copy(recievedBuffer, dataBuffer, recieved);

            int clientID;
            int msgType;

            clientID = BitConverter.ToInt32(dataBuffer, 0);
            msgType = BitConverter.ToInt32(dataBuffer, 4);
            dataBuffer = dataBuffer.Skip(8).ToArray();

            RecieveMessage(clientID, msgType, dataBuffer);

            socket.BeginReceive(recievedBuffer, 0, recievedBuffer.Length, SocketFlags.None, new AsyncCallback(RecieveCallback), socket);
        }

        private static void SendMessageToAll(int msgType, byte[] data)
        {
            List<byte> buffer = new List<byte>();
            buffer.AddRange(BitConverter.GetBytes(msgType));
            buffer.AddRange(data);

            foreach (Socket socket in clientSockets)
            {
                socket.BeginSend(buffer.ToArray(), 0, buffer.Count, SocketFlags.None, new AsyncCallback(SendCallback), socket);
            }
        }

        private static void SendCallback(IAsyncResult AR)
        {
            Socket socket = (Socket)AR.AsyncState;
            socket.EndSend(AR);
        }
    }
}
