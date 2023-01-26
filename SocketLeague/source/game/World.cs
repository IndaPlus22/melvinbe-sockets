using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SocketLeague
{
    public class World
    {
        public List<Sprite> sprites = new List<Sprite>();

        public List<Body>   bodies  = new List<Body>();

        public BoostPad[]   boosts  = new BoostPad[6];

        public Player[]     players = new Player[4];

        public Ball ball;

        // Starting positions and rotations of players:
        public static Vector2[] startingPositions = new Vector2[4]
        {
            new Vector2(-184, -60),
            new Vector2( 184,  60),
            new Vector2(-184,  60),
            new Vector2( 184, -60),
        };
        public static float[] startingRotations = new float[4]
        {
            0.0f,
            (float)Math.PI, // Opposing team is flipped 180 degrees
            0.0f,
            (float)Math.PI,
        };

        // Positions of boostPads
        public static Vector2[] boostPositions = new Vector2[6]
        {
            new Vector2(-188, -96 ),
            new Vector2( 188,  96 ),
            new Vector2(-188,  96 ),
            new Vector2( 188, -96 ),
            new Vector2( 0,    120),
            new Vector2( 0,   -120),
        };

        public World() 
        {
            // Add 4 players
            for (int i = 0; i < 4; i++)
            {
                players[i] = new Player(i);
                sprites.Add(players[i]);
                bodies.Add(players[i]);
            }

            // Add 6 boosts
            for (int i = 0; i < 6; i++)
            {
                boosts[i] = new BoostPad(boostPositions[i]);
                sprites.Add(boosts[i]);
            }

            // Add 1 ball
            ball = new Ball();
            sprites.Add(ball);
            bodies.Add(ball);

            Reset();
        }

        // Restores world inhabitants to initial values
        public void Reset()
        {
            ClientMain.countDownTime = 3.0f;

            // Reset Players
            for (int i = 0; i < 4; i++)
            {
                players[i].position    = startingPositions[i];
                players[i].rotation    = startingRotations[i];
                players[i].velocity    = Vector2.Zero;
                players[i].boostAmount = 1.0f / 3.0f;
                players[i].boosting    = false;
            }

            // Reset boostPads
            for (int i = 0; i < 6; i++)
            {
                boosts[i].refillTime = BoostPad.refillDuration;
            }

            // Reset ball
            ball.position = Vector2.Zero;
            ball.velocity = Vector2.Zero;

            Particles.Clear();
        }

        public void Update(float deltaTime)
        {
            // Don't do anything if no time has passed. 
            // Prevents division by 0
            if (deltaTime == 0.0f) return;

            // Update all sprites
            foreach (Sprite sprite in sprites)
            {
                if (sprite.isActive)
                {
                    sprite.Update(deltaTime);
                }
            }

            // Check collision between every body and every other body
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

            // Check collision between every body and the stage
            foreach (Body body in bodies)
            {
                if (body.isActive)
                {
                    Collision.BodyToStage(body);
                }
            }

            // Check collision between players and boostPads with juice
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
            // Draw every sprite that has a texture
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
