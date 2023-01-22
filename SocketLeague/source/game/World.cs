using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System.Collections.Generic;

namespace SocketLeague
{
    public static class World
    {
        public static List<Player> players = new List<Player>();

        public static List<Body> bodies = new List<Body>();

        public static void Update(float deltaTime)
        {
            foreach (Body body in bodies)
            {
                body.Update(deltaTime);
            }
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            foreach (Body body in bodies)
            {
                body.Draw(spriteBatch);
            }
        }
    }
}
