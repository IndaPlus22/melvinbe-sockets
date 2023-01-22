using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System.Collections.Generic;

namespace SocketLeague
{
    public abstract class Body : Sprite
    {
        public List<Collider> colliders = new List<Collider>();

        public Vector2 velocity;

        public bool isStatic;

        public Body(Texture2D texture, Vector2 position)
            : base(texture)
        {
            this.position = position;
        }

        public override void Update(float deltaTime)
        {
            
        }
    }
}
