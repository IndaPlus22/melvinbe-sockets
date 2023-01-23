using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SocketLeague
{
    public class ClientMain : Game
    {
        private static byte[] recievedBuffer = new byte[4096];

        private static Socket clientSocket = new Socket(
            AddressFamily.InterNetwork, 
            SocketType.Stream, 
            ProtocolType.Tcp);
        private static Socket serverSocket;


        public static int localID = -1;

        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        public static World localGame;

        public static Texture2D playerTexture = null;

        public ClientMain()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            Player.playerTexture = Content.Load<Texture2D>("textures/circle");

            localGame = new World();

            clientSocket.BeginConnect(IPAddress.Loopback, 100, ConnectCallback, clientSocket);
        }

        public static void RecieveMessage(MsgTypes msgType, byte[] data)
        {
            Debug.WriteLine(msgType);

            if (msgType == MsgTypes.AssignID)
            {
                int newID = BitConverter.ToInt32(data, 0);
                localID = newID;

                Debug.WriteLine("Assigning new ID: " + newID);
            }
            if (msgType == MsgTypes.SetPlayer)
            {
                int playerToUpdate = BitConverter.ToInt32(data, 0);
                data = data.Skip(4).ToArray();

                Debug.WriteLine("Updating player: " + playerToUpdate);
                localGame.players[playerToUpdate].SetData(data);
            }
        }

        private static void ConnectCallback(IAsyncResult ar)
        {
            serverSocket = (Socket)ar.AsyncState;

            serverSocket.BeginReceive(recievedBuffer, 0, recievedBuffer.Length, SocketFlags.None, new AsyncCallback(RecieveCallback), serverSocket);
        }


        private static void RecieveCallback(IAsyncResult ar)
        {
            Socket socket = (Socket)ar.AsyncState;
            int recievedSize = socket.EndReceive(ar);
            Debug.WriteLine(recievedSize);
            byte[] data = new byte[recievedSize];
            Array.Copy(recievedBuffer, data, recievedSize);

            while (recievedSize > 0)
            {
                int dataSize = BitConverter.ToInt32(data, 0);
                MsgTypes msgType = (MsgTypes)BitConverter.ToInt32(data, 4);
                data = data.Skip(8).ToArray();

                byte[] msgData = data.Take(dataSize).ToArray();
                data = data.Skip(dataSize).ToArray();

                recievedSize -= 8 + dataSize;

                RecieveMessage(msgType, msgData);
            }

            socket.BeginReceive(recievedBuffer, 0, recievedBuffer.Length, SocketFlags.None, new AsyncCallback(RecieveCallback), socket);
        }

        public static void SendMessage(int msgType, byte[] data)
        {
            List<byte> buffer = new List<byte>();
            buffer.AddRange(BitConverter.GetBytes(localID));
            buffer.AddRange(BitConverter.GetBytes(msgType));
            buffer.AddRange(data);

            clientSocket.BeginSend(buffer.ToArray(), 0, buffer.Count, SocketFlags.None, new AsyncCallback(SendCallback), clientSocket);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            Socket socket = (Socket)ar.AsyncState;
            socket.EndSend(ar);
        }

        protected override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Input.Update();

            if (Input.Exit()) Exit();

            localGame.Update(deltaTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            localGame.Draw(spriteBatch);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}