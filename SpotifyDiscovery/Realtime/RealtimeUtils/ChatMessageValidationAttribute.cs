using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpotifyDiscovery.Realtime.RealtimeUtils
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ChatMessageValidationAttribute : Attribute
    {
        public int MinLength { get; set; }
        public int MaxLength { get; set; }
        public ChatMessageValidationAttribute(int minLength, int maxLength)
        {
            MinLength = minLength;
            MaxLength = maxLength;
        }
    }
}
