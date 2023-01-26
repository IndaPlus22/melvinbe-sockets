using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;

namespace SocketLeague
{
    public abstract class Body : Sprite
    {
        // Physics properties
        public float radius; // Radius of collider
        public float mass;   // More massive objects push less massive objects more
        public float bounce; // How much of velocity should be kept after bounce?
        public float drag;   // Percentage of velocity to be kept every second

        public Vector2 velocity;

        public Body(Texture2D texture)
            : base(texture)
        {
            
        }

        public override void Update(float deltaTime)
        {
            // This formula apparently calculates frame rate independant drag.
            // Which is important as the client is limited to 60 fps and the server is not!
            position += velocity * (float)(Math.Pow(drag, deltaTime * deltaTime) - 1.0f) / (deltaTime * (float)Math.Log2(drag));

            velocity *= (float)Math.Pow(drag, deltaTime);
        }
    }
}
