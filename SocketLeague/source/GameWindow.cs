using Microsoft.Xna.Framework;
using System;

namespace SocketLeague
{
    public static class GameWindow
    {
        private static GraphicsDeviceManager graphics;
        private static Microsoft.Xna.Framework.GameWindow window;

        public const int REFERENCE_WIDTH = 480;
        public const int REFERENCE_HEIGHT = 270;

        public static int pixelSize = 4;

        public static int windowWidth = REFERENCE_WIDTH * pixelSize;
        public static int windowHeight = REFERENCE_HEIGHT * pixelSize;

        public static void Initialize(GraphicsDeviceManager _graphics, Microsoft.Xna.Framework.GameWindow _window)
        {
            graphics = _graphics;
            window = _window;

            window.ClientSizeChanged += OnResize;

            graphics.PreferredBackBufferWidth = windowWidth;
            graphics.PreferredBackBufferHeight = windowHeight;
            graphics.ApplyChanges();
        }

        private static void OnResize(object sender, EventArgs e)
        {
            window.ClientSizeChanged -= OnResize;

            windowWidth = Math.Max(window.ClientBounds.Width, REFERENCE_WIDTH);
            windowHeight = Math.Max(window.ClientBounds.Height, REFERENCE_HEIGHT);

            graphics.PreferredBackBufferWidth = windowWidth;
            graphics.PreferredBackBufferHeight = windowHeight;
            graphics.ApplyChanges();

            window.ClientSizeChanged += OnResize;

            SetPixelSize();
        }

        private static void SetPixelSize()
        {
            int resX = graphics.PreferredBackBufferWidth / REFERENCE_WIDTH;
            int resY = graphics.PreferredBackBufferHeight / REFERENCE_HEIGHT;
            pixelSize = Math.Min(resX, resY);
        }
    }
}
