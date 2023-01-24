using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Microsoft.Xna.Framework.Input;

namespace SocketLeague
{
    public static class Camera
    {
        public static Sprite followTarget;

        public static Vector2 position;

        public static Vector2 offset = new Vector2(-GameWindow.REFERENCE_WIDTH / 2, -GameWindow.REFERENCE_HEIGHT / 2);

        public static void Update(float deltaTime)
        {
            if (followTarget != null)
            {
                Vector2 midPosition = Vector2.Lerp(followTarget.position, ClientMain.localGame.ball.position, 1.0f / 3.0f);
                midPosition = Vector2.Lerp(Vector2.Zero, midPosition, 1.0f / 3.0f);
                position = Vector2.Floor(midPosition + offset);
            }
        }
    }
}
