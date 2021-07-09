using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Threading;
using WPF_GalacticRunSolver.Model;


namespace WPF_GalacticRunSolver.Bot
{
    public class Bot
    {
        public Bot(MainWindow main, string name, string gameId)
        {
            //Anonymous Sign In
            HttpContent c = new StringContent("");
            var t = Task.Run(() => BotUtils.PostURI(new Uri("https://identitytoolkit.googleapis.com/v1/accounts:signUp?key=AIzaSyCx4Ea4ZOS8_XaEodY9Eckcom2uKOhObFI"), c));
            t.Wait();

            JsonDocument doc = JsonDocument.Parse(t.Result);
            JsonElement root = doc.RootElement;

            _idToken = root.GetProperty("idToken");
            _localId = root.GetProperty("localId");
            _gameId = null;
            _parent = main;

            //Initialize Bot
            SetDisplayName(name); //GetUserInfo();

            //ConnectToGame(gameId);
            //SendMessage("Hello world !");
            _gameId = gameId;

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(refreshTimerForDiscoveringMap);
            _timer.Tick += OnTimedEvent;
            _timer.Start();
        }

        MainWindow _parent;
        JsonElement _idToken;
        JsonElement _localId;
        string _gameId;

        const double refreshTimerForDiscoveringMap = 1.0;
        const double refreshTimerForSendingSolution = 0.2;

        DispatcherTimer _timer;
        bool _mapLoaded = false;
        bool _mapSolved = false;
        bool _solutionSent = true;

        private void OnTimedEvent(Object source, EventArgs e)
        {
            GameInfo info = GetGameInfo();
            HandleGameState(info);
        }

        private void HandleGameState(GameInfo info)
        {
            switch (info.GetGameState())
            {
                case EGameState.RoundInProgress:
                    _timer.Stop();
                    //LoadMap(info);
                    SendSolution();
                    _timer.Interval = TimeSpan.FromSeconds(refreshTimerForSendingSolution);
                    _timer.Start();
                    break;
                case EGameState.RoundStarting:
                    _timer.Stop();
                    LoadMap(info);
                    SolveMap();
                    _timer.Interval = TimeSpan.FromSeconds(refreshTimerForSendingSolution);
                    _timer.Start();
                    break;
                case EGameState.RoundFinished:
                    _mapLoaded = false;
                    break;
                case EGameState.RoundFinishing:
                    _timer.Stop();
                    _timer.Interval = TimeSpan.FromSeconds(refreshTimerForDiscoveringMap);
                    _timer.Start();
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
                _solutionSent = _parent.Send();
            }
        }


        private void SetDisplayName(string name)
        {
            HttpContent c = new StringContent("{ \"uid\": \"" + _localId.ToString() + "\", \"displayName\": \"" + name + "\", \"photoURL\": \"\"}");
            var t = Task.Run(() => BotUtils.PatchUri(new Uri(
                "https://galactic-run.firebaseio.com/users/" + _localId.ToString() + ".json?auth=" + _idToken),
                c));
            t.Wait();
        }

        private void GetUserInfo()
        {
            var t = Task.Run(() => BotUtils.GetURI(new Uri(
                "https://galactic-run.firebaseio.com/users/" + _localId.ToString() + ".json?auth=" + _idToken)));
            t.Wait();
        }

        private void ConnectToGame(string gameID)
        {
            HttpContent c = new StringContent("{ \"type\": \"PlayerJoined\", \"uid\": \"" + _localId.ToString() + "\", \"id\": \"\"}");
            string eventId = "-KUY6wIoxwYFJbKhYuxA"; //FirebasePushIDGenerator.GeneratePushID();
            var t = Task.Run(() => BotUtils.PatchUri(new Uri(
                "https://galactic-run.firebaseio.com/events/" +gameID +"/" + eventId + ".json?auth=" + _idToken),
                c));
            t.Wait();
            if (t.Result != "")
            {
                _gameId = gameID;
            }
        }

        private GameInfo GetGameInfo()
        {
            if (_gameId != null)
            { 
                string eventId = FirebasePushIDGenerator.GeneratePushID(); //"-KUY6wJ4amFnKQlcEgiJ"; 
                var t = Task.Run(() => BotUtils.GetURI(new Uri(
                    "https://galactic-run.firebaseio.com/games/" + _gameId + ".json?auth=" + _idToken)));
                t.Wait();

                return new GameInfo(t.Result);
            }
            return null;
        }

