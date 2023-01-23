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

        public World() 
        {
            Add(new Player());
            Add(new Player());
            Add(new Player());
            Add(new Player());
        }

        public void Reset()
        {
            players[0].position = new Vector2(100.0f, 100.0f);
            players[1].position = new Vector2(100.0f, 300.0f);
            players[2].position = new Vector2(300.0f, 100.0f);
            players[3].position = new Vector2(300.0f, 300.0f);
        }

        public void Add(Sprite sprite)
        {
            sprites.Add(sprite);
            if (sprite is Body) bodies.Add(sprite as Body);
            if (sprite is Player) players.Add(sprite as Player);
        }

        public void Update(float deltaTime)
        {
            foreach (Sprite sprite in sprites)
            {
                if (sprite.isActive)
                {
                    sprite.Update(deltaTime);
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
