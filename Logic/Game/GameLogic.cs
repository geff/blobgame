using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using FarseerGames.FarseerPhysics;
using FarseerGames.GettingStarted;
using BlobGame.Render;

namespace BlobGame.Logic.Game
{
    public class GameLogic
    {
        #region Properties
        public List<BlobEntity> ListBlobs { get; set; }
        public List<BlobGroup> ListBlobGroup { get; set; }
        public BlobGroup currentBlobGroup { get; set; }

        public Octree mainOctree;

        public float mapWidth = 100f;
        public float mapHeight = 100f;

        public RectangleF recMap = new RectangleF(0, 0, 50f, 100f);
        #endregion

        #region Physic
        public static PhysicsSimulator PhysicsSimulator;
        #endregion

        public GameEngine gameEngine { get; set; }

        public GameLogic(GameEngine gameEngine)
        {
            this.gameEngine = gameEngine;
        }

        #region Init
        public void Initialize()
        {
            //--- Octree
            InitOctree(mapWidth, mapHeight, 5, 5);
            recMap = new RectangleF(0, 0, mapWidth, mapHeight);
            //---

            //--- Physic
            InitPhysicEngin();
            //---

            //--- Init blob
            InitBlobGroup();
            //---
        }

        private void InitOctree(float width, float height, int subX, int subY)
        {
            mainOctree = new Octree(width, height, subX, subY);
        }

        private void InitBlobGroup()
        {
            this.ListBlobs = new List<BlobEntity>();
            this.ListBlobGroup = new List<BlobGroup>();

            Vector2 vecCenter = new Vector2(0.5f * mapWidth, 0.5f * mapHeight);
            Vector2 vecExtrem = new Vector2(0.95f * mapWidth, 0.95f * mapHeight);
            Vector2 vecOrig = new Vector2(0.1f * mapWidth, 0.1f * mapHeight);

            //---
            BlobGroup blobGroup = new BlobGroup(30, vecCenter, 0.2f, Color.Green, 1f, 3, Color.Green);
            currentBlobGroup = blobGroup;
            //currentBlobGroup.ListBlob[0].Body.IsStatic = true;

            ListBlobGroup.Add(blobGroup);
            ListBlobs.AddRange(blobGroup.ListBlob);
            //---

            //---
            blobGroup = new BlobGroup(10, new Vector2(vecCenter.X - 5f, vecCenter.Y), 1f, Color.Red, 1f, 1, Color.Red);
            //blobGroup.ListBlob[0].Body.IsStatic = true;

            ListBlobGroup.Add(blobGroup);
            ListBlobs.AddRange(blobGroup.ListBlob);
            //---


            //---
            //blobGroup = new BlobGroup(20, vecExtrem, 1f, Color.Yellow, 1f, 1, Color.Red);
            ////blobGroup.ListBlob[0].Body.IsStatic = true;

            //ListBlobGroup.Add(blobGroup);
            //ListBlobs.AddRange(blobGroup.ListBlob);
            //---


            //---
            //blobGroup = new BlobGroup(20, vecOrig, 1f, Color.Black, 1f, 1, Color.Red);
            ////blobGroup.ListBlob[0].Body.IsStatic = true;

            //ListBlobGroup.Add(blobGroup);
            //ListBlobs.AddRange(blobGroup.ListBlob);
            //---


            /*
            //---
            blobGroup = new BlobGroup(10, new Vector2(6, -4f), 3f, Color.Blue, 1f, 2, Color.Blue);
            //blobGroup.ListBlob[0].Body.IsStatic = true;

            ListBlobGroup.Add(blobGroup);
            ListBlobs.AddRange(blobGroup.ListBlob);
            //---
            */


            int nb = 0;

            for (int i = 1; i < nb; i++)
            {
                float angle = MathHelper.TwoPi / ((float)nb - 1) * (float)i;

                float c = ((float)i) / (float)nb;

                //Color col1 = new Color(c * 2f, c * 10f, c * 3f);
                //Color col2 = new Color(c * 10f, c * 5f, c * 6f);

                Color col1 = Color.Lerp(Color.Red, Color.Green, c);
                Color col2 = Color.Lerp(Color.Yellow, Color.Blue, c);

                BlobGroup blobGroup2 = new BlobGroup(10, new Vector2(vecCenter.X + (float)nb / 1f * (float)Math.Cos(angle), vecCenter.Y + (float)nb / 1f * (float)Math.Sin(angle)), 1f, col1, 1f, i, col2);
                currentBlobGroup = blobGroup2;
                //currentBlobGroup.ListBlob[0].Body.IsStatic = true;

                ListBlobGroup.Add(blobGroup2);
                ListBlobs.AddRange(blobGroup2.ListBlob);
            }
        } 
        #endregion