        private void SendMessage(string message)
        {
            if (_gameId != null)
            {
                HttpContent c = new StringContent("{\"type\": \"Message\", \"id\": \"\", \"uid\": \"" + _localId.ToString() + "\", \"message\": \"" + message + "\"}");
                string eventId = "zKUY6wJ4amFnKQlcEgiJ"; // FirebasePushIDGenerator.GeneratePushID();; 
                var t = Task.Run(() => BotUtils.PatchUri(new Uri(
                    "https://galactic-run.firebaseio.com/events/" + _gameId + "/" + eventId + ".json?auth=" + _idToken),
                    c));
                t.Wait();
            }
        }
    }



    public static class BotUtils
    {
        public static async Task<string> PatchUri(Uri u, HttpContent c)
        {
            var method = new HttpMethod("PATCH");
            var request = new HttpRequestMessage(method, u)
            {
                Content = c
            };

            var response = string.Empty;
            using (var client = new HttpClient())
            {
                HttpResponseMessage result = await client.SendAsync(request);
                if (result.IsSuccessStatusCode)
                {
                    response = await result.Content.ReadAsStringAsync();
                }
            }
            return response;
        }

        public static async Task<string> PostURI(Uri u, HttpContent c)
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

        public static async Task<string> GetURI(Uri u)
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


    public static class FirebasePushIDGenerator
    {
        /**
         * Fancy ID generator that creates 20-character string identifiers with the following properties:
         *
         * 1. They're based on timestamp so that they sort *after* any existing ids.
         * 2. They contain 72-bits of random data after the timestamp so that IDs won't collide with other clients' IDs.
         * 3. They sort *lexicographically* (so the timestamp is converted to characters that will sort properly).
         * 4. They're monotonically increasing.  Even if you generate more than one in the same timestamp, the
         *    latter ones will sort after the former ones.  We do this by using the previous random bits
         *    but "incrementing" them by 1 (only in the case of a timestamp collision).
         */
        // Modeled after base64 web-safe chars, but ordered by ASCII.
        const string PUSH_CHARS = "-0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ_abcdefghijklmnopqrstuvwxyz";

        // Timestamp of last push, used to prevent local collisions if you push twice in one ms.
        private static long lastPushTime = 0;

        // We generate 72-bits of randomness which get turned into 12 characters and appended to the
        // timestamp to prevent collisions with other clients.  We store the last characters we
        // generated because in the event of a collision, we'll use those same characters except
        // "incremented" by one.
        private static char[] lastRandChars = new char[12];

        // Random number generator
        private static Random rng = new Random();

        public static string GeneratePushID()
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            Console.WriteLine("now=" + now);

            var duplicateTime = (now == lastPushTime);
            lastPushTime = now;

            var timeStampChars = new char[8];
            for (var i = 7; i >= 0; i--)
            {
                timeStampChars[i] = PUSH_CHARS[(int)(now % 64)];
                now = now >> 6;
            }
            if (now != 0) throw new Exception("We should have converted the entire timestamp.");

            var id = string.Join(string.Empty, timeStampChars);

            if (!duplicateTime)
            {
                for (var i = 0; i < 12; i++)
                {
                    lastRandChars[i] = (char)rng.Next(0, 63);
                }
            }
            else
            {
                // If the timestamp hasn't changed since last push, use the same random number, except incremented by 1.
                int i;
                for (i = 11; i >= 0 && lastRandChars[i] == 63; i--)
                {
                    lastRandChars[i] = (char)0;
                }
                lastRandChars[i]++;
            }
            for (var i = 0; i < 12; i++)
            {
                id += PUSH_CHARS[lastRandChars[i]];
            }
            if (id.Length != 20) throw new Exception("Length should be 20.");

            return id;
        }

        public static long ConvertPushID(string id)
        {
            var timestamp = 0L;
            for (var i = 0; i < 8; i++)
            {
                var n = PUSH_CHARS.IndexOf(id[i]);
                timestamp = (timestamp << 6) + n;
            }
            return timestamp;
        }
    }
}
