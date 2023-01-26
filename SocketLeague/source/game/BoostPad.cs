using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SocketLeague
{
    public class BoostPad : Sprite
    {
        public static Texture2D boostPadTexture;

        public const float radius = 10.0f;

        // How long it takes for pad to regain its juice
        public const float refillDuration = 8.0f;
        public float refillTime = refillDuration;

        public bool hasJuice;

        public BoostPad(Vector2 position)
            : base(boostPadTexture)
        {
            this.position = position;

            scale = new Vector2(0.1f);
        }

        public override void Update(float deltaTime)
        {
            hasJuice = refillTime >= refillDuration;

            // Change texture if has juice or not
            if (hasJuice)
            {
                color = Color.Gold;
            }
            else
            {
                color = Color.Black;
            }

            refillTime += deltaTime;
        }
    }
}
