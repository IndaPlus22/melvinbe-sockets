using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace SocketLeague
{
    public static class GameWindow
    {
        private static GraphicsDeviceManager graphics;
        private static Microsoft.Xna.Framework.GameWindow window;

        public const int REFERENCE_WIDTH = 480;
        public const int REFERENCE_HEIGHT = 270;

        public static int pixelSize = 2;

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

        public static void ToggleFullscreen()
        {
            ToggleFullscreen(!graphics.IsFullScreen);
        }

        public static void ToggleFullscreen(bool fullscreen)
        {
            graphics.IsFullScreen = fullscreen;

            graphics.PreferredBackBufferWidth = fullscreen ? GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width : windowWidth;
            graphics.PreferredBackBufferHeight = fullscreen ? GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height : windowHeight;

            graphics.ApplyChanges();

            SetPixelSize();
        }

        private static void OnResize(object sender, EventArgs e)
        {
            if (graphics.IsFullScreen) return;

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
