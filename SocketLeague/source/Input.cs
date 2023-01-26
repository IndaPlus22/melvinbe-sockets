using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using System;

namespace SocketLeague
{
    public static class Input
    {
        // Save both current and last keyboard state to compare for changes
        private static KeyboardState keyboardState;
        private static KeyboardState lastKeyboardState;

        public static void Update()
        {
            lastKeyboardState = keyboardState;

            keyboardState = Keyboard.GetState();
        }

        public static bool Exit()
        {
            return KeyDown(Keys.Escape);
        }

        // Input on horizontal axis
        public static float Horizontal()
        {
            float horizontal = 0.0f;
            if (KeyHeld(Keys.Left )) horizontal -= 1.0f;
            if (KeyHeld(Keys.Right)) horizontal += 1.0f;
            if (KeyHeld(Keys.A    )) horizontal -= 1.0f;
            if (KeyHeld(Keys.D    )) horizontal += 1.0f;

            return Math.Clamp(horizontal, -1.0f, 1.0f);
        }

        // Input on vertical axis
        public static float Vertical()
        {
            float vertical = 0.0f;
            if (KeyHeld(Keys.Up  )) vertical += 1.0f;
            if (KeyHeld(Keys.Down)) vertical -= 1.0f;
            if (KeyHeld(Keys.W   )) vertical += 1.0f;
            if (KeyHeld(Keys.S   )) vertical -= 1.0f;

            return Math.Clamp(vertical, -1.0f, 1.0f);
        }

        //Key states
        public static bool KeyDown(Keys k)
        {
            return keyboardState.IsKeyDown(k) && lastKeyboardState.IsKeyUp(k);
        }
        public static bool KeyHeld(Keys k)
        {
            return keyboardState.IsKeyDown(k);
        }
        public static bool KeyUp(Keys k)
        {
            return lastKeyboardState.IsKeyDown(k) && keyboardState.IsKeyUp(k);
        }
    }
}
