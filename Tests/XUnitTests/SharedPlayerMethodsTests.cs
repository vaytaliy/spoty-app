using System;
using Xunit;
using SpotifyDiscovery;
using SpotifyDiscovery.Dtos;
using SpotifyDiscovery.Objects;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Newtonsoft.Json.Serialization;

namespace XUnitTests
{
    public class SharedPlayerMethodsTests
    {
        /*
         *  Testing if json can be parsed into dictionary
         */
        
        [Fact]
        public void ParseRoomConnectionsDataShouldSucceed()
        {
            //Arrange
            ProfileReadDto personOne = new ProfileReadDto()
            {
                DisplayName = "Bob",
                SpotifyId = "spotifyid11111",
                Images = new List<ImageObject>()
                {
                    new ImageObject()
                    {
                        Width = 1000,
                        Height = 300,
                        ImageURL = "person1_im_url"
                    },
                    new ImageObject()
                    {
                        Width = 500,
                        Height = 300,
                        ImageURL = "person1_im_url2"
                    }
                }
            };

            ProfileReadDto hostPerson = new ProfileReadDto()
            {
                DisplayName = "Gob",
                SpotifyId = "spotifyid22222",
                Images = new List<ImageObject>()
                {
                    new ImageObject()
                    {
                        Width = 1000,
                        Height = 300,
                        ImageURL = "person2_im_url"
                    },
                    new ImageObject()
                    {
                        Width = 500,
                        Height = 300,
                        ImageURL = "person2_im_url2"
                    }
                }
            };

            personOne.ConnectionId = "connidperson1";
            hostPerson.ConnectionId = "hostpersonconnid";

            hostPerson.IsHost = true;

            var rawFile = File.ReadAllText("D:/Projects/SpotifyDiscovery/SpotifyDiscoveryBackend/SpotifyDiscovery/Tests/XUnitTests/Resources/SampleData/redis_room_connections_sample.json");
            //var rawFile = File.ReadAllText(AppContext.BaseDirectory + "/Resources/SampleData/redis_room_connections_sample.json");

            //Act
            var receivedInformation = JsonSerializer.Deserialize<ConnectionDetailsDto>(rawFile);
            var actualName = "";

            foreach (var profile in receivedInformation.ConnectionDetail)
            {
                if (profile.IsHost == true)
                {
                    actualName = profile.DisplayName;
                    break;
                }
            }

            //Assert
            Assert.Equal(hostPerson.DisplayName, actualName);
        }
    }
}
