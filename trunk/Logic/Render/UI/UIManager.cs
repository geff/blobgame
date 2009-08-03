using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BlobGame.Logic.Render.UI
{
    public class UIManager : UIComponent
    {
        public GameEngine gameEngine { get; set; }
        public List<UIComponent> ListUIComponent { get; set; }
        public bool IsMouseCaught { get; set; }

        public UIManager(GameEngine gameEngine)
        {
            this.gameEngine = gameEngine;
            this.ListUIComponent = new List<UIComponent>();
        }

        public override void Initialize(UIManager UIManager)
        {
            //--- Init MiniMap
            MiniMap miniMap = new MiniMap(this.gameEngine);
            miniMap.Initialize(this);

            ListUIComponent.Add(miniMap);
            //---
        }

        public override void Update(GameTime gameTime)
        {
            IsMouseCaught = false;

            for (int i = 0; i < ListUIComponent.Count&& !IsMouseCaught; i++)
            {
                ListUIComponent[i].Update(gameTime);
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            for (int i = 0; i < ListUIComponent.Count; i++)
            {
                ListUIComponent[i].Draw(gameTime, spriteBatch);
            }
        }
    }
}
