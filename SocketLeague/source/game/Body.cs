using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace SocketLeague
{
    public abstract class Body : Sprite
    {
        public float radius;

        public float mass;
        public float bounce;
        public float drag;

        public Vector2 velocity;

        public Body(Texture2D texture)
            : base(texture)
        {
            
        }

        public override void Update(float deltaTime)
        {
            position += velocity * (float)(Math.Pow(drag, deltaTime * deltaTime) - 1.0f) / (deltaTime * (float)Math.Log2(drag));

            velocity *= (float)Math.Pow(drag, deltaTime);
        }
    }
}
