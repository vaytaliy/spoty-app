using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SpotifyDiscovery.Dtos;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace SpotifyDiscovery.Filters
{
    public class SpotifyAuthFilterAttribute : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var tokenExists = context.HttpContext.Request.Headers.ContainsKey("Authorization");

            if (tokenExists == false)
            {
                context.Result = new ContentResult
                {
                    Content = JsonSerializer.Serialize(new { name = "error", description = "No bearer token was provided" }),
                    ContentType = "application/json",
                    StatusCode = 401
                };
            }

            var isValidHeader = AuthenticationHeaderValue.TryParse(context.HttpContext.Request.Headers["Authorization"], out AuthenticationHeaderValue accessToken);

            if (isValidHeader)
            {
                var userProfile = await GetProfileFromTokenSpotify(accessToken.Parameter);

                if (userProfile != null)
                {
                    context.HttpContext.Items["User"] = userProfile;
                    await next();
                    return;
                }
            }
            context.Result = new ContentResult
            {
                Content = JsonSerializer.Serialize(new { name = "error", description = "Invalid authorization token" }),
                ContentType = "application/json",
                StatusCode = 401
            };
        }

        private static async Task<ProfileReadDto> GetProfileFromTokenSpotify(string accessToken)
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
