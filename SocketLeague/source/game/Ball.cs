using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SocketLeague
{
    public class Ball : Body
    {
        public static Texture2D ballTexture;

        public Ball()
            : base(ballTexture)
        {
            scale = new Vector2(0.09f);

            radius = 0.09f * 256.0f / 2.0f;

            mass = 0.4f;
            bounce = 0.8f;
            drag = 0.70f;
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
        }

        public byte[] GetData()
        {
            List<byte> data = new List<byte>();
            data.AddRange(BitConverter.GetBytes(position.X));
            data.AddRange(BitConverter.GetBytes(position.Y));
            data.AddRange(BitConverter.GetBytes(velocity.X));
            data.AddRange(BitConverter.GetBytes(velocity.Y));

            return data.ToArray();
        }

        public void SetData(byte[] data)
        {
            position.X = BitConverter.ToSingle(data, 0);
            position.Y = BitConverter.ToSingle(data, 4);
            velocity.X = BitConverter.ToSingle(data, 8);
            velocity.Y = BitConverter.ToSingle(data, 12);

            Debug.WriteLine(isActive);
        }
    }
}
