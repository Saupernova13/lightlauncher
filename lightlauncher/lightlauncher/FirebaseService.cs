using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace lightlauncher
{
    public class FirebaseService
    {
        private static readonly HttpClient client = new HttpClient();
        private readonly string apiKey;
        private readonly string dbUrl;

        public FirebaseService(IConfiguration configuration)
        {
            apiKey = configuration["FIREBASE_API_KEY"];
            dbUrl = configuration["FIREBASE_DB_URL"];
        }

        public async Task<List<Game>> GetUserGames(string userId)
        {
            var response = await client.GetStringAsync($"{dbUrl}/users/{userId}/games.json?auth={apiKey}");
            if (response == "null")
            {
                return new List<Game>();
            }
            return JsonSerializer.Deserialize<Dictionary<string, Game>>(response).Values.ToList();
        }

        public async Task AddGame(string userId, Game game)
        {
            var json = JsonSerializer.Serialize(game);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            await client.PostAsync($"{dbUrl}/users/{userId}/games.json?auth={apiKey}", content);
        }

        public async Task AddEmulator(string userId, Emulator emulator)
        {
            var json = JsonSerializer.Serialize(emulator);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            await client.PostAsync($"{dbUrl}/users/{userId}/emulators.json?auth={apiKey}", content);
        }

        public async Task<List<Emulator>> GetUserEmulators(string userId)
        {
            var response = await client.GetStringAsync($"{dbUrl}/users/{userId}/emulators.json?auth={apiKey}");
            if (response == "null")
            {
                return new List<Emulator>();
            }
            return JsonSerializer.Deserialize<Dictionary<string, Emulator>>(response).Values.ToList();
        }
    }
}
