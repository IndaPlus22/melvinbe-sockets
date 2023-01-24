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
    public class BoostPad : Sprite
    {
        public static Texture2D boostPadTexture;

        public const float radius = 0.08f * 256.0f / 2.0f;

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
