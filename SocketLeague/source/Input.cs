using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using System;

namespace SocketLeague
{
    public static class Input
    {
        private static KeyboardState keyboardState;
        private static KeyboardState lastKeyboardState;

        private static GamePadState gamePadState;
        private static GamePadState lastGamePadState;

        public static bool usingController;

        public static void Update()
        {
            lastKeyboardState = keyboardState;
            lastGamePadState = gamePadState;

            keyboardState = Keyboard.GetState();
            gamePadState = GamePad.GetState(PlayerIndex.One);

            if (keyboardState.GetPressedKeyCount() != lastKeyboardState.GetPressedKeyCount())
                usingController = false;
            if (gamePadState.PacketNumber != lastGamePadState.PacketNumber)
                usingController = true;
        }

        public static bool Exit()
        {
            return KeyDown(Keys.Escape) || ButtonDown(Buttons.A) || ButtonDown(Buttons.Y);
        }

        public static float Horizontal()
        {
            float horizontal = LeftStick().X;
            if (KeyHeld(Keys.Left))
                horizontal -= 1.0f;
            if (KeyHeld(Keys.Right))
                horizontal += 1.0f;
            return Math.Clamp(horizontal, -1.0f, 1.0f);
        }
        public static float vertical()
        {
            float vertical = LeftStick().Y;
            if (KeyHeld(Keys.Up))
                vertical -= 1.0f;
            if (KeyHeld(Keys.Down))
                vertical += 1.0f;
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

        //Button states
        public static bool ButtonDown(Buttons b)
        {
            return gamePadState.IsButtonDown(b) && lastGamePadState.IsButtonUp(b);
        }
        public static bool ButtonHeld(Buttons b)
        {
            return gamePadState.IsButtonDown(b);
        }
        public static bool ButtonUp(Buttons b)
        {
            return gamePadState.IsButtonUp(b) && lastGamePadState.IsButtonDown(b);
        }

        //Stick states
        public static Vector2 LeftStick()
        {
            return gamePadState.ThumbSticks.Left;
        }
        public static Vector2 RightStick()
        {
            return gamePadState.ThumbSticks.Right;
        }
    }
}
