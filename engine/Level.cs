using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Numerics;
using static SFBE.Actor;

namespace SFBE
{
    abstract public class Level
    {
        private List<ActorController> actorCtrls = new List<ActorController>();
        private List<Actor> actorsToAdd = new List<Actor>();
        private List<Actor> actorsToDestroy = new List<Actor>();

        /// <summary>
        /// All the settings required for proper window management
        /// </summary>
        public WindowSettings Settings = new WindowSettings();

        private Vector2f mousePos;
        public Vector2f MousePos { get => mousePos; private set => mousePos = value; }
        /// <summary>
        /// Text displayed on the window titlebar.
        /// </summary>
        public string WindowName = "SFML Basic Engine";

        /// <summary>
        /// Update script for a level. Can be used to properly give input, check win conditions etc.
        /// </summary>
        /// <param name="dt">deltaTime from last frame.</param>
        /// <param name="inst">Instance data that can be used to change variables in the engine.</param>
        protected abstract void UpdateScript(float dt, Instance inst);
        /// <summary>
        /// Update script with fixed deltaTime for a level. 
        /// Can be used for strict physics calculations etc.
        /// </summary>
        /// <param name="dt">deltaTime from last FixedUpdate() call.</param>
        /// <param name="inst">Instance data that can be used to change variables in the engine.</param>
        protected abstract void FixedUpdateScript(float dt, Instance inst);
        /// <summary>
        /// This variable is checked every frame (both after Update and FixedUpdate) and if is True, this
        /// instance is deleted from the world.
        /// </summary>
        public bool ToDestroy { get; protected set; }
        /// <summary>
        /// Instantiates a new actor making it updated and drawn each frame according to settings.
        /// </summary>
        /// <param name="act">Target actor</param>
        /// <returns>Added actor reference for easier assignment.</returns>
        public Actor InstantiateActor(Actor act)
        {
            actorsToAdd.Add(act);
            return act;
        }
        /// <summary>
        /// Destroys actor given by reference.
        /// </summary>
        /// <param name="act">Target actor</param>
        public void DestroyActor(Actor act)
        {
            actorsToDestroy.Add(act);
        }
        /// <summary>
        /// Returns a list of actors that match given class.
        /// </summary>
        /// <param name="ActorType">Class to search</param>
        /// <returns>List of level references</returns>
        public List<ActorType> GetActorsOfClass<ActorType>()
        {
            List<ActorType> list = new List<ActorType>();
            foreach (ActorController actorCtrl in actorCtrls)
            {
                if (actorCtrl.actor is ActorType)
                    list.Add((ActorType)(object)actorCtrl.actor);
            }
            return list;
        }
        internal class LevelController
        {
            public Level level;
            public LevelController(Level lvl)
            {
                level = lvl;
            }

            private void ManagePendingActorActions()
            {
                foreach (Actor a in level.actorsToDestroy)
                    level.actorCtrls.RemoveAll(actorCtrl => actorCtrl.actor == a);
                foreach (Actor a in level.actorsToAdd)
                    level.actorCtrls.Add(new ActorController(a));
                level.actorsToDestroy.Clear();
                level.actorsToAdd.Clear();
            }

            public void Update(float dt, Instance inst)
            {
                level.UpdateScript(dt, inst);
                for (int i = 0; i < level.actorCtrls.Count; i++)
                {
                    level.actorCtrls[i].Update(dt, level);
                    if (level.actorCtrls[i].actor.ToDestroy)
                        level.actorCtrls.RemoveAt(i);
                }
                ManagePendingActorActions();
            }
            public void FixedUpdate(float dt, Instance inst)
            {
                level.FixedUpdateScript(dt, inst);
                for (int i = 0; i < level.actorCtrls.Count; i++)
                {
                    level.actorCtrls[i].FixedUpdate(dt, level);
                    if (level.actorCtrls[i].actor.ToDestroy)
                        level.actorCtrls.RemoveAt(i);
                }
                ManagePendingActorActions();
            }
            public void Draw(RenderWindow window, AssetManager assets)
            {
                // Setting the correct mouse position.
                level.mousePos = window.MapPixelToCoords(SFML.Window.Mouse.GetPosition(window));

                List<ActorController> drawables = new List<ActorController>(level.actorCtrls);
                drawables.Sort(delegate (ActorController a, ActorController b)
                    {
                        if (a.actor.DrawOrder > b.actor.DrawOrder) return 1;
                        else if (a.actor.DrawOrder < b.actor.DrawOrder) return -1;
                        else return 0;
                    });
                foreach (ActorController a in drawables)
                        a.Draw(window, assets);
            }
        }
    }
}
