using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Sharpmine
{
    internal class Game : GameWindow
    {
        private static int SCREEN_WIDTH;
        private static int SCREEN_HEIGHT;
        public Game(int width, int height, string title) : base(GameWindowSettings.Default, NativeWindowSettings.Default)
        {
            this.CenterWindow(new Vector2i(width, height));
            SCREEN_WIDTH = width;
            SCREEN_HEIGHT = height;
        }

    }
}
