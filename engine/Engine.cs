using System;
using System.Collections.Generic;
using System.Text;

namespace SFBE
{
    /// <summary>
    /// User-interface class to instantiate entire game engine.
    /// Use this if you want to create the engine.
    /// </summary>
    public class Engine
    {
        Instance.InstanceController instance = new Instance.InstanceController();

        /// <summary>
        /// Returns the (modifiable) settings alongside with data that is also available to levels if needed.
        /// </summary>
        public Instance Data { get => instance.Data; }

        /// <summary>
        /// Starts the engine. This function is blocking ie. it runs until you close the window.
        /// </summary>
        public void Run()
        {
            instance.Loop();
        }
        public Engine()
        {

        }
    }
}
