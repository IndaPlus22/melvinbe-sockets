using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System.Collections.Generic;

namespace SocketLeague
{
    public abstract class Body : Sprite
    {
        public float radius;

        public Vector2 velocity;

        public Vector2 potentialPosition;

        public Body(Texture2D texture)
            : base(texture)
        {
            
        }

        public override void Update(float deltaTime)
        {
            potentialPosition = position + velocity * deltaTime;
        }

        public virtual void Move()
        {
            position = potentialPosition;
        }
    }
}
