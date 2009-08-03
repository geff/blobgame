using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FarseerGames.FarseerPhysics.Dynamics.Springs;
using FarseerGames.FarseerPhysics.Factories;
using BlobGame.Render;
using FarseerGames.FarseerPhysics.Dynamics.Joints;
using BlobGame.Logic.Game;

namespace BlobGame
{
    public class BlobGroup
    {
        #region Constants
        public const float SPRING_CONSTANT = 100f;
        public const float DAMPNING_CONSTANT = 50f;
        #endregion

        #region Private properties
        private Vector2 _initialPosition;
        #endregion

        #region Public properties
        public List<BlobEntity> ListBlob { get; set; }
        public List<Spring> ListSpring { get; set; }
        public int GroupNumber { get; set; }

        public List<Vector2> ListMoves { get; set; }
        public Color PlayerColor = Color.Green;

        public BlobEntity[] BlobFoot;
        #endregion

        #region PhysicProperties
        public FixedLinearSpring[] PositionSprings { get; set; }

        #endregion

        public BlobGroup(int nbBlob, Vector2 initialPosition)
        {
            ConstructBlobGroup2(nbBlob, initialPosition, 1f, Color.Red, 1f, 1, Color.Green);
        }

        public BlobGroup(int nbBlob, Vector2 initialPosition, float size, Color color, float polarity, int group, Color playerColor)
        {
            ConstructBlobGroup2(nbBlob, initialPosition, size, color, polarity, group, playerColor);
        }

        List<float> listAngle = new List<float>();

        private void ConstructBlobGroup2(int nbBlob, Vector2 initialPosition, float size, Color color, float polarity, int group, Color playerColor)
        {
            //---> Atache au sol
            this.PositionSprings = new FixedLinearSpring[3];

            // --> Distance entre chaque blob
            float lengthBase = 0.2f;

            Vector2 prevPoint = new Vector2(0, 0);
            List<Vector2> listPoint = new List<Vector2>();
            listPoint.Add(prevPoint);

            // Vector2 oldPoint = Vector2.Zero;

            // int index = 0;
            float delta = 0.2f;
            float rayon = lengthBase * 1.0f;

            float angle = (float)Math.PI / 4f;
            float theta1 = angle;
            listAngle.Add(0.0f);
            // ---

            // --- Calcul de la spirale
            for (int i = 1; i <= nbBlob; i++)
            {
                //--> Angle entre le blob et l'axe Y
                float alpha = (float)Math.Asin(lengthBase / rayon) * 1.8f;

                theta1 += alpha;

                //---> Distance du blob par rapport au centre
                rayon = 2f * lengthBase * theta1 / ((float)Math.PI);
            }
            // ---

            //---> rayon = rayon max de la sphere
            //---> Calcul de la circonférence de la sphère
            float circum = MathHelper.Pi * 2f * rayon;

            //---> Calcul du nombre de points sur le cercle
            int nbPointOnCircle = (int)(circum / lengthBase);

            //--- Création des cercles
            List<Vector2> listPointCircle = new List<Vector2>();
            List<float> listAngleCircle = new List<float>();

            for (int i = 1; i <= nbPointOnCircle; i++)
            {
                float anglePoint = MathHelper.TwoPi / (float)nbPointOnCircle * (float)i;
                //--- Cercle extérieur
                prevPoint = new Vector2(rayon * (float)Math.Cos(anglePoint), rayon * (float)Math.Sin(anglePoint));
                listPointCircle.Add(prevPoint);
                //---

                //--- Cercle intérieur
                prevPoint = new Vector2(rayon * 0.75f * (float)Math.Cos(anglePoint), rayon * 0.75f * (float)Math.Sin(anglePoint));
                listPointCircle.Add(prevPoint);
                //---

                listAngleCircle.Add(anglePoint);
            }
            //---

            //--- Ajoute un blob physique au centre
            listPointCircle.Add(Vector2.Zero);
            //---

            //--- pour l'instant les blobs graphiques sont représentés par les blob physiques
            listAngle = listAngleCircle;
            //---

            List<Edge> listEdge = Triangulation.GetLinks(listPointCircle);

            for (int i = 0; i < listPointCircle.Count; i++)
            {
                Color clr = color;

                if (i < 5)
                    clr = Color.White;

                if (i >= 5 && i <= 10)
                    clr = Color.Blue;

                BlobEntity blob = AddBlob(initialPosition + listPointCircle[i], size, clr, polarity, group, rayon);

                if (false && i > 0)
                {
                    float dist = Vector2.Distance(blob.Position, ListBlob[0].Position);

                    CreateJoint(blob, ListBlob[0], dist);
                }
            }


            for (int i = 0; i < listEdge.Count; i++)
            {
                BlobEntity blob1 = ListBlob[listEdge[i].p1];
                BlobEntity blob2 = ListBlob[listEdge[i].p2];

                //---> Ne pas faire de lien avec le blob du centre
                //if(listEdge[i].p1 == 0 || listEdge[i].p2 ==0)
                //    continue;

                float dist = Vector2.Distance(blob1.Position, blob2.Position);

                CreateJoint(blob1, blob2, dist, listAngle[listEdge[i].p1 % listAngle.Count], listAngle[listEdge[i].p2 % listAngle.Count]);
                //CreateJoint(blob1, blob2, dist);
            }

            //--- Création des liens au sol
            CalcBlobFoot();
            //---
        }

