using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using Castle.Components.DictionaryAdapter;
using LoLShuffler.DAL.Models;
using LoLShuffler.Models;
using LoLShuffler.Models.RiotResponse;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace LoLShuffler
{
    public class RiotService
    {
        private readonly IHttpClientFactory clientFactory;
        private readonly Random random;
        private readonly string key;
        
        public RiotService(IHttpClientFactory clientFactory, IOptionsSnapshot<Keys> keys)
        {
            this.clientFactory = clientFactory;
            random = new Random();
            key = keys.Value.RiotKey;
        }

        public string GetRiotIdByName(string name)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://ru.api.riotgames.com/lol/summoner/v4/summoners/by-name/{name}");
            request.Headers.Add("X-Riot-Token", key);

            var client = clientFactory.CreateClient();
            var response = client.SendAsync(request).Result;

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var responseStream = response.Content.ReadAsStreamAsync().Result;
            var summoner = DeserializeFromStream<RiotSummoner>(responseStream);
            return summoner.Puuid;
        }

        public List<Champion> GetAllChampions(string clientVersion)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"http://ddragon.leagueoflegends.com/cdn/{clientVersion}/data/en_US/champion.json");
            var client = clientFactory.CreateClient();
            var response = client.SendAsync(request).Result;

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var responseStream = response.Content.ReadAsStreamAsync().Result;
            var data = DeserializeFromStream<ChampionsList>(responseStream);
            return data.Data.Values.ToList();
        }

        public List<string> GetRandomChampions(string riotId, int championsCount, int minRank, int maxRank, List<ChampionClass> classList, List<Champion> champions)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://ru.api.riotgames.com/lol/champion-mastery/v4/champion-masteries/by-puuid/{riotId}");
            request.Headers.Add("X-Riot-Token", key);

            var client = clientFactory.CreateClient();
            var response = client.SendAsync(request).Result;

            if (!response.IsSuccessStatusCode)
            {
                return new List<string>();
            }

            var responseStream = response.Content.ReadAsStreamAsync().Result;
            var championsInfo = DeserializeFromStream<List<ChampionInfo>>(responseStream)
                .Where(c => champions.Any(cc => cc.Key == c.ChampionId))
                .Select(ci => new ChampionInfo
                {
                    ChampionId =  ci.ChampionId,
                    ChampionLevel = ci.ChampionLevel,
                    Class = champions.First(c => c.Key == ci.ChampionId).Tags.First(),
                    Name = champions.First(c => c.Key == ci.ChampionId).Id
                })
                .ToList();

            var possibleChampionsName = championsInfo
                .Where(ci => classList.Contains(ci.Class))
                .Where(ci => ci.ChampionLevel >= minRank)
                .Where(ci => ci.ChampionLevel <= maxRank)
                .Select(ci => ci.Name)
                .Select(x => x.Replace(" ", ""))
                .ToList();

            if (possibleChampionsName.Count == 0)
            {
                return new List<string>();
            }

            if (possibleChampionsName.Count <= championsCount)
            {
                return possibleChampionsName;
            }

            var result = new List<string>();

            var badRandom = new List<int>();

            for (var i = 0; i <= championsCount - 1; i++)
            {
                var randomInt = random.Next(0, possibleChampionsName.Count);
                while (badRandom.Contains(randomInt))
                {
                    randomInt = random.Next(0, possibleChampionsName.Count);
                }
                result.Add(possibleChampionsName[randomInt]);
                badRandom.Add(randomInt);
            }

            return result;
        }

        public string GetClientVersion()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://ddragon.leagueoflegends.com/api/versions.json");
            var client = clientFactory.CreateClient();
            var response = client.SendAsync(request).Result;

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var responseStream = response.Content.ReadAsStreamAsync().Result;
            var data = DeserializeFromStream<List<string>>(responseStream);
            return data.First();
        }

        private static T DeserializeFromStream<T>(Stream stream)
        {
            var serializer = new JsonSerializer();

            using var sr = new StreamReader(stream);
            using var jsonTextReader = new JsonTextReader(sr);
            return serializer.Deserialize<T>(jsonTextReader);
        }
    }
}