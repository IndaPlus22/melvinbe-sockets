using Microsoft.Xna.Framework;
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
    public enum MsgType
    {
        // Server messages:
        ResetGame = 0,
        AssignID = 1,

        SetOtherPlayers = 2,
        SetBall = 3,

        // Client messages:
        SetPlayer = 11,
    }

    public class ClientMain : Game
    {
        private static byte[] recievedBuffer = new byte[1024];
        private static Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static Socket serverSocket;
        private static int clientID = 0;

        private static void ConnectCallback(IAsyncResult ar)
        {
            serverSocket = (Socket)ar.AsyncState;

            serverSocket.BeginReceive(recievedBuffer, 0, recievedBuffer.Length, SocketFlags.None, new AsyncCallback(RecieveCallback), serverSocket);
        }

        private static void RecieveMessage(int msgType, byte[] data)
        {
            Debug.WriteLine("Here");

            if (msgType == 0)
            {
                Vector2 position = new Vector2
                (
                BitConverter.ToSingle(data, 0),
                BitConverter.ToSingle(data, 4)
                );

                player.position = position;
            }
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

            RecieveMessage(msgType, data);

            socket.BeginReceive(recievedBuffer, 0, recievedBuffer.Length, SocketFlags.None, new AsyncCallback(RecieveCallback), socket);
        }

        private static void SendMessage(int msgType, byte[] data)
        {
            List<byte> buffer = new List<byte>();
            buffer.AddRange(BitConverter.GetBytes(clientID));
            buffer.AddRange(BitConverter.GetBytes(msgType));
            buffer.AddRange(data);

            clientSocket.BeginSend(buffer.ToArray(), 0, buffer.Count, SocketFlags.None, new AsyncCallback(SendCallback), clientSocket);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            Socket socket = (Socket)ar.AsyncState;
            socket.EndSend(ar);
        }

        public static World localGame;

        private static Player player;

        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

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

            player = new Player(-1, Content.Load<Texture2D>("textures/circle"), new Vector2(100.0f, 100.0f));

            localGame.Add(player);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Input.Update();

            if (Input.Exit()) Exit();

            localGame.Update(deltaTime);

            List<byte> buffer = new List<byte>();
            buffer.AddRange(BitConverter.GetBytes(0));
            buffer.AddRange(BitConverter.GetBytes(0));
            SendMessage(0, player.GetData().ToArray());

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