        private void CalcBlobFoot()
        {
            //--- Calcul des blobs de fixation
            BlobFoot = new BlobEntity[3];
            BlobFoot[0] = ListBlob.Last();
            int footsFound = 0;

            float lastSelectedAngle = listAngle.Last();
            for (int i = listAngle.Count - 1; i >= 0; i--)
            {
                if (Math.Abs(listAngle[i] - lastSelectedAngle) - MathHelper.TwoPi / 3f > 0f)
                {
                    lastSelectedAngle = listAngle[i];
                    footsFound++;
                    BlobFoot[footsFound] = ListBlob[i];
                }
                if (footsFound == 2)
                    break;
            }
            //---

            if (footsFound < 2)
            {
                footsFound++;
                BlobFoot[footsFound] = ListBlob[0];
            }
        }

        private void ConstructBlobGroup(int nbBlob, Vector2 initialPosition, float size, Color color, float polarity, int group, Color playerColor)
        {
            // --> Distance entre chaque blob
            float lengthBase = 0.2f;

            Vector2 prevPoint = new Vector2(0, 0);
            List<Vector2> listPoint = new List<Vector2>();
            listPoint.Add(prevPoint);

            // Vector2 oldPoint = Vector2.Zero;

            // int index = 0;
            float delta = 0.2f;
            float rayon = lengthBase * 1.0f;

            float angle = (float)Math.PI / 4f;
            float theta1 = angle;
            listAngle.Add(0.0f);
            // ---

            // --- Calcul de la spirale
            // Stockages des points et des angles
            for (int i = 1; i <= nbBlob; i++)
            {
                //--> Angle entre le blob et l'axe Y
                float alpha = (float)Math.Asin(lengthBase / rayon) * 1.8f;

                theta1 += alpha;

                //---> Distance du blob par rapport au centre
                rayon = 2f * lengthBase * theta1 / ((float)Math.PI);

                //--- Calcul et enregistrement de l'angle
                // Faire un clamp de l'angle theta1
                float blobAngle = 0.0f;
                //int nb = (int)(theta1 / ((float)Math.PI * 2f));

                blobAngle = MathHelper.Clamp(theta1, 0f, MathHelper.TwoPi);

                //blobAngle = theta1 - nb * ((float)Math.PI * 2f);

                listAngle.Add(blobAngle);
                //---

                // --- Ajout du point
                prevPoint = new Vector2(rayon * (float)Math.Cos(theta1), rayon * (float)Math.Sin(theta1));
                listPoint.Add(prevPoint);
                // ---
            }
            // ---

            //---> rayon = rayon max de la sphere
            //---> Calcul de la circonférence de la sphère
            float circum = MathHelper.Pi * 2f * rayon;

            //---> Calcul du nombre de points sur le cercle
            int nbPointOnCircle = (int)(circum / lengthBase);

            //--- Création des cercles
            List<Vector2> listPointCircle = new List<Vector2>();
            List<float> listAngleCircle = new List<float>();

            for (int i = 1; i <= nbPointOnCircle; i++)
            {
                float anglePoint = MathHelper.TwoPi / (float)nbPointOnCircle * (float)i;
                //--- Cercle extérieur
                prevPoint = new Vector2(rayon * (float)Math.Cos(anglePoint), rayon * (float)Math.Sin(anglePoint));
                listPointCircle.Add(prevPoint);
                //---
                //--- Cercle intérieur
                prevPoint = new Vector2(rayon * 0.75f * (float)Math.Cos(anglePoint), rayon * 0.75f * (float)Math.Sin(anglePoint));
                listPointCircle.Add(prevPoint);
                //---

                listAngleCircle.Add(anglePoint);
            }
            //---

            //--- Calcul la triangulation des points
            List<Edge> listEdge = Triangulation.GetLinks(listPointCircle);
            //---

            //--- Création des blobs et du lien spiral
            for (int i = 0; i < listPointCircle.Count; i++)
            {
                Color clr = color;

                if (i < 5)
                    clr = Color.White;

                if (i >= 5 && i <= 10)
                    clr = Color.Blue;

                BlobEntity blob = AddBlob(initialPosition + listPointCircle[i], size, clr, polarity, group, rayon);

                if (false && i > 0)
                {
                    float dist = Vector2.Distance(blob.Position, ListBlob[0].Position);

                    CreateJoint(blob, ListBlob[0], dist);
                }
            }
            //---

            //--- Création des liens
            for (int i = 0; i < listEdge.Count; i++)
            {
                BlobEntity blob1 = ListBlob[listEdge[i].p1];
                BlobEntity blob2 = ListBlob[listEdge[i].p2];

                float dist = Vector2.Distance(blob1.Position, blob2.Position);

                CreateJoint(blob1, blob2, dist, listAngle[listEdge[i].p1], listAngle[listEdge[i].p2]);
                // CreateJoint(blob1, blob2, dist);
            }
            //---
        }

