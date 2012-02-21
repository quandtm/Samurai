using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SamuraiServer.Data;

namespace Samurai.Client.Wp7.Graphics
{
    public class Renderer
    {
        private const int CellWidth = 64;
        private readonly List<Texture2D> textures = new List<Texture2D>();
        private Rectangle drawRect = new Rectangle(0, 0, CellWidth, CellWidth);

        public void LoadContent(ContentManager content)
        {
            textures.Clear();
            textures.Add(content.Load<Texture2D>("Textures\\grass"));
            textures.Add(content.Load<Texture2D>("Textures\\rock"));
            textures.Add(content.Load<Texture2D>("Textures\\trees"));
            textures.Add(content.Load<Texture2D>("Textures\\water"));

            textures.Add(content.Load<Texture2D>("Textures\\samurai_red_64"));
        }

        public void DrawMap(GraphicsDevice device, SpriteBatch sb, Map map, GameState state, int xOffset, int yOffset)
        {
            if (sb == null || map == null)
                return;

            int xStart = xOffset % CellWidth;
            int yStart = yOffset % CellWidth;
            int width = Math.Min((xOffset / CellWidth) + (device.Viewport.Width / CellWidth) + 2, map.Tiles.Length);
            int height = Math.Min((yOffset / CellWidth) + (device.Viewport.Height / CellWidth) + 2, map.Tiles[0].Length); // All columns are of equal height

            sb.Begin();
            drawRect.X = -xStart;
            for (int xIndex = xOffset / CellWidth; xIndex < width && xIndex >= 0; ++xIndex)
            {
                drawRect.Y = -yStart;
                for (int yIndex = yOffset / CellWidth; yIndex < height && yIndex >= 0; ++yIndex)
                {
                    var tex = GetTex(map.Tiles[xIndex][yIndex]);
                    if (tex != null)
                        sb.Draw(tex, drawRect, Color.White);
                    drawRect.Y += CellWidth;
                }
                drawRect.X += CellWidth;
            }
            sb.End();

            sb.Begin();
            for (int p = 0; p < state.Players.Count; p++)
            {
                for (int u = 0; u < state.Players[p].Units.Count; u++)
                {
                    var unit = state.Players[p].Units[u];
                    if (unit.CurrentHitPoints > 0)
                    {
                        if (unit.X >= (xOffset / CellWidth) && unit.Y >= (yOffset / CellWidth) && unit.X < width && unit.Y < height)
                        {
                            drawRect.Y = -yStart + ((unit.Y - (yOffset / CellWidth)) * CellWidth);
                            drawRect.X = -xStart + ((unit.X - (xOffset / CellWidth)) * CellWidth);
                            var tex = GetUnitTex(unit);
                            if (tex != null)
                                sb.Draw(tex, drawRect, Color.White);
                        }
                    }
                }
            }
            sb.End();
        }

        public Point GetMapSize(Map map)
        {
            var x = map.Tiles.Length;
            var y = map.Tiles[0].Length;
            return new Point(x * CellWidth, y * CellWidth);
        }

        protected Texture2D GetTex(TileType cell)
        {
            switch (cell.Name)
            {
                case "Grass":
                    return textures[0];
                case "Rock":
                    return textures[1];
                case "Tree":
                    return textures[2];
                case "Water":
                    return textures[3];

                default:
                    return null;
            }
        }

        protected Texture2D GetUnitTex(Unit unit)
        {
            switch (unit.ImageSpriteResource)
            {
                case "samurai_red_64":
                    return textures[4];

                default:
                    return null;
            }
        }
    }
}
