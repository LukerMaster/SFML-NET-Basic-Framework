using System;
using System.Collections.Generic;
using System.Text;

namespace SFBE
{
    public interface AssetManager
    {
        /// <summary>
        /// Function that should unload every texture, sound or any file references
        /// that may take up memory.
        /// </summary>
        public void UnloadAllAssets();
    }
}
