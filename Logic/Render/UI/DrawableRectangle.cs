using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BlobGame.Logic.Render.UI
{
    public class DrawableRectangle
    {
        public Rectangle rec { get; set; }

        private Rectangle recTop = new Rectangle();
        private Rectangle recLeft = new Rectangle();
        private Rectangle recRight = new Rectangle();
        private Rectangle recBottom = new Rectangle();

        public DrawableRectangle(Rectangle rectangle)
        {
            this.rec = rectangle;
        }

        public DrawableRectangle(RectangleF rectangle, Point location)
        {
            SetRectangle(rectangle, location, 1f);
        }

        public void SetRectangle(RectangleF rectangle, Point location, float ratio)
        {
            this.rec = new Rectangle((int)(rectangle.X *ratio) + location.X, (int)(rectangle.Y*ratio) + location.Y, (int)(rectangle.Width*ratio), (int)(rectangle.Height*ratio));
        }

        public void Init(int inflateSize, int borderSize)
        {
            recTop = new Rectangle(rec.Left - inflateSize, rec.Top - inflateSize, rec.Width + 2 * inflateSize, borderSize);
            recLeft = new Rectangle(rec.Left - inflateSize, rec.Top - inflateSize, borderSize, rec.Height + 2 * inflateSize);
            recRight = new Rectangle(rec.Left + rec.Width + inflateSize, rec.Top - inflateSize, borderSize, rec.Height + 2 * inflateSize);
            recBottom = new Rectangle(rec.Left - inflateSize, rec.Top + rec.Height + inflateSize, rec.Width + 2 * inflateSize + borderSize, borderSize);
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D texBlank, Color color)
        {
            spriteBatch.Draw(texBlank, recTop, color);
            spriteBatch.Draw(texBlank, recLeft, color);
            spriteBatch.Draw(texBlank, recRight, color);
            spriteBatch.Draw(texBlank, recBottom, color);
        }
    }
}
