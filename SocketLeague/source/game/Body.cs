using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System.Collections.Generic;

namespace SocketLeague
{
    public abstract class Body
    {
        public Vector2 position;
        public Vector2 velocity;

        public bool isStatic;

        public List<Collider> colliders = new List<Collider>();

        public Body(Vector2 position)
        {
            this.position = position;
        }

        public virtual void Update(float deltaTime)
        {
            
        }

        public abstract void Draw(SpriteBatch spriteBatch);
    }
}
