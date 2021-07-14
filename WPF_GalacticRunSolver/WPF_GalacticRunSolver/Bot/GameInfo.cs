using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
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
        Invalid,
    }

    public class GameInfo
    {
        public GameInfo(string jsonInfo)
        {
            if (jsonInfo != "")
            {
                _doc = JsonDocument.Parse(jsonInfo).RootElement;
            }
        }

        JsonElement _doc { get; }

        public JsonElement GetBoard()
        {
            return _doc.GetProperty("round").GetProperty("board");
        }

        public string GetGameDataAfterAddingPlayer(string playerId)
        {
            string players = _doc.GetProperty("players").ToString();
            players = players.Remove(players.Length - 1);
            string newPlayer = ",\"" + playerId + "\":\"" + playerId + "\"";
            string withNewPlayer = players + newPlayer;

            string data = _doc.ToString();
            return data.Replace(players, withNewPlayer);
        }

        public string GetGameDataAfterSendingSolution(string bestSolution, string userId)
        {
            string result = _doc.ToString();
            //Add best move
            JsonElement round = _doc.GetProperty("round");
            JsonElement best;
            if (round.TryGetProperty("best", out best))
            {
                result = result.Replace(best.ToString(), bestSolution);
            }
            else
            {
                result = result.Replace("round\":{\"board\":", "round\":{\"best\":"+ bestSolution + ", \"board\":");
            }
            //Add to score
            int numberOfWins = 0;
            JsonElement scores;
            string mapId = round.GetProperty("id").GetString();
            string newScore = "\"" + mapId + "\":\"" + userId + "\"";
            if (_doc.TryGetProperty("scores", out scores))
            {
                string scoresAsString = scores.ToString();
                numberOfWins = scoresAsString.Select((c, i) => scoresAsString.Substring(i)).Count(sub => sub.StartsWith(userId));
                scoresAsString = scoresAsString.Remove(scoresAsString.Length - 1);
                result = result.Replace(scoresAsString, scoresAsString + "," + newScore);
            }
            else
            {
                string newScores = ",\"scores\":{" + newScore + "}";
                result = result.Replace(",\"state\":{\"state\":", newScores + ",\"state\":{\"state\":");
            }
            numberOfWins++;
            //Change status to done
            JsonElement state = _doc.GetProperty("state");
            string currentState = state.GetProperty("state").GetString();
            int numberOfWinsNeeded = _doc.GetProperty("config").GetProperty("numberOfRoundsToWin").GetInt32();
            if (numberOfWins >= numberOfWinsNeeded)
            {
                result = result.Replace("\"state\":\"" + currentState, "\"state\":\"GameFinished");
            }
            else
            {
                result = result.Replace("\"state\":\"" + currentState, "\"state\":\"RoundFinished");
            }
            result = result.Replace("\"state\":\"" + currentState, "\"state\":\"RoundFinished");
            string timestamp = state.GetProperty("timestamp").GetDouble().ToString();
            result = result.Replace("\"timestamp\":" + timestamp, "\"timestamp\":" + ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds().ToString());
            return result;
        }

        public string GetRoundWinnerId()
        {
            JsonElement scores = _doc.GetProperty("scores");
            JsonElement mapId = _doc.GetProperty("round").GetProperty("id");
            return scores.GetProperty(mapId.GetString()).GetString();
        }

        public static string StateToString(EGameState state)
        {
            switch (state)
            {
                case EGameState.GameNotStarted:
                    return "Game Not Started";
                case EGameState.RoundStarting:
                    return "Round Starting";
                case EGameState.RoundInProgress:
                    return "Round In Progress";
                case EGameState.RoundFinishing:
                    return "Round Finishing";
                case EGameState.RoundFinished:
                    return "Round Finished";
                case EGameState.GameFinished:
                    return "Game Finished";
                case EGameState.Invalid:
                default:
                    return "Invalid Game ID...";
            }
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
                    return EGameState.GameFinished;
                default:
                    return EGameState.Invalid;
            }
        }
        public EGameState GetGameState()
        {
            string state = "";
            if (_doc.ValueKind != JsonValueKind.Undefined && _doc.ValueKind != JsonValueKind.Null)
            {
                state = _doc.GetProperty("state").GetProperty("state").GetString();
            }
            return StringToState(state);
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
            JsonElement board = GetBoard();
            int size = board.GetProperty("boardSize").GetProperty("h").GetInt32();

            Map result = new Map(size);

            //Robots
            JsonElement robots = board.GetProperty("robots");
            foreach (string key in robotColors)
            {
                JsonElement robot = robots.GetProperty(key);
                JsonElement point = robot.GetProperty("point");
                int x = point.GetProperty("x").GetInt32();
                int y = point.GetProperty("y").GetInt32();
                string color = robot.GetProperty("color").GetString();

                Robot robotModel = result._Robots.Where(r => r._Color == GetColorFromString(color)).First();
                robotModel._Position = new Position(x, y);
            }

            //Target
            {
                JsonElement target = board.GetProperty("target");
                JsonElement point = target.GetProperty("point");
                int x = point.GetProperty("x").GetInt32();
                int y = point.GetProperty("y").GetInt32();
                string color = target.GetProperty("color").GetString();

                result._Target._Color = GetColorFromString(color);
                result._Target._Position = new Position(x, y);
            }

            //Cases
            {
                JsonElement walls = board.GetProperty("walls");
                foreach(JsonElement wall in walls.EnumerateArray())
                {
                    JsonElement point = wall.GetProperty("point");
                    int x = point.GetProperty("x").GetInt32();
                    int y = point.GetProperty("y").GetInt32();
                    string wallType = wall.GetProperty("wall").GetString();

                    result._Cases[y][x]._WallType = StringToWallType(wallType);
                }
            }

            return result;
        }
    }
}
