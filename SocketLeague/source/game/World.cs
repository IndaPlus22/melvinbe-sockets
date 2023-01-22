using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System.Collections.Generic;

namespace SocketLeague
{
    public class World
    {
        public List<Sprite> sprites = new List<Sprite>();

        public List<Body> bodies = new List<Body>();

        public List<Player> players = new List<Player>();

        public void Reset()
        {

        }

        public void Add(Sprite sprite)
        {
            sprites.Add(sprite);
            if (sprite is Body) bodies.Add(sprite as Body);
            if (sprite is Player) players.Add(sprite as Player);
        }

        public void Update(float deltaTime)
        {
            foreach (Body body in bodies)
            {
                body.Update(deltaTime);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (Body body in bodies)
            {
                body.Draw(spriteBatch);
            }
        }
    }
}
