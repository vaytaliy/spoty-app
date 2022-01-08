using SpotifyDiscovery.Dtos;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace SpotifyDiscovery.Utilities
{
    public static class SpotifyAuthorizationUtil
    {
        public static async Task<ProfileReadDto> GetProfileFromTokenSpotify(string accessToken)
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var response = await httpClient.GetAsync("https://api.spotify.com/v1/me");

            if (response.IsSuccessStatusCode)
            {

                var responseContent = await response.Content.ReadAsStringAsync();
                ProfileReadDto userProfile = (ProfileReadDto)JsonSerializer.Deserialize(responseContent, typeof(ProfileReadDto));

                return userProfile;
            }

            return null;
        }
    }
}
