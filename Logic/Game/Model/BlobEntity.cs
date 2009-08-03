using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using BlobGame.Render;
using FarseerGames.FarseerPhysics.Collisions;
using BlobGame.Logic.Game;

namespace BlobGame
{
    public class BlobEntity
    {
        public Vector2 _position = Vector2.Zero;
        public Vector2 Position
        {
            get
            {
                if (Body == null)
                    return _position;

                return Body.Position;
            }
            set
            {
                if (Body == null)
                    _position = value;
                else
                {
                    Body.Position = value;
                }
            }
        }

        public Vector2 Vector { get; set; }
        public float Size { get; set; }
        public Color Color { get; set; }
        public float Polarity { get; set; }

        private bool _selected = false;
        public bool Selected 
        { 
            get
            {
                return this._selected;
            }
            set
            {
                this._selected = value;
                this.VertexBlob.Selected = (value? 1f:0f);
            }
        }
        public BlobGroup BlobGroupParent { get; set; }

        public VertexBlob VertexBlob;

        public Octree ParentOctree { get; set; }

        #region PhysicProperties
        public Body Body { get; set; }
        public Geom Geom { get; set; }
        //private static Body bodyTemplate = BodyFactory.Instance.CreateRectangleBody(Map.PhysicsSimulator, 2f / 60f, 2f / 60f, 0.7f);
        private static Body bodyTemplate = BodyFactory.Instance.CreateCircleBody(GameLogic.PhysicsSimulator, 0.5f, 10f);
        //private static Body bodyTemplate = BodyFactory.Instance.CreateRectangleBody(Map.PhysicsSimulator, 0.5f, 0.5f, 10f);
        #endregion

        public void Updateblob()
        {
            VertexBlob.Position = new Vector3(Body.Position, 0);
            //VertexBlob.Position = new Vector3(Body.Position, 0);
        }

        public BlobEntity(BlobGroup blobGroupParent, Vector2 position, float size, Color color, float polarity)
        {
            this.Position = position;
            this.Size = size;
            this.Color = color;
            this.Polarity = polarity;
            this.Vector = new Vector2(0, 0);
            this.BlobGroupParent = blobGroupParent;

            Body = BodyFactory.Instance.CreateBody(GameLogic.PhysicsSimulator, BlobEntity.bodyTemplate);
            Body.Position = position;


            Body.RotationalDragCoefficient = 0.8f;
            Body.IgnoreGravity = false;

            //Geom = GeomFactory.Instance.CreateRectangleGeom(Map.PhysicsSimulator, Body,
            //                                                 2f, 2f);

            //Body.MomentOfInertia = 0f;
            Body.Mass = 10f;

            Geom = GeomFactory.Instance.CreateCircleGeom(GameLogic.PhysicsSimulator, Body, 0.5f, 5, 0.0f);

            //Geom = GeomFactory.Instance.CreateRectangleGeom(Map.PhysicsSimulator, Body, 0.5f,0.5f);
                                                  //0.8f, 12, new Vector2(-40, -40), 0f, 0.1f);
            Geom.FrictionCoefficient = 0.0001f;
            Geom.RestitutionCoefficient = 0.000001f;


            Geom.CollisionGroup = 1;
            Geom.CollisionCategories = FarseerGames.FarseerPhysics.CollisionCategory.Cat1;
            Geom.CollidesWith = FarseerGames.FarseerPhysics.CollisionCategory.All;

            //Body.AngularVelocity = 0.0f;
            

            VertexBlob = new VertexBlob(new Vector3(Position, 0), Color, Size, Polarity, 0f, BlobGroupParent.PlayerColor);
        }

        public void Select()
        {
            BlobGroupParent.Select();
        }

        public void Deselect()
        {
            BlobGroupParent.Deselect();
        }
    }
}
