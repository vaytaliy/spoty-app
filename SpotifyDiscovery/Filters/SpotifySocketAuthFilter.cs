using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.SignalR;
using SpotifyDiscovery.Dtos;
using SpotifyDiscovery.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpotifyDiscovery.Filters
{
    public class SpotifySocketAuthFilter : IHubFilter
    {
        public async ValueTask<object> InvokeMethodAsync(
        HubInvocationContext invocationContext, Func<HubInvocationContext, ValueTask<object>> next)
        {
            var accessTokenParam = invocationContext.HubMethodArguments.Where(pre => pre.ToString() == "accessToken").FirstOrDefault().ToString();

            if (accessTokenParam == null)
            {
                throw new HubException("Auth token wasn't provided");
            }
            var account = await SpotifyAuthorizationUtil.GetProfileFromTokenSpotify(accessTokenParam);

            if (account == null)
            {
                throw new HubException("Invalid auth token provided");
            }

            invocationContext.HubMethodArguments.Append(account);
            return await next(invocationContext);
        }
    }
}