        //private void Constructor(int nbBlob, Vector2 initialPosition, float size, Color color, float polarity, int group, Color playerColor)
        //{
        //    this.ListBlob = new List<BlobEntity>();
        //    this.ListSpring = new List<Spring>();
        //    this.GroupNumber = group;
        //    this._initialPosition = initialPosition;
        //    this.PlayerColor = playerColor;
        //    this.PositionSprings = new FixedLinearSpring[3];
        //    /*
        //    for (int i = 0; i < nbBlob; i++)
        //    {
        //        float rayon = 0.15f * (float)i;
        //        float angle = MathHelper.TwoPi / 10f * (float)i;

        //        //float angle = (float)Math.PI * 2f / 20f * (float)i;

        //        //Vector2 position = new Vector2(1.5f + 2f * (float)Math.Cos(angle), 1.5f + 2f * (float)Math.Sin(angle));
        //        //Vector2 position = new Vector2((float)i * 0.3f, 0);
        //        Vector2 position = new Vector2(rayon * (float)Math.Cos(angle), rayon * (float)Math.Sin(angle));
        //        //Vector2 position = new Vector2(-100f + (float)i * 5f, 20f);

        //        AddBlob(initialPosition + position, size, color, polarity, group, rayon);
        //    }
        //     * */

        //    float lengthBase = 0.2f;

        //    Vector2 prevPoint = new Vector2(0, 0);

        //    List<Vector2> listPoint = new List<Vector2>();
        //    listPoint.Add(prevPoint);

        //    Vector2 oldPoint = Vector2.Zero;

        //    int index = 0;
        //    float delta = 0.2f;
        //    float rayon = lengthBase * 1.0f;

        //    float angle = (float)Math.PI / 4f;
        //    float theta1 = angle;

        //    listAngle.Add(0.0f);

        //    for (int i = 1; i <= nbBlob; i++)
        //    {
        //        float alpha = (float)Math.Asin(lengthBase / rayon) * 1.8f;

        //        //if (i == 5 || i == 10)
        //        //    alpha *= 4f;

        //        theta1 += alpha;
        //        rayon = 2f * lengthBase * theta1 / ((float)Math.PI);

        //        float result = 0.0f;
        //        int nb = (int)(theta1 / ((float)Math.PI * 2f));

        //        result = theta1 - nb * ((float)Math.PI * 2f);

        //        listAngle.Add(result);

        //        //--- Ajout du point
        //        prevPoint = new Vector2(rayon * (float)Math.Cos(theta1), rayon * (float)Math.Sin(theta1));
        //        listPoint.Add(prevPoint);
        //        //---
        //    }

        //    List<Edge> listEdge = Triangulation.GetLinks(listPoint);

        //    for (int i = 0; i < listPoint.Count; i++)
        //    {
        //        Color clr = color;

        //        if (i < 5)
        //            clr = Color.White;

        //        if (i >= 5 && i <= 10)
        //            clr = Color.Blue;

        //        BlobEntity blob = AddBlob(initialPosition + listPoint[i], size, clr, polarity, group, rayon);

        //        if (i > 0)
        //        {
        //            float dist = Vector2.Distance(blob.Position, ListBlob[0].Position);

        //            CreateJoint(blob, ListBlob[0], dist);
        //        }
        //    }


        //    for (int i = 0; i < listEdge.Count; i++)
        //    {
        //        BlobEntity blob1 = ListBlob[listEdge[i].p1];
        //        BlobEntity blob2 = ListBlob[listEdge[i].p2];

        //        float dist = Vector2.Distance(blob1.Position, blob2.Position);