        #region Physics
        private void InitPhysicEngin()
        {
            //--- Physic simulator
            PhysicsSimulator = new PhysicsSimulator(new Vector2(0, 200));
            PhysicsSimulator.MaxContactsToDetect = 2;
            PhysicsSimulator.Gravity = new Vector2(0, 0f);
            //---
        }

        #endregion

        #region Update
        public void Update(GameTime gameTime)
        {
            //---
            PhysicsSimulator.Update(gameTime.ElapsedGameTime.Milliseconds * .001f);
            //---

            //--- UpdateMoveBlob
            UpdateMoveBlob(gameTime);
            //---

            //UpdateBlobs();
        }

        private void UpdateMoveBlob(GameTime gameTime)
        {
            Vector2 fixedPosition = Vector2.Zero;

            for (int i = 0; i < ListBlobGroup.Count; i++)
            {
                if (ListBlobGroup[i].ListMoves != null && ListBlobGroup[i].ListMoves.Count > 0)
                {
                    bool finished = false;

                    Vector2 distance = ListBlobGroup[i].ListBlob[0].Position - ListBlobGroup[i].ListMoves[0];
                    float distanceLength = distance.Length();

                    if (distanceLength < 0.7f)// && (ListBlobGroup[i].ListBlob[j].Geom.ContactList == null || ListBlobGroup[i].ListBlob[j].Geom.ContactList.Count == 0))
                    {
                        finished = true;
                        fixedPosition = ListBlobGroup[i].ListBlob[0].Position;
                    }
                    else
                    {
                        //distance.Normalize();
                        //distance /= 5f;

                        //---> Si la position d'un des blobs est très proche de la position attendue, alors le groupblob s'arrête
                    for (int j = 0; j < ListBlobGroup[i].ListBlob.Count; j++)
                    {
                            //--- Le compressionFactor représente le % de place occupée par l'ensemble des blobs par rapport à leur place initiale
                            //    Plus le taux est bas, plus le groupBlob est rammassé à son point d'arrivé, ce qui donne un effet gélatineux mais peut causer
                            //    une instabilité. Il faudrait calculer dynamiquement ce taux selon le nombre de blobs. Plus le nombre est faible, plus le taux peut être petit
                            float compressionFactor = 0.8f;

                            Vector2 evaluatedPosition = ListBlobGroup[i].ListMoves[0] + compressionFactor * (ListBlobGroup[i].ListBlob[j].Position - ListBlobGroup[i].ListBlob[0].Position);

                            distance = ListBlobGroup[i].ListBlob[j].Position - evaluatedPosition;
                            distance.Normalize();
                            distance /= 5f;

                            //if (distanceLength >= 0.7f)// && (ListBlobGroup[i].ListBlob[j].Geom.ContactList == null || ListBlobGroup[i].ListBlob[j].Geom.ContactList.Count == 0))
                            {

                            //if (distanceLength > 2f)
                            //    distance /= 10f;
                            //else
                            //    distance /= -2f * distanceLength + 20;

                            if (recMap.Contains(ListBlobGroup[i].ListBlob[j].Position - distance))
                            {
                                ListBlobGroup[i].ListBlob[j].Position -= distance;
                            }
                            else
                            {
                                finished = true;
                                fixedPosition = ListBlobGroup[i].ListBlob[j].Position;
                            }
                            //ListBlobGroup[i].ListBlob.ForEach(blob => blob.Body.ApplyForce(ref distance);
                        }
                            //else
                            //{
                            //    finished = true;
                            //    fixedPosition = ListBlobGroup[i].ListBlob[j].Position;

                            //    //ListBlobGroup[i].ListBlob.ForEach(blob => blob.Body.ClearImpulse());
                            //}
                        }
                    }

                    /*
                    Vector2 distance = ListBlobGroup[i].ListBlob[0].Position - ListBlobGroup[i].ListMoves[0];
                    float distanceLength = distance.Length();

                    if (distanceLength >= 0.05f)
                    {
                        distance.Normalize();

                        if (distanceLength > 2f)
                            distance /= 5f;
                        else
                            distance /= -5f * distanceLength + 20;

                        ListBlobGroup[i].ListBlob[0].Position -= distance;
                        //ListBlobGroup[i].ListBlob.ForEach(blob => blob.Body.ApplyForce(ref distance);
                    }
                    else
                    {
                        finished = true;
                        //ListBlobGroup[i].ListBlob.ForEach(blob => blob.Body.ClearImpulse());
                    }
                    */

                    if (finished)
                    {
                        //ListBlobGroup[i].SetPosition(ListBlobGroup[i].ListMoves[0]);
                        ListBlobGroup[i].SetPosition(fixedPosition);
                        ListBlobGroup[i].ListMoves = new List<Vector2>();
                    }
                }
            }
        } 
        #endregion

