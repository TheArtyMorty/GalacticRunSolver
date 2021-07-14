using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Threading;
using WPF_GalacticRunSolver.Model;
using WPF_GalacticRunSolver.ViewModel;

namespace WPF_GalacticRunSolver.Bot
{
    public class Bot : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };

        public Bot(MainWindow main)
        {
            _gameInfo = new GameInfo("");
            _connectionStatus.SetGameInfo(_gameInfo);
            PropertyChanged(this, new PropertyChangedEventArgs(nameof(_connectionStatus)));
            _gameId = null;

            //Anonymous Sign In
            HttpContent c = new StringContent("");
            var t = Task.Run(() => BotUtils.PostURI(new Uri("https://identitytoolkit.googleapis.com/v1/accounts:signUp?key=AIzaSyCx4Ea4ZOS8_XaEodY9Eckcom2uKOhObFI"), c));
            t.Wait();

            JsonDocument doc = JsonDocument.Parse(t.Result);
            JsonElement root = doc.RootElement;

            _idToken = root.GetProperty("idToken");
            _localId = root.GetProperty("localId");
        }

        public void SetParent(MainWindow main)
        {
            _parent = main;
        }

        public void ConnectToGame(string gameId, string name)
        {
            //Initialize Bot
            SetDisplayName(name); //GetUserInfo();

            JoinGame(gameId);
            SendMessage("Hello world !");

            _gameInfo = GetGameInfo();

            var t = Task.Run(() => BotUtils.GetURI(new Uri(
                String.Format("https://galactic-run.firebaseio.com/events/{0}.json?auth={1}", _gameId.ToString(), _idToken))));
            t.Wait();

            if (_connectionStatus.IsConnected())
            {
                _timer = new DispatcherTimer();
                _timer.Interval = TimeSpan.FromSeconds(refreshTimerForDiscoveringMap);
                _timer.Tick += OnTimedEvent;
                _timer.Start();
            }
        }

        public CConnectionStatus _connectionStatus { get; set; } = new CConnectionStatus();

        MainWindow _parent;
        JsonElement _idToken;
        JsonElement _localId;
        string _gameId;
        GameInfo _gameInfo;

        const double refreshTimerForDiscoveringMap = 1.0;
        const double refreshTimerForSendingSolution = 0.2;

        DispatcherTimer _timer;
        bool _mapLoaded = false;
        bool _mapSolved = false;
        bool _solutionSent = true;

        private void OnTimedEvent(Object source, EventArgs e)
        {
            _gameInfo = GetGameInfo();
            PropertyChanged(this, new PropertyChangedEventArgs(nameof(_connectionStatus)));
            HandleGameState();
        }

        private void HandleGameState()
        {
            switch (_gameInfo.GetGameState())
            {
                case EGameState.RoundInProgress:
                    _timer.Stop();
                    LoadMap(_gameInfo);
                    SendSolution();
                    _timer.Interval = TimeSpan.FromSeconds(refreshTimerForSendingSolution);
                    _timer.Start();
                    break;
                case EGameState.RoundStarting:
                    _timer.Stop();
                    LoadMap(_gameInfo);
                    SolveMap();
                    _timer.Interval = TimeSpan.FromSeconds(refreshTimerForSendingSolution);
                    _timer.Start();
                    break;
                case EGameState.RoundFinished:
                    _timer.Stop();
                    _timer.Interval = TimeSpan.FromSeconds(refreshTimerForDiscoveringMap);
                    _timer.Start();
                    CheckIfIWonOnce();
                    _mapLoaded = false;
                    break;
                case EGameState.RoundFinishing:
                    break;
                case EGameState.GameFinished:
                    _timer.Stop();
                    break;
            }
        }

        private void LoadMap(GameInfo info)
        {
            if (!_mapLoaded)
            {
                _mapLoaded = true;
                Map map = info.GetMap();
                _parent.LoadMap(map);
                _solutionSent = false;
                _mapSolved = false;
                _checkWon = false;
            }
        }


        private void SolveMap()
        {
            if (!_mapSolved)
            {
                _mapSolved = true;
                _parent.Solve();
            }
        }

        private void SendSolution()
        {
            SolveMap();
            if (!_solutionSent)
            {
                SolutionViewModel solution = _parent.GetFirstSolution();
                if (solution != null)
                {
                    _solutionSent = true;
                    SendSolutionWithEvent(solution);
                }
            }
        }

        private bool _checkWon = false;
        private void CheckIfIWonOnce()
        {
            if (!_checkWon)
            {
                _checkWon = true;
                _gameInfo = GetGameInfo();
                bool IWon = _gameInfo.GetRoundWinnerId() == _localId.GetString();
                if (IWon)
                {
                    Taunt();
                }
            }
        }


        private List<string> taunts = new List<string>
        {
            "Ez",
            "That was close... almost !",
            "Should I go easier on you?",
            "You did your best... I'm sure.",
            "I'M NOT A ROBOT !",
            "Never gonna give you up...",
        };

        private void Taunt()
        {
            Random rnd = new Random();
            SendMessage(taunts[rnd.Next(0, taunts.Count - 1)]);
        }

        private void SetDisplayName(string name)
        {
            HttpContent data = new StringContent(new JsonDisplayName(_localId.ToString(), name, "").AsJson());
            var t = Task.Run(() => BotUtils.PatchUri(new Uri(
                String.Format("https://galactic-run.firebaseio.com/users/{0}.json?auth={1}", _localId.ToString(), _idToken)),
                data));
            t.Wait();
        }

        private void GetUserInfo()
        {
            var t = Task.Run(() => BotUtils.GetURI(new Uri(
                String.Format("https://galactic-run.firebaseio.com/users/{0}.json?auth={1}", _localId.ToString(), _idToken))));
            t.Wait();
        }

        private void JoinGame(string gameID)
        {
            _gameId = gameID;

            //Add player
            //HttpContent data = new StringContent("{\"" + _localId.ToString() +"\"}");
            //var t = Task.Run(() => BotUtils.PatchUri(new Uri(
            //    String.Format("https://galactic-run.firebaseio.com/games/{0}/players/{1}.json?auth={2}", gameID, _localId.ToString(), _idToken)), data));
            //t.Wait();
            
            //Add event player joined
            string eventId = FirebasePushIDGenerator.GeneratePushID();
            HttpContent eventData = new StringContent(new JsonEvent("PlayerJoined", _localId.ToString(), eventId).AsJson());
            var t2 = Task.Run(() => BotUtils.PatchUri(new Uri(
                String.Format("https://galactic-run.firebaseio.com/events/{0}/{1}.json?auth={2}", gameID, eventId, _idToken)), eventData));
            t2.Wait();

            //Add player 2 ?
            string updatedGameData = GetGameInfo().GetGameDataAfterAddingPlayer(_localId.ToString());
            HttpContent gameData = new StringContent(updatedGameData);
            var t3 = Task.Run(() => BotUtils.PatchUri(new Uri(
                String.Format("https://galactic-run.firebaseio.com/games/{0}.json?auth={1}", gameID, _idToken)), gameData));
            t3.Wait();
        }

        private void SendSolutionWithEvent(SolutionViewModel solution)
        {
            //Send event
            string eventId = FirebasePushIDGenerator.GeneratePushID(); ;
            HttpContent data = new StringContent(new JsonEvent("SequenceSubmitted", _localId.ToString(), eventId, "", "Optimal", solution).AsJson());
            var t = Task.Run(() => BotUtils.PatchUri(new Uri(
                    String.Format("https://galactic-run.firebaseio.com/events/{0}/{1}.json?auth={2}", _gameId, eventId, _idToken)), data));
            t.Wait();

            //Update game with solution
            string updatedGameData = GetGameInfo().GetGameDataAfterSendingSolution(new JsonSolution(_localId.ToString(), solution).AsJson(), _localId.ToString());
            HttpContent gameData = new StringContent(updatedGameData);
            var t2 = Task.Run(() => BotUtils.PatchUri(new Uri(
                String.Format("https://galactic-run.firebaseio.com/games/{0}.json?auth={1}", _gameId, _idToken)), gameData));
            t2.Wait();
        }

        private GameInfo GetGameInfo()
        {
            GameInfo result = new GameInfo("");
            if (_gameId != null)
            { 
                string eventId = FirebasePushIDGenerator.GeneratePushID();
                var t = Task.Run(() => BotUtils.GetURI(new Uri(
                    String.Format("https://galactic-run.firebaseio.com/games/{0}.json?auth={1}", _gameId, _idToken))));
                t.Wait();

                result = new GameInfo(t.Result);
            }

            _connectionStatus.SetGameInfo(result);
            PropertyChanged(this, new PropertyChangedEventArgs(nameof(_connectionStatus)));
            return result;
        }

        private void SendMessage(string message)
        {
            if (_gameId != null)
            {
                string eventId = FirebasePushIDGenerator.GeneratePushID(); ;
                HttpContent messageData = new StringContent(new JsonEvent("Message", _localId.ToString(), eventId, message).AsJson());

                var t = Task.Run(() => BotUtils.PatchUri(new Uri(
                    String.Format("https://galactic-run.firebaseio.com/events/{0}/{1}.json?auth={2}", _gameId, eventId, _idToken)), messageData));
                t.Wait();
            }
        }
    }


    public class JsonEvent
    {
        public JsonEvent(string eventType, string userId, string eventId, string messageContent = "", string solutionState = "", SolutionViewModel solution = null)
        {
            type = eventType;
            uid = userId;
            id = eventId;
            message = messageContent;
            state = solutionState;
            moves = new List<JsonMove>();
            if (solution != null)
            {
                foreach (MoveViewModel move in solution._Moves)
                {
                    moves.Insert(0, new JsonMove(move));
                }
            }
        }

        public string type { get; set; } = "";
        public string id { get; set; } = "";
        public string uid { get; set; } = "";
        public string message { get; set; } = "";
        public string state { get; set; } = "";
        public List<JsonMove> moves { get; set; }

        public string AsJson()
        {
            return JsonSerializer.Serialize(this);
        }
    }

    public class JsonDisplayName
    {
        public JsonDisplayName(string userId, string name, string photo)
        {
            uid = userId;
            displayName = name;
            photoURL = photo;
        }

        public string uid { get; set; } = "";
        public string displayName { get; set; } = "";
        public string photoURL { get; set; } = "";

        public string AsJson()
        {
            return JsonSerializer.Serialize(this);
        }
    }

    
    public class JsonSolution
    {
        public JsonSolution(string userId, SolutionViewModel solution)
        {
            moves = new List<JsonMove>();
            if (solution != null)
            {
                foreach (MoveViewModel move in solution._Moves)
                {
                    moves.Insert(0, new JsonMove(move));
                }
            }

            timestamp = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds();
            uid = userId;
        }

        public List<JsonMove> moves { get; set; }
        public long timestamp { get; set; }

        public string uid { get; set; } = "";

        public string AsJson()
        {
            return JsonSerializer.Serialize(this);
        }
    }

    public class JsonMove
    {
        public JsonMove(MoveViewModel move)
        {
            color = GetStringFromColor(move._Color);
            direction = GetStringFromDirection(move._Direction);
        }

        public string color { get; set; } = "";
        public string direction { get; set; } = "";

        public static string GetStringFromColor(CLI.ERobotColor color)
        {
            switch (color)
            {
                case CLI.ERobotColor.Blue:
                    return "Blue";
                case CLI.ERobotColor.Red:
                    return "Red";
                case CLI.ERobotColor.Green:
                    return "Green";
                case CLI.ERobotColor.Yellow:
                default:
                    return "Yellow";
            }
        }

        public static string GetStringFromDirection(CLI.EMoveDirection direction)
        {
            switch (direction)
            {
                case CLI.EMoveDirection.Up:
                    return "Up";
                case CLI.EMoveDirection.Down:
                    return "Down";
                case CLI.EMoveDirection.Left:
                    return "Left";
                case CLI.EMoveDirection.Right:
                default:
                    return "Right";
            }
        }
    }
}
