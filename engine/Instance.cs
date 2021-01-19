using System;
using System.Collections.Generic;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using static SFBF.Level;

namespace SFBF
{
    public class Instance
    {
        public bool IsOn = true;


        /// <summary>
        /// Interface that allows to open textures, sounds etc.
        /// Should be user-implemented since it is dependent on the program.
        /// </summary>
        public AssetManager assets;

        // Window-related code -----------------
        private RenderWindow window;
        /// <summary>
        /// SFML don't provide any method to check if window was fullscreen
        /// so very ugly way of checking is needed.
        /// </summary>
        private bool wasWindowFullScreen;

        // Timing-related code -----------------

        private float desired_dt = 0.004f;
        private float desired_fixed_dt = 0.02f;
        private float fluctuation_tolerancy = 0.1f;

        /// <summary>
        /// Used to determine maximum rate at which Update() and Draw() methods are called. Can be set to 0 to remove the cap.
        /// Setting negative value will ignore minus.
        /// </summary>
        public float UpdateRate { get => 1/desired_dt;
            set
            {
                if (value == 0)
                    desired_dt = 0;
                else
                    desired_dt = 1 / Math.Abs(value);
            } 
        }

        /// <summary>
        /// Used to determine rate at which FixedUpdate() method is called.
        /// If computer can't keep up with FixedUpdate() rate, deltaTime given to FixedUpdate() methods of actors and levels
        /// will be capped at FluctuationTolerancy times deltaTime.
        /// </summary>
        public float FixedUpdateRate { get => 1/desired_fixed_dt;
            set 
            {
                if (value == 0)
                {
                    Console.WriteLine("Warning: FixedUpdate() disabled.");
                    desired_fixed_dt = 0;
                }
                else
                    desired_fixed_dt = 1 / Math.Abs(value);
            } 
        }

        /// <summary>
        /// Used to determine maximum fluctuation in FixedUpdate() rate deltaTime.
        /// Since time between each call to FixedUpdate() is fixedUpdateDeltaTime = 1/FixedUpdateRate
        /// When computer can't keep up with FixedUpdate() call rate, the time that passed since last call increases which may cause
        /// bugs in physics calculations inside actors and levels. To prevent this you can cap the maximum deltaTime GIVEN to FixedUpdate() method
        /// to a multiplier of desired fixedUpdateDeltaTime. FluctuationTolerancy is exactly this. Setting it to 0 will make deltaTime given to method
        /// always the same regardless of call rate which can slow down the game but prevent any physics bugs. Setting it to 0.5 will make the maximum
        /// deltaTime given to 150% of fixedUpdateDeltaTime so in case of computer lag game still runs at constant speed but calculates with less accuracy.
        /// </summary>
        public float FluctuationTolerancy { get => fluctuation_tolerancy;
            set
            {
                fluctuation_tolerancy = Math.Max(0.0f, value);
            }
        }

        // Scene-related code
        /// <summary>
        /// List of levels wrapped in nested LevelController class to have access to all of the fields from the level class.
        /// </summary>
        private List<LevelController> levelCtrls = new List<LevelController>();
        private List<Level> levelsToAdd = new List<Level>();
        private List<Level> levelsToDestroy = new List<Level>();

        /// <summary>
        /// Adds a new level to the list.
        /// Ex. InstantiateLevel(new SomeLevel(arg1, arg2, arg3));
        /// All levels in the list are updated but only 1 drawn at a time.
        /// </summary>
        /// <param name="level">A new level to add.</param>
        /// <returns>Created level reference for easier assignment.</returns>
        public Level InstantiateLevel(Level level)
        {
            levelCtrls.Add(new LevelController(level));
            return level;
        }
        /// <summary>
        /// Removes the level from updated list.
        /// </summary>
        /// <param name="level">Target level to delete</param>
        public void DestroyLevel(Level level)
        {
            levelCtrls.RemoveAll(levelCtrl => levelCtrl.level == level);
        }
        /// <summary>
        /// Returns a list of levels that match given class.
        /// </summary>
        /// <param name="LevelType">Class to check</param>
        /// <returns>List of level references</returns>
        public List<LevelType> GetLevelsOfClass<LevelType>() where LevelType : Level
        {
            List<LevelType> list = new List<LevelType>();
            foreach (LevelController scene in levelCtrls)
            {
                if (scene.level.GetType() == typeof(LevelType))
                    list.Add((LevelType)scene.level);
            }
            return list;
        }
        /// <summary>
        /// Set which level is currently drawn.
        /// </summary>
        /// <param name="level">Target level</param>
        public void SetDrawnLevel(Level level)
        {
            for (int i = 0; i < levelCtrls.Count; i++)
            {
                if (levelCtrls[i].level == level)
                {
                    LevelController temp = levelCtrls[0];
                    levelCtrls[0] = levelCtrls[i];
                    levelCtrls[i] = temp;
                }
            }
        }

        private Instance()
        {
        }

        internal class InstanceController
        {
            public Instance Data = new Instance();

