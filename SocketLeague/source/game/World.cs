using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SocketLeague
{
    public class World
    {
        public List<Sprite> sprites = new List<Sprite>();

        public List<Body> bodies = new List<Body>();

        public BoostPad[] boosts = new BoostPad[6];

        public Player[] players = new Player[4];

        public Ball ball;

        public static Vector2[] startingPositions = new Vector2[4]
        {
            new Vector2(-184, -60),
            new Vector2(184, 60),
            new Vector2(-184, 60),
            new Vector2(184, -60),
        };
        public static float[] startingRotations = new float[4]
        {
            0.0f,
            (float)Math.PI,
            0.0f,
            (float)Math.PI,
        };

        public static Vector2[] boostPositions = new Vector2[6]
        {
            new Vector2(-188, -96),
            new Vector2(188, 96),
            new Vector2(-188, 96),
            new Vector2(188, -96),
            new Vector2(0, 120),
            new Vector2(0, -120),
        };

        public World() 
        {
            for (int i = 0; i < 4; i++)
            {
                players[i] = new Player(i);
                sprites.Add(players[i]);
                bodies.Add(players[i]);
            }

            for (int i = 0; i < 6; i++)
            {
                boosts[i] = new BoostPad(boostPositions[i]);
                sprites.Add(boosts[i]);
            }

            ball = new Ball();
            sprites.Add(ball);
            bodies.Add(ball);

            Reset();
        }

        public void Reset()
        {
            ClientMain.countDownTime = 3.0f;

            for (int i = 0; i < 4; i++)
            {
                players[i].position = startingPositions[i];
                players[i].rotation = startingRotations[i];
                players[i].velocity = Vector2.Zero;
            }

            for (int i = 0; i < 6; i++)
            {
                boosts[i].refillTime = BoostPad.refillDuration;
            }

            ball.position = Vector2.Zero;
            ball.velocity = Vector2.Zero;
        }

        public void Update(float deltaTime)
        {
            if (deltaTime == 0.0f) return;

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

            foreach (Player player in players)
            {
                if (player.isActive)
                {
                    foreach (BoostPad boost in boosts)
                    {
                        if (boost.hasJuice)
                        {
                            Collision.PlayerToBoost(player, boost);
                        }
                    }
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
