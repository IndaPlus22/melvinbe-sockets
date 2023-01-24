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
        public static Texture2D stageTexture;

        public static Texture2D circleTexture;
        public static Texture2D squareTexture;

        public static List<Collider> colliders = new List<Collider>
        {
            // Border
            new SquareCollider(-480, -184,        960, 32),
            new SquareCollider(-480, 184 - 32,    960, 32),
            new SquareCollider(-340, -270,        48, 540),
            new SquareCollider(340 - 48, -270,    48, 540),
             
            // Corner squares
            new SquareCollider(-340, -184, 96, 144),
            new SquareCollider(-340, 184 - 144, 96, 144),
            new SquareCollider(340 - 96, -184, 96, 144),
            new SquareCollider(340 - 96, 184 - 144, 96, 144),

            // Corner circles
            new InvertedCircleCollider(-172, -80, 72, CircleCorners.TopLeft),
            new InvertedCircleCollider(172, -80, 72, CircleCorners.TopRight),
            new InvertedCircleCollider(-172, 80, 72, CircleCorners.BottomLeft),
            new InvertedCircleCollider(172, 80, 72, CircleCorners.BottomRight),

            // Goal corner circles
            new InvertedCircleCollider(-276, -24, 16, CircleCorners.TopLeft),
            new InvertedCircleCollider(276, -24, 16, CircleCorners.TopRight),
            new InvertedCircleCollider(-276, 24, 16, CircleCorners.BottomLeft),
            new InvertedCircleCollider(276, 24, 16, CircleCorners.BottomRight),
        };

        public static void Draw(SpriteBatch spriteBatch)
        {
            foreach (Collider collider in colliders)
            {
                if (true)
                {
                    spriteBatch.Draw
                    (
                        stageTexture,
                        Vector2.Floor(-Vector2.Floor(Camera.position)),
                        stageTexture.Bounds,
                        Color.White,
                        0.0f,
                        new Vector2(stageTexture.Width / 2, stageTexture.Height / 2),
                        Vector2.One,
                        SpriteEffects.None,
                        0.5f + -1000.0f / 10000f
                    );
                }
                if (false)
                {
                    if (collider is InvertedCircleCollider)
                    {
                        InvertedCircleCollider col = collider as InvertedCircleCollider;

                        spriteBatch.Draw
                        (
                            circleTexture,
                            Vector2.Floor(new Vector2(col.x, col.y) - Vector2.Floor(Camera.position)),
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
                            Vector2.Floor(new Vector2(col.x, col.y) - Vector2.Floor(Camera.position)),
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
}
