using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Intrinsics.X86;
using System.Security.AccessControl;

namespace SocketLeague
{
    public static class Collision
    {
        public static void BodyToBody(Body b1, Body b2)
        {
            float diameter = b1.radius + b2.radius;
            float distance = Vector2.Distance(b1.position, b2.position);

            float dif = distance - diameter;

            if (dif <= 0.0f)
            {
                Vector2 normal = b1.position - b2.position;
                if (normal != Vector2.Zero) normal.Normalize();

                b1.position -= new Vector2(normal.X, normal.Y) * dif / 2.0f;
                b2.position += new Vector2(normal.X, normal.Y) * dif / 2.0f;

                float p = 2.0f * (b1.velocity.X * normal.X + b1.velocity.Y * normal.Y - b2.velocity.X * normal.X - b2.velocity.Y * normal.Y) /
                        (b1.mass + b2.mass);

                b1.velocity -= new Vector2(normal.X, normal.Y) * p * b2.mass * b1.bounce;
                b2.velocity += new Vector2(normal.X, normal.Y) * p * b1.mass * b2.bounce;
            }
        }

        public static void PlayerToBoost(Player player, BoostPad boost)
        {
            float diameter = player.radius + BoostPad.radius;
            float distance = Vector2.Distance(player.position, boost.position);

            float dif = distance - diameter;

            if (dif <= 0.0f)
            {
                boost.refillTime = 0.0f;

                player.boostAmount = 1.0f;
            }
        }

        public static void BodyToStage(Body body)
        {
            foreach (Collider collider in Stage.colliders)
            {
                if (collider is InvertedCircleCollider)
                {
                    BodyToInvertedCircle(body, collider as InvertedCircleCollider);
                }
                else if (collider is SquareCollider)
                {
                    BodyToSquare(body, collider as SquareCollider);
                }
            }
        }

        private static void BodyToInvertedCircle(Body body, InvertedCircleCollider circle)
        {
            Vector2 circlePosition = new Vector2(circle.x, circle.y);

            if (!circle.bounds.Contains(body.position)) return;

            float diameter = -body.radius + circle.radius;
            float distance = Vector2.Distance(body.position, circlePosition);

            float dif = distance - diameter;

            if (dif >= 0.0f)
            {
                Vector2 normal = body.position - circlePosition;
                if (normal != Vector2.Zero) normal.Normalize();

                body.position -= new Vector2(normal.X, normal.Y) * dif;

                float p = 2.0f * (body.velocity.X * normal.X + body.velocity.Y * normal.Y);

                body.velocity -= new Vector2(normal.X, normal.Y) * p * body.bounce;
            }
        }

        private static void BodyToSquare(Body body, SquareCollider square) 
        {
            Vector2 nearestPoint;
            nearestPoint.X = Math.Max(square.x, Math.Min(square.x + square.width, body.position.X));
            nearestPoint.Y = Math.Max(square.y, Math.Min(square.y + square.height, body.position.Y));

            Vector2 rayToNearest = nearestPoint - body.position;

            float overlap = body.radius - rayToNearest.Length();

            Vector2 dir = rayToNearest == Vector2.Zero ? Vector2.Zero : Vector2.Normalize(rayToNearest);

            if (overlap > 0.0f)
            {
                body.position -= dir * overlap;

                Vector2 normal = body.position - nearestPoint;
                if (normal != Vector2.Zero) normal.Normalize();

                float p = 2.0f * (body.velocity.X * normal.X + body.velocity.Y * normal.Y);

                body.velocity -= new Vector2(normal.X, normal.Y) * p * body.bounce;
            }
        }
    }
}
