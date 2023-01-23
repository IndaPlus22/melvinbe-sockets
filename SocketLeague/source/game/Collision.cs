using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.AccessControl;

namespace SocketLeague
{
    public static class Collision
    {
        public static void BodyToBody(Body b1, Body b2)
        {
            float diameter = b1.radius + b2.radius;
            float distance = Vector2.Distance(b1.potentialPosition, b2.potentialPosition);

            float dif = distance - diameter;

            if (dif <= 0.0f)
            {
                Vector2 normal = Vector2.Normalize(b1.potentialPosition - b2.potentialPosition);

                b1.potentialPosition -= new Vector2(normal.X, normal.Y) * dif / 2.0f;
                b2.potentialPosition += new Vector2(normal.X, normal.Y) * dif / 2.0f;

                b1.velocity += new Vector2(normal.X, normal.Y);
                b2.velocity -= new Vector2(normal.X, normal.Y);
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

            if (!circle.bounds.Contains(body.potentialPosition)) return;

            float diameter = -body.radius + circle.radius;
            float distance = Vector2.Distance(body.potentialPosition, circlePosition);

            float dif = distance - diameter;

            if (dif >= 0.0f)
            {
                Vector2 normal = Vector2.Normalize(body.potentialPosition - circlePosition);

                body.potentialPosition -= new Vector2(normal.X, normal.Y) * dif;

                body.velocity -= new Vector2(normal.X, normal.Y);
            }
        }

        private static void BodyToSquare(Body body, SquareCollider square) 
        {
            Vector2 nearestPoint;
            nearestPoint.X = Math.Max(square.x, Math.Min(square.x + square.width, body.potentialPosition.X));
            nearestPoint.Y = Math.Max(square.y, Math.Min(square.y + square.height, body.potentialPosition.Y));

            Vector2 rayToNearest = nearestPoint - body.potentialPosition;

            float overlap = body.radius - rayToNearest.Length();

            Vector2 normalized = rayToNearest;
            normalized.Normalize();
            Vector2 dir = rayToNearest == Vector2.Zero ? Vector2.Zero : normalized;

            if (overlap > 0.0f)
            {
                body.potentialPosition -= dir * overlap;
                body.velocity -= dir * overlap;
            }
        }
    }
}