        //        CreateJoint(blob1, blob2, dist, listAngle[listEdge[i].p1], listAngle[listEdge[i].p2]);
        //        //CreateJoint(blob1, blob2, dist);
        //    }

        //    //--- Calcul des blobs de fixation
        //    BlobFoot = new BlobEntity[3];
        //    BlobFoot[0] = ListBlob.Last();
        //    int footsFound = 0;

        //    //BlobEntity firstBlob = ListBlob[0];
        //    //BlobEntity lastblob = ListBlob.Last();
        //    ////Vector2 vecA = lastblob.Position - firstBlob.Position;
        //    //Vector2 vecA = firstBlob.Position - lastblob.Position;


        //    ////foreach (BlobEntity blob in ListBlob)
        //    //for(int i = ListBlob.Count-1; i>0; i--)
        //    //{
        //    //    BlobEntity blob = ListBlob[i];

        //    //    //Vector2 vecB = blob.Position - firstBlob.Position;
        //    //    Vector2 vecB = firstBlob.Position - blob.Position;

        //    //    float bAngle = GetAngle(vecA, vecB);

        //    //    if (bAngle - MathHelper.TwoPi / 3f > 0f)
        //    //    {
        //    //        footsFound++;
        //    //        BlobFoot[footsFound] = blob;
        //    //        vecA = vecB;
        //    //    }

        //    //    if (footsFound == 2)
        //    //        break;
        //    //}
        //    ////---


        //    //if (footsFound < 2)
        //    //{
        //    //    footsFound++;
        //    //    BlobFoot[footsFound] = ListBlob[0];
        //    //}
        //    float lastSelectedAngle = listAngle.Last();
        //    for (int i = listAngle.Count - 1; i >= 0; i--)
        //    {
        //        if (Math.Abs(listAngle[i] - lastSelectedAngle) - MathHelper.TwoPi / 3f > 0f)
        //        {
        //            lastSelectedAngle = listAngle[i];
        //            footsFound++;
        //            BlobFoot[footsFound] = ListBlob[i];
        //        }
        //        if (footsFound == 2)
        //            break;
        //    }
        //    //---

        //    if (footsFound < 2)
        //    {
        //        footsFound++;
        //        BlobFoot[footsFound] = ListBlob[0];
        //    }

        //    #region old
        //    /*
        //    foreach (Vector2 point in listPoint)
        //    {

        //        Color clr = color;

        //        if (index < 5)
        //            clr = Color.White;

        //        if (index >= 5 && index <= 10)
        //            clr = Color.Blue;

        //        BlobEntity blob = AddBlob(initialPosition + point, size, clr, polarity, group, rayon);

        //        if (index == 0)
        //        {
        //            //JointFactory.Instance.CreateFixedAngleJoint(Map.PhysicsSimulator, blob.Body);
        //        }

        //        if (index > 0)
        //        {
        //            float dist = Vector2.Distance(blob.Position, ListBlob[index - 1].Position);

        //            CreateJoint(ListBlob[index - 1], blob, dist, listAngle[index-1], listAngle[index]);

        //            dist = Vector2.Distance(blob.Position, ListBlob[0].Position);

        //            CreateJoint(blob, ListBlob[0], dist);
        //        }




        //        //--- lien avec le plus proche
        //        if (index > 1)
        //        {

        //            int indexPoint = -1;

        //            if (index > 20)
        //            {
        //                float anglePoint = listAngle[index];

        //                List<float> listTempAngle = listAngle.GetRange(0, index - 2);

        //                indexPoint = listTempAngle.FindLastIndex(tempAngle => tempAngle >= anglePoint - delta && tempAngle <= anglePoint + delta);

        //                if (indexPoint != -1)
        //                {
        //                    int max = Math.Max(0, indexPoint - 5);
        //                    int count = listTempAngle.Count - 1 - max;
        //                    listTempAngle = listAngle.GetRange(max, count);

        //                    int minIndex = -1;
        //                    float oldDist = float.MaxValue;
        //                    for (int i = 0; i < listTempAngle.Count; i++)
        //                    {
        //                        float dist = Math.Abs(anglePoint - listTempAngle[i]);
        //                        if (dist < oldDist)
        //                        {
        //                            oldDist = dist;
        //                            minIndex = i;
        //                        }
        //                    }

        //                    if (minIndex != -1)
        //                        indexPoint = indexPoint - 5 + minIndex;
        //                }

        //            }
        //            else
        //            {
        //                indexPoint = (int)Math.Round((5.0f * (float)index - 20f) / 7.9f);
        //            }

        //            if (indexPoint < 0)
        //                indexPoint = 0;

