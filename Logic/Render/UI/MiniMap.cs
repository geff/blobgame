using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BlobGame.Render;

namespace BlobGame.Logic.Render.UI
{
    public class MiniMap : UIComponent
    {

        private Rectangle recMap = new Rectangle();
        private Rectangle recMapBorder = new Rectangle();

        private DrawableRectangle recMapDraw;
        private DrawableRectangle recMapBorderDraw;

        private DrawableRectangle recViewScreen;

        private Rectangle[] recGridV;
        private Rectangle[] recGridH;

        private GameEngine gameEngine;

        private Vector2 Location;
        private bool isViewScreenMoving = false;
        private float ratio;

        Effect fxUIMapShader;
        VertexBlob[] vertexBlobArray;

        RenderTarget2D mapTexTarget;
        
        public Texture2D mapTexShader;
        public Texture2D texMap;

        private UIManager UIManager { get; set; }

        public MiniMap(GameEngine gameBlob)
        {
            this.gameEngine = gameBlob;
        }

        public override void Initialize(UIManager UIManager)
        {
            this.UIManager = UIManager;
            
            //this.fxUIMapShader = gameEngine.Game.Content.Load<Effect>("Effects\\UIMapShader");

            InitRecMap();

            //mapTexTarget = CreateRenderTarget();
            //CreateTexture2D();
        }

        public void InitRecMap()
        {
            float mapViewWidth = 100;
            float mapViewHeight = 100;

            int mapViewX = 10;
            int mapViewY = 10;

            recMapBorder = new Rectangle(mapViewX, mapViewY, (int)mapViewWidth, (int)mapViewHeight);

            if (this.gameEngine.GameLogic.mapWidth > this.gameEngine.GameLogic.mapHeight)
            {
                recMap = new Rectangle(0, 0, (int)mapViewWidth, (int)(this.gameEngine.GameLogic.mapHeight / this.gameEngine.GameLogic.mapWidth * mapViewHeight));
                recMap.Location = new Point(recMapBorder.Left, recMapBorder.Top + (int)(mapViewHeight / 2f - recMap.Height / 2f));
            }
            else
            {
                recMap = new Rectangle(0, 0, (int)(this.gameEngine.GameLogic.mapWidth / this.gameEngine.GameLogic.mapHeight * mapViewWidth), (int)mapViewHeight);
                recMap.Location = new Point(recMapBorder.Left + (int)(mapViewWidth / 2f - recMap.Width / 2f), recMapBorder.Top);
            }

            ratio = mapViewWidth / this.gameEngine.GameLogic.mapWidth;

            Location = new Vector2(recMap.Location.X, recMap.Location.Y);

            //--- Border
            recMapBorderDraw = new DrawableRectangle(recMapBorder);
            recMapBorderDraw.Init(5, 3);
            //---

            //--- Map
            recMapDraw = new DrawableRectangle(recMap);
            recMapDraw.Init(0, 2);
            //---

            //---
            recViewScreen = new DrawableRectangle(gameEngine.RenderLogic.viewScreen, recMap.Location);
            recViewScreen.Init(0, 1);
            //---

            //--- Grid
            int gridSize = 1;

            recGridV = new Rectangle[gameEngine.GameLogic.mainOctree.NbOctreeV];
            recGridH = new Rectangle[gameEngine.GameLogic.mainOctree.NbOctreeH];

            int caseWidth = recMap.Width / gameEngine.GameLogic.mainOctree.NbOctreeV;
            int caseHeight = recMap.Height / gameEngine.GameLogic.mainOctree.NbOctreeH;

            for (int i = 0; i < gameEngine.GameLogic.mainOctree.NbOctreeV; i++)
            {
                recGridV[i] = new Rectangle(recMap.X + (i + 1) * caseWidth, recMap.Y, gridSize, recMap.Height);
            }

            for (int i = 0; i < gameEngine.GameLogic.mainOctree.NbOctreeH; i++)
            {
                recGridH[i] = new Rectangle(recMap.X, recMap.Y + (i + 1) * caseHeight, recMap.Width, gridSize);
            }
            //---

            //---
            //gameEngine.RenderLogic.ViewUI = Matrix.CreateTranslation(new Vector3(-gameEngine.RenderLogic.vecTranslation.X, gameEngine.RenderLogic.vecTranslation.Y, -100f * 1f / ratio));
            //gameEngine.RenderLogic.ViewUI = Matrix.CreateTranslation(new Vector3(-150, 150, -250f * 1f / ratio));
            //gameEngine.RenderLogic.ViewProjectionUI = gameEngine.RenderLogic.ViewUI * gameEngine.RenderLogic.ProjectionUI;
            //---
        }

        private void CreateTexture2D()
        {
            //texMap = new Texture2D(gameEngine.Game.GraphicsDevice, recMap.Width, recMap.Height);

            //texMap = Texture2D.FromFile(gameEngine.Game.GraphicsDevice,"Mapv0");
            texMap = gameEngine.Game.Content.Load<Texture2D>("Textures\\Mapv0");
        }

        public void SetPosition(Point Location)
        {

        }

