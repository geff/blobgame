using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BlobGame.Render
{
    public struct VertexBlob
    {
        public Vector3 Position;
        public Color Color;
        public float Size;
        public float Polarity;
        public float Selected;
        public Color PlayerColor;

        public VertexBlob(Vector3 position, Color color, float size, float polarity, float selected, Color playerColor)
        {
            this.Position = position;
            this.Color = color;
            this.Size = size;
            this.Polarity = polarity;
            this.Selected = selected;
            this.PlayerColor = playerColor;
        }

        public static VertexElement[] VertexElements =
             {
                 new VertexElement(0, 0, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Position, 0),
                 new VertexElement(0, sizeof(float)*3, VertexElementFormat.Color, VertexElementMethod.Default, VertexElementUsage.Color, 0),
                 new VertexElement(0, sizeof(float)*4, VertexElementFormat.Single, VertexElementMethod.Default, VertexElementUsage.PointSize, 0),
                 new VertexElement(0, sizeof(float)*5, VertexElementFormat.Single, VertexElementMethod.Default, VertexElementUsage.PointSize, 1),
                 new VertexElement(0, sizeof(float)*6, VertexElementFormat.Single, VertexElementMethod.Default, VertexElementUsage.PointSize, 2),
                 new VertexElement(0, sizeof(float)*7, VertexElementFormat.Color, VertexElementMethod.Default, VertexElementUsage.Color, 1)
             };

        public static int SizeInBytes = 28 ;//sizeof(float) * (2+1+2);
    }
}
