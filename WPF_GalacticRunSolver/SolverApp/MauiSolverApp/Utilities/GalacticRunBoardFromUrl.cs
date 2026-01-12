using System.Text.Json;

namespace MauiSolverApp.Utilities
{
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
