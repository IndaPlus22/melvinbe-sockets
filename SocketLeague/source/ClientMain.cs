using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SocketLeague
{
    public enum MsgTypes
    {
        // Server messages:
        ResetGame = 0,
        AssignID = 1,

        SetBall = 2,

        // Client messages:
        SetPlayer = 11,
    }

    public class ClientMain : Game
    {
        private static Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static Socket serverSocket;

        private static byte[] recievedBuffer = new byte[1024];

        public static int localID = -1;

        

        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        public static World localGame;
        public static Player player;

        public static Texture2D playerTexture = null;

        public ClientMain()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            clientSocket.BeginConnect(IPAddress.Loopback, 100, ConnectCallback, clientSocket);

            localGame = new World();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            playerTexture = Content.Load<Texture2D>("textures/circle");

            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        public static void RecieveMessage(MsgTypes msgType, byte[] data)
        {
            if (msgType == MsgTypes.AssignID)
            {
                int newID = BitConverter.ToInt32(data, 0);
                localID = newID;

            }
            if (msgType == MsgTypes.SetPlayer)
            {
                float x = BitConverter.ToSingle(data, 0);
                float y = BitConverter.ToSingle(data, 4);

                player.position = new Vector2(x, y);
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
            byte[] data = new byte[recievedSize];
            Array.Copy(recievedBuffer, data, recievedSize);

            int msgType;

            msgType = BitConverter.ToInt32(data, 0);
            data = data.Skip(4).ToArray();

            RecieveMessage((MsgTypes)msgType, data);

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

            if (player != null)
            {
                List<byte> buffer = new List<byte>();
                buffer.AddRange(BitConverter.GetBytes(0));
                buffer.AddRange(BitConverter.GetBytes(0));
                SendMessage(0, player.GetData().ToArray());
            }

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