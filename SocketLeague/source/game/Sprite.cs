using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SocketLeague
{
    public class Sprite
    {
        public Texture2D texture;

        public Vector2 position;
        public float   rotation;
        public Vector2 scale = Vector2.One;
        public Color   color = Color.White;
        public int     sortingLayer; // Determines drawing order

        public bool isActive = true;

        public Sprite(Texture2D texture)
        {
            this.texture = texture;
        }

        public virtual void Update(float deltaTime)
        {
            
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw
            (
                texture,
                Vector2.Floor(position - Vector2.Floor(Camera.position)),
                texture.Bounds,
                color,
                rotation,
                new Vector2(texture.Width / 2, texture.Height / 2),
                scale,
                SpriteEffects.None,
                0.5f + sortingLayer / 10000f
            );
        }
    }
}