        //            if (indexPoint > -1)
        //            {
        //                //DrawLineBetweenPoints(listPoint[indexPoint], point, center);
        //                float dist = Vector2.Distance(blob.Position, ListBlob[indexPoint].Position);

        //                CreateJoint(blob, ListBlob[indexPoint], dist);
        //            }
        //        }

        //        //---

        //        oldPoint = point;
        //        index++;
        //    }
        //    */

        //    #endregion

        //    SetPosition(initialPosition);
        //}

        public static float GetAngle(Vector2 vec1, Vector2 vec2)
        {
            float dot = vec1.X * vec2.X + vec1.Y * vec2.Y;
            float pdot = vec1.X * vec2.Y - vec1.Y * vec2.X;
            return (float)Math.Atan2(pdot, dot);
        }

        public BlobEntity AddBlob(float size, Color color, float polarity, int group)
        {
            /*
            float rayon = 0.15f * (float)ListBlob.Count;
            float angle = MathHelper.TwoPi / 10f * (float)ListBlob.Count;

            Vector2 position = new Vector2(rayon * (float)Math.Cos(angle), rayon * (float)Math.Sin(angle));

            return AddBlob(_initialPosition + position, size, color, polarity, group, rayon);
            */

            float lengthBase = 0.2f;
            float rayon = lengthBase * 1.0f;
            float angle = (float)Math.PI / 4f;
            float theta1 = angle;

            float theta2 = angle;


            Vector2 prevPoint = Vector2.Zero;

            for (int i = 1; i <= ListBlob.Count; i++)
            {
                float alpha = (float)Math.Asin(lengthBase / rayon) * 1.8f;

                theta1 += alpha;
                rayon = 2f * lengthBase * theta1 / ((float)Math.PI);
            }

            theta1 += this.ListBlob[0].Body.Rotation;
            theta2 = listAngle.Last() + this.ListBlob[0].Body.Rotation;

            prevPoint = new Vector2(rayon * (float)Math.Cos(theta1), rayon * (float)Math.Sin(theta1));

            prevPoint += ListBlob[0].Position;

            //BlobEntity blobEntity = new BlobEntity(this, prevPoint, size, color, polarity);
            BlobEntity blobEntity = AddBlob(prevPoint, size, color, polarity, group, 0f);

            //---
            int index = ListBlob.Count;
            float dist = Vector2.Distance(blobEntity.Position, ListBlob[index - 2].Position);

            CreateJoint(ListBlob[index - 2], blobEntity, dist, theta2, theta1);

            dist = Vector2.Distance(blobEntity.Position, ListBlob[0].Position);

            CreateJoint(blobEntity, ListBlob[0], dist);
            //---

            float delta = 0.2f;


            theta1 -= this.ListBlob[0].Body.Rotation;

            float result = 0.0f;
            int nb = (int)(theta1 / ((float)Math.PI * 2f));
            result = theta1 - nb * ((float)Math.PI * 2f);

            listAngle.Add(result);

            //--- lien avec le plus proche
            if (index > 1)
            {
                int indexPoint = -1;

                if (index > 20)
                {

                    index--;
                    float anglePoint = result;// theta1 - this.ListBlob[0].Body.Rotation;// listAngle[index] - this.ListBlob[0].Body.Rotation;

                    List<float> listTempAngle = listAngle.GetRange(0, index - 2);

                    indexPoint = listTempAngle.FindLastIndex(tempAngle => tempAngle >= anglePoint - delta && tempAngle <= anglePoint + delta);

                    if (indexPoint != -1)
                    {
                        int max = Math.Max(0, indexPoint - 5);
                        int count = listTempAngle.Count - 1 - max;
                        listTempAngle = listAngle.GetRange(max, count);

                        int minIndex = -1;
                        float oldDist = float.MaxValue;
                        for (int i = 0; i < listTempAngle.Count; i++)
                        {
                            dist = Math.Abs(anglePoint - listTempAngle[i]);
                            if (dist < oldDist)
                            {
                                oldDist = dist;
                                minIndex = i;
                            }
                        }

                        if (minIndex != -1)
                            indexPoint = indexPoint - 5 + minIndex;
                    }

                }
                else
                {
                    indexPoint = (int)Math.Round((5.0f * (float)index - 20f) / 7.9f);
                }

                if (indexPoint < 0)
                    indexPoint = 0;

                if (indexPoint > -1)
                {
                    //DrawLineBetweenPoints(listPoint[indexPoint], point, center);
                    dist = Vector2.Distance(blobEntity.Position, ListBlob[indexPoint].Position);

                    CreateJoint(blobEntity, ListBlob[indexPoint], dist);
                }
            }

            //---

            //Vector2 ps = ListBlob[0].Position + new Vector2(10, 0);
            //FarseerGames.FarseerPhysics.Factories.ComplexFactory.Instance.CreateChain(GameLogic.PhysicsSimulator, ps, ps + new Vector2(20f, 0f), 20f, 5f, true, true);

            return blobEntity;
        }

