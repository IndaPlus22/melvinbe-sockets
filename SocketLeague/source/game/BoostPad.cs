using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SocketLeague
{
    public class BoostPad : Sprite
    {
        public static Texture2D boostPadTexture;
        public static Texture2D drainedBoostPadTexture;

        public const float radius = 10.0f;

        // How long it takes for pad to regain its juice
        public const float refillDuration = 8.0f;
        public float refillTime = refillDuration;

        public bool hasJuice;

        public BoostPad(Vector2 position)
            : base(boostPadTexture)
        {
            this.position = position;

            sortingLayer = -10;
        }

        public override void Update(float deltaTime)
        {
            hasJuice = refillTime >= refillDuration;

            // Change texture to if has juice or not
            texture = hasJuice ? boostPadTexture : drainedBoostPadTexture;

            refillTime += deltaTime;
        }
    }
}
