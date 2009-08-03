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
using BlobGame.Logic;

namespace BlobGame
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class GameBlob : Microsoft.Xna.Framework.Game
    {
        #region Properties

        public GraphicsDeviceManager graphics;
        public SpriteBatch spriteBatch;

        GameEngine gameEngine;
        public QuadRenderComponent quad;

        #endregion

        public GameBlob()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 600;

            graphics.PreferredDepthStencilFormat = DepthFormat.Depth16;

            //graphics.PreferMultiSampling = true;
            graphics.PreferredBackBufferFormat = SurfaceFormat.Color;

            //graphics.GraphicsDevice.DepthStencilBuffer.MultiSampleType = MultiSampleType.TwoSamples;
            //graphics.GraphicsDevice.PresentationParameters.MultiSampleQuality = 
            //graphics.SynchronizeWithVerticalRetrace = true;
            
            //graphics.IsFullScreen = true;

            gameEngine = new GameEngine(this);

            this.Components.Add(gameEngine);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            quad = new QuadRenderComponent(this);
            Components.Add(quad);

            //fps = new FPSCounter(this, "Blobs");
            //Components.Add(fps);

            //angleX = angleY = 0.0f;
            //prevPos = currentPos = Vector2.Zero;

            //View = Matrix.Identity;
            //Projection = Matrix.Identity;

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //gameEngine.LoadContent();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            //graphics.GraphicsDevice.Clear(Color.White);

            graphics.GraphicsDevice.PresentationParameters.MultiSampleType = MultiSampleType.SixteenSamples;
            graphics.GraphicsDevice.PresentationParameters.MultiSampleQuality = 4;
            graphics.GraphicsDevice.PresentationParameters.EnableAutoDepthStencil = true;
            graphics.GraphicsDevice.PresentationParameters.IsFullScreen = graphics.IsFullScreen;

            gameEngine.Draw(gameTime);

            base.Draw(gameTime);
        }
    }
}