        public BlobEntity AddBlobNew(float size, Color color, float polarity, int group)
        {
            /*
            //---
            float lengthBase = 0.2f;

            Vector2 prevPoint = new Vector2(0, 0);

            List<Vector2> listPoint = new List<Vector2>();
            listPoint.Add(prevPoint);

            Vector2 oldPoint = Vector2.Zero;

            int index = 0;
            float delta = 0.6f;
            float rayon = lengthBase * 1.0f;

            float angle = (float)Math.PI / 4f;
            float theta1 = angle;

            List<float> listAngle = new List<float>();
            listAngle.Add(0.0f);
            //---

            float alpha = (float)Math.Asin(lengthBase / rayon) * 4f;

            theta1 += alpha;
            rayon = lengthBase * theta1 / ((float)Math.PI);

            float result = 0.0f;
            int nb = (int)(theta1 / ((float)Math.PI * 2f));

            result = theta1 - nb * ((float)Math.PI * 2f);

            listAngle.Add(result);

            //--- Ajout du point
            prevPoint = new Vector2(rayon * (float)Math.Cos(theta1), rayon * (float)Math.Sin(theta1));
            listPoint.Add(prevPoint);
            //---
             */

            return null;
        }

        public BlobEntity AddBlob(Vector2 position, float size, Color color, float polarity, int group, float restLength)
        {
            BlobEntity blob = new BlobEntity(this, position, size, color, polarity);

            blob.Geom.CollidesWith = FarseerGames.FarseerPhysics.CollisionCategory.All;
            blob.Geom.CollisionGroup = group;

            blob.BlobGroupParent = this;

            /*
            if(ListBlob.Count>0)
                CreateJoint(ListBlob.Last(), blob, restLength);

            if (ListBlob.Count > 0)
                CreateJoint(ListBlob.First(), blob, restLength);
            */
            if (ListBlob == null)
                ListBlob = new List<BlobEntity>();

            ListBlob.Add(blob);

            return blob;
        }

        public void DeleteLastBlob()
        {
            if (ListBlob.Count > 1)
            {
                ListBlob.RemoveAt(ListBlob.Count - 1);

                ListSpring[ListSpring.Count - 1] = null;
                ListSpring.RemoveAt(ListSpring.Count - 1);
            }
        }

        public void SetPosition(Vector2 position)
        {
            if (PositionSprings == null)
                return;

            for (int i = 0; i < 3; i++)
            {
                if (PositionSprings[i] != null)
                    PositionSprings[i].Dispose();

                Vector2 vec = BlobFoot[i].Body.Position - ListBlob[0].Body.Position;

                PositionSprings[i] = SpringFactory.Instance.CreateFixedLinearSpring(GameLogic.PhysicsSimulator,
                                                      BlobFoot[i].Body, Vector2.Zero, position + vec, 10000, 0.1f);

                PositionSprings[i].RestLength = 0f;
            }
        }

        private void CreateJoint(BlobEntity blob1, BlobEntity blob2, float restLength)
        {
            //LinearSpring spring = SpringFactory.Instance.CreateLinearSpring(Map.PhysicsSimulator, blob1.Body, Vector2.Zero, blob2.Body, Vector2.Zero, SPRING_CONSTANT, DAMPNING_CONSTANT);


            //AngleSpring spring = SpringFactory.Instance.CreateAngleSpring(Map.PhysicsSimulator, blob1.Body, blob2.Body, SPRING_CONSTANT, DAMPNING_CONSTANT);

            //JointFactory.Instance.CreateRevoluteJoint(Map.PhysicsSimulator, blob1.Body, blob2.Body, Vector2.Zero);
            //AngleJoint joint = JointFactory.Instance.CreateAngleJoint(Map.PhysicsSimulator, blob1.Body, blob2.Body);

            //joint.Softness = 0.1f;
            //joint.BiasFactor = 0.1f;
            //spring.RestLength = restLength;
            //ListSpring.Add(spring);
            //PinJoint pinJoint = JointFactory.Instance.CreatePinJoint(Map.PhysicsSimulator, blob1.Body, Vector2.Zero, blob2.Body, Vector2.Zero);
            SliderJoint joint = JointFactory.Instance.CreateSliderJoint(GameLogic.PhysicsSimulator, blob1.Body, Vector2.Zero, blob2.Body, Vector2.Zero, restLength - 0.001f, restLength + 0.001f);
            //joint.

            //JointFactory.Instance.CreatePinJoint(Map.PhysicsSimulator, blob1.Body, Vector2.Zero, blob2.Body, Vector2.Zero);

            //JointFactory.Instance.CreateAngleLimitJoint(Map.PhysicsSimulator, blob1.Body, blob2.Body, -0.01f, 0.01f);

            //JointFactory.Instance.CreateFixedAngleJoint(Map.PhysicsSimulator, blob1.Body);
            //JointFactory.Instance.CreateFixedAngleJoint(Map.PhysicsSimulator, blob2.Body);

            //JointFactory.Instance.CreatePinJoint(Map.PhysicsSimulator, blob1.Body, Vector2.Zero, blob2.Body, Vector2.Zero);

            //JointFactory.Instance.(Map.PhysicsSimulator, blob1.Body, Vector2.Zero, blob2.Body, Vector2.Zero);
        }

