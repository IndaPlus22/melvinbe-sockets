using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SocketLeague
{
    public static class Camera
    {
        public static Sprite followTarget;

        public static Vector2 position;

        // Position followed sprite at center of window
        public static Vector2 offset = new Vector2(-Screen.REFERENCE_WIDTH / 2, -Screen.REFERENCE_HEIGHT / 2);

        public static void Update(float deltaTime)
        {
            if (followTarget != null)
            {
                // Calculate position between player and ball
                Vector2 midPosition = Vector2.Lerp(followTarget.position, ClientMain.localGame.ball.position, 1.0f / 3.0f);
                // Calculate psotition between player, ball and middle of stage
                midPosition = Vector2.Lerp(Vector2.Zero, midPosition, 1.0f / 3.0f);

                position = Vector2.Floor(midPosition + offset);
            }
            else
            {
                position = Vector2.Floor(offset);
            }
        }
    }
}
