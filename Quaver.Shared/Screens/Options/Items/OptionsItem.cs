using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Quaver.Shared.Assets;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Managers;
using ColorHelper = Quaver.Shared.Helpers.ColorHelper;

namespace Quaver.Shared.Screens.Options.Items
{
    public class OptionsItem : Sprite
    {
        /// <summary>
        /// </summary>
        protected SpriteTextPlus Name { get; set; }

        /// <summary>
        /// </summary>
        protected RectangleF ContainerRectangle { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="containerRect"></param>
        /// <param name="name"></param>
        public OptionsItem(RectangleF containerRect, string name)
        {
            ContainerRectangle = containerRect;

            Image = AssetLoader.LoadTexture2DFromFile(@"C:\users\swan\desktop\options-item-bg.png");
            Size = new ScalableVector2(containerRect.Width * 0.96f, 54);

            Tint = ColorHelper.HexToColor("#242424");
            CreateName(name);

            UsePreviousSpriteBatchOptions = true;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            var color = IsHovered() ? ColorHelper.HexToColor("#3F3F3F") : ColorHelper.HexToColor("#242424");;

            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;
            FadeToColor(color, dt, 20);

            // Set visibility based on if the options item is visible inside of the container.
            // Helps to raise FPS by not drawing unnecessary items
            if (Parent is Drawable contentContainer && contentContainer.Parent is ScrollContainer container)
                Visible = !RectangleF.Intersect(ScreenRectangle, container.ScreenRectangle).IsEmpty;
            else
                Visible = true;

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }

        /// <summary>
        /// </summary>
        /// <param name="name"></param>
        private void CreateName(string name)
        {
            Name = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), name, 21)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = 16,
                UsePreviousSpriteBatchOptions = true,
            };
        }
    }
}