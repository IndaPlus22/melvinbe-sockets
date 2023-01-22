using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SocketLeague
{
    public class Player : Body
    {
        public static Texture2D texture;

        public Player(Vector2 position)
            : base(position)
        {
            
        }

        public override void Update(float deltaTime)
        {
            position.X += Input.Horizontal() * 50.0f * deltaTime;
            position.Y += Input.vertical() * 50.0f * deltaTime;
        }   

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw
            (
                texture,
                position,
                texture.Bounds,
                Color.White,
                0.0f,
                new Vector2(texture.Width / 2, texture.Height / 2),
                100.0f / texture.Width,
                SpriteEffects.None,
                0.0f
            );
        }

        public List<byte> GetData()
        {
            List<byte> data = new List<byte>();
            data.AddRange(BitConverter.GetBytes(position.X));
            data.AddRange(BitConverter.GetBytes(position.Y));

            return data;
        }
    }
}
