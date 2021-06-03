using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using WPF_GalacticRunSolver.Model;

namespace WPF_GalacticRunSolver.Utility
{
    public static class Utilities
    {
        private static EWallType StringToWallType(string t)
        {
            if (t == "NE") return EWallType.TopRight;
            if (t == "SE") return EWallType.BottomRight;
            if (t == "SW") return EWallType.BottomLeft;
            if (t == "NW") return EWallType.TopLeft;
            return EWallType.None;
        }
        private static Position GetPositionFromCellId(int id, int mapsize)
        {
            int y = id / mapsize;
            int x = id - y * mapsize;
            return new Position(x, y);
        }
        private static Map GetMapFromUrlBoardString(string board)
        {
            int mapsize = 16;
            Map map = new Map(mapsize);
            //splitboard
            string[] splittedBoard = board.Split(' ');
            //target and robots
            map._Target._Position = GetPositionFromCellId(Int16.Parse(splittedBoard[0]), mapsize);
            map._Target._Color = (EColor)Int16.Parse(splittedBoard[1]);
            for (int i = 0; i < 4; i++)
            {
                map._Robots[i]._Position = GetPositionFromCellId(Int16.Parse(splittedBoard[i+2]), mapsize);
            }
            //walls
            string[] walls = splittedBoard[6].Split('-');
            foreach (string wall in walls)
            {
                int cellID = Int16.Parse(wall.Split(':').First());
                var position = GetPositionFromCellId(cellID, mapsize);
                string walltype = wall.Split(':').Last();
                map._Cases[position.Y][position.X]._WallType = StringToWallType(walltype);
            }
            return map;
        }

        public static Map GetMapFromWeburl(string url)
        {
            var id = url.Split('/').Last();
            var boardAsString = GalacticRunBoardFromUrl.GetBoardFromBoardID(id).GetString();
            return GetMapFromUrlBoardString(boardAsString);
        }
    }

    public static class GalacticRunBoardFromUrl
    {
        private static readonly HttpClient client = new HttpClient();

        public static JsonElement GetBoardFromBoardID(string id)
        {
            HttpContent c = new StringContent("");
            var t = Task.Run(() => PostURI(new Uri("https://identitytoolkit.googleapis.com/v1/accounts:signUp?key=AIzaSyCx4Ea4ZOS8_XaEodY9Eckcom2uKOhObFI"), c));
            t.Wait();

            JsonDocument doc = JsonDocument.Parse(t.Result);
            JsonElement root = doc.RootElement;

            var idToken = root.GetProperty("idToken");

            t = Task.Run(() => GetURI(new Uri("https://galactic-run.firebaseio.com/solutions/" + id + ".json?auth=" + idToken)));
            t.Wait();
            doc = JsonDocument.Parse(t.Result);
            root = doc.RootElement;
            var board = root.GetProperty("board");
            return board;
        }

        static async Task<string> PostURI(Uri u, HttpContent c)
        {
            var response = string.Empty;
            using (var client = new HttpClient())
            {
                HttpResponseMessage result = await client.PostAsync(u, c);
                if (result.IsSuccessStatusCode)
                {
                    response = await result.Content.ReadAsStringAsync();
                }
            }
            return response;
        }

        static async Task<string> GetURI(Uri u)
        {
            var response = string.Empty;
            using (var client = new HttpClient())
            {
                HttpResponseMessage result = await client.GetAsync(u);
                if (result.IsSuccessStatusCode)
                {
                    response = await result.Content.ReadAsStringAsync();
                }
            }
            return response;
        }
    }
}
