using System;
using System.IO;
using System.Reflection;
using SamuraiServer.Data;

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

            var assembly = Assembly.GetExecutingAssembly();
            var names = assembly.GetManifestResourceNames();
            using (var str = assembly.GetManifestResourceStream("Samurai.Client.Wp7." + mapGuid.ToString() + ".map"))
            using (TextReader tr = new StreamReader(str))
            {
                int stage = 0;
                bool parsing = true;
                int height = 0;
                int y = 0;
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
                            if (y < height)
                                mapTiles[y++] = text;
                            else
                                ++stage;
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
            return map;
        }

        public GameState CreateNewState(Map map, int numAi)
        {
            var gs = new GameState();
            gs.MapId = map.Id;
            gs.Id = Guid.NewGuid();
            gs.Name = "Practice";
            gs.Started = true;
            gs.Turn = 0;

            var human = CreatePlayer("Human");
            gs.Players.Add(human);

            for (int i = 0; i < numAi; i++)
            {
                var ai = CreatePlayer("AI #" + i.ToString());
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

        private GamePlayer CreatePlayer(string playerName)
        {
            var player = new GamePlayer();
            player.Id = Guid.NewGuid();
            player.IsAlive = true;
            player.Player = new Player();
            player.Player.Id = Guid.NewGuid();
            player.Player.Name = playerName;
            player.Score = 0;
            return player;
        }
    }
}
