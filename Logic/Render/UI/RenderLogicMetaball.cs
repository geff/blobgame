using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BlobGame.Logic.Render.UI;
using Microsoft.Xna.Framework.Graphics;
using BlobGame.Render;
using FarseerGames.GettingStarted;
using BlobGame.Logic.Game;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Input;

namespace BlobGame.Logic.Render
{
    public class RenderLogicMetaBall : RenderLogic
    {
        public bool drawPhysicObjects = false;
        public bool doScreenShot = false;
        public Vector2 vecTranslation = new Vector2(0, 0);
        public float zoom = -5;
        public new bool updateViewScreen = false;

        public RectangleF viewScreen = new RectangleF();

        public new UIManager UIManager { get; set; }

        [DllImport("user32.dll")]
        static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        #region Properties

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        //FPSCounter fps;

        Texture2D gaussianTex;

        QuadRenderComponent quad;

        Effect blobEffect, blobColorEffect;

        int GAUSSIANSIZE = 64;
        float GAUSSIANDEVIATION = 0.125f;

        VertexPositionColor[] spriteArray;
        VertexDeclaration vertexPosColDecl;

        Matrix View, Projection;

        int screenWidth, screenHeight;

        RenderTarget2D colorTarget, normalTarget;

        Texture2D colorTex, normalTex;
        TextureCube cubeTex;

        float angleX, angleY;
        Vector2 prevPos, currentPos;
        GameEngine _gameEngine;
        #endregion

        #region Constructor

        public RenderLogicMetaBall(GameEngine gameEngine)
            : base(gameEngine)
        {
            _gameEngine = gameEngine;

            //graphics = new GraphicsDeviceManager(_gameEngine.Game);
            _gameEngine.Game.Content.RootDirectory = "Content";

            //graphics = _gameEngine.Game.gr
            //graphics.PreferredBackBufferWidth = screenWidth = 120;
            //graphics.PreferredBackBufferHeight = screenHeight = 120;

            //this.IsFixedTimeStep = true;
            //graphics.SynchronizeWithVerticalRetrace = true;
        }

        #endregion

        #region Initialize
        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        public override void Initialize()
        {
            this.UIManager = new UIManager(this._gameEngine);
            this.UIManager.Initialize(null);

            quad = new QuadRenderComponent(_gameEngine.Game);

            _gameEngine.Game.Components.Add(quad);
            //Components.Add(quad);

            //fps = new FPSCounter(this, "Blobs");
            //Components.Add(fps);

            angleX = angleY = 0.0f;
            prevPos = currentPos = Vector2.Zero;

            View = Matrix.Identity;
            Projection = Matrix.Identity;

            if (true)
            {
                int tilteBarHeight = 24;// System.Windows.Forms.SystemInformation.CaptionHeight + 4;
                //MoveWindow(this.Window.Handle, 1300, 700, graphics.PreferredBackBufferWidth + 4, graphics.PreferredBackBufferHeight + tilteBarHeight, true);
            }

            this.screenWidth = _gameEngine.Game.GraphicsDevice.Viewport.Width;
            this.screenHeight = _gameEngine.Game.GraphicsDevice.Viewport.Height;

            LoadContent();
            //base.Initialize();
        }

        #endregion

        #region Load
        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected void LoadContent()
        {
            UIManager = new UIManager(this._gameEngine);
            UIManager.Initialize(null);

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(_gameEngine.Game.GraphicsDevice);

            GenerateGaussianTexture();

            blobEffect = _gameEngine.Game.Content.Load<Effect>("Effects/Blobs");
            blobColorEffect = _gameEngine.Game.Content.Load<Effect>("Effects/BlobFinalPass");

            //blobEffect = _gameEngine.Game.Content.Load<Effect>("Effects/oldBlobs");
            //blobColorEffect = _gameEngine.Game.Content.Load<Effect>("Effects/oldBlobFinalPass");

            colorTarget = new RenderTarget2D(_gameEngine.Game.GraphicsDevice, this.screenWidth,
                this.screenHeight, 0, SurfaceFormat.HalfVector4);
            normalTarget = new RenderTarget2D(_gameEngine.Game.GraphicsDevice, this.screenWidth,
                this.screenHeight, 0, SurfaceFormat.HalfVector4);

            cubeTex = _gameEngine.Game.Content.Load<TextureCube>("Textures/LobbyCube");

            spriteArray = new VertexPositionColor[5];
            vertexPosColDecl = new VertexDeclaration(_gameEngine.Game.GraphicsDevice,
                VertexPositionColor.VertexElements);

            spriteArray[0].Position = new Vector3(0, 0, 0);
            spriteArray[0].Color = Color.Red;
            spriteArray[1].Position = new Vector3(3, 0, 0);
            spriteArray[1].Color = Color.Blue;
            spriteArray[2].Position = new Vector3(-3, 0, 0);
            spriteArray[2].Color = Color.Green;
            spriteArray[3].Position = new Vector3(0, 3, 0);
            spriteArray[3].Color = Color.Yellow;
            spriteArray[4].Position = new Vector3(0, -3, 0);
            spriteArray[4].Color = Color.Cyan;

            View = Matrix.CreateTranslation(new Vector3(0.0f, 0.0f, -5.0f));

            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver2,
                (float)screenWidth / (float)screenHeight, 1.0f, 5000f);
        }

