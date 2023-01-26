using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System.Collections.Generic;

namespace SocketLeague
{
    public abstract class Collider
    {
        // Position of collider
        public int x, y;
    }

    public enum CircleQuadrant
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }
    public class InvertedCircleCollider : Collider
    {
        public int radius;

        // Limit collision to a specified quadrant of the circle
        public Rectangle bounds;

        public InvertedCircleCollider(int x, int y, int radius, CircleQuadrant quadrant)
        {
            this.x = x;
            this.y = y;
            this.radius = radius;

            // Set rect based on quadrant chosen
            switch (quadrant)
            {
                case CircleQuadrant.TopLeft:
                    bounds = new Rectangle(x - radius, y - radius, radius, radius); break;
                case CircleQuadrant.TopRight:
                    bounds = new Rectangle(x         , y - radius, radius, radius); break;
                case CircleQuadrant.BottomLeft:
                    bounds = new Rectangle(x - radius, y         , radius, radius); break;
                case CircleQuadrant.BottomRight:
                    bounds = new Rectangle(x         , y         , radius, radius); break;
            }
        }
    }

    public class RectangleCollider : Collider
    {
        public int width, height;

        public RectangleCollider(int x, int y, int width, int height)
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

        // Debug textures for primitive shape
        private const bool drawDebug = false;
        public static Texture2D circleTexture;
        public static Texture2D squareTexture;

        public static List<Collider> colliders = new List<Collider>
        {
            // Border
            new RectangleCollider(-480, -184, 960, 32 ),
            new RectangleCollider(-480,  152, 960, 32 ),
            new RectangleCollider(-340, -270, 48 , 540),
            new RectangleCollider( 292, -270, 48 , 540),
             
            // Corner rectangles
            new RectangleCollider(-340, -184, 96, 144),
            new RectangleCollider(-340,  40 , 96, 144),
            new RectangleCollider( 244, -184, 96, 144),
            new RectangleCollider( 244,  40 , 96, 144),

            // Corner circles
            new InvertedCircleCollider(-172, -80, 72, CircleQuadrant.TopLeft),
            new InvertedCircleCollider( 172, -80, 72, CircleQuadrant.TopRight),
            new InvertedCircleCollider(-172,  80, 72, CircleQuadrant.BottomLeft),
            new InvertedCircleCollider( 172,  80, 72, CircleQuadrant.BottomRight),

            // Goal corner circles
            new InvertedCircleCollider(-276, -24, 16, CircleQuadrant.TopLeft),
            new InvertedCircleCollider( 276, -24, 16, CircleQuadrant.TopRight),
            new InvertedCircleCollider(-276,  24, 16, CircleQuadrant.BottomLeft),
            new InvertedCircleCollider( 276,  24, 16, CircleQuadrant.BottomRight),
        };

        public static void Draw(SpriteBatch spriteBatch)
        {
            if (!drawDebug)
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
                    0.1f
                );
            }
            else
            {
                foreach (Collider collider in colliders)
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
                            0.1f
                        );
                    }
                    else if (collider is RectangleCollider)
                    {
                        RectangleCollider col = collider as RectangleCollider;

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
                            0.1f
                        );
                    }
                }
            }
        }
    }
}
