using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Win32.SafeHandles;

namespace SocketLeague
{
    public class Player : Body
    {
        // playerTexture should remain null in server World
        public static Texture2D playerTexture = null;

        // Connects players to respective client
        public int ID;

        // Modifiable player input values
        private float vertical;
        private float horizontal;

        private const float speed        = 320.0f;      // Forward speed
        private const float turnSpeed    = 50.0f;       // Rotation speed
        private const float wheelDist    = 10.0f;       // Distance between front and back wheels
        private const float steerAngle   = 0.5f;        // Front wheel rotation angle

        private const float dashForce    = 180.0f;      // Speed gain from dashing
        private const float dashDuration = 0.2f;        // How long player loses control while dashing
        private const float dashDelay    = 1.1f;        // How long player must wait to dash again
        private       float dashTime;

        private       bool  boosting;                   // true for all clients while player is boosting
        private const float boostMult = 2.0f;           // How much faster to drive while boosting
        public        float boostAmount = 1.0f / 3.0f;  // How much boost player has
        private const float boostParticleDelay = 0.06f; // Time between every boost particle
        private       float boostParticleTime;

        private Random random = new Random();

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

            // Starts of inactive when first joining game
            isActive = false;
        }

        public override void Update(float deltaTime)
        {
            // Can only control player on connected client and if countdown has ended
            if (ID == ClientMain.localID && ClientMain.countDownTime <= 0.0f)
            {
                // Boost if button pressed, has juice and is not dashing
                boosting = Input.KeyHeld(Keys.LeftShift) && boostAmount > 0.0f && dashTime > dashDuration;

                // Can only drive while not dashing
                if (dashTime > dashDuration)
                {
                    ModifyInput(deltaTime);

                    Driving(deltaTime);
                    
                    Dashing(deltaTime);
                }

                dashTime += deltaTime;

                drag = dashTime > dashDuration ? 0.05f : 0.9f;

                ClientMain.SendMessage(MsgTypes.SetPlayer, GetData());
            }

            if (boosting)
            {
                boostAmount -= 0.5f * deltaTime;

                while (boostParticleTime > boostParticleDelay)
                {
                    Vector2 randomOffset = new Vector2((float)random.NextDouble(), (float)random.NextDouble());
                    Particles.AddBoostParticle(position, (Vector2.Normalize(-velocity) + randomOffset * 0.5f) * 25.0f);

                    boostParticleTime -= boostParticleDelay;
                }
                boostParticleTime += deltaTime;

                mass = 4.0f;
            }
            else
            {
                mass = 1.0f;
            }

            base.Update(deltaTime);
        }

        private void Dashing(float deltaTime)
        {
            if (dashTime > dashDelay)
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

                    Vector2 ballPosition = ClientMain.localGame.ball.position;

                    if (Vector2.Distance(position, ballPosition) <= 75.0f)
                    {
                        Vector2 dif = Vector2.Normalize(ballPosition - position);

                        if (Vector2.Dot(dir, dif) > 0.4f)
                        {
                            dir = dif;
                        }
                    }

                    velocity += dir * dashForce;

                    dashTime = 0;
                }
            }
        }

        // Makes directional input less responsive -> more realistic
        private void ModifyInput(float deltaTime)
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

            if (boosting) vertical = 1.0f;
        }

        private void Driving(float deltaTime)
        {
                        // Calculate positions of front and back wheels relative to center of car
            Vector2 dir = new Vector2((float)Math.Cos(rotation), (float)Math.Sin(rotation));
            Vector2 frontWheel = position + wheelDist * dir / 2.0f;
            Vector2 backWheel  = position - wheelDist * dir / 2.0f;

            // Move wheels according to player input 
            backWheel += vertical * turnSpeed * deltaTime * dir;
            frontWheel += vertical * turnSpeed * deltaTime * new Vector2(
                (float)Math.Cos(rotation + steerAngle * horizontal), 
                (float)Math.Sin(rotation + steerAngle * horizontal)); // Front wheels rotate with steerAngle

            // Calculate new velocity based on new position.
            // Not very happy with this, but it seems to work
            velocity += ((frontWheel + backWheel) / 2.0f - position) * speed * (boosting ? boostMult : 1.0f) * deltaTime;
            // Set rotation according to wheel positions
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
            data.AddRange(BitConverter.GetBytes(boosting  ));


            return data.ToArray();
        }

        public void SetData(byte[] data)
        {
            isActive   = BitConverter.ToBoolean(data, 0);
            position.X = BitConverter.ToSingle (data, 1);
            position.Y = BitConverter.ToSingle (data, 5);
            rotation   = BitConverter.ToSingle (data, 9);
            velocity.X = BitConverter.ToSingle (data, 13);
            velocity.Y = BitConverter.ToSingle (data, 17);
            boosting   = BitConverter.ToBoolean(data, 21);
        }
    }
}
