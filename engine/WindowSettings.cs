using SFML.System;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace SFBE
{
    public class WindowSettings
    {
        /// <summary>
        /// Defines the size of currently displayed part of the world.
        /// </summary>
        public Vector2u ViewSize { get; set; } = new Vector2u(640, 480);
        /// <summary>
        /// Defines where camera is currently located.
        /// </summary>
        public Vector2i ViewCenterPos { get; set; } = new Vector2i(640 / 2, 480 / 2);

        /// <summary>
        /// Determines whether the view is stretched to match window size or shows black bars to fit aspect ratio.
        /// </summary>
        public bool Stretched = false;
        /// <summary>
        /// Sets if the window should be fullscreen. Beware that using wrong resolution while fullscreen can cause weird bugs and will not work.
        /// </summary>
        public bool Fullscreen = false;
        /// <summary>
        /// Is window freely resizable
        /// </summary>
        public bool Resizable = false;
        /// <summary>
        /// Resolution of the window. Defaults to 640x480.
        /// Cannot go below 1x1.
        /// </summary>
        private Vector2u resolution = new Vector2u(640, 480);
        public Vector2u Resolution
        {
            get => resolution;
            set => resolution = new Vector2u(Math.Max(1, value.X), Math.Max(1, value.Y)); 
        }
    }
}