        #endregion

        #region Unload
        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        #endregion

        #region Update
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            //if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            //    this.Exit();

            float step = (float)gameTime.ElapsedGameTime.TotalSeconds;

            ProcessInput(step);

            UpdateViewMatrix();

            UpdateBlob();

            prevPos = currentPos;

            //base.Update(gameTime);
        }

        private void ProcessInput(float step)
        {
            MouseState currentState = Mouse.GetState();
            KeyboardState keyState = Keyboard.GetState();

            currentPos = new Vector2((float)currentState.X, (float)currentState.Y);

            Vector2 dPos = new Vector2(currentPos.X - prevPos.X, currentPos.Y - prevPos.Y);

            angleX += dPos.Y * step;
            angleY += dPos.X * step;

            //if (keyState.IsKeyDown(Keys.Escape))
            //    this.Exit();

            if (keyState.IsKeyDown(Keys.Up))
                angleX += 1.0f * step;
            if (keyState.IsKeyDown(Keys.Down))
                angleX -= 1.0f * step;
            if (keyState.IsKeyDown(Keys.Left))
                angleY -= 1.0f * step;
            if (keyState.IsKeyDown(Keys.Right))
                angleY += 1.0f * step;
        }


        private void UpdateViewMatrix()
        {
            //View = Matrix.CreateRotationY(angleX);
            //View *= Matrix.CreateRotationX(angleY);
            View = Matrix.CreateRotationY(MathHelper.Pi);
            View *= Matrix.CreateRotationX(MathHelper.Pi * 1.75f);
            View *= Matrix.CreateTranslation(new Vector3(0.0f, 0.0f, -5.0f));
        }

        private void UpdateBlob()
        {
            for (int i = 0; i < _gameEngine.GameLogic.ListBlobs.Count; i++)
            {
                BlobEntity blobEntity = _gameEngine.GameLogic.ListBlobs[i];

                //--- temp
                //ListBlobs[i].Updateblob();
                //vertexBlobArray[i] = ListBlobs[i].VertexBlob;
                //---

                //ListBlobs[i].ParentOctree.ListBlobEntity.Remove

                Octree octree = _gameEngine.GameLogic.mainOctree.GetOctree(blobEntity.Position);

                if (octree != blobEntity.ParentOctree && octree != null && blobEntity.ParentOctree != null)
                {
                    blobEntity.ParentOctree.ListBlobEntity.Remove(blobEntity);

                    blobEntity.ParentOctree = octree;
                    octree.ListBlobEntity.Add(blobEntity);
                }
            }

            //viewScreen = new RectangleF(10, 10, 100, 100);
            //List<BlobEntity> listBlobEntityViewable = _gameEngine.GameLogic.mainOctree.GetBlobEntity(viewScreen);

            List<BlobEntity> listBlobEntityViewable = _gameEngine.GameLogic.ListBlobs;

            spriteArray = new VertexPositionColor[listBlobEntityViewable.Count];

            for (int i = 0; i < listBlobEntityViewable.Count; i++)
            {
                listBlobEntityViewable[i].Updateblob();
                spriteArray[i].Color = listBlobEntityViewable[i].Color;
                spriteArray[i].Position = new Vector3(listBlobEntityViewable[i].Position, 0f);
            }
        }

        #endregion

        #region Draw
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Draw(GameTime gameTime)
        {
            if (spriteArray.Length == 0)
                return;

            DrawMRT(gameTime);

            colorTex = colorTarget.GetTexture();
            normalTex = normalTarget.GetTexture();

            if (gameTime.TotalGameTime.TotalSeconds % 10 == 0)
            {
                colorTex.Save(@"d:\metaball_color.bmp", ImageFileFormat.Bmp);
                normalTex.Save(@"d:\metaball_normal.bmp", ImageFileFormat.Bmp);
            }

            blobColorEffect.Parameters["NormalBuffer"].SetValue(normalTex);
            blobColorEffect.Parameters["ColorBuffer"].SetValue(colorTex);
            blobColorEffect.Parameters["CubeTex"].SetValue(cubeTex);
            blobColorEffect.Begin();
            blobColorEffect.Techniques[0].Passes[0].Begin();
            quad.Render(Vector2.One * -1, Vector2.One);
            blobColorEffect.Techniques[0].Passes[0].End();
            blobColorEffect.End();

            //base.Draw(gameTime);
        }


        private void DrawMRT(GameTime gameTime)
        {
            SetRTs();

            _gameEngine.Game.GraphicsDevice.Clear(Color.Black);

            #region RenderStates
            _gameEngine.Game.GraphicsDevice.RenderState.PointSpriteEnable = true;
            _gameEngine.Game.GraphicsDevice.RenderState.AlphaBlendEnable = true;
            _gameEngine.Game.GraphicsDevice.RenderState.SourceBlend = Blend.One;
            _gameEngine.Game.GraphicsDevice.RenderState.DestinationBlend = Blend.One;
            _gameEngine.Game.GraphicsDevice.RenderState.DepthBufferWriteEnable = false;
            #endregion

            _gameEngine.Game.GraphicsDevice.VertexDeclaration = vertexPosColDecl;

            blobEffect.Parameters["WorldViewProjection"].SetValue(View * Projection);
            blobEffect.Parameters["Projection"].SetValue(Projection);
            blobEffect.Parameters["ParticleSize"].SetValue(4);
            blobEffect.Parameters["ViewportHeight"].SetValue(screenHeight);
            blobEffect.Parameters["GaussBlob"].SetValue(gaussianTex);
            blobEffect.Begin();
            blobEffect.Techniques[0].Passes[0].Begin();

            _gameEngine.Game.GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.PointList,
                    spriteArray, 0, spriteArray.Length);

            blobEffect.Techniques[0].Passes[0].End();
            blobEffect.End();

            #region UnsetRenderStates
            _gameEngine.Game.GraphicsDevice.RenderState.PointSpriteEnable = false;
            _gameEngine.Game.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
            _gameEngine.Game.GraphicsDevice.RenderState.AlphaBlendEnable = false;
            #endregion

            ResolveRTs();
        }

        private void SetRTs()
        {
            _gameEngine.Game.GraphicsDevice.SetRenderTarget(1, colorTarget);
            _gameEngine.Game.GraphicsDevice.SetRenderTarget(0, normalTarget);
        }

        private void ResolveRTs()
        {
            _gameEngine.Game.GraphicsDevice.SetRenderTarget(0, null);
            _gameEngine.Game.GraphicsDevice.SetRenderTarget(1, null);
        }

        #endregion

        #region Other Methods

        private void GenerateGaussianTexture()
        {
            gaussianTex = new Texture2D(this._gameEngine.Game.GraphicsDevice, 64, 64, 0, TextureUsage.None,
                SurfaceFormat.Single);

            int u, v;
            float dx, dy, I;

            float[] temp = new float[GAUSSIANSIZE * GAUSSIANSIZE];

            for (v = 0; v < GAUSSIANSIZE; ++v)
            {
                for (u = 0; u < GAUSSIANSIZE; ++u)
                {
                    dx = 2.0f * u / (float)GAUSSIANSIZE - 1.0f;
                    dy = 2.0f * v / (float)GAUSSIANSIZE - 1.0f;
                    I = GAUSSIANSIZE * (float)Math.Exp(-(dx * dx + dy * dy) / GAUSSIANDEVIATION);

                    int pos = u + v * GAUSSIANSIZE;
                    temp[pos] = I;
                }
            }

            gaussianTex.SetData<float>(temp);
        }

        #endregion
    }
}
