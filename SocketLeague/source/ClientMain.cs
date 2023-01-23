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
using System.Linq.Expressions;
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

        private RenderTarget2D nativeRenderTarget;

        public static World localGame;

        public static Texture2D playerTexture = null;

        public ClientMain()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "content";

            IsMouseVisible = true;
            Window.AllowUserResizing = true;
        }

        protected override void Initialize()
        {
            nativeRenderTarget = new RenderTarget2D(GraphicsDevice, GameWindow.REFERENCE_WIDTH, GameWindow.REFERENCE_HEIGHT);
            GameWindow.Initialize(graphics, Window);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            Player.playerTexture = Content.Load<Texture2D>("textures/direction_circle");
            Ball.ballTexture = Content.Load<Texture2D>("textures/circle");

            Stage.circleTexture = Content.Load<Texture2D>("textures/circle");
            Stage.squareTexture = Content.Load<Texture2D>("textures/square");

            localGame = new World();

            clientSocket.BeginConnect(IPAddress.Loopback, 100, ConnectCallback, clientSocket);
        }

        public static void RecieveMessage(MsgTypes msgType, byte[] data)
        {
            switch (msgType)
            {
                case MsgTypes.AssignID:
                {
                    int newID = BitConverter.ToInt32(data, 0);
                    localID = newID;

                    Debug.WriteLine("Assigning new ID: " + newID);

                    break;
                }
                case MsgTypes.SetPlayer:
                {
                    int playerToUpdate = BitConverter.ToInt32(data, 0);
                    data = data.Skip(4).ToArray();

                    Debug.WriteLine("Updating player: " + playerToUpdate);
                    localGame.players[playerToUpdate].SetData(data);

                    break;
                }
                case MsgTypes.SetBall:
                {
                    Debug.WriteLine("Updating ball");
                    localGame.ball.SetData(data);

                    break;
                }
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

            while (recievedSize > 0)
            {
                int dataSize = BitConverter.ToInt32(data, 0);
                MsgTypes msgType = (MsgTypes)BitConverter.ToInt32(data, 4);
                data = data.Skip(8).ToArray();

                byte[] msgData = data.Take(dataSize).ToArray();
                data = data.Skip(dataSize).ToArray();

                recievedSize -= 8 + dataSize;

                if(msgType == MsgTypes.SetBall)
                {
                    Debug.WriteLine("here");
                }

                RecieveMessage(msgType, msgData);
            }

            socket.BeginReceive(recievedBuffer, 0, recievedBuffer.Length, SocketFlags.None, new AsyncCallback(RecieveCallback), socket);
        }

        public static void SendMessage(MsgTypes msgType, byte[] data)
        {
            List<byte> buffer = new List<byte>();
            buffer.AddRange(BitConverter.GetBytes(localID));
            buffer.AddRange(BitConverter.GetBytes(data.Length));
            buffer.AddRange(BitConverter.GetBytes((int)msgType));
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
            GraphicsDevice.SetRenderTarget(nativeRenderTarget);
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin(samplerState: SamplerState.PointClamp, sortMode: SpriteSortMode.FrontToBack);

            localGame.Draw(spriteBatch);

            Stage.Draw(spriteBatch);

            spriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            Rectangle screenRect = nativeRenderTarget.Bounds;
            Vector2 screenCenter = Vector2.Floor(new Vector2(Window.ClientBounds.Width, Window.ClientBounds.Height) / 2);
            Vector2 screenOrigin = Vector2.Floor(new Vector2(GameWindow.REFERENCE_WIDTH, GameWindow.REFERENCE_HEIGHT) / 2);

            spriteBatch.Draw
            (
                nativeRenderTarget, 
                screenCenter, 
                screenRect, 
                Color.White, 
                0.0f, 
                screenOrigin, 
                GameWindow.pixelSize, 
                SpriteEffects.None, 
                0.0f
            );

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}