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
    public class AuthService
    {
        private static readonly HttpClient client = new HttpClient();
        private readonly string apiKey;

        public AuthService(IConfiguration configuration)
        {
            apiKey = configuration["FIREBASE_API_KEY"];
        }

        public async Task<FirebaseAuthResponse> SignInAsync(string email, string password)
        {
            var request = new FirebaseAuthRequest
            {
                Email = email,
                Password = password,
                ReturnSecureToken = true
            };

            var jsonRequest = JsonSerializer.Serialize(request);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={apiKey}", content);
            var jsonResponse = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Firebase Auth Error: {jsonResponse}");
            }

            return JsonSerializer.Deserialize<FirebaseAuthResponse>(jsonResponse);
        }

        public async Task<FirebaseAuthResponse> SignUpAsync(string email, string password)
        {
            var request = new FirebaseAuthRequest
            {
                Email = email,
                Password = password,
                ReturnSecureToken = true
            };

            var jsonRequest = JsonSerializer.Serialize(request);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={apiKey}", content);
            var jsonResponse = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Firebase Auth Error: {jsonResponse}");
            }

            return JsonSerializer.Deserialize<FirebaseAuthResponse>(jsonResponse);
        }
    }
}
