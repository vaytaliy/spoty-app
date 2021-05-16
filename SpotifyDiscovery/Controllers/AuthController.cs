using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SpotifyDiscovery.Data;
using SpotifyDiscovery.Objects;
using SpotifyDiscovery.Utilities;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SpotifyDiscovery.Controllers
{
    [ApiController]
    [Route("discovery")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _client;
        private readonly ILogger<AuthController> _logger;
        private readonly AccountService _accountService;

        public AuthController(IConfiguration configuration, 
            HttpClient client, 
            ILogger<AuthController> logger,
            AccountService accountService)
        {
            _configuration = configuration;
            _accountService = accountService;
            _client = client;
            _logger = logger;
            var bytes = Encoding.UTF8.GetBytes(_configuration["ClientId"] + ":" + _configuration["ClientSecret"]);
            var idAndSecretEncoded = WebEncoders.Base64UrlEncode(bytes);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", idAndSecretEncoded);
        }

        [HttpGet("login")]
        public void RedirectToLogin()
        {
            var scopes = "user-read-email user-read-private streaming user-modify-playback-state user-read-currently-playing user-read-playback-position playlist-read-private";

            Response.Redirect($"https://accounts.spotify.com/authorize?response_type={"code"}&" +
                $"client_id={_configuration["ClientId"]}" +
                $"&scope={WebUtility.UrlEncode(scopes)}" +
                $"&redirect_uri={WebUtility.UrlEncode(_configuration["RedirectUri"])}");
        }

        [HttpGet("authorization")]
        public async Task GetAuthorization()
        {
            var queryDict = HttpContext.Request.Query;

            if (queryDict.ContainsKey("error"))
            {
                //send error response
                Response.Redirect(Helper.BaseURL + "/main" + $"?error={queryDict["error"]}");
                return;
            }

            var requestContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", WebUtility.UrlEncode(queryDict["code"])),
                new KeyValuePair<string, string>("redirect_uri", _configuration["RedirectUri"])
            });

            _logger.LogInformation(_configuration["RedirectUri"]);

            

            using var response = await _client.PostAsync("https://accounts.spotify.com/api/token", requestContent);


            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();

                TokenObject tokenObject = (TokenObject)JsonSerializer.Deserialize(responseContent, typeof(TokenObject));

                var playlistId = await _accountService.HandleUserAccount(tokenObject.AccessToken);

                if (playlistId == null)
                {
                    //TODO test if thats possible
                    Response.Redirect(Helper.BaseURL + "/callback" + $"?access_token={tokenObject.AccessToken}&refresh_token={tokenObject.RefreshToken}");
                }

                Response.Redirect(Helper.BaseURL + "/callback" + $"?access_token={tokenObject.AccessToken}&refresh_token={tokenObject.RefreshToken}&playlist={playlistId}");
                return;
            }

            var resMsg = await response.Content.ReadAsStringAsync();
            _logger.LogInformation(resMsg);
            Response.Redirect(Helper.BaseURL + "/callback" + $"?error=invalid_token");
        }

        [HttpPost("refresh_token")]
        public async Task<IActionResult> GetRefreshToken([FromBody] TokenObject tokenfromQuerystring)
        {
            var requestContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("refresh_token", tokenfromQuerystring.RefreshToken)
            });

            using var response = await _client.PostAsync("https://accounts.spotify.com/api/token", requestContent);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();

                TokenObject tokenObject = (TokenObject)JsonSerializer.Deserialize(responseContent, typeof(TokenObject));
                return Ok(tokenObject);
            }

            return Unauthorized("perform authorization again");
        }
    }
}
