using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace SocketLeague
{
    public class Player : Body
    {
        public static Texture2D playerTexture;

        private const float speed = 1000.0f;
        private const float turnSpeed = 25.0f;
        private const float wheelBase = 10.0f;
        private const float steerAngle = 1.0f;

        private float vertical = 0.0f;
        private float horizontal = 0.0f;

        public int ID;

        public Player(int ID)
            : base(playerTexture)
        {
            this.ID = ID;

            scale = new Vector2(0.05f);

            radius = 0.05f * 256.0f / 2.0f;

            isActive = false;
        }

        public override void Update(float deltaTime)
        {
            if (ID == ClientMain.localID)
            {
                if (Math.Abs(Input.Horizontal()) != 0.0f)
                {
                    horizontal += Input.Horizontal() * deltaTime * 5.0f;
                    horizontal = Math.Clamp(horizontal, -1.0f, 1.0f);
                }
                else
                {
                    horizontal -= Math.Clamp(horizontal * 10000.0f, - 1.0f, 1.0f) * deltaTime * 5.0f;
                }

                if (Math.Abs(Input.Vertical()) != 0.0f)
                {
                    vertical += Input.Vertical() * deltaTime * 2.0f;
                    vertical = Math.Clamp(vertical, -1.0f, 1.0f);
                }
                else
                {
                    vertical -= Math.Clamp(vertical * 10000.0f, -1.0f, 1.0f) * deltaTime;
                }

                Vector2 frontWheel = position + wheelBase / 2.0f * new Vector2((float)Math.Cos(rotation), (float)Math.Sin(rotation));
                Vector2 backWheel = position - wheelBase / 2.0f * new Vector2((float)Math.Cos(rotation), (float)Math.Sin(rotation));

                backWheel += vertical * turnSpeed * deltaTime * new Vector2((float)Math.Cos(rotation), (float)Math.Sin(rotation));
                frontWheel += vertical * turnSpeed * deltaTime * new Vector2((float)Math.Cos(rotation + steerAngle * horizontal), (float)Math.Sin(rotation + steerAngle * horizontal));

                velocity += ((frontWheel + backWheel) / 2 - position) * speed * deltaTime;
                velocity *= 0.85f / (deltaTime * 60.0f);
                rotation = (float)Math.Atan2(frontWheel.Y - backWheel.Y, frontWheel.X - backWheel.X);
            }

            base.Update(deltaTime);
        }

        public override void Move()
        {
            if (ID == ClientMain.localID)
            {
                ClientMain.SendMessage(MsgTypes.SetPlayer, GetData());
            }

            base.Move();
        }

        public byte[] GetData()
        {
            List<byte> data = new List<byte>();
            data.AddRange(BitConverter.GetBytes(isActive));
            data.AddRange(BitConverter.GetBytes(position.X));
            data.AddRange(BitConverter.GetBytes(position.Y));
            data.AddRange(BitConverter.GetBytes(velocity.X));
            data.AddRange(BitConverter.GetBytes(velocity.Y));

            return data.ToArray();
        }

        public void SetData(byte[] data)
        {
            isActive = BitConverter.ToBoolean(data, 0);
            position.X = BitConverter.ToSingle(data, 1);
            position.Y = BitConverter.ToSingle(data, 5);
            velocity.X = BitConverter.ToSingle(data, 9);
            velocity.Y = BitConverter.ToSingle(data, 13);

            Debug.WriteLine(isActive);
        }
    }
}
