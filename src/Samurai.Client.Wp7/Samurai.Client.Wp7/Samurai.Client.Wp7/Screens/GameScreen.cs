using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Samurai.Client.Wp7.Api;
using Samurai.Client.Wp7.Graphics;
using SamuraiServer.Data;

namespace Samurai.Client.Wp7.Screens
{
    public class GameScreen : BaseScreen
    {
        private ContentManager content;
        private SpriteBatch sb;
        private Renderer renderer;

        // For map scrolling
        private int xOffset = 0;
        private int yOffset = 0;
        private Vector2 prevPos = Vector2.Zero;

        private Map map;
        private Player player;
        private GamePlayer gamePlayer;
        private GameState game;
        private ServerApi api;
        private bool isPractice;
        private Unit selectedUnit = null;
        private readonly List<MoveIntent> intents = new List<MoveIntent>();

        public GameScreen()
            : base()
        {
            renderer = new Renderer();
        }

        public void Init(ServerApi api, Player player, GameState game, Map map)
        {
            this.api = api;
            this.player = player;
            this.game = game;
            this.map = map;
            this.isPractice = false;
        }

        public void InitPractice(Map map, GameState game)
        {
            this.map = map;
            this.game = game;
            this.gamePlayer = game.Players[0]; // We know that player 0 is human for single player games
            this.player = gamePlayer.Player;
            this.isPractice = true;
        }

        public override void LoadContent()
        {
            if (IsReady)
                return;

            content = new ContentManager(Manager.Game.Services, "Content");
            sb = new SpriteBatch(Manager.GraphicsDevice);

            Manager.Jobs.CreateJob(
                () =>
                {
                    renderer.LoadContent(content);

                    // This indicates that the screen has finished loading and can be displayed without issues
                    IsReady = true;
                });
            base.LoadContent();
        }

        public override void UnloadContent()
        {
            content.Unload();
            content.Dispose();
            sb.Dispose();
            base.UnloadContent();
        }

        public override void Update(double elapsedSeconds)
        {
            if (GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.Back))
                Manager.TransitionTo<MainMenuScreen>();

            var touches = TouchPanel.GetState();
            if (touches.Count > 0)
            {
                var mapSize = renderer.GetMapSize(map);
                if (touches[0].State == TouchLocationState.Pressed)
                    prevPos = touches[0].Position;
                else if (touches[0].State == TouchLocationState.Moved)
                {
                    if (mapSize.X > Manager.GraphicsDevice.Viewport.Width)
                    {
                        xOffset -= (int)(touches[0].Position.X - prevPos.X);
                        if (xOffset < 0)
                            xOffset = 0;
                        else if (xOffset >= (mapSize.X - Manager.GraphicsDevice.Viewport.Width))
                            xOffset = mapSize.X - Manager.GraphicsDevice.Viewport.Width;
                    }

                    if (mapSize.Y > Manager.GraphicsDevice.Viewport.Height)
                    {
                        yOffset -= (int)(touches[0].Position.Y - prevPos.Y);
                        if (yOffset < 0)
                            yOffset = 0;
                        else if (yOffset >= (mapSize.Y - Manager.GraphicsDevice.Viewport.Height))
                            yOffset = mapSize.Y - Manager.GraphicsDevice.Viewport.Height;
                    }
                    prevPos = touches[0].Position;
                }
            }

            // Handle Gestures
            while (TouchPanel.IsGestureAvailable)
                HandleGesture();

            base.Update(elapsedSeconds);
        }

        private void HandleGesture()
        {
            var gesture = TouchPanel.ReadGesture();
            if (gesture.GestureType != GestureType.Tap)
                return;

            if (selectedUnit == null)
            {
                // Figure out which unit was selected (if any)
                GamePlayer p;
                GetTappedUnit(out selectedUnit, gesture.Position, out p);
                if (p != gamePlayer)
                    selectedUnit = null;
            }
            else
            {
                // Process move
                Unit target;
                GamePlayer p;
                if (GetTappedUnit(out target, gesture.Position, out p))
                {
                    // Change selection if player taps another of their units, or deselect if they tap the same unit
                    if (target == selectedUnit)
                    {
                        // If select unit is re-tapped, deselect and cancel any pending moves for that unit
                        int removeIndex;
                        if (HasExistingIntent(selectedUnit, out removeIndex))
                            intents.RemoveAt(removeIndex);
                        selectedUnit = null;
                    }
                    else if (p == gamePlayer)
                        selectedUnit = target;
                    else
                    {
                        // Must be enemy unit, attack!
                    }
                }
                else
                {
                    int x, y;
                    GetCellLocation(gesture.Position, out x, out y);
                    if (x >= 0 && x < map.Tiles.Length && y >= 0 && y < map.Tiles[0].Length)
                    {
                        // Determine path
                        // Validate distance vs # moves
                        // Move unit if move is valid
                        // TODO: Add proper path planning to determine distance
                        int distance = Math.Abs(x - selectedUnit.X) + Math.Abs(y - selectedUnit.Y);
                        if (distance > selectedUnit.Moves)
                            selectedUnit = null;
                        else
                        {
                            int removeIndex;
                            if (HasExistingIntent(selectedUnit, out removeIndex))
                                intents.RemoveAt(removeIndex);
                            intents.Add(Singleplayer.IntendMove(selectedUnit, x, y));
                            selectedUnit = null;
                        }
                    }
                    else
                        selectedUnit = null;
                }
            }
        }

        private bool HasExistingIntent(Unit unit, out int index)
        {
            index = -1;
            for (int i = 0; i < intents.Count; i++)
            {
                if (intents[i].Unit == unit)
                {
                    index = i;
                    return true;
                }
            }
            return false;
        }

        private void GetCellLocation(Vector2 position, out int x, out int y)
        {
            position.X += xOffset;
            position.Y += yOffset;
            x = (int)(position.X / Renderer.CellWidth);
            y = (int)(position.Y / Renderer.CellWidth);
        }

        private bool GetTappedUnit(out Unit selectedUnit, Vector2 position, out GamePlayer player)
        {
            selectedUnit = null;
            player = null;

            int x, y;
            GetCellLocation(position, out x, out y);

            for (int p = 0; p < game.Players.Count; p++)
            {
                if (game.Players[p].IsAlive)
                {
                    for (int u = 0; u < game.Players[p].Units.Count; u++)
                    {
                        var unit = game.Players[p].Units[u];
                        if (unit.X == x && unit.Y == y)
                        {
                            selectedUnit = unit;
                            player = game.Players[p];
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public override void Draw(double elapsedSeconds, GraphicsDevice device)
        {
            if (renderer == null)
                return;

            renderer.DrawMap(device, sb, map, game, xOffset, yOffset, selectedUnit, intents);

            base.Draw(elapsedSeconds, device);
        }

        public override void OnNavigatedFrom()
        {
            base.OnNavigatedFrom();
        }

        public override void OnNavigatedTo()
        {
            TouchPanel.EnabledGestures = GestureType.Tap;
            base.OnNavigatedTo();
        }
    }
}