        private void CreateJoint(BlobEntity blob1, BlobEntity blob2, float restLength, float angle1, float angle2)
        {
            //LinearSpring spring = SpringFactory.Instance.CreateLinearSpring(
            //    Map.PhysicsSimulator, blob1.Body, Vector2.Zero, blob2.Body,
            //    Vector2.Zero, SPRING_CONSTANT, DAMPNING_CONSTANT);

            //AngleSpring spring = SpringFactory.Instance.CreateAngleSpring(Map.PhysicsSimulator, blob1.Body, blob2.Body, SPRING_CONSTANT, DAMPNING_CONSTANT);

            //JointFactory.Instance.CreateRevoluteJoint(Map.PhysicsSimulator, blob1.Body, blob2.Body, Vector2.Zero);
            //AngleJoint joint = JointFactory.Instance.CreateAngleJoint(Map.PhysicsSimulator, blob1.Body, blob2.Body);

            //joint.Softness = 0.1f;
            //joint.BiasFactor = 0.1f;
            //spring.RestLength = restLength;
            //ListSpring.Add(spring);

            SliderJoint sliderJoint = JointFactory.Instance.CreateSliderJoint(GameLogic.PhysicsSimulator, blob1.Body, Vector2.Zero, blob2.Body, Vector2.Zero, restLength - 0.01f, restLength + 0.01f);
            //sliderJoint.Softness = -0.00005f;
            sliderJoint.Softness = -0.00005f;
            sliderJoint.BiasFactor = 0.05f;

            JointFactory.Instance.CreateAngleLimitJoint(GameLogic.PhysicsSimulator, blob1.Body, blob2.Body, -0.1f, 0.1f);

            return;
            //JointFactory.Instance.CreateFixedAngleJoint(GameLogic.PhysicsSimulator, blob1.Body);
            //JointFactory.Instance.CreateFixedAngleJoint(GameLogic.PhysicsSimulator, blob2.Body);

            //PinJoint pinJoint = JointFactory.Instance.CreatePinJoint(GameLogic.PhysicsSimulator, blob1.Body, Vector2.Zero, blob2.Body, Vector2.Zero);
            //pinJoint.BiasFactor = 0.25f;
            //JointFactory.Instance.(Map.PhysicsSimulator, blob1.Body, Vector2.Zero, blob2.Body, Vector2.Zero);

            float lengthBase = 0.2f;

            float lengthD = lengthBase * 0.75f;


            //--- point 1
            Vector2 blobPos = blob1.Position - this._initialPosition;

            float rayon = blobPos.Length() * 1.3f;
            float angle = 0.0f;


            angle = angle1;

            float deltaAngle = (float)Math.Atan((lengthD) / rayon);
            angle += deltaAngle;

            Vector2 vecAttach11 = new Vector2(rayon * (float)Math.Cos(angle), rayon * (float)Math.Sin(angle));
            Vector2 vecAttach12 = new Vector2((rayon - 0.1f) * (float)Math.Cos(angle), (rayon - 0.1f) * (float)Math.Sin(angle));

            vecAttach11 -= blobPos;
            vecAttach12 -= blobPos;
            //---

            //--- point 2
            blobPos = blob2.Position - this._initialPosition;

            rayon = blobPos.Length() * 1.3f;
            angle = 0.0f;

            angle = angle2;

            deltaAngle = (float)Math.Atan((lengthD) / rayon);
            angle -= deltaAngle;

            Vector2 vecAttach21 = new Vector2(rayon * (float)Math.Cos(angle), rayon * (float)Math.Sin(angle));
            Vector2 vecAttach22 = new Vector2((rayon - 0.1f) * (float)Math.Cos(angle), (rayon - 0.1f) * (float)Math.Sin(angle));

            vecAttach21 -= blobPos;
            vecAttach22 -= blobPos;
            //---

            float stdLength1 = 0.0f;

            Vector2 vec11 = vecAttach11 + blob1.Position;
            Vector2 vec21 = vecAttach21 + blob2.Position;

            stdLength1 = (vec11 - vec21).Length();

            float stdLength2 = 0.0f;

            Vector2 vec12 = vecAttach12 + blob1.Position;
            Vector2 vec22 = vecAttach22 + blob2.Position;

            stdLength2 = (vec12 - vec22).Length();


            //vecAttach1 = new Vector2(-0.1f, -0.1f);
            //vecAttach2 = new Vector2(0.1f, 0.1f);

            //JointFactory.Instance.CreateSliderJoint(Map.PhysicsSimulator, blob1.Body, vecAttach11, blob2.Body, vecAttach21, stdLength1 - 0.01f, stdLength1 + 0.01f);
            //JointFactory.Instance.CreateSliderJoint(Map.PhysicsSimulator, blob1.Body, vecAttach12, blob2.Body, vecAttach22, stdLength2 - 0.01f, stdLength2 + 0.01f);

            //PinJoint pinJoint = JointFactory.Instance.CreatePinJoint(GameLogic.PhysicsSimulator, blob1.Body, Vector2.Zero, blob2.Body, Vector2.Zero);


            //SliderJoint joint = JointFactory.Instance.CreateSliderJoint(Map.PhysicsSimulator, blob1.Body, Vector2.Zero, blob2.Body, Vector2.Zero, restLength - 0.01f, restLength + 0.01f);

            //PinJoint pinJoint = JointFactory.Instance.CreatePinJoint(Map.PhysicsSimulator, blob1.Body, vecAttach11, blob2.Body, vecAttach21);
            //PinJoint pinJoint2 = JointFactory.Instance.CreatePinJoint(Map.PhysicsSimulator, blob1.Body, vecAttach12, blob2.Body, vecAttach22);
            //pinJoint.Softness = .1f;
            //pinJoint.BiasFactor = .1f;

            //JointFactory.Instance.CreatePinJoint(Map.PhysicsSimulator, blob1.Body, Vector2.Zero, blob2.Body, Vector2.Zero);

            //JointFactory.Instance.CreateSliderJoint(Map.PhysicsSimulator, blob1.Body, vecAttach11, blob2.Body, vecAttach21, restLength - 0.001f, restLength + 0.001f);

            //JointFactory.Instance.CreateAngleJoint(Map.PhysicsSimulator, blob1.Body, blob2.Body, 0.1f, 0.1f);

            //LinearSpring spring = SpringFactory.Instance.CreateLinearSpring(
            //    Map.PhysicsSimulator, blob1.Body, vecAttach11, blob2.Body,
            //    vecAttach21, SPRING_CONSTANT, DAMPNING_CONSTANT);

            //LinearSpring spring = SpringFactory.Instance.CreateLinearSpring(Map.PhysicsSimulator, blob1.Body, Vector2.Zero, blob2.Body, Vector2.Zero, SPRING_CONSTANT, DAMPNING_CONSTANT);

            //SpringFactory.Instance.CreateAngleSpring(Map.PhysicsSimulator, blob1.Body, blob2.Body, 10.0f, 10.0f);

            float angleLimit = 0.1f;

            JointFactory.Instance.CreateFixedAngleLimitJoint(GameLogic.PhysicsSimulator, blob1.Body, -angleLimit, angleLimit);
            //JointFactory.Instance.CreateFixedAngleJoint(GameLogic.PhysicsSimulator, blob1.Body);
            //JointFactory.Instance.CreateAngleLimitJoint(Map.PhysicsSimulator, blob1.Body, blob2.Body, -angleLimit, angleLimit);
        }

        #region Methods
        public void StopMovement()
        {
            for (int i = 0; i < ListBlob.Count; i++)
            {
                ListBlob[i].Body.ClearImpulse();
                ListBlob[i].Body.ClearTorque();
                ListBlob[i].Body.ClearForce();
                ListBlob[i].Body.LinearVelocity = Vector2.Zero;

            }
        }

        public void UpdateBlobGroup()
        {
            //ListBlob.ForEach(blob => blob.Updateblob());

            for (int i = 0; i < ListBlob.Count; i++)
            {
                ListBlob[i].Updateblob();
            }
        }

        private void ChangeSelection(bool selection)
        {
            for (int i = 0; i < ListBlob.Count; i++)
            {
                ListBlob[i].Selected = selection;
            }
        }

        public void Select()
        {
            ChangeSelection(true);
        }

        public void Deselect()
        {
            ChangeSelection(false);
        }
        #endregion
    }
}
