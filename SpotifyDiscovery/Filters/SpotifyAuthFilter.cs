using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SpotifyDiscovery.Dtos;
using SpotifyDiscovery.Utilities;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace SpotifyDiscovery.Filters
{
    public class SpotifyAuthFilter : Attribute, IAsyncActionFilter
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
                var userProfile = await SpotifyAuthorizationUtil.GetProfileFromTokenSpotify(accessToken.Parameter);

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
    }
}
