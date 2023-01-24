using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Microsoft.Xna.Framework.Input;

namespace SocketLeague
{
    public class Player : Body
    {
        public static Texture2D playerTexture = null;

        public int ID;

        private float vertical = 0.0f;
        private float horizontal = 0.0f;

        private const float speed = 320.0f;
        private const float turnSpeed = 50.0f;
        private const float wheelDist = 10.0f;
        private const float steerAngle = 0.5f;

        private const float dashForce = 180.0f;
        private const float dashDuration = 0.2f;
        private const float dashDelay = 1.1f;
        private       float dashTime;

        private const float boostMult = 2.0f;
        public        float boostAmount = 1.0f / 3.0f;

        public Player(int ID)
            : base(playerTexture)
        {
            this.ID = ID;

            color = ID % 2 == 0 ? Color.Orange : Color.Blue;

            scale = new Vector2(0.05f);

            radius = 0.05f * 256.0f / 2.0f;

            mass = 1.0f;
            bounce = 0.4f;
            drag = 0.05f;

            isActive = false;
        }

        public override void Update(float deltaTime)
        {
            if (ID == ClientMain.localID && ClientMain.countDownTime <= 0.0f)
            {
                if (dashTime < 0.0f)
                {
                    Drive(deltaTime);

                    if (dashTime < dashDuration - dashDelay)
                    {
                        if (Input.KeyDown(Keys.Space))
                        {
                            Vector2 dir;
                            if (Input.Horizontal() != 0.0f)
                            {
                                dir = new Vector2(Input.Horizontal(), 0.0f);
                            }
                            else
                            {
                                dir = new Vector2(0.0f, -Input.Vertical());
                            }

                            float sin = (float)Math.Sin(rotation + Math.PI / 2.0f);
                            float cos = (float)Math.Cos(rotation + Math.PI / 2.0f);
                            float tx = dir.X;
                            float ty = dir.Y;
                            dir.X = (cos * tx) - (sin * ty);
                            dir.Y = (sin * tx) + (cos * ty);

                            velocity += dir * dashForce;

                            dashTime = dashDuration;
                        }
                    }
                }
                else
                {
                    //rotation += Input.Horizontal() * Input.Vertical() * 8.0f * deltaTime;
                }

                dashTime -= deltaTime;

                drag = dashTime <= 0.0f ? 0.05f : 0.9f;

                ClientMain.SendMessage(MsgTypes.SetPlayer, GetData());
            }

            base.Update(deltaTime);
        }

        public void Drive(float deltaTime)
        {
            if (Math.Abs(Input.Horizontal()) != 0.0f)
            {
                horizontal += Input.Horizontal() * deltaTime * 5.0f;
                horizontal = Math.Clamp(horizontal, -1.0f, 1.0f);
            }
            else
            {
                horizontal -= Math.Clamp(horizontal * 10000.0f, -1.0f, 1.0f) * deltaTime * 5.0f;
            }

            if (Math.Abs(Input.Vertical()) != 0.0f)
            {
                vertical += Input.Vertical() * deltaTime * 2.0f;
                vertical = Math.Clamp(vertical, -1.0f, 1.0f);
            }
            else
            {
                vertical = 0.0f;
            }

            float boost = 1.0f;
            if (Input.KeyHeld(Keys.LeftShift) && boostAmount > 0.0f)
            {
                vertical = 1.0f;

                boost = boostMult;

                boostAmount -= 0.5f * deltaTime;
            }

            Vector2 frontWheel = position + wheelDist / 2.0f * new Vector2((float)Math.Cos(rotation), (float)Math.Sin(rotation));
            Vector2 backWheel = position - wheelDist / 2.0f * new Vector2((float)Math.Cos(rotation), (float)Math.Sin(rotation));

            backWheel += vertical * turnSpeed * deltaTime * new Vector2((float)Math.Cos(rotation), (float)Math.Sin(rotation));
            frontWheel += vertical * turnSpeed * deltaTime * new Vector2((float)Math.Cos(rotation + steerAngle * horizontal), (float)Math.Sin(rotation + steerAngle * horizontal));

            velocity += ((frontWheel + backWheel) / 2.0f - position) * speed * boost * deltaTime;
            rotation = (float)Math.Atan2(frontWheel.Y - backWheel.Y, frontWheel.X - backWheel.X);
        }

        public byte[] GetData()
        {
            List<byte> data = new List<byte>();
            data.AddRange(BitConverter.GetBytes(isActive  ));
            data.AddRange(BitConverter.GetBytes(position.X));
            data.AddRange(BitConverter.GetBytes(position.Y));
            data.AddRange(BitConverter.GetBytes(rotation  ));
            data.AddRange(BitConverter.GetBytes(velocity.X));
            data.AddRange(BitConverter.GetBytes(velocity.Y));

            return data.ToArray();
        }

        public void SetData(byte[] data)
        {
            isActive   = BitConverter.ToBoolean(data, 0);
            position.X = BitConverter.ToSingle(data, 1);
            position.Y = BitConverter.ToSingle(data, 5);
            rotation   = BitConverter.ToSingle(data, 9);
            velocity.X = BitConverter.ToSingle(data, 13);
            velocity.Y = BitConverter.ToSingle(data, 17);

            Debug.WriteLine(isActive);
        }
    }
}
