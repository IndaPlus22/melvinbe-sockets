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
        public int ID;

        public Player(int ID, Texture2D texture, Vector2 position)
            : base(texture, position)
        {
            this.ID = ID;
        }

        public override void Update(float deltaTime)
        {
            position.X += Input.Horizontal() * 50.0f * deltaTime;
            position.Y += Input.vertical() * 50.0f * deltaTime;

            base.Update(deltaTime);
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
