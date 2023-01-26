using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SocketLeague
{
    public class Player : Body
    {
        public static Texture2D bluePlayerTexture;
        public static Texture2D orangePlayerTexture;

        // Connects players to respective client
        public int ID;

        // Modifiable player input values
        private float vertical;
        private float horizontal;

        // Driving
        private const float speed              = 320.0f; // Forward speed
        private const float turnSpeed          = 50.0f;  // Rotation speed
        private const float wheelDist          = 10.0f;  // Distance between front and back wheels
        private const float steerAngle         = 0.5f;   // Front wheel rotation angle

        // Dashing
        private       bool  dashing;
        private const float dashForce          = 180.0f; // Speed gain from dashing
        private const float dashDuration       = 0.2f;   // How long player loses control while dashing
        private const float dashDelay          = 1.1f;   // How long player must wait to dash again
        private       float dashTime;

        // Boosting
        public        bool  boosting;                    // true for all clients while player is boosting
        private const float boostMult          = 2.0f;   // How much faster to drive while boosting
        public        float boostAmount;                 // How much juice player has
        private const float boostParticleDelay = 0.06f;  // Time between every boost particle
        private       float boostParticleTime;

        private Random random = new Random();

        public Player(int ID)
            : base(ID % 2 == 0 ? orangePlayerTexture : bluePlayerTexture) // Choose color texture based on ID
        {
            this.ID = ID;

            // Set physics properties
            radius = 10.0f;
            mass   = 1.0f;
            bounce = 0.4f;
            drag   = 0.9f;

            // Starts of inactive when first joining game
            isActive = false;
        }

        public override void Update(float deltaTime)
        {
            // Can only control player on connected client and if countdown has ended
            if (ID == ClientMain.localID && ClientMain.countDownTime <= 0.0f)
            {
                // Boost if key pressed, has juice and is not dashing
                boosting = Input.KeyHeld(Keys.LeftShift) && boostAmount > 0.0f && dashTime > dashDuration;

                // Dash for dashDuration seconds after button pressed
                dashing = dashTime > dashDuration;

                // Can only drive while not dashing
                if (dashing)
                {
                    ModifyInput(deltaTime);

                    Driving(deltaTime);
                    
                    Dashing();
                }

                // Reduce drag while dashing
                drag = dashing ? 0.05f : 0.9f;

                dashTime += deltaTime;
            }

            // Do boosting for all players to make all clients draw particles
            if (boosting) Boosting(deltaTime);

            // Increase mass while boosting
            mass = boosting ? 4.0f : 1.0f;

            // Only update server if this player belongs to client
            if (ID == ClientMain.localID) ClientMain.SendMessage(MsgTypes.SetPlayer, GetData());

            base.Update(deltaTime);
        }

        // Makes directional input less responsive -> more realistic.
        // Could be done better but no time
        private void ModifyInput(float deltaTime)
        {
            if (Math.Abs(Input.Horizontal()) != 0.0f)
            {
                horizontal += Input.Horizontal() * deltaTime * 5.0f;
                horizontal = Math.Clamp(horizontal, -1.0f, 1.0f);
            }
            else
            {
                // Slowly bring steering back to middle when not steering
                horizontal -= Math.Clamp(horizontal * 10000.0f, -1.0f, 1.0f) * deltaTime * 4.0f;
            }

            if (Math.Abs(Input.Vertical()) != 0.0f)
            {
                vertical += Input.Vertical() * deltaTime * 2.0f;
                vertical = Math.Clamp(vertical, -1.0f, 1.0f);
            }
            else
            {
                // Release gas instantly
                vertical = 0.0f;
            }

            // Always go forward while boosting
            if (boosting) vertical = 1.0f;
        }

        // Drives and steer (mostly) like a car
        private void Driving(float deltaTime)
        {
            // Calculate positions of front and back wheels relative to center of car
            Vector2 dir = new Vector2((float)Math.Cos(rotation), (float)Math.Sin(rotation));
            Vector2 frontWheel = position + wheelDist * dir / 2.0f;
            Vector2 backWheel = position - wheelDist * dir / 2.0f;

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

        // Gain a burst of speed and lose control on a short cooldown
        private void Dashing()
        {
            // If dash is not on cooldown and key pressed
            if (dashTime > dashDelay && Input.KeyDown(Keys.Space))
            {
                // Calculate dash direction based on input:
                Vector2 dir;
                if (Input.Horizontal() != 0.0f)
                {
                    dir = new Vector2(Input.Horizontal(), 0.0f);
                }
                else
                {
                    dir = new Vector2(0.0f, -Input.Vertical());
                }

                // Rotate dash direction vector
                float sin = (float)Math.Sin(rotation + Math.PI / 2.0f);
                float cos = (float)Math.Cos(rotation + Math.PI / 2.0f);
                float tx = dir.X;
                float ty = dir.Y;
                dir.X = (cos * tx) - (sin * ty);
                dir.Y = (sin * tx) + (cos * ty);

                // Aim assist towards ball position:
                Vector2 ballPosition = ClientMain.localGame.ball.position;

                if (Vector2.Distance(position, ballPosition) <= 75.0f)
                {
                    Vector2 dif = Vector2.Normalize(ballPosition - position);

                    if (Vector2.Dot(dir, dif) > 0.3f)
                    {
                        dir = dif;
                    }
                }

                velocity += dir * dashForce;

                dashTime = 0.0f;
            }
        }

        // Go faster and with more force at the cost of boost juice
        private void Boosting(float deltaTime)
        {
            boostAmount -= 0.5f * deltaTime;

            while (boostParticleTime > boostParticleDelay)
            {
                // Create particles at exhaust position
                Vector2 posOffset = new Vector2((float)Math.Cos(rotation), (float)Math.Sin(rotation));
                posOffset *= -10.0f;

                // With random velocity direction
                Vector2 velOffset = new Vector2((float)random.NextDouble(), (float)random.NextDouble());
                velOffset.Normalize();

                Particles.AddBoostParticle(position + posOffset, (Vector2.Normalize(-velocity) + velOffset * 0.5f) * 30.0f);

                boostParticleTime -= boostParticleDelay;
            }

            boostParticleTime += deltaTime;
        }

        // Converts important player values to byte array
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

        // Converts byte array to important player values
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