        public override void Update(GameTime gameTime)
        {
            if (gameEngine.ControllerLogic.IsMouseLeftButtonPressed && !isViewScreenMoving)
            {
                if (recMap.Contains(gameEngine.ControllerLogic.mouseState.X, gameEngine.ControllerLogic.mouseState.Y))
                {
                    isViewScreenMoving = true;
                    UIManager.IsMouseCaught = true;
                }
            }
            //---> Bouge la caméra
            else if (gameEngine.ControllerLogic.IsMouseLeftButtonPressed && isViewScreenMoving)
            {
                Vector2 vec = new Vector2();

                vec.X = (gameEngine.ControllerLogic.mouseState.X - recMap.X);
                vec.Y = (gameEngine.ControllerLogic.mouseState.Y - recMap.Y);

                //---> Positionnement horizontal de la minimap
                if (vec.X - gameEngine.RenderLogic.viewScreen.Width * ratio / 2f >= 0 &&
                    vec.X + gameEngine.RenderLogic.viewScreen.Width * ratio / 2f <= recMap.Width)
                {
                    gameEngine.RenderLogic.vecTranslation.X = vec.X / ratio;
                    gameEngine.RenderLogic.updateViewScreen = true;
                    UIManager.IsMouseCaught = true;
                }
                //---> Bloqué à gauche
                else if (vec.X - gameEngine.RenderLogic.viewScreen.Width * ratio / 2f < 0)
                {
                    gameEngine.RenderLogic.vecTranslation.X = gameEngine.RenderLogic.viewScreen.Width / 2f;
                    gameEngine.RenderLogic.updateViewScreen = true;
                    UIManager.IsMouseCaught = true;
                }
                //--- Bloqué à droite
                else if (vec.X + gameEngine.RenderLogic.viewScreen.Width * ratio / 2f > recMap.Width)
                {
                    gameEngine.RenderLogic.vecTranslation.X = recMap.Width - gameEngine.RenderLogic.viewScreen.Width / 2f;
                    gameEngine.RenderLogic.updateViewScreen = true;
                    UIManager.IsMouseCaught = true;
                }

                //---> Positionnement vertical de la minimap
                if (vec.Y - gameEngine.RenderLogic.viewScreen.Height * ratio / 2f >= 0 &&
                    vec.Y + gameEngine.RenderLogic.viewScreen.Height * ratio / 2f <= recMap.Height)
                {
                    gameEngine.RenderLogic.vecTranslation.Y = vec.Y / ratio;
                    gameEngine.RenderLogic.updateViewScreen = true;
                    UIManager.IsMouseCaught = true;
                }
                //---> Bloqué en haut
                else if (vec.Y - gameEngine.RenderLogic.viewScreen.Height * ratio / 2f < 0)
                {
                    gameEngine.RenderLogic.vecTranslation.Y = gameEngine.RenderLogic.viewScreen.Height / 2f;
                    gameEngine.RenderLogic.updateViewScreen = true;
                    UIManager.IsMouseCaught = true;
                }
                //--- Bloqué en bas
                else if (vec.Y + gameEngine.RenderLogic.viewScreen.Height * ratio / 2f > recMap.Height)
                {
                    gameEngine.RenderLogic.vecTranslation.Y = recMap.Height - gameEngine.RenderLogic.viewScreen.Height / 2f;
                    gameEngine.RenderLogic.updateViewScreen = true;
                    UIManager.IsMouseCaught = true;
                }
            }
            //---> Le bouton de la souris est relâché
            else if (gameEngine.ControllerLogic.IsMouseLeftButtonPressedAndReleased && isViewScreenMoving)
            {
                isViewScreenMoving = false;
            }
            //---> Déplacement d'un groupe de blob depuis la minimap
            else if (gameEngine.ControllerLogic.IsMouseRightButtonPressedAndReleased &&
                     recMap.Contains(gameEngine.ControllerLogic.mousePositionPoint) &&
                     gameEngine.GameLogic.currentBlobGroup != null)
            {
                Vector2 vec = new Vector2();

                vec.X = (gameEngine.ControllerLogic.mouseState.X - recMap.X);
                vec.Y = (gameEngine.ControllerLogic.mouseState.Y - recMap.Y);

                gameEngine.GameLogic.MoveBlobGroup(gameEngine.GameLogic.currentBlobGroup, vec * 1/ratio);
                UIManager.IsMouseCaught = true;
            }

            //---
            //InitVertexBlobArray();
            //---
        }

        private void InitVertexBlobArray()
        {
            vertexBlobArray = new VertexBlob[gameEngine.GameLogic.ListBlobs.Count];
            for (int i = 0; i < gameEngine.GameLogic.ListBlobs.Count; i++)
            {
                vertexBlobArray[i] = gameEngine.GameLogic.ListBlobs[i].VertexBlob;
            }
        }

