using System;
using System.IO;
using System.Reflection;
using SamuraiServer.Data;
using System.Collections.Generic;

namespace Samurai.Client.Wp7.Api
{
    public class Singleplayer
    {
        public Map LoadMap(Guid mapGuid)
        {
            string[] mapTiles = null;
            string mapName = "";
            int minPlayers = 0;
            int maxPlayers = 0;
            List<List<Unit>> startingUnits = new List<List<Unit>>();

            var assembly = Assembly.GetExecutingAssembly();
            var names = assembly.GetManifestResourceNames();
            using (var str = assembly.GetManifestResourceStream("Samurai.Client.Wp7." + mapGuid.ToString() + ".map"))
            using (TextReader tr = new StreamReader(str))
            {
                int stage = 0;
                bool parsing = true;
                int height = 0;
                int y = 0;
                int player = 0;
                int unitNum = 0;
                int unitCount = 0;
                while (parsing)
                {
                    var text = tr.ReadLine();
                    parsing = text != null;
                    string[] parts;
                    switch (stage)
                    {
                        case 0:
                            mapName = text;
                            ++stage;
                            break;

                        case 1:
                            parts = text.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            minPlayers = int.Parse(parts[0]);
                            maxPlayers = int.Parse(parts[1]);
                            ++stage;
                            break;

                        case 2:
                            height = int.Parse(text);
                            mapTiles = new string[height];
                            ++stage;
                            break;

                        case 3:
                            mapTiles[y++] = text;
                            if (y == height)
                                ++stage;
                            break;

                        case 4:
                            unitCount = int.Parse(text);
                            unitNum = 0;
                            stage = 5;
                            startingUnits.Add(new List<Unit>());
                            break;

                        case 5:
                            parts = text.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            Unit unit = null;
                            switch (parts[0])
                            {
                                case "samurai":
                                    unit = new SamuraiServer.Data.Samurai();
                                    break;

                                default:
                                    return null;
                            }

                            unit.X = int.Parse(parts[1]);
                            unit.Y = int.Parse(parts[2]);
                            startingUnits[player].Add(unit);

                            ++unitNum;
                            if (unitNum == unitCount)
                            {
                                ++player;
                                stage = player == maxPlayers ? 6 : 4;
                            }
                            break;

                        default:
                            parsing = false;
                            break;
                    }
                }
            }

            if (mapTiles == null)
                return null;

            var map = Map.FromStringRepresentation(mapGuid, mapTiles);
            map.MaxPlayers = minPlayers;
            map.MinPlayers = maxPlayers;
            map.Name = mapName;
            for (int p = 0; p < maxPlayers; p++)
                map.StartingUnits.Add(p, startingUnits[p]);
            return map;
        }

        public GameState CreateNewState(Map map, int numAi)
        {
            int players = numAi + 1;
            if (players < map.MinPlayers || players > map.MaxPlayers)
                return null;

            var gs = new GameState();
            gs.MapId = map.Id;
            gs.Id = Guid.NewGuid();
            gs.Name = "Practice";
            gs.Started = true;
            gs.Turn = 0;

            var human = CreatePlayer("Human", map.StartingUnits[0]);
            gs.Players.Add(human);

            for (int i = 0; i < numAi; i++)
            {
                var ai = CreatePlayer("AI #" + i.ToString(), map.StartingUnits[i + 1]);
                gs.Players.Add(ai);
            }

            int ind = (new Random()).Next(0, gs.Players.Count - 1);
            for (int count = 0; count < gs.Players.Count; count++)
            {
                gs.PlayerOrder.Add(gs.Players[ind].Id);
                ind = (ind + 1) % gs.Players.Count;
            }

            return gs;
        }

        private GamePlayer CreatePlayer(string playerName, List<Unit> startingUnits)
        {
            var player = new GamePlayer();
            player.Id = Guid.NewGuid();
            player.IsAlive = true;
            player.Player = new Player();
            player.Player.Id = Guid.NewGuid();
            player.Player.Name = playerName;
            player.Score = 0;
            for (int i = 0; i < startingUnits.Count; i++)
                player.Units.Add(startingUnits[i]);
            return player;
        }
    }
}
