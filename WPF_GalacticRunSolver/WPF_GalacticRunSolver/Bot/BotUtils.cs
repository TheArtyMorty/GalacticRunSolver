﻿using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WPF_GalacticRunSolver.Bot
{
    public class CConnectionStatus
    {
        public void SetGameInfo(GameInfo info)
        {
            _info = info;
        }

        private GameInfo _info;

        public string _Message
        {
            get
            {
                string result = IsConnected() ? "Connected : " : "Not connected : ";
                result += GameInfo.StateToString(_info.GetGameState());
                return result;
            }
        }

        public bool IsConnected()
        {
            switch (_info.GetGameState())
            {
                case EGameState.Invalid:
                case EGameState.GameFinished:
                    return false;
                default:
                    return true;
            }
        }

        public Brush _Color
        {
            get
            {
                switch (_info.GetGameState())
                {
                    case EGameState.Invalid:
                        return Brushes.Red;
                    case EGameState.GameFinished:
                        return Brushes.Orange;
                    default:
                        return Brushes.Green;
                }
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

        public static async Task<string> PutURI(Uri u, HttpContent c)
        {
            var response = string.Empty;
            using (var client = new HttpClient())
            {
                HttpResponseMessage result = await client.PutAsync(u, c);
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
