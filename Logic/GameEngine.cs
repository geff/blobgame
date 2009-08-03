using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using BlobGame.Render;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysicsDemos.DrawingSystem;
using FarseerGames.FarseerPhysicsDemos.Demos.DemoShare;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics.Springs;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.FarseerPhysicsDemos;
using FarseerGames.GettingStarted;
using System.Threading;
using BlobGame.Logic.Controller;
//using BlobGame.Logic.Sound;
//using BlobGame.Logic.Network;
using BlobGame.Logic.Game;
using BlobGame.Logic.Render;


namespace BlobGame.Logic
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class GameEngine : Microsoft.Xna.Framework.GameComponent
    {
        #region Properties


        #region Modules
        public ControllerLogic ControllerLogic { get; set; }
        public RenderLogic RenderLogic { get; set; }
        public GameLogic GameLogic { get; set; }
        #endregion

        #endregion

        #region Constructor
        public GameEngine(GameBlob game)
            : base(game)
        {
        }
        #endregion

        #region Initialize
        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            //--- Create modules
            this.GameLogic = new GameLogic(this);
            this.ControllerLogic = new ControllerLogic(this);
            this.RenderLogic = new RenderLogic(this);
            //---

            //--- Initialize modules
            this.GameLogic.Initialize();
            this.RenderLogic.Initialize();
            //---

            base.Initialize();
        }
        #endregion

        #region Update
        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            //--- pre update initilization
            RenderLogic.updateViewScreen = false;
            //---

            //--- Update modules
            ControllerLogic.UpdateBegin(gameTime);
            
            GameLogic.Update(gameTime);
            RenderLogic.Update(gameTime);

            ControllerLogic.UpdateEnd(gameTime);
            //---

            //Game.Window.Title = String.Format("Mouse.X : {0:0.00} - Mouse.Y : {1:0.00} - Zoom : {2:0.00} /// VS.Width : {3:0.00} - VS.Height : {4:0.00} - VS.Left : {5:0.00} - VS.Top : {6:0.00}", mousePosition.X, mousePosition.Y, zoom, viewScreen.Width, viewScreen.Height, viewScreen.Left, viewScreen.Top);

            base.Update(gameTime);
        }
        #endregion

        #region Draw
        public void Draw(GameTime gameTime)
        {
            this.RenderLogic.Draw(gameTime);
        }
        #endregion
    }
}