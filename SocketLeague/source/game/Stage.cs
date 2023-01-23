using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System.Collections.Generic;

namespace SocketLeague
{
    public abstract class Collider
    {

    }

    public enum CircleCorners
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }
    public class InvertedCircleCollider : Collider
    {
        public int x, y;

        public int radius;

        public Rectangle bounds;

        public InvertedCircleCollider(int x, int y, int radius, CircleCorners corner)
        {
            this.x = x;
            this.y = y;
            this.radius = radius;

            if (corner == CircleCorners.TopLeft)
            {
                bounds = new Rectangle(x - radius, y - radius, radius, radius);
            }
            if (corner == CircleCorners.TopRight)
            {
                bounds = new Rectangle(x, y - radius, radius, radius);
            }
            if (corner == CircleCorners.BottomLeft)
            {
                bounds = new Rectangle(x - radius, y, radius, radius);
            }
            if (corner == CircleCorners.BottomRight)
            {
                bounds = new Rectangle(x, y, radius, radius);
            }
        }
    }

    public class SquareCollider : Collider
    {
        public int x, y;

        public int width, height;

        public SquareCollider(int x, int y, int width, int height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }
    }

    public static class Stage
    {
        public static Texture2D circleTexture;
        public static Texture2D squareTexture;

        public static List<Collider> colliders = new List<Collider>
        {
            new InvertedCircleCollider(GameWindow.REFERENCE_WIDTH / 2, GameWindow.REFERENCE_HEIGHT / 2, 100, CircleCorners.TopRight),
            new SquareCollider(250, 150, 50, 75),
        };

        public static void Draw(SpriteBatch spriteBatch)
        {
            foreach (Collider collider in colliders)
            {
                if (collider is InvertedCircleCollider)
                {
                    InvertedCircleCollider col = collider as InvertedCircleCollider;

                    spriteBatch.Draw
                    (
                        circleTexture,
                        new Vector2(col.x, col.y),
                        circleTexture.Bounds,
                        Color.DarkBlue,
                        0.0f,
                        new Vector2(circleTexture.Width / 2, circleTexture.Height / 2),
                        col.radius / 128.0f,
                        SpriteEffects.None,
                        0.5f + -1000.0f / 10000f
                    );
                }
                else if (collider is SquareCollider)
                {
                    SquareCollider col = collider as SquareCollider;

                    spriteBatch.Draw
                    (
                        squareTexture,
                        new Vector2(col.x, col.y),
                        squareTexture.Bounds,
                        Color.Green,
                        0.0f,
                        new Vector2(0, 0),
                        new Vector2(col.width, col.height),
                        SpriteEffects.None,
                        0.5f + 1000.0f / 10000f
                    );
                }
            }
        }
    }
}
