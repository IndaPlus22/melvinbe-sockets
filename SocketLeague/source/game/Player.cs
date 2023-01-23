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

        public int ID;

        public Player()
            : base(playerTexture)
        {
            scale = new Vector2(0.5f);

            isActive = false;
        }

        public override void Update(float deltaTime)
        {
            if (ID == ClientMain.localID)
            {
                position.X += Input.Horizontal() * 50.0f * deltaTime;
                position.Y += Input.vertical() * 50.0f * deltaTime;
            }

            Debug.WriteLine(texture);

            base.Update(deltaTime);
        }   

        public List<byte> GetData()
        {
            List<byte> data = new List<byte>();
            data.AddRange(BitConverter.GetBytes(isActive));
            data.AddRange(BitConverter.GetBytes(position.X));
            data.AddRange(BitConverter.GetBytes(position.Y));

            return data;
        }

        public void SetData(byte[] data)
        {
            isActive = BitConverter.ToBoolean(data, 0);
            position.X = BitConverter.ToSingle(data, 1);
            position.Y = BitConverter.ToSingle(data, 5);

            Debug.WriteLine(isActive);
        }
    }
}
