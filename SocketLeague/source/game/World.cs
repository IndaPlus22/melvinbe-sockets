using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System.Collections.Generic;
using System.Diagnostics;

namespace SocketLeague
{
    public class World
    {
        public List<Sprite> sprites = new List<Sprite>();

        public List<Body> bodies = new List<Body>();

        public Player[] players = new Player[4];

        public Ball ball;

        public World() 
        {
            for (int i = 0; i < 4; i++)
            {
                players[i] = new Player(i);
                Add(players[i]);
            }

            ball = new Ball();
            Add(ball);

            Reset();
        }

        public void Reset()
        {
            players[0].position = new Vector2(100.0f, 100.0f);
            players[1].position = new Vector2(100.0f, 100.0f);
            players[2].position = new Vector2(100.0f, 100.0f);
            players[3].position = new Vector2(100.0f, 100.0f);
            for (int i = 0; i < 4; i++)
            {
                players[i].velocity = Vector2.Zero;
            }

            ball.position = new Vector2(200.0f, 150.0f);
            ball.velocity = Vector2.Zero;
        }

        public void Add(Sprite sprite)
        {
            sprites.Add(sprite);

            if (sprite is Body) bodies.Add(sprite as Body);
        }

        public void Update(float deltaTime)
        {
            foreach (Sprite sprite in sprites)
            {
                if (sprite.isActive)
                {
                    sprite.Update(deltaTime);
                }
            }

            for (int a = 0; a < bodies.Count; a++)
            {
                if (!bodies[a].isActive) continue;

                for (int b = a; b < bodies.Count; b++)
                {
                    if (!bodies[b].isActive) continue;

                    if (a != b)
                    {
                        Collision.BodyToBody(bodies[a], bodies[b]);
                    }
                }
            }

            foreach (Body body in bodies)
            {
                if (body.isActive)
                {
                    Collision.BodyToStage(body);
                }
            }

            foreach (Body body in bodies)
            {
                if (body.isActive)
                {
                    body.Move();
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (Sprite sprite in sprites)
            {
                if (sprite.texture != null && sprite.isActive)
                {
                    sprite.Draw(spriteBatch);
                }
            }
        }
    }
}
