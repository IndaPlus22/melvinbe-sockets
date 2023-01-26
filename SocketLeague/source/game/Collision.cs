using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SocketLeague
{
    public static class Collision
    {
        // Detect and resolve collision between two circular bodies
        public static void BodyToBody(Body b1, Body b2)
        {
            // Diameter of both circles
            float diameter = b1.radius + b2.radius;
            // Distance between circle centers
            float distance = Vector2.Distance(b1.position, b2.position);

            float dif = distance - diameter;

            // Check intersection
            if (dif <= 0.0f)
            {
                // Normal between circles
                Vector2 normal = b1.position - b2.position;
                if (normal != Vector2.Zero) normal.Normalize();

                // Solve intersection in normal direction
                b1.position -= normal * dif / 2.0f;
                b2.position += normal * dif / 2.0f;

                // Calculate velocity change for both bodies:
                float p = 2.0f * (
                    b1.velocity.X * normal.X + b1.velocity.Y * normal.Y - 
                    b2.velocity.X * normal.X - b2.velocity.Y * normal.Y) /
                        (b1.mass + b2.mass);
                
                // Add velocity along normal scaled by various values
                b1.velocity -= normal * p * b2.mass * b1.bounce;
                b2.velocity += normal * p * b1.mass * b2.bounce;
            }
        }

        // Detect collision between player and boostPad
        public static void PlayerToBoost(Player player, BoostPad boost)
        {
            // Diameter of both circles
            float diameter = player.radius + BoostPad.radius;
            // Distance between circle centers
            float distance = Vector2.Distance(player.position, boost.position);

            float dif = distance - diameter;

            // Check intersection
            if (dif <= 0.0f)
            {
                // No resolution here, only detection

                boost.refillTime = 0.0f;

                player.boostAmount = 1.0f;
            }
        }

        // Check collision of a body with every collider on stage
        public static void BodyToStage(Body body)
        {
            foreach (Collider collider in Stage.colliders)
            {
                if (collider is InvertedCircleCollider)
                {
                    BodyToInvertedCircle(body, collider as InvertedCircleCollider);
                }
                else if (collider is RectangleCollider)
                {
                    BodyToRectangle(body, collider as RectangleCollider);
                }
            }
        }

        private static void BodyToInvertedCircle(Body body, InvertedCircleCollider circle)
        {
            Vector2 circlePosition = new Vector2(circle.x, circle.y);

            // Only do collisions in specified quadrant
            if (!circle.bounds.Contains(body.position)) return;

            // Diameter of both circles, with inverted as negative
            float diameter = -body.radius + circle.radius;
            // Distance between circle centers
            float distance = Vector2.Distance(body.position, circlePosition);

            float dif = distance - diameter;

            // Check intersection
            if (dif >= 0.0f)
            {
                // Normal between circles
                Vector2 normal = body.position - circlePosition;
                if (normal != Vector2.Zero) normal.Normalize();

                // Solve intersection in normal direction
                body.position -= normal * dif;

                // Calculate velocity change of body.
                // Inverted circle collider has infinite mass.
                float p = 2.0f * (body.velocity.X * normal.X + body.velocity.Y * normal.Y);

                // Add velocity along normal scaled by various values
                body.velocity -= normal * p * body.bounce;
            }
        }

        private static void BodyToRectangle(Body body, RectangleCollider square) 
        {
            // Get nearset point on rectangle
            Vector2 nearestPoint;
            nearestPoint.X = Math.Max(square.x, Math.Min(square.x + square.width, body.position.X));
            nearestPoint.Y = Math.Max(square.y, Math.Min(square.y + square.height, body.position.Y));

            // Direction to that point
            Vector2 rayToNearest = nearestPoint - body.position;

            float overlap = body.radius - rayToNearest.Length();

            // Avoid normalizing zero vector
            Vector2 dir = rayToNearest == Vector2.Zero ? Vector2.Zero : Vector2.Normalize(rayToNearest);

            // Check intersection
            if (overlap > 0.0f)
            {
                // Resolve collision in dir direction
                body.position -= dir * overlap;

                // Normal between circle and point on rectangle
                Vector2 normal = body.position - nearestPoint;
                if (normal != Vector2.Zero) normal.Normalize();

                // Calculate velocity change of body.
                // Rectangle collider has infinite mass.
                float p = 2.0f * (body.velocity.X * normal.X + body.velocity.Y * normal.Y);

                // Add velocity along normal scaled by various values
                body.velocity -= normal * p * body.bounce;
            }
        }
    }
}
