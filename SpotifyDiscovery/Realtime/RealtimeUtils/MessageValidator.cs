using SpotifyDiscovery.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpotifyDiscovery.Realtime.RealtimeUtils
{
    public static class MessageValidator
    {
        public static bool ValidateMessage(string message)
        {

            if (message == null)
            {
                return false;
            }

            if (message.Length >= 1 && message.Length <= 200)
            {
                return true;
            }

            return false;
        }
    }
}
