using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPF_GalacticRunSolver.Model;

namespace WPF_GalacticRunSolver.Bot
{
    public enum EGameState
    {
        GameNotStarted = 0,
        RoundStarting, 
        RoundInProgress, 
        RoundFinishing, 
        RoundFinished,
        GameFinished,
    }

    public class GameInfo
    {
        public GameInfo(string jsonInfo)
        {
            _json = jsonInfo;
        }

        string _json { get; }

        public string GetUnparsedInfo()
        {
            return _json;
        }

        public string GetBoard()
        {
            return JsonDeserializer.GetJsonElementByName(_json, "board");
        }

        public string GetBoardId()
        {
            return JsonDeserializer.GetJsonElementByName(_json, "boardId");
        }

        public string GetConfig()
        {
            return JsonDeserializer.GetJsonElementByName(_json, "config");
        }


        private static EGameState StringToState(string state)
        {
            switch (state)
            {
                case "GameNotStarted":
                    return EGameState.GameNotStarted;
                case "RoundStarting":
                    return EGameState.RoundStarting;
                case "RoundInProgress":
                    return EGameState.RoundInProgress;
                case "RoundFinishing":
                    return EGameState.RoundFinishing;
                case "RoundFinished":
                    return EGameState.RoundFinished;
                case "GameFinished":
                default:
                    return EGameState.GameFinished;
            }
        }
        public EGameState GetGameState()
        {
            return StringToState(JsonDeserializer.GetJsonElementByName(JsonDeserializer.GetJsonElementByName(_json, "state"), "state"));
        }

        static List<string> robotColors = new List<string>{ "blue", "red", "green", "yellow" };

        private EColor GetColorFromString(string color)
        {
            if (color == "Blue") return EColor.Blue;
            if (color == "Red") return EColor.Red;
            if (color == "Yellow") return EColor.Yellow;
            else return EColor.Green;
        }

        private static EWallType StringToWallType(string t)
        {
            if (t == "Tr") return EWallType.TopRight;
            if (t == "Br") return EWallType.BottomRight;
            if (t == "Bl") return EWallType.BottomLeft;
            if (t == "Tl") return EWallType.TopLeft;
            return EWallType.None;
        }

        public Map GetMap()
        {
            string board = GetBoard();
            int size = Int32.Parse(JsonDeserializer.GetJsonElementByName(JsonDeserializer.GetJsonElementByName(board, "boardSize"),"h"));

            Map result = new Map(size);

            //Robots
            string robots = JsonDeserializer.GetJsonElementByName(board, "robots");
            foreach (string color in robotColors)
            {
                string robotString = JsonDeserializer.GetJsonElementByName(robots, color);
                string pos = JsonDeserializer.GetJsonElementByName(robotString, "point");
                string c = JsonDeserializer.GetJsonElementByName(robotString, "color");
                int x = Int32.Parse(JsonDeserializer.GetJsonElementByName(pos, "x"));
                int y = Int32.Parse(JsonDeserializer.GetJsonElementByName(pos, "y"));

                Robot robot = result._Robots.Where(r => r._Color == GetColorFromString(c)).First();
                robot._Position = new Position(x, y);
            }

            //Target
            {
                string target = JsonDeserializer.GetJsonElementByName(board, "target");
                string pos = JsonDeserializer.GetJsonElementByName(target, "point");
                int x = Int32.Parse(JsonDeserializer.GetJsonElementByName(pos, "x"));
                int y = Int32.Parse(JsonDeserializer.GetJsonElementByName(pos, "y"));
                string color = JsonDeserializer.GetJsonElementByName(target, "color");

                result._Target._Color = GetColorFromString(color);
                result._Target._Position = new Position(x, y);
            }

            //Cases
            {
                string walls = JsonDeserializer.GetJsonElementByName(board, "walls");
                List<string> positions = JsonDeserializer.GetMultipleJsonElementsByName(walls, "point");
                List<string> type = JsonDeserializer.GetMultipleJsonElementsByName(walls, "wall");
                for (int i = 0; i < positions.Count; i++)
                {
                    int x = Int32.Parse(JsonDeserializer.GetJsonElementByName(positions[i], "x"));
                    int y = Int32.Parse(JsonDeserializer.GetJsonElementByName(positions[i], "y"));

                    result._Cases[y][x]._WallType = StringToWallType(type[i]);
                }
            }

            return result;
        }
    }



    public static class JsonDeserializer
    {
        public static List<string> GetMultipleJsonElementsByName(string json, string name)
        {
            List<string> result = new List<string>();
            int startOfElement = json.IndexOf(name);
            while ( startOfElement > -1)
            {
                result.Add(GetJsonElementByName(json, name));
                json = json.Substring(startOfElement + name.Length);
                startOfElement = json.IndexOf(name);
            }

            return result;
        }

        public static string GetJsonElementByName(string json, string name)
        {
            int startOfElement = json.IndexOf(name);
            if (startOfElement > -1)
            {
                string cropped = json.Substring(startOfElement + name.Length + 2);
                bool isStringElement = (cropped[0] == '"');
                bool isMultiElement = (cropped[0] == '{');
                bool isMultiArray = (cropped[0] == '[');
                bool isNumber = (cropped[0] >= '0' && cropped[0] <= '9');
                if (isStringElement)
                {
                    int endOfElement = cropped.Substring(1).IndexOf('"');
                    return cropped.Substring(1, endOfElement);
                }
                else if (isMultiElement)
                {
                    int numberOfOpenBracket = 0;
                    int i = 1;
                    bool done = false;
                    while (!done)
                    {
                        char c = cropped[i];
                        switch (c)
                        {
                            case '{':
                                numberOfOpenBracket++;
                                break;
                            case '}':
                                numberOfOpenBracket--;
                                done = numberOfOpenBracket < 0;
                                break;
                            default:
                                break;
                        }
                        i++;
                    }
                    return cropped.Substring(1, i-2);
                }
                else if (isMultiArray)
                {
                    int numberOfOpenBracket = 0;
                    int i = 1;
                    bool done = false;
                    while (!done)
                    {
                        char c = cropped[i];
                        switch (c)
                        {
                            case '[':
                                numberOfOpenBracket++;
                                break;
                            case ']':
                                numberOfOpenBracket--;
                                done = numberOfOpenBracket < 0;
                                break;
                            default:
                                break;
                        }
                        i++;
                    }
                    return cropped.Substring(1, i - 2);
                }
                else if (isNumber)
                {
                    int i = 1;
                    while (i < cropped.Length && cropped[i] >= '0' && cropped[i] <= '9')
                    {
                        i++;
                    }
                    return cropped.Substring(0, i);
                }
                else
                {
                    throw new Exception("Json case not handled :p");
                }
                
            }
            return "";
        }
    }
}
