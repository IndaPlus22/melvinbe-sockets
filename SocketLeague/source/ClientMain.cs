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

        private static bool connecting;

        private static bool connectionLost;

        public static int localID = -1;

        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private RenderTarget2D nativeRenderTarget;

        public static float countDownTime;
        private SpriteFont countdownFont;

        private Texture2D boostMeterTexture;

        public static World localGame;

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

            countdownFont = Content.Load<SpriteFont>("fonts/countdown_font");
            boostMeterTexture = Content.Load<Texture2D>("textures/circle");

            Player.playerTexture = Content.Load<Texture2D>("textures/direction_circle");
            Ball.ballTexture = Content.Load<Texture2D>("textures/circle");
            BoostPad.boostPadTexture = Content.Load<Texture2D>("textures/circle");
            Particles.boostTexture = Content.Load<Texture2D>("textures/circle");
            Stage.stageTexture = Content.Load<Texture2D>("textures/stage");

            Stage.circleTexture = Content.Load<Texture2D>("textures/circle");
            Stage.squareTexture = Content.Load<Texture2D>("textures/square");

            localGame = new World();
        }

        public static void RecieveMessage(MsgTypes msgType, byte[] data)
        {
            switch (msgType)
            {
                case MsgTypes.ResetGame:
                {
                    localGame.Reset();

                    break;
                }
                case MsgTypes.AssignID:
                {
                    int newID = BitConverter.ToInt32(data, 0);
                    
                    localID = newID;
                    Camera.followTarget = localGame.players[newID];

                    break;
                }
                case MsgTypes.SetPlayer:
                {
                    int playerToUpdate = BitConverter.ToInt32(data, 0);
                    data = data.Skip(4).ToArray();

                    localGame.players[playerToUpdate].SetData(data);

                    break;
                }
                case MsgTypes.SetBall:
                {
                    localGame.ball.SetData(data);

                    break;
                }
            }
        }

        private static void ConnectCallback(IAsyncResult ar)
        {
            connecting = false;
            try
            {
                serverSocket = (Socket)ar.AsyncState;

                serverSocket.EndConnect(ar);

                serverSocket.BeginReceive(recievedBuffer, 0, recievedBuffer.Length, SocketFlags.None, new AsyncCallback(RecieveCallback), serverSocket);
            } 
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
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
            catch (Exception e)
            {
                Debug.WriteLine("Connection Lost...");
                Debug.WriteLine(e.ToString());
                connectionLost = true;
            }
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
            if (!clientSocket.Connected)
            {
                if (!connecting)
                {
                    clientSocket.BeginConnect(IPAddress.Loopback, 100, ConnectCallback, clientSocket);
                    connecting = true;
                }
            }

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Input.Update();

            if (Input.Exit() || connectionLost) Exit();

            if (Input.KeyDown(Keys.F)) GameWindow.ToggleFullscreen();

            localGame.Update(deltaTime);

            Particles.Update(deltaTime);

            countDownTime -= deltaTime;

            Camera.Update(deltaTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(nativeRenderTarget);
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin(samplerState: SamplerState.PointClamp, sortMode: SpriteSortMode.FrontToBack);

            localGame.Draw(spriteBatch);

            Particles.Draw(spriteBatch);

            Stage.Draw(spriteBatch);

            if (countDownTime > 0.0f)
            {
                spriteBatch.DrawString
                (
                    countdownFont,
                    ((int)countDownTime + 1).ToString(),
                    new Vector2(GameWindow.REFERENCE_WIDTH / 2, GameWindow.REFERENCE_HEIGHT / 4) + new Vector2(-10, -10),
                    Color.Gold,
                    0.0f,
                    Vector2.Zero,
                    Vector2.One,
                    SpriteEffects.None,
                    1.0f
                );
            }

            if (localID != -1)
            {
                string drawString = ((int)(localGame.players[localID].boostAmount * 100.0f)).ToString();
                spriteBatch.DrawString
                (
                    countdownFont,
                    drawString,
                    new Vector2(390 + drawString.Length * -5, 210),
                    Color.Gold,
                    0.0f,
                    Vector2.Zero,
                    new Vector2(0.5f),
                    SpriteEffects.None,
                    1.0f
                );

                spriteBatch.Draw
                (
                    boostMeterTexture,
                    new Vector2(394, 222),
                    boostMeterTexture.Bounds,
                    Color.White,
                    0,
                    new Vector2(boostMeterTexture.Width / 2, boostMeterTexture.Height / 2),
                    0.2f,
                    SpriteEffects.None,
                    0.99f
                );
            }

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