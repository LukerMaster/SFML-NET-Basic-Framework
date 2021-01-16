﻿using SFML.Graphics;
using SFML.System;

namespace SFBE
{
    public abstract class Actor
    {
        /// <summary>
        /// Function called every frame. Can be used for drawing calculations but not displaying.
        /// Ex. Calculate sprite positions, scales of objects etc. 
        /// Do NOT draw or play sounds here as they would be played even if state is not active (drawn).
        /// </summary>
        /// <param name="dt">deltaTime from previous frame</param>
        /// <param name="level">Level data used to communicate with other actors</param>
        protected abstract void Update(float dt, Level level);
        /// <summary>
        /// Function called every fixed amount of time. Should be used for physics/collision calculations.
        /// </summary>
        /// <param name="dt">deltaTime from previous fixedUpdate() call. Should not vary much unless FluctuationTolerance is high.</param>
        /// <param name="level">Level data used to communicate with other actors</param>
        protected abstract void FixedUpdate(float dt, Level level);
        /// <summary>
        /// Function called every frame. Should be used to draw stuff on the screen and play sounds/music.
        /// </summary>
        /// <param name="w">Window to draw in.</param>
        /// <param name="assets">AssetManager interface used to get textures and sound buffers.</param>
        protected abstract void Draw(RenderWindow w, AssetManager assets);
        /// <summary>
        /// This variable is checked every frame (both after Update and FixedUpdate) and if is True, this
        /// instance is deleted from the world.
        /// </summary>
        public virtual bool ToDestroy { get; }
        /// <summary>
        /// Defines on which layer current actor is drawn. Higher number means drawn later/on top. Lower number means drawn earlier/below.
        /// </summary>
        public int DrawOrder { get; set; }
        /// <summary>
        /// Provides access to fields and methods of engine instance class.
        /// Mouse position, screen size etc.
        /// </summary>
        internal class ActorController
        {
            public Actor actor;

            public ActorController(Actor ac)
            {
                actor = ac;
            }
            public void Update(float dt, Level level) => actor.Update(dt, level);
            public void FixedUpdate(float dt, Level level) => actor.FixedUpdate(dt, level);
            public void Draw(RenderWindow w, AssetManager assetMgr) => actor.Draw(w, assetMgr);
        }
    }
}
