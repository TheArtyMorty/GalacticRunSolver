using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using WPF_GalacticRunSolver.Model;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using WPF_GalacticRunSolver.ViewModel;

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

        public static bool IsValidMapUrl(string url)
        {
            var splittedUrl = url.Split('/');
            if (splittedUrl.Length == 1)
            {
                return false;
            }
            else
            {
                try
                {
                    var id = splittedUrl.Last();
                    var boardAsString = GalacticRunBoardFromUrl.GetBoardFromBoardID(id).GetString();
                }
                catch(Exception)
                {
                    return false;
                }
            }
            return true;
        }


        private static RobotViewModel GetInitialRobotFromMap(MapViewModel map)
        {
            return map._Robots.Where(r => r._Color == map._Target._Color).First();
        }

        private static void TabToProperRobot(FirefoxWrapper firefox, MapViewModel map, EColor targetColor, ref RobotViewModel currentRobot)
        {
            List<RobotViewModel> tabOrderedRobots = map._Robots.ToList();
            tabOrderedRobots.Sort();

            int currentPos = 0;
            int targetPos = 0;
            int i = 0;
            foreach (RobotViewModel robot in tabOrderedRobots)
            {
                if (robot._Color == targetColor) targetPos = i;
                if (robot._Color == currentRobot._Color) currentPos = i;
                i++;
            }

            for (int j = 0; j< (targetPos - currentPos + 4)%4; j++)
            {
                firefox.SendKey(0x09); // tab
                Thread.Sleep(10);
            }

            currentRobot = tabOrderedRobots[targetPos];
        }

        public async static void SendSolutionToFirefox(SolutionViewModel solution, MapViewModel map)
        {
            FirefoxWrapper firefox = new FirefoxWrapper();
            firefox.SendKey(0x20); // space

            RobotViewModel currentRobot = GetInitialRobotFromMap(map);

            foreach (MoveViewModel move in solution._Moves)
            {
                if (currentRobot._Color != (EColor)move._Color)
                {
                    TabToProperRobot(firefox, map, (EColor)move._Color, ref currentRobot);
                }

                switch(move._Direction)
                {
                    case CLI.EMoveDirection.Left:
                        firefox.SendKey(0x25);
                        break;
                    case CLI.EMoveDirection.Up:
                        firefox.SendKey(0x26);
                        break;
                    case CLI.EMoveDirection.Right:
                        firefox.SendKey(0x27);
                        break;
                    case CLI.EMoveDirection.Down:
                        firefox.SendKey(0x28);
                        break;
                }

                await map.PlayMove(move, 0);

                Thread.Sleep(10);
            }
        }
    }

    public class FirefoxWrapper
    {
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        // the keystroke signals. you can look them up at the msdn pages
        private static uint WM_KEYDOWN = 0x100, WM_KEYUP = 0x101;

        // the reference to the firefox process
        private Process firefoxProcess;

        public FirefoxWrapper()
        {
            Process[] firefoxProcesses = Process.GetProcessesByName("firefox");
            foreach (Process firefox in firefoxProcesses)
            {
                if (firefox.MainWindowHandle == IntPtr.Zero)// the firefox process must have a window
                    continue;
                firefoxProcess = firefox; //now you have a handle to the main firefox (either a new one or the one that was already open).
                return;
            }
        }

        public void SendKey(int key)
        {
            try
            {
                if (firefoxProcess.MainWindowHandle != IntPtr.Zero)
                {
                    // send the keydown signal
                    SendMessage(firefoxProcess.MainWindowHandle, WM_KEYDOWN, (IntPtr)key, IntPtr.Zero);
                    // give the process some time to "realize" the keystroke
                    Thread.Sleep(30); //On my system it works fine without this Sleep.
                                      // send the keyup signal
                    SendMessage(firefoxProcess.MainWindowHandle, WM_KEYUP, (IntPtr)key, IntPtr.Zero);
                }
            }
            catch (Exception) //without the GetProcessesByName you'd get an exception.
            {
            }
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
