using Microsoft.Extensions.Configuration;
using SpotifyDiscovery.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace XUnitTests
{
    public class AuthControllerTests
    {
        public static IConfiguration InitConfiguration()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.test.json")
                .AddEnvironmentVariables()
                .Build();

            return configuration;
        }
        
        [Fact]
        public async Task RediectToLogin_HasRequiredURLProperties()
        {
            //Arrange
            //var controller = new AuthController();
            //Act

            //Assert
        }
    }
}
