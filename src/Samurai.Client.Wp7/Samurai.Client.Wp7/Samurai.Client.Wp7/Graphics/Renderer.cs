using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SamuraiServer.Data;
using Samurai.Client.Wp7.Api;

namespace Samurai.Client.Wp7.Graphics
{
    public class Renderer
    {
        public const int CellWidth = 64;
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

        public void DrawMap(GraphicsDevice device, SpriteBatch sb, Map map, GameState state, int xOffset, int yOffset, Unit selectedUnit, List<MoveIntent> intents)
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

            var origin = new Vector2(CellWidth / 2, CellWidth / 2);
            sb.Begin();
            for (int p = 0; p < state.Players.Count; p++)
            {
                if (state.Players[p].IsAlive)
                {
                    for (int u = 0; u < state.Players[p].Units.Count; u++)
                    {
                        var unit = state.Players[p].Units[u];
                        if (unit.CurrentHitPoints > 0)
                        {
                            if (unit.X >= (xOffset / CellWidth) && unit.Y >= (yOffset / CellWidth) && unit.X < width && unit.Y < height)
                            {
                                DrawUnit(sb, xOffset, yOffset, selectedUnit, xStart, yStart, ref origin, unit, unit.X, unit.Y, Color.White);
                            }
                        }
                    }
                }
            }
            sb.End();

            if (intents.Count > 0)
            {
                sb.Begin();
                for (int i = 0; i < intents.Count; i++)
                    DrawUnit(sb, xOffset, yOffset, null, xStart, yStart, ref origin, intents[i].Unit, intents[i].X, intents[i].Y, Color.Gray);
                sb.End();
            }
        }

        private void DrawUnit(SpriteBatch sb, int xOffset, int yOffset, Unit selectedUnit, int xStart, int yStart, ref Vector2 origin, Unit unit, int unitX, int unitY, Color tint)
        {
            drawRect.Y = -yStart + ((unitY - (yOffset / CellWidth)) * CellWidth) + (int)origin.X;
            drawRect.X = -xStart + ((unitX - (xOffset / CellWidth)) * CellWidth) + (int)origin.X;
            var tex = GetUnitTex(unit);
            if (tex != null)
            {
                if (selectedUnit == unit)
                {
                    drawRect.Width = (int)(drawRect.Width * 1.2f);
                    drawRect.Height = (int)(drawRect.Height * 1.2f);
                }
                sb.Draw(tex, drawRect, null, tint, 0, origin, SpriteEffects.None, 0);
                if (selectedUnit == unit)
                {
                    drawRect.Width = CellWidth;
                    drawRect.Height = CellWidth;
                }
            }
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
