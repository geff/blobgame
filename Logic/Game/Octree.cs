using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace BlobGame
{
    public class Octree
    {
        public Octree Parent { get; set; }
        public List<Octree> ListChildOctree { get; set; }

        public float X { get; set; }
        public float Y { get; set; }

        public Vector2 Position { get; set; }
        public Vector2 Size { get; set; }

        public float Width { get; set; }
        public float Height { get; set; }

        public float ChildWidth { get; set; }
        public float ChildHeight { get; set; }

        public int NbOctreeV {get;set;}
        public int NbOctreeH {get;set;}

        public List<BlobEntity> ListBlobEntity { get; set; }

        protected RectangleF Rectangle { get; set; }

        public Octree(float width, float height, int subX, int subY)
        {
            Constructor(null, 0, 0, width, height);

            this.ChildWidth = width / (float)subX;
            this.ChildHeight = height / (float)subY;

            this.NbOctreeH = subX;
            this.NbOctreeV = subY;

            InitChildOctrees(subX, subY);
        }

        public Octree(Octree octreeParent, float posX, float posY, float width, float height)
        {
            Constructor(octreeParent, posX, posY, width, height);
        }

        private void Constructor(Octree octreeParent, float posX, float posY, float width, float height)
        {
            this.ListChildOctree = new List<Octree>();
            this.ListBlobEntity = new List<BlobEntity>();
            this.Parent = octreeParent;

            this.X = posX;
            this.Y = posY;
            this.Width = width;
            this.Height = height;

            this.ChildWidth = 0f;
            this.ChildHeight = 0f;

            this.Position = new Vector2(posX, posY);
            this.Size = new Vector2(width, height);

            this.Rectangle = new RectangleF(this.X, this.Y, this.Width, this.Height);
        }

        private void InitChildOctrees(int subX, int subY)
        {
            for (int i = 0; i < subX; i++)
            {
                for (int j = 0; j < subY; j++)
                {
                    Octree childOctree = new Octree(this, (float)i * this.ChildWidth, (float)j * this.ChildHeight, this.ChildWidth, this.ChildHeight);

                    this.ListChildOctree.Add(childOctree);
                }
            }
        }

        public List<Octree> GetOctrees(Vector2 position, float length)
        {
            //List<Octree> listOctree = new List<Octree>();

            return ListChildOctree.FindAll(octree => (octree.Position - position).Length() <= length || (octree.Position + octree.Size - position).Length() <= length);

            //return listOctree;
        }

        public Octree GetOctree(Vector2 position)
        {
            return ListChildOctree.Find(octree => octree.Rectangle.Contains(position));
        }

        public List<BlobEntity> GetBlobEntity(RectangleF rectangle)
        {
            /*
            for (int i = 0; i < ListChildOctree.Count; i++)
            {
                for (int j = 0; j <  ListChildOctree[i].ListBlobGroup.Count; j++)
                {
                    
                }
            }
             * */
            List<BlobEntity> listBlobEntity = new List<BlobEntity>();

            List<Octree> listOctree = ListChildOctree.FindAll(octree => octree.Rectangle.Intersect(rectangle) || octree.Rectangle.Contains(rectangle));

            for (int i = 0; i < listOctree.Count; i++)
            {
                listBlobEntity.AddRange(listOctree[i].ListBlobEntity);
            }

            return listBlobEntity;
        }
    }
}
