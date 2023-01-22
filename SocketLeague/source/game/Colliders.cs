using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SocketLeague
{
    public abstract class Collider
    {

    }

    public class SquareCollider : Collider
    {

    }

    public class CircleCollider : Collider
    {
        public bool inverted;

        public float radius;
    }
}