            /// <summary>
            /// Loops the private Update() function while trying to maintain fixed framerate and frametime-independency.
            /// Slows down however if frametime exceeds 10 times the desired value.
            /// This function is technically "blocking" as it loops until the app is closed.
            /// </summary>
            public void Loop()
            {
                float elapsed_dt;
                float elapsed_fixed_dt;
                Clock updateClock = new Clock();
                Clock fixedUpdateClock = new Clock();
                updateClock.Restart();
                fixedUpdateClock.Restart();
                while (Data.IsOn && Data.levelCtrls.Count > 0)
                {
                    elapsed_dt = updateClock.ElapsedTime.AsSeconds();
                    if (elapsed_dt > Data.desired_dt)
                    {
                        Update(elapsed_dt);
                        updateClock.Restart();

                        AssertWindowCorrectness(Data.levelCtrls[0]);
                        Data.window.DispatchEvents();
                        Data.window.Clear(Color.Black);

                        Draw();
                        Data.window.Display();
                    }

                    elapsed_fixed_dt = fixedUpdateClock.ElapsedTime.AsSeconds();
                    if (Data.desired_fixed_dt != 0 && elapsed_fixed_dt > Data.desired_fixed_dt)
                    {
                        FixedUpdate(Math.Min(Data.desired_fixed_dt * (1 + Data.fluctuation_tolerancy), elapsed_fixed_dt));
                        fixedUpdateClock.Restart();
                    }
                }

                Data.window?.Close();
                Data.window?.Dispose();
                
            }

            private void ManagePendingLevels()
            {
                foreach (Level l in Data.levelsToDestroy)
                    Data.levelCtrls.RemoveAll(levelCtrl => levelCtrl.level == l);
                foreach (Level l in Data.levelsToAdd)
                    Data.levelCtrls.Add(new LevelController(l));
                Data.levelsToDestroy.Clear();
                Data.levelsToAdd.Clear();
            }

            private void Update(float dt)
            {
                for (int i = 0; i < Data.levelCtrls.Count; i++)
                {
                    Data.levelCtrls[i].Update(dt, Data);
                    if (Data.levelCtrls[i].level.ToDestroy)
                        Data.levelCtrls.RemoveAt(i);
                }
                ManagePendingLevels();
            }

            private void FixedUpdate(float dt)
            {
                for (int i = 0; i < Data.levelCtrls.Count; i++)
                {
                    Data.levelCtrls[i].FixedUpdate(dt, Data);
                    if (Data.levelCtrls[i].level.ToDestroy)
                        Data.levelCtrls.RemoveAt(i);
                }
                ManagePendingLevels();
            }

            private void Draw()
            {

                if (Data.levelCtrls.Count > 0)
                    Data.levelCtrls[0].Draw(Data.window, Data.assets);
                    
            }
            /// <summary>
            /// This function makes sure that the window settings match current level settings.
            /// </summary>
            /// <param name="ctrl"></param>
            private void AssertWindowCorrectness(LevelController ctrl)
            {
                if (Data.window == null || (Data.wasWindowFullScreen != ctrl.level.Settings.Fullscreen) || (Data.wasWindowFullScreen && ctrl.level.Settings.Resolution != Data.window.Size))
                {

                    Data.window?.Close();
                    Data.window?.Dispose();
                        
                    Data.window = new RenderWindow(new VideoMode((uint)ctrl.level.Settings.Resolution.X, (uint)ctrl.level.Settings.Resolution.Y), ctrl.level.Settings.WindowName,
                    (ctrl.level.Settings.Resizable ? Styles.Resize : 0) |
                    (ctrl.level.Settings.Fullscreen ? Styles.Fullscreen : 0) |
                    Styles.Close |
                    Styles.Titlebar);

                    Data.wasWindowFullScreen = ctrl.level.Settings.Fullscreen;
                }
                Data.window.Size = new Vector2u((uint)ctrl.level.Settings.Resolution.X, (uint)ctrl.level.Settings.Resolution.Y);
                Data.window.SetTitle(ctrl.level.Settings.WindowName);

                // Setting the correct viewport and view.
                View view = new View();
                view.Size = new Vector2f(ctrl.level.Settings.ViewSize.X, ctrl.level.Settings.ViewSize.Y);
                view.Center = new Vector2f(ctrl.level.Settings.ViewCenterPos.X, ctrl.level.Settings.ViewCenterPos.Y);
                if (!ctrl.level.Settings.Stretched)
                {
                    float screenAspectRatio = (float)Data.window.Size.X / Data.window.Size.Y;
                    float fieldAspectRatio = (float)ctrl.level.Settings.ViewSize.X / ctrl.level.Settings.ViewSize.Y;

                    float fieldToScreenRatio = fieldAspectRatio / screenAspectRatio;

                    FloatRect viewport = new FloatRect();
                    viewport.Left = Math.Max(0, (1 - fieldToScreenRatio) / 2);
                    viewport.Width = Math.Min(1, fieldToScreenRatio);

                    viewport.Top = Math.Max(0, (1 - (1 / fieldToScreenRatio)) / 2);
                    viewport.Height = Math.Min(1 / fieldToScreenRatio, 1);
                    view.Viewport = viewport;
                }
                Data.window.SetView(view);
            }
        }
    }
}