        public void SelectGroupBlob(Vector2 position)
        {
            float distance = float.MaxValue;
            float oldDistance = float.MaxValue;
            int indexSelectedBlob = -1;

            //--- Search the closest blob group
            for (int i = 0; i < gameEngine.GameLogic.ListBlobs.Count; i++)
            {
                Vector2 blobPosition = gameEngine.GameLogic.ListBlobs[i].Position;
                Vector2.Distance(ref position, ref blobPosition, out distance);

                if (distance <= 0.9f && distance <= oldDistance)
                {
                    indexSelectedBlob = i;
                    oldDistance = distance;
                }
            }
            //---

            //---> deselect previous selected blob group
            if (gameEngine.GameLogic.currentBlobGroup != null)
                gameEngine.GameLogic.currentBlobGroup.Deselect();

            //---> select current blob group
            if (indexSelectedBlob != -1)
            {
                gameEngine.GameLogic.currentBlobGroup = gameEngine.GameLogic.ListBlobs[indexSelectedBlob].BlobGroupParent;
                gameEngine.GameLogic.currentBlobGroup.Select();
            }
            else
                gameEngine.GameLogic.currentBlobGroup = null;
        }

        public void MoveBlobGroup(BlobGroup blobGroup, Vector2 position)
        {
            blobGroup.StopMovement();

            blobGroup.ListMoves = new List<Vector2>();

            Vector2 pointMove = position;
            blobGroup.ListMoves.Add(pointMove);

            for(int i=0;i<3;i++)
            {
                if(blobGroup.PositionSprings[i] != null)
                blobGroup.PositionSprings[i].Dispose();
            }
            //prevLeftButtonState = ButtonState.Released;
            //if (_mousePickSpring != null)
            //{
            //    _mousePickSpring.Dispose();
            //}
            //for (int i = 0; i < 3; i++)
            //{
            //    blobGroup.PositionSprings[i].Dispose();
            //}

            //currentBlobGroup.ListBlob.ForEach(blob => { blob.Body.ClearTorque(); blob.Body.ClearForce(); });
            //Vector2 distance = currentBlobGroup.ListBlob.Last().Position - currentBlobGroup.ListMoves[0];

            //    if (distance.Length() >= 0.01f)
            //    {
            //        //distance.Normalize();
            //        distance = -distance;
            //        //currentBlobGroup.ListBlob.ForEach(blob => blob.Body.ApplyImpulse(ref distance));
            //        currentBlobGroup.ListBlob.First().Body.ApplyImpulse(ref distance);
            //    }

            //currentBlobGroup.SetPosition(mousePosition);
        }

        public void AddBlob(BlobEntity newBlob)
        {
            ListBlobs.Add(newBlob);

            //Update DuplicateWaitObjectException RenderLogic
        }
        
        public void AddBlob(BlobGroup blobGroup, float size, Color color, float polarity)
        {
            BlobEntity newBlob = blobGroup.AddBlob(size, color, polarity, blobGroup.GroupNumber);

            if (blobGroup == this.currentBlobGroup)
                newBlob.Selected = true;

            Octree octree = gameEngine.GameLogic.mainOctree.GetOctree(newBlob.Position);

            if (octree != null)
            {
                newBlob.ParentOctree = octree;
                octree.ListBlobEntity.Add(newBlob);
            }

            ListBlobs.Add(newBlob);

            /*
            gameEngine.RenderLogic.vertexBlobArray = new VertexBlob[ListBlobs.Count];
            for (int i = 0; i < ListBlobs.Count; i++)
            {
                gameEngine.RenderLogic.vertexBlobArray[i] = ListBlobs[i].VertexBlob;
            }
            */
        }
    }
}
