using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xTile;

namespace TotalBathhouseOverhaul
{
    public class CustomBathhouse : GameLocation
    {
        private static Vector2 SteamPosition;

        private static Texture2D SteamAnimation;

        private static Vector2 fountainPosition = new Vector2(27, 12);

        public CustomBathhouse()
        {
        }

        public CustomBathhouse(Map map, string name) : base(map, name)
        {
        }

        public CustomBathhouse(Map map, string name, Texture2D steam = null) : base(map, name)
        {
            SteamAnimation = steam;
        }

        public override void resetForPlayerEntry()
        {
            base.resetForPlayerEntry();
            Game1.changeMusicTrack("echos");
            //0 is the babbling brook sound
            AmbientLocationSounds.addSound(fountainPosition, 0);

            SteamPosition = new Vector2(0f, 0f);
            SteamAnimation = SteamAnimation ?? Game1.temporaryContent.Load<Texture2D>("LooseSprites\\steamAnimation");
        }

        public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
        {
            base.drawAboveAlwaysFrontLayer(b);
            // End vanilla
            b.End();
            Rectangle viewport = new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height);
            // Custom clamping
            b.GraphicsDevice.ScissorRectangle = Rectangle.Intersect(new Rectangle(15 * Game1.tileSize - Game1.viewport.X, 1 * Game1.tileSize - Game1.viewport.Y, 25 * Game1.tileSize, 20 * Game1.tileSize - 7 * Game1.pixelZoom), viewport);
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, new RasterizerState() { ScissorTestEnable = true });
            for (float num = SteamPosition.X; num < Game1.viewport.Width + Game1.viewport.X + 256f; num += 256f)
            {
                for (float num2 = SteamPosition.Y; num2 < Game1.viewport.Height + Game1.viewport.Y + 128; num2 += 256f)
                {
                    b.Draw(SteamAnimation, new Rectangle((int)Math.Round(num - Game1.viewport.X), (int)Math.Round(num2 - Game1.viewport.Y), 256, 256), null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 1f);
                }
            }
            b.End();
            // Left-door magic
            float px = Game1.tileSize * 2 + 1;
            double pxp = 1 / px;
            int h = 4 * Game1.tileSize - 7 * Game1.pixelZoom;
            int y = 9 * Game1.tileSize - Game1.viewport.Y;
            int x = 15 * Game1.tileSize - Game1.viewport.X;
            for (int c = 1; c < px; c++)
            {
                b.GraphicsDevice.ScissorRectangle = Rectangle.Intersect(new Rectangle(x - c, y, 1, h), viewport);
                b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, new RasterizerState() { ScissorTestEnable = true });
                for (float num = SteamPosition.X; num < Game1.viewport.Width + Game1.viewport.X + 256f; num += 256f)
                {
                    for (float num2 = SteamPosition.Y; num2 < Game1.viewport.Height + Game1.viewport.Y + 128; num2 += 256f)
                    {
                        b.Draw(SteamAnimation, new Rectangle((int)Math.Round(num - Game1.viewport.X), (int)Math.Round(num2 - Game1.viewport.Y), 256, 256), null, Color.White * (float)(pxp * (px - c)), 0f, Vector2.Zero, SpriteEffects.None, 1f);
                    }
                }
                b.End();
            }
            x += (25 * Game1.tileSize) - 1;
            for (int c = 1; c < px; c++)
            {
                b.GraphicsDevice.ScissorRectangle = Rectangle.Intersect(new Rectangle(x + c, y, 1, h), viewport);
                b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, new RasterizerState() { ScissorTestEnable = true });
                for (float num = SteamPosition.X; num < Game1.viewport.Width + Game1.viewport.X + 256f; num += 256f)
                {
                    for (float num2 = SteamPosition.Y; num2 < Game1.viewport.Height + Game1.viewport.Y + 128; num2 += 256f)
                    {
                        b.Draw(SteamAnimation, new Rectangle((int)Math.Round(num - Game1.viewport.X), (int)Math.Round(num2 - Game1.viewport.Y), 256, 256), null, Color.White * (float)(pxp * (px - c)), 0f, Vector2.Zero, SpriteEffects.None, 1f);
                    }
                }
                b.End();
            }
            // Restore vanilla
            Game1.spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
        }

        private bool reverse = false;
        public override void UpdateWhenCurrentLocation(GameTime time)
        {
            base.UpdateWhenCurrentLocation(time);
            SteamPosition.Y = SteamPosition.Y - time.ElapsedGameTime.Milliseconds * 0.1f;
            SteamPosition.Y = SteamPosition.Y % -256f;
            if (SteamPosition.X < -32f || SteamPosition.X > 32f)
                this.reverse = !this.reverse;
            if (this.reverse)
                SteamPosition.X = SteamPosition.X + (float)Math.Sqrt(Math.Pow(time.ElapsedGameTime.Milliseconds * 0.05f, 33 - (SteamPosition.X % 32)));
            else
                SteamPosition.X = SteamPosition.X - (float)Math.Sqrt(Math.Pow(time.ElapsedGameTime.Milliseconds * 0.05f, 33 - (SteamPosition.X % 32)));
        }        
    }
}
