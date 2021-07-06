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
using System.Drawing;

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

        private static bool AreColorsEqual(Color c1, Color c2)
        {
            if (c1.ToArgb() == c2.ToArgb()) return true;
            int result = 0;
            result += Math.Abs(c2.A - c1.A);
            result += Math.Abs(c2.R - c1.R);
            result += Math.Abs(c2.G - c1.G);
            result += Math.Abs(c2.B - c1.B);
            return result < 75 ;
        }

        public static Map GetMapFromImage(Bitmap image)
        {
            int size = 16;
            Map result = new Map(size);
            
            Bitmap mapImage = GetCroppedImageFromImage(image);
            int width = (mapImage.Width-1) / size;
            int height = width;

            Color wall = Color.White;
            Color redRobot = Color.FromArgb(255,116,32,65); //Color.FromArgb(255, 191, 35, 91);
            Color greenRobot = Color.FromArgb(255, 41, 141, 103); //Color.FromArgb(255, 67, 217, 154);
            Color blueRobot = Color.FromArgb(255, 22, 65, 119); //Color.FromArgb(255, 36, 96, 185);
            Color yellowRobot = Color.FromArgb(255, 117, 102, 46); //Color.FromArgb(255, 195, 155, 62);

            Color HredRobot = Color.FromArgb(255, 191, 35, 91);
            Color HgreenRobot = Color.FromArgb(255, 67, 217, 154);
            Color HblueRobot = Color.FromArgb(255, 36, 96, 185);
            Color HyellowRobot = Color.FromArgb(255, 195, 155, 62);

            Color redTarget = Color.FromArgb(255, 255, 68, 135);
            Color greenTarget = Color.FromArgb(255, 100, 249, 186);
            Color blueTarget = Color.FromArgb(255, 100, 160, 251);
            Color yellowTarget = Color.FromArgb(255, 255, 192, 68);

            for (int i = 0; i< size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    bool leftWall = false;
                    bool rightWall = false;
                    bool topWall = false;
                    bool bottomWall = false;

                    var pixel = mapImage.GetPixel(j*width, i * height + height / 2);
                    var nextpixel = mapImage.GetPixel(j*width+1, i * height + height / 2);
                    var previouspixel = pixel;
                    if (j > 0) previouspixel = mapImage.GetPixel(j*width-1, i * height + height / 2);
                    if (AreColorsEqual(pixel, wall) || AreColorsEqual(nextpixel, wall) || AreColorsEqual(previouspixel, wall))
                    {
                        leftWall = true;
                    }

                    pixel = mapImage.GetPixel((j+1) * width, i * height + height / 2);
                    if (j < size-1) nextpixel = mapImage.GetPixel((j + 1) * width + 1, i * height + height / 2);
                    else nextpixel = pixel;
                    previouspixel = mapImage.GetPixel((j+1) * width - 1, i * height + height / 2);
                    if (AreColorsEqual(pixel, wall) || AreColorsEqual(nextpixel, wall) || AreColorsEqual(previouspixel, wall))
                    {
                        rightWall = true;
                    }
                    
                    pixel = mapImage.GetPixel(j*width + width / 2, i * height);
                    nextpixel = mapImage.GetPixel(j * width + width / 2, i * height + 1);
                    previouspixel = pixel;
                    if (i > 0) previouspixel = mapImage.GetPixel(j * width + width / 2, i * height - 1);
                    if (AreColorsEqual(pixel, wall) || AreColorsEqual(nextpixel, wall) || AreColorsEqual(previouspixel, wall))
                    {
                        topWall = true;
                    }

                    pixel = mapImage.GetPixel(j * width + width / 2, (i+1) * height);
                    if (i < size-1) nextpixel = mapImage.GetPixel(j * width + width / 2, (i+1) * height + 1);
                    else nextpixel = pixel;
                    previouspixel = mapImage.GetPixel(j * width + width / 2, (i + 1) * height - 1);
                    if (AreColorsEqual(pixel, wall) || AreColorsEqual(nextpixel, wall) || AreColorsEqual(previouspixel, wall))
                    {
                        bottomWall = true;
                    }

                    if (leftWall && topWall)
                    {
                        result._Cases[i][j]._WallType = EWallType.TopLeft;
                    }
                    else if (leftWall && bottomWall)
                    {
                        result._Cases[i][j]._WallType = EWallType.BottomLeft;
                    }
                    else if (rightWall && topWall)
                    {
                        result._Cases[i][j]._WallType = EWallType.TopRight;
                    }
                    else if (rightWall && bottomWall)
                    {
                        result._Cases[i][j]._WallType = EWallType.BottomRight;
                    }

                    Color centerPixel = mapImage.GetPixel(j * width + width / 2, i * height + height / 2);

                    if (AreColorsEqual(centerPixel, redRobot) || AreColorsEqual(centerPixel, HredRobot))
                    {
                        result._Robots.Where(r => r._Color == EColor.Red).First()._Position = new Position(j, i);
                    }
                    else if (AreColorsEqual(centerPixel, yellowRobot) || AreColorsEqual(centerPixel, HyellowRobot))
                    {
                        result._Robots.Where(r => r._Color == EColor.Yellow).First()._Position = new Position(j, i);
                    }
                    else if (AreColorsEqual(centerPixel, greenRobot) || AreColorsEqual(centerPixel, HgreenRobot))
                    {
                        result._Robots.Where(r => r._Color == EColor.Green).First()._Position = new Position(j, i);
                    }
                    else if (AreColorsEqual(centerPixel, blueRobot) || AreColorsEqual(centerPixel, HblueRobot))
                    {
                        result._Robots.Where(r => r._Color == EColor.Blue).First()._Position = new Position(j, i);
                    }
                    else if (AreColorsEqual(centerPixel, redTarget))
                    {
                        result._Target._Position = new Position(j, i);
                        result._Target._Color = EColor.Red;
                    }
                    else if (AreColorsEqual(centerPixel, yellowTarget))
                    {
                        result._Target._Position = new Position(j, i);
                        result._Target._Color = EColor.Yellow;
                    }
                    else if (AreColorsEqual(centerPixel, greenTarget))
                    {
                        result._Target._Position = new Position(j, i);
                        result._Target._Color = EColor.Green;
                    }
                    else if (AreColorsEqual(centerPixel, blueTarget))
                    {
                        result._Target._Position = new Position(j, i);
                        result._Target._Color = EColor.Blue;
                    }
                }
            }

            return result;
        }


        private static Bitmap GetCroppedImageFromImage(Bitmap image)
        {
            bool wasFirstWallFound = false;
            Color firstPixelColor = image.GetPixel(0, image.Height / 2);
            int leftx=0;
            int rightx=image.Width;
            int topy=0;
            int bottomy=image.Height;
            for (int i = 0; i < image.Width && !wasFirstWallFound; i++)
            {
                var pixel = image.GetPixel(i, image.Height / 2);
                if (pixel != firstPixelColor)
                {
                    wasFirstWallFound = true;
                    leftx = i;
                }
            }

            wasFirstWallFound = false;
            firstPixelColor = image.GetPixel(image.Width / 2, 0);
            for (int i = 0; i < image.Height && !wasFirstWallFound; i++)
            {
                var pixel = image.GetPixel(image.Width/2, i);
                if (pixel != firstPixelColor)
                {
                    wasFirstWallFound = true;
                    topy = i;
                }
            }

            wasFirstWallFound = false;
            firstPixelColor = image.GetPixel(image.Width - 1, image.Height / 2);
            for (int i = 1; i < image.Width && !wasFirstWallFound; i++)
            {
                var pixel = image.GetPixel(image.Width-i, image.Height/2);
                if (pixel != firstPixelColor)
                {
                    wasFirstWallFound = true;
                    rightx = image.Width - i+1;
                }
            }

            wasFirstWallFound = false;
            firstPixelColor = image.GetPixel(image.Width / 2, image.Height-1);
            for (int i = 1; i < image.Height && !wasFirstWallFound; i++)
            {
                var pixel = image.GetPixel(image.Width / 2, image.Height-i);
                if (pixel != firstPixelColor)
                {
                    wasFirstWallFound = true;
                    bottomy = image.Height - i+1;
                }
            }

            Bitmap result = image.Clone(new Rectangle(leftx, topy, rightx - leftx, bottomy - topy), image.PixelFormat);
            image.Save(System.AppDomain.CurrentDomain.BaseDirectory + "test.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
            result.Save(System.AppDomain.CurrentDomain.BaseDirectory + "test_cropped.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

            return result;
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
