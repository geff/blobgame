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
    public class RenderLogic
    {
        #region Graphics
        Texture2D gaussianTex;
        Texture2D gaussianUnitTex;

        public Texture2D texBlank;

        Effect blobEffect, blob2Effect, blobColorEffect;
        int GAUSSIANSIZE = 64;
        float GAUSSIANDEVIATION = 0.125f;

        VertexBlob[] vertexBlobArray;
        VertexPositionColor[] vertexBlobArrayTest;

        public VertexDeclaration vertexPosColDecl;

        public Matrix View, Projection, ViewProjection;

        public Matrix ViewUI, ProjectionUI, ViewProjectionUI;

        RenderTarget2D colorTarget, normalTarget, playerTarget, unitTarget, infoTarget, UITarget;
        Texture2D colorTex, normalTex, playerTex, unitTex, infoTex, UITex;
        TextureCube cubeTex;

        public bool drawPhysicObjects = true;
        public bool doScreenShot = false;

        public GraphicsDeviceManager Graphics
        {
            get
            {
                return ((GameBlob)this.gameEngine.Game).graphics;
            }
        }

        public SpriteBatch SpriteBatch
        {
            get
            {
                return ((GameBlob)this.gameEngine.Game).spriteBatch;
            }
        }
        #endregion

        #region Physic
        PhysicsSimulatorView PhysicsSimulatorView;
        #endregion

        public Vector2 vecTranslation = new Vector2(0, 0);
        public float zoom = -24;
        public bool updateViewScreen = false;

        //private Vector2 vecTempTranslation;

        public RectangleF viewScreen = new RectangleF();

        public UIManager UIManager { get; set; }

        private GameEngine gameEngine { get; set; }

        public RenderLogic(GameEngine gameEngine)
        {
            this.gameEngine = gameEngine;
        }

        #region Init
        public virtual void Initialize()
        {
            GenerateGaussianTexture(ref gaussianTex, GAUSSIANDEVIATION);
            GenerateGaussianTexture(ref gaussianUnitTex, GAUSSIANDEVIATION / 7f);

            texBlank = new Texture2D(gameEngine.Game.GraphicsDevice, 1, 1);
            texBlank.SetData<Color>(new Color[] { Color.White });

            vecTranslation = new Vector2(gameEngine.GameLogic.mapWidth * 0.5f, gameEngine.GameLogic.mapHeight * 0.5f);

            //--- 
            LoadContent();
            //---

            //---
            InitVertexBlobArray();
            //---

            //---
            InitPhysicView();
            //---

            //---
            this.UIManager = new UIManager(this.gameEngine);
            this.UIManager.Initialize(null);
            //---
        }

        private void InitPhysicView()
        {
            //--- Physic simulator visualization
            PhysicsSimulatorView = new PhysicsSimulatorView(GameLogic.PhysicsSimulator);
            PhysicsSimulatorView.EnableGridView = false;
            PhysicsSimulatorView.EnableEdgeView = false;
            PhysicsSimulatorView.EnableAABBView = false;
            PhysicsSimulatorView.EnableCoordinateAxisView = false;
            PhysicsSimulatorView.EnableVerticeView = false;
            PhysicsSimulatorView.EnableSpingView = false;
            PhysicsSimulatorView.EnableRevoluteJointView = false;

            PhysicsSimulatorView.EnablePinJointView = true;
            PhysicsSimulatorView.EnablePerformancePanelView = false;
            PhysicsSimulatorView.EnableSliderJointView = true;

            PhysicsSimulatorView.ContactColor = Color.White;
            PhysicsSimulatorView.EdgeColor = Color.Yellow;
            PhysicsSimulatorView.SpringLineColor = Color.Red;
            PhysicsSimulatorView.SliderJointLineColor = Color.Red;

            PhysicsSimulatorView.EnableRevoluteJointView = false;
            PhysicsSimulatorView.RevoluteJointLineColor = Color.Green;
            PhysicsSimulatorView.EdgeColor = Color.Orange;

            PhysicsSimulatorView.LoadContent(gameEngine.Game.GraphicsDevice, gameEngine.Game.Content);
            //---
        }
        bool simpleVertex = true;

        private void InitVertexBlobArray()
        {
            if (simpleVertex)
            {
                vertexBlobArrayTest = new VertexPositionColor[gameEngine.GameLogic.ListBlobs.Count];
                for (int i = 0; i < gameEngine.GameLogic.ListBlobs.Count; i++)
                {
                    vertexBlobArrayTest[i].Color = gameEngine.GameLogic.ListBlobs[i].VertexBlob.Color;
                    vertexBlobArrayTest[i].Position = gameEngine.GameLogic.ListBlobs[i].VertexBlob.Position;
                    //vertexBlobArrayTest[i] = gameEngine.GameLogic.ListBlobs[i].VertexBlob;
                    //vertexBlobArrayTest[i] = gameEngine.GameLogic.ListBlobs[i].VertexBlob;

                    Octree octree = gameEngine.GameLogic.mainOctree.GetOctree(gameEngine.GameLogic.ListBlobs[i].Position);

                    gameEngine.GameLogic.ListBlobs[i].ParentOctree = octree;
                    octree.ListBlobEntity.Add(gameEngine.GameLogic.ListBlobs[i]);
                }
            }
            else
            {
            vertexBlobArray = new VertexBlob[gameEngine.GameLogic.ListBlobs.Count];
            for (int i = 0; i < gameEngine.GameLogic.ListBlobs.Count; i++)
            {
                vertexBlobArray[i] = gameEngine.GameLogic.ListBlobs[i].VertexBlob;

                Octree octree = gameEngine.GameLogic.mainOctree.GetOctree(gameEngine.GameLogic.ListBlobs[i].Position);

                gameEngine.GameLogic.ListBlobs[i].ParentOctree = octree;
                octree.ListBlobEntity.Add(gameEngine.GameLogic.ListBlobs[i]);
            }
        }
        }

        public void LoadContent()
        {
            //--- Effects
            blobEffect = gameEngine.Game.Content.Load<Effect>("Effects/Blobs");
            //blob2Effect = gameEngine.Game.Content.Load<Effect>("Effects/Blobs2");

            blobColorEffect = gameEngine.Game.Content.Load<Effect>("Effects/BlobFinalPass");
            //---

            //--- Render target
            colorTarget = CreateRenderTarget();
            normalTarget = CreateRenderTarget();
            playerTarget = CreateRenderTarget();
            unitTarget = CreateRenderTarget();
            infoTarget = CreateRenderTarget();

            UITarget = CreateRenderTarget();
            //---

            //spriteArray = new VertexBlob[20];
            cubeTex = gameEngine.Game.Content.Load<TextureCube>("Textures/LobbyCube");

            vertexPosColDecl = new VertexDeclaration(gameEngine.Game.GraphicsDevice,
                VertexBlob.VertexElements);

            //vertexPosColDecl = new VertexDeclaration(gameEngine.Game.GraphicsDevice,
            //    VertexPositionColor.VertexElements);

            //--- Matrix
            //Projection = Matrix.CreatePerspectiveFieldOfView((float)Math.PI / 4f,
            //    (float)this.gameEngine.Game.GraphicsDevice.Viewport.Width / (float)this.gameEngine.Game.GraphicsDevice.Viewport.Height, 0.01f, 5000.0f);

            Projection = Matrix.CreateOrthographic((float)this.gameEngine.Game.GraphicsDevice.Viewport.Width , (float)this.gameEngine.Game.GraphicsDevice.Viewport.Height, 0.01f, 5000.0f);

            View = Matrix.CreateTranslation(new Vector3(-vecTranslation.X, vecTranslation.Y, zoom));
            ViewProjection = View * Projection;
            //---

            ProjectionUI = Projection;

            ViewUI = Matrix.CreateTranslation(new Vector3(-vecTranslation.X, vecTranslation.Y, zoom*10));
            ViewProjectionUI = ViewUI * ProjectionUI;
        }

        private RenderTarget2D CreateRenderTarget()
        {
            return new RenderTarget2D(gameEngine.Game.GraphicsDevice, this.gameEngine.Game.GraphicsDevice.Viewport.Width,
                this.gameEngine.Game.GraphicsDevice.Viewport.Height, 0, SurfaceFormat.HalfVector4, gameEngine.Game.GraphicsDevice.PresentationParameters.MultiSampleType, gameEngine.Game.GraphicsDevice.PresentationParameters.MultiSampleQuality);
        }
        #endregion

        #region Update
        public virtual void Update(GameTime gameTime)
        {
            //---
            UIManager.Update(gameTime);
            //---

            //--- Set up View
            if (updateViewScreen)
            {
                //Vector2 vecScreen = vecTempTranslation;
                //vecScreen.X -= viewScreen.Width / 2f;
                //vecScreen.Y -= viewScreen.Height / 2f;

                //if (recMap.Contains(vecScreen))
                //vecTranslation = vecTempTranslation;

                View = Matrix.CreateTranslation(new Vector3(-vecTranslation.X, vecTranslation.Y, zoom));
                ViewProjection = View * Projection;

                if (drawPhysicObjects)
                {
                    PhysicsSimulatorView.factor = 1f / zoom * -750f;

                    PhysicsSimulatorView.centerScreen.X = vecTranslation.X * (1f / zoom * -750f) + this.gameEngine.Game.GraphicsDevice.Viewport.Width / 2f;
                    PhysicsSimulatorView.centerScreen.Y = vecTranslation.Y * (1f / zoom * 750f) + this.gameEngine.Game.GraphicsDevice.Viewport.Height / 2f;
                }
            }
            //---

            //--- Update view screen
            if (updateViewScreen || viewScreen.IsEmpty)
            {
                viewScreen = new RectangleF();

                float ratio = -zoom / 750f;

                viewScreen.Width = ratio * (float)gameEngine.Game.GraphicsDevice.Viewport.Width;
                viewScreen.Height = ratio * (float)gameEngine.Game.GraphicsDevice.Viewport.Height;

                viewScreen.Left = this.vecTranslation.X - viewScreen.Width / 2f;
                viewScreen.Top = this.vecTranslation.Y - viewScreen.Height / 2f;
            }
            //---


            float step = (float)gameTime.ElapsedGameTime.TotalSeconds;

            ProcessInput(step);

            prevPos = currentPos;


            UpdateViewMatrix();

            UpdateBlobs();
        }

        float angleX, angleY;
        Vector2 prevPos, currentPos;

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
            View = Matrix.CreateRotationY(0);//MathHelper.Pi);
            View *= Matrix.CreateRotationX(0);//MathHelper.Pi * 1.75f);
            //View *= Matrix.CreateTranslation(new Vector3(0.0f, 0.0f, -5.0f));
            View *= Matrix.CreateTranslation(new Vector3(-vecTranslation.X, vecTranslation.Y, zoom));

        }

        public void UpdateBlobs()
        {
            for (int i = 0; i < gameEngine.GameLogic.ListBlobs.Count; i++)
            {
                BlobEntity blobEntity = gameEngine.GameLogic.ListBlobs[i];

                //--- temp
                //ListBlobs[i].Updateblob();
                //vertexBlobArray[i] = ListBlobs[i].VertexBlob;
                //---

                //ListBlobs[i].ParentOctree.ListBlobEntity.Remove

                Octree octree = gameEngine.GameLogic.mainOctree.GetOctree(blobEntity.Position);

                if (octree != blobEntity.ParentOctree && octree != null)
                {
                    blobEntity.ParentOctree.ListBlobEntity.Remove(blobEntity);

                    blobEntity.ParentOctree = octree;
                    octree.ListBlobEntity.Add(blobEntity);
                }
            }

            //--- Vertex
            List<BlobEntity> listBlobEntityViewable = gameEngine.GameLogic.mainOctree.GetBlobEntity(viewScreen);
            listBlobEntityViewable = gameEngine.GameLogic.ListBlobs;

            if (simpleVertex)
            {
                vertexBlobArrayTest = new VertexPositionColor[listBlobEntityViewable.Count];

                for (int i = 0; i < listBlobEntityViewable.Count; i++)
                {
                    listBlobEntityViewable[i].Updateblob();
                    vertexBlobArrayTest[i].Color = listBlobEntityViewable[i].Color;
                    vertexBlobArrayTest[i].Position = new Vector3(listBlobEntityViewable[i].Position, 0f);
                }
            }
            else
            {
            vertexBlobArray = new VertexBlob[listBlobEntityViewable.Count];

            for (int i = 0; i < listBlobEntityViewable.Count; i++)
            {
                listBlobEntityViewable[i].Updateblob();
                vertexBlobArray[i] = listBlobEntityViewable[i].VertexBlob;
            }
            }
            //---
        }
        #endregion

        #region Draw

        public virtual void Draw(GameTime gameTime)
        {
            if ((simpleVertex && vertexBlobArrayTest.Length > 0) || (vertexBlobArray != null && vertexBlobArray.Length > 0))
            {
                //---
                DrawMRT(gameTime);
                //---
            }
            else
            {
                /*
                int size = colorTex.Width*colorTex.Height;
                Color[] clrEmpty = new Color[size];

                clrEmpty.SetValue(Color.White, 0);

                colorTex.SetData<Color>(clrEmpty);
                */
                
            }

            //gameEngine.Game.GraphicsDevice.RenderState.ColorWriteChannels = ColorWriteChannels.All;
            
            //---
            colorTex = colorTarget.GetTexture();
            normalTex = normalTarget.GetTexture();
            playerTex = playerTarget.GetTexture();
            unitTex = unitTarget.GetTexture();
            //infoTex = infoTarget.GetTexture();
            //---

            if (doScreenShot)
            {
                normalTex.Save(@"d:\normalTex.Bmp", ImageFileFormat.Bmp);
                colorTex.Save(@"d:\colorTex.Bmp", ImageFileFormat.Bmp);
                //playerTex.Save(@"d:\playerTex.Bmp", ImageFileFormat.Bmp);
                //unitTex.Save(@"d:\unitTex.Bmp", ImageFileFormat.Bmp);
                //infoTex.Save(@"d:\infoTex.Bmp", ImageFileFormat.Bmp);

                doScreenShot = false;
            }

            //---
            blobColorEffect.Parameters["NormalBuffer"].SetValue(normalTex);
            blobColorEffect.Parameters["ColorBuffer"].SetValue(colorTex);
            blobColorEffect.Parameters["CubeTex"].SetValue(cubeTex);

            if (!simpleVertex)
            {
            blobColorEffect.Parameters["PlayerBuffer"].SetValue(playerTex);
            blobColorEffect.Parameters["UnitBuffer"].SetValue(unitTex);
            blobColorEffect.Parameters["InfoBuffer"].SetValue(infoTex);
            blobColorEffect.Parameters["Zoom"].SetValue(-zoom);
            }

            blobColorEffect.Begin();
            blobColorEffect.Techniques[0].Passes[0].Begin();

            ((GameBlob)this.gameEngine.Game).quad.Render(Vector2.One * -1, Vector2.One);

            blobColorEffect.Techniques[0].Passes[0].End();
            blobColorEffect.End();
            //---

            //---
            SpriteBatch.Begin();

            if (drawPhysicObjects)
            {
                DrawPhysicObjects();
            }

            UIManager.Draw(gameTime, SpriteBatch);

            SpriteBatch.End();
            //if (drawPhysicObjects)
            //{
            //    SpriteBatch.Begin();
            //    DrawPhysicObjects();
            //    SpriteBatch.End();
            //}
            //---
        }

        private void DrawPhysicObjects()
        {
            float ratio = 750f / -zoom;
            PhysicsSimulatorView.factor = ratio;
            PhysicsSimulatorView.centerScreen = new Vector2(-vecTranslation.X * ratio + (float)this.gameEngine.Game.GraphicsDevice.Viewport.Width/2f, -vecTranslation.Y * ratio + (float)this.gameEngine.Game.GraphicsDevice.Viewport.Height/2f);
            PhysicsSimulatorView.Draw(SpriteBatch);
        }

        private void DrawMRT(GameTime gameTime)
        {
            SetRTs();

            gameEngine.Game.GraphicsDevice.Clear(Color.White);
            
            #region RenderStates
            gameEngine.Game.GraphicsDevice.RenderState.PointSpriteEnable = true;
            gameEngine.Game.GraphicsDevice.RenderState.AlphaBlendEnable = true;
            gameEngine.Game.GraphicsDevice.RenderState.SourceBlend = Blend.One;
            gameEngine.Game.GraphicsDevice.RenderState.DestinationBlend = Blend.One;
            gameEngine.Game.GraphicsDevice.RenderState.DepthBufferWriteEnable = false;

            gameEngine.Game.GraphicsDevice.RenderState.PointSizeMin = 100f;
            gameEngine.Game.GraphicsDevice.RenderState.PointSizeMax = 2000f;

            gameEngine.Game.GraphicsDevice.RenderState.PointSize = 100.0f;

            #endregion

            gameEngine.Game.GraphicsDevice.VertexDeclaration = vertexPosColDecl;

            ViewProjection = View * Projection;

            //--- BlobEffect 1
            blobEffect.Parameters["WorldViewProjection"].SetValue(ViewProjection);
            blobEffect.Parameters["Projection"].SetValue(Projection);
            blobEffect.Parameters["ParticleSize"].SetValue(4f);
            blobEffect.Parameters["ViewportHeight"].SetValue((float)this.gameEngine.Game.GraphicsDevice.Viewport.Height);
            blobEffect.Parameters["GaussBlob"].SetValue(gaussianTex);

            if (!simpleVertex)
            {
            blobEffect.Parameters["GaussUnit"].SetValue(gaussianUnitTex);
            blobEffect.Parameters["Zoom"].SetValue(-zoom);
            }

            blobEffect.Begin();
            blobEffect.Techniques[0].Passes[0].Begin();

            if (simpleVertex)
            {
                gameEngine.Game.GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.PointList,
                         vertexBlobArrayTest, 0, vertexBlobArrayTest.Length);
            }
            else
            {
                gameEngine.Game.GraphicsDevice.DrawUserPrimitives<VertexBlob>(PrimitiveType.PointList,
                     vertexBlobArray, 0, vertexBlobArray.Length);
            }

            blobEffect.Techniques[0].Passes[0].End();
            blobEffect.End();
            //---
            ResolveRTs();

            //SetRTs2();

            ////--- BlobEffect 2
            //blob2Effect.Parameters["WorldViewProjection"].SetValue(ViewProjection);
            //blob2Effect.Parameters["Projection"].SetValue(Projection);
            //blob2Effect.Parameters["ParticleSize"].SetValue(4f);
            //blob2Effect.Parameters["ViewportHeight"].SetValue((float)this.gameEngine.Game.GraphicsDevice.Viewport.Height);
            //blob2Effect.Parameters["GaussBlob"].SetValue(gaussianTex);
            //blob2Effect.Parameters["Zoom"].SetValue(-zoom);

            //blob2Effect.Begin();
            //blob2Effect.Techniques[0].Passes[0].Begin();

            //gameEngine.Game.GraphicsDevice.DrawUserPrimitives<VertexBlob>(PrimitiveType.PointList,
            //         vertexBlobArrayTest, 0, vertexBlobArray.Length);

            //blob2Effect.Techniques[0].Passes[0].End();
            //blob2Effect.End();
            ////---

            #region UnsetRenderStates
            gameEngine.Game.GraphicsDevice.RenderState.PointSpriteEnable = false;
            gameEngine.Game.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
            gameEngine.Game.GraphicsDevice.RenderState.AlphaBlendEnable = false;
            #endregion

            //ResolveRTs2();
        }

        private void SetRTs()
        {
            //gameEngine.Game.GraphicsDevice.SetRenderTarget(4, infoTarget);
            gameEngine.Game.GraphicsDevice.SetRenderTarget(3, unitTarget);
            gameEngine.Game.GraphicsDevice.SetRenderTarget(2, playerTarget);
            gameEngine.Game.GraphicsDevice.SetRenderTarget(1, colorTarget);
            gameEngine.Game.GraphicsDevice.SetRenderTarget(0, normalTarget);
        }

        private void ResolveRTs()
        {
            gameEngine.Game.GraphicsDevice.SetRenderTarget(0, null);
            gameEngine.Game.GraphicsDevice.SetRenderTarget(1, null);
            gameEngine.Game.GraphicsDevice.SetRenderTarget(2, null);
            gameEngine.Game.GraphicsDevice.SetRenderTarget(3, null);
            //gameEngine.Game.GraphicsDevice.SetRenderTarget(4, null);
        }

        private void SetRTs2()
        {
            gameEngine.Game.GraphicsDevice.SetRenderTarget(0, infoTarget);
        }

        private void ResolveRTs2()
        {
            gameEngine.Game.GraphicsDevice.SetRenderTarget(0, null);
        }
        #endregion

        #region Methods
        public void Addblob()
        {
            //gameEngine.RenderLogic.vertexBlobArray = new VertexBlob[ListBlobs.Count];
            //for (int i = 0; i < ListBlobs.Count; i++)
            //{
            //    gameEngine.RenderLogic.vertexBlobArray[i] = ListBlobs[i].VertexBlob;
            //}
        }

        private void GenerateGaussianTexture(ref Texture2D texture, float gaussianDeviation)
        {
            texture = new Texture2D(this.gameEngine.Game.GraphicsDevice, GAUSSIANSIZE, GAUSSIANSIZE, 0, TextureUsage.None, SurfaceFormat.Single);

            int u, v;
            float dx, dy, I;

            float[] temp = new float[GAUSSIANSIZE * GAUSSIANSIZE];

            for (v = 0; v < GAUSSIANSIZE; ++v)
            {
                for (u = 0; u < GAUSSIANSIZE; ++u)
                {
                    dx = 2.0f * u / (float)GAUSSIANSIZE - 1.0f;
                    dy = 2.0f * v / (float)GAUSSIANSIZE - 1.0f;
                    I = GAUSSIANSIZE * (float)Math.Exp(-(dx * dx + dy * dy) / gaussianDeviation);

                    int pos = u + v * GAUSSIANSIZE;
                    temp[pos] = I;
                }
            }

            texture.SetData<float>(temp);
            //texture.Save(@"d:\gaussian.bmp", ImageFileFormat.Png);
        }
        #endregion
    }
}