        private RenderTarget2D CreateRenderTarget()
        {
            //return new RenderTarget2D(gameEngine.Game.GraphicsDevice, this.gameEngine.Game.GraphicsDevice.Viewport.Width,
            //    this.gameEngine.Game.GraphicsDevice.Viewport.Height, 0, SurfaceFormat.HalfVector4, gameEngine.Game.GraphicsDevice.PresentationParameters.MultiSampleType, gameEngine.Game.GraphicsDevice.PresentationParameters.MultiSampleQuality);

            return new RenderTarget2D(gameEngine.Game.GraphicsDevice, recMap.Width,
                recMap.Height, 0, SurfaceFormat.HalfVector4, gameEngine.Game.GraphicsDevice.PresentationParameters.MultiSampleType, gameEngine.Game.GraphicsDevice.PresentationParameters.MultiSampleQuality);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            DrawMap(spriteBatch);
        }

        private void DrawMap(SpriteBatch spriteBatch)
        {
            #region old spritebatch
            //--- Draw map
            Color clr = Color.Gray;
            clr.A = 200;
            spriteBatch.Draw(gameEngine.RenderLogic.texBlank, recMap, clr);

            recMapDraw.Draw(spriteBatch, gameEngine.RenderLogic.texBlank, Color.Gray);
            //---

            //--- Draw Grid
            //for (int i = 0; i < gameEngine.GameLogic.mainOctree.NbOctreeV; i++)
            //{
            //    spriteBatch.Draw(gameEngine.RenderLogic.texBlank, recGridV[i], Color.DarkGray);
            //}

            //for (int i = 0; i < gameEngine.GameLogic.mainOctree.NbOctreeH; i++)
            //{
            //    spriteBatch.Draw(gameEngine.RenderLogic.texBlank, recGridH[i], Color.DarkGray);
            //}
            //---

            for (int i = 0; i < gameEngine.GameLogic.ListBlobs.Count; i++)
            {
                spriteBatch.Draw(gameEngine.RenderLogic.texBlank, gameEngine.GameLogic.ListBlobs[i].Position * ratio + Location, null, gameEngine.GameLogic.ListBlobs[i].BlobGroupParent.PlayerColor, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);
            }

            //--- Draw Border
            //recMapBorderDraw.Draw(spriteBatch, gameEngine.RenderLogic.texBlank, Color.White);
            //---

            //---
            clr = Color.GreenYellow;
            clr.A = 100;
            spriteBatch.Draw(gameEngine.RenderLogic.texBlank, recViewScreen.rec, clr);

            recViewScreen.SetRectangle(gameEngine.RenderLogic.viewScreen, recMap.Location, ratio);
            recViewScreen.Init(0, 2);
            recViewScreen.Draw(spriteBatch, gameEngine.RenderLogic.texBlank, Color.GreenYellow);
            //---

            #endregion

            /*
            #region RenderStates
            gameEngine.Game.GraphicsDevice.RenderState.PointSpriteEnable = true;
            gameEngine.Game.GraphicsDevice.RenderState.AlphaBlendEnable = true;
            gameEngine.Game.GraphicsDevice.RenderState.SourceBlend = Blend.One;
            gameEngine.Game.GraphicsDevice.RenderState.DestinationBlend = Blend.One;
            gameEngine.Game.GraphicsDevice.RenderState.DepthBufferWriteEnable = false;

            gameEngine.Game.GraphicsDevice.RenderState.PointSizeMin = 2f;
            gameEngine.Game.GraphicsDevice.RenderState.PointSizeMax = 2000f;

            gameEngine.Game.GraphicsDevice.RenderState.PointSize = 2f;
            #endregion

            gameEngine.Game.GraphicsDevice.SetRenderTarget(0, mapTexTarget);

            //--- BlobEffect 1
            fxUIMapShader.Parameters["Tex"].SetValue(texMap);

            fxUIMapShader.Parameters["WorldViewProjection"].SetValue(gameEngine.RenderLogic.ViewProjectionUI);
            fxUIMapShader.Parameters["Projection"].SetValue(gameEngine.RenderLogic.ProjectionUI);
            //fxUIMapShader.Parameters["ParticleSize"].SetValue(4f);
            fxUIMapShader.Parameters["ViewportHeight"].SetValue((float)this.gameEngine.Game.GraphicsDevice.Viewport.Height);
            fxUIMapShader.Parameters["Zoom"].SetValue(1);//-zoom);

            fxUIMapShader.Begin();
            fxUIMapShader.Techniques[0].Passes[0].Begin();

            gameEngine.Game.GraphicsDevice.DrawUserPrimitives<VertexBlob>(PrimitiveType.PointList,
                     vertexBlobArray, 0, vertexBlobArray.Length);

            fxUIMapShader.Techniques[0].Passes[0].End();
            fxUIMapShader.End();
            //---

            gameEngine.Game.GraphicsDevice.SetRenderTarget(0, null);

            mapTexShader = mapTexTarget.GetTexture();

            #region UnsetRenderStates
            gameEngine.Game.GraphicsDevice.RenderState.PointSpriteEnable = false;
            gameEngine.Game.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
            gameEngine.Game.GraphicsDevice.RenderState.AlphaBlendEnable = false;
            #endregion

            if(false)
                mapTexShader.Save("c:\\testmap2.png", ImageFileFormat.Png);
            */
        }
    }
}
