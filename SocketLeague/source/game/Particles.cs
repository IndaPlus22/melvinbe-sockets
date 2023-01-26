using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace SocketLeague
{
    // A simple particle created as players are boosting
    public struct BoostParticle
    {
        public const float minScale = 0.02f;
        public const float maxScale = 0.08f;
        public const float lifeSpan = 0.8f;

        public float lifeTime;
        public Vector2 position;
        public Vector2 velocity;
        public float scale;

        public BoostParticle(Vector2 position, Vector2 velocity)
        {
            lifeTime = 0.0f;
            this.position = position;
            this.velocity = velocity;
            scale = minScale;
        }
    }

    public static class Particles
    {
        public static Texture2D boostTexture;

        private static List<BoostParticle> boostParticles = new List<BoostParticle>();
        
        public static void Clear()
        {
            boostParticles.Clear();
        }

        // Creates a new particle with specified values
        public static void AddBoostParticle(Vector2 position, Vector2 velocity)
        {
            BoostParticle newParticle = new BoostParticle(position, velocity);

            boostParticles.Add(newParticle);
        }

        public static void Update(float deltaTime)
        {
            for (int i = 0; i < boostParticles.Count; i++)
            {
                // Create new particle with new values
                BoostParticle particle = boostParticles[i];

                particle.lifeTime += deltaTime;

                // Linearly interpolate scale based on lifetime
                float lerp = BoostParticle.minScale + (particle.lifeTime / BoostParticle.lifeSpan) * (BoostParticle.maxScale - BoostParticle.minScale);
                particle.scale = lerp;

                particle.position.Y -= 30.0f * deltaTime; // Make particle drift upwards as it were smoke
                particle.position += particle.velocity * deltaTime;

                // Replace old particle with new
                boostParticles[i] = particle;
            }

            // Remove particles that have reached the end of their life
            boostParticles = boostParticles.Where(particle => particle.lifeTime < BoostParticle.lifeSpan).ToList();
        }
        public static void Draw(SpriteBatch spriteBatch)
        {
            foreach (BoostParticle particle in boostParticles)
            {
                spriteBatch.Draw
                (
                    boostTexture,
                    Vector2.Floor(particle.position - Vector2.Floor(Camera.position)),
                    boostTexture.Bounds,
                    Color.White,
                    0.0f,
                    new Vector2(boostTexture.Width / 2, boostTexture.Height / 2),
                    particle.scale,
                    SpriteEffects.None,
                    0.5f + 0.0f / 10000f
                );
            }
        }
    }
}
