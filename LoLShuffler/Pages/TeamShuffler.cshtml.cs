using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using LoLShuffler.Models;
using LoLShuffler.DAL;
using LoLShuffler.DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Filter = LoLShuffler.DAL.Models.Filter;
using Summoner = LoLShuffler.DAL.Models.Summoner;
using NuGet.Packaging;
using System.Xml.Linq;

namespace LoLShuffler.Pages
{
    public class TeamShufflerModel : PageModel
    {
        private readonly ShufflerDbContext dbContext;
        private readonly RiotService riotService;

        public TeamShufflerData Data { get; set; }
        public bool IsSummonerAlreadyExist { get; set; }
        public bool IsSummonerNotFound { get; set; }
        public string ClientVersion { get; set; }

        public TeamShufflerModel(ShufflerDbContext dbContext, RiotService riotSevice)
        {
            this.dbContext = dbContext;
            this.riotService = riotSevice;
        }

        public ActionResult OnGetReload(string key, int v)
        {
            var dbTeam = dbContext.Teams.First(t => t.PrivateKey == key || t.PublicKey == key);
            return new OkObjectResult(v != dbTeam.Version);
        }

        public ActionResult OnGet(string key, bool isSummonerAlreadyExist = false, bool isSummonerNotFound = false)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return Redirect("/TeamShufflerStart?IsTeamDoesNotExist=true");
            }

            var dbTeam = dbContext.Teams.FirstOrDefault(t => t.PrivateKey == key || t.PublicKey == key);
            if (dbTeam == null)
            {
                return Redirect("/TeamShufflerStart?IsTeamDoesNotExist=true");
            }

            IsSummonerAlreadyExist = isSummonerAlreadyExist;
            IsSummonerNotFound = isSummonerNotFound;

            Data = GetDataByDbTeam(dbTeam, key);
            ClientVersion = riotService.GetClientVersion();

            return Page();
        }

        public ActionResult OnGetCreate(string privateKey, string publicKey)
        {
            if (string.IsNullOrWhiteSpace(privateKey) || string.IsNullOrWhiteSpace(publicKey))
            {
                return Redirect("/TeamShufflerStart?IsKeyNotValid=true");
            }

            if (!Regex.IsMatch(privateKey, "^\\w*$") || !Regex.IsMatch(publicKey, "^\\w*$"))
            {
                return Redirect("/TeamShufflerStart?IsKeyNotValid=true");
            }

            var dbTeams = dbContext.Teams.Where(t => t.PrivateKey == privateKey || t.PublicKey == publicKey || t.PrivateKey == publicKey || t.PublicKey == privateKey);
            if (dbTeams.Any())
            {
                return Redirect("/TeamShufflerStart?IsTeamAlreadyExist=true");
            }

            var dbTeam = new Team
            {
                Filter = new Filter
                {
                    ChampionClasses = new List<int>{0,1,2,3,4,5},
                    MinRang = 3,
                    MaxRang = 6,
                    ChampionsCount = 3
                },

                PrivateKey = privateKey,
                PublicKey = publicKey,
                Summoners = new List<Summoner>(),
                Version = 0
            };

            dbContext.Teams.Add(dbTeam);

            dbContext.SaveChanges();

            return Redirect($"/TeamShuffler?key={privateKey}");
        }

        public ActionResult OnGetAddSummoner(string key, TeamColor team, string name)
        {
            var a = PageContext;
            var dbTeam = dbContext.Teams.FirstOrDefault(t => t.PrivateKey == key || t.PublicKey == key);
            if (dbTeam == null)
            {
                return Redirect("/TeamShufflerStart?IsTeamDoesNotExist=true");
            }

            if (dbTeam.Summoners.Any(s => s.Name == name))
            {
                IsSummonerAlreadyExist = true;
                Data = GetDataByDbTeam(dbTeam, key);
                return Redirect($"/TeamShuffler?key={key}&isSummonerAlreadyExist=true");
            }

            var summoners = GetSummoner(name, team);

            if (summoners == null)
            {
                IsSummonerNotFound = true;
                Data = GetDataByDbTeam(dbTeam, key);
                return Redirect($"/TeamShuffler?key={key}&isSummonerNotFound=true");
            }

            dbTeam.Summoners.Add(summoners);
            dbTeam.Version += 1;

            dbContext.SaveChanges();

            return Redirect($"/TeamShuffler?key={key}");
        }

        public ActionResult OnGetSwap(string key, string name)
        {
            var dbTeam = dbContext.Teams.FirstOrDefault(t => t.PrivateKey == key || t.PublicKey == key);
            if (dbTeam == null)
            {
                return Redirect("/TeamShufflerStart?IsTeamDoesNotExist=true");
            }

            var summoner = dbTeam.Summoners.First(s => s.Name == name);
            summoner.TeamColor = summoner.TeamColor == TeamColor.Blue ? TeamColor.Red : TeamColor.Blue;
            dbTeam.Version += 1;
            dbContext.SaveChanges();

            return Redirect($"/TeamShuffler?key={key}");
        }

        public ActionResult OnGetSwapActivity(string key, string name)
        {
            var dbTeam = dbContext.Teams.FirstOrDefault(t => t.PrivateKey == key);
            if (dbTeam == null)
            {
                return Redirect("/TeamShufflerStart?IsTeamDoesNotExist=true");
            }

            var summoner = dbTeam.Summoners.First(s => s.Name == name);
            summoner.IsActiv = !summoner.IsActiv;
            dbTeam.Version += 1;
            dbContext.SaveChanges();

            return Redirect($"/TeamShuffler?key={key}");
        }

        public ActionResult OnGetDeleteSummoner(string key, string name)
        {
            var dbTeam = dbContext.Teams.FirstOrDefault(t => t.PrivateKey == key);
            if (dbTeam == null)
            {
                return Redirect("/TeamShufflerStart?IsTeamDoesNotExist=true");
            }

            var summoner = dbTeam.Summoners.First(s => s.Name == name);
            dbContext.Summoners.Remove(summoner);
            dbTeam.Version += 1;
            dbContext.SaveChanges();

            return Redirect($"/TeamShuffler?key={key}");
        }

        public ActionResult OnGetSuffle(string key, int championsCount, int minRank, int maxRank, string classesStr)
        {
            var dbTeam = dbContext.Teams.FirstOrDefault(t => t.PrivateKey == key);
            if (dbTeam == null)
            {
                return Redirect("/TeamShufflerStart?IsTeamDoesNotExist=true");
            }

            var classList = classesStr?
                .Split('|')
                .Where(b => !string.IsNullOrWhiteSpace(b))
                .Select(Enum.Parse<ChampionClass>)
                .ToList() 
            ?? new List<ChampionClass>();

            var summoners = dbTeam.Summoners.Where(s => s.IsActiv).ToList();
            ClientVersion = riotService.GetClientVersion();
            var champions = riotService.GetAllChampions(ClientVersion);

            foreach (var summoner in summoners)
            {
                summoner.ChampionsNames = riotService.GetRandomChampions(summoner.RiotId, championsCount, minRank, maxRank, classList, champions);
                champions.RemoveAll(x => summoner.ChampionsNames.Any(xx => x.Id == xx));
            }

            dbTeam.Filter.ChampionClasses = classList
                .Select(b => (int) b)
                .ToList();

            dbTeam.Filter.MinRang = minRank;
            dbTeam.Filter.MaxRang = maxRank;
            dbTeam.Filter.ChampionsCount = championsCount;
            dbTeam.Version += 1;

            dbContext.SaveChanges();

            return Redirect($"/TeamShuffler?key={key}");
        }

        public ActionResult OnGetChangeRang(string key, string name, Rang rang)
        {
            var dbTeam = dbContext.Teams.FirstOrDefault(t => t.PrivateKey == key);
            if (dbTeam == null)
            {
                return Redirect("/TeamShufflerStart?IsTeamDoesNotExist=true");
            }

            var summoner = dbTeam.Summoners.First(s => s.Name == name);
            summoner.Rang = rang;
            dbContext.SaveChanges();

            return Redirect($"/TeamShuffler?key={key}");
        }

        public ActionResult OnGetBalance(string key, string ranks)
        {
            var dbTeam = dbContext.Teams.FirstOrDefault(t => t.PrivateKey == key);
            if (dbTeam == null)
            {
                return Redirect("/TeamShufflerStart?IsTeamDoesNotExist=true");
            }

            var ranksDict = new Dictionary<string, Rang>();
            var summonersSplit = ranks.Split("|");
            foreach (var summonerStr in summonersSplit)
            {
                if (string.IsNullOrEmpty(summonerStr))
                {
                    break;
                }
                var summonerSplit = summonerStr.Split(":");
                ranksDict.Add(summonerSplit[0], Enum.Parse<Rang>(summonerSplit[1]));
            }

            var summonersName = ranksDict
                .OrderBy(x => x.Value)
                .ThenBy(x => Guid.NewGuid())
                .Select(x => x.Key)
                .ToList();

            var color = TeamColor.Blue;
            while (summonersName.Count != 0)
            {
                var name = summonersName.First();
                var summoner = dbTeam.Summoners.First(s => s.Name == name);
                summoner.TeamColor = color;
                summonersName.Remove(name);

                if (color == TeamColor.Red)
                {
                    summonersName.Reverse();
                    color = TeamColor.Blue;
                }
                else
                {
                    color = TeamColor.Red;
                }
            }

            dbTeam.Version += 1;
            dbContext.SaveChanges();

            return Redirect($"/TeamShuffler?key={key}");
        }

        private Summoner GetSummoner(string name, TeamColor team)
        {
            var riotId = riotService.GetRiotIdByName(name);

            if (string.IsNullOrWhiteSpace(riotId))
            {
                return null;
            }

            return new Summoner
            {
                RiotId = riotId,
                Name = name,
                TeamColor = team,
                IsActiv = true,
                ChampionsNames = new List<string>()
            };
        }

        private TeamShufflerData GetDataByDbTeam(Team dbTeam, string key)
        {
            return new TeamShufflerData
            {
                PrivateKey = dbTeam.PrivateKey,
                PublicKey = dbTeam.PublicKey,
                IsOwner = dbTeam.PrivateKey == key,
                Version = dbTeam.Version,
                Filter = new Models.Filter
                {
                    MinRang = dbTeam.Filter.MinRang,
                    MaxRang = dbTeam.Filter.MaxRang,
                    ChampionsCount = dbTeam.Filter.ChampionsCount,
                    IgnoredChampionClasses = dbTeam.Filter.ChampionClasses.Select(i => (ChampionClass) i).ToList()
                },
                Summoners = dbTeam.Summoners.Select(s => new Models.Summoner
                {
                    ChampionsNames = s.ChampionsNames.Select(ci => ci).ToList(),
                    IsActiv = s.IsActiv,
                    Name = s.Name,
                    TeamColor = (TeamColor)s.TeamColor,
                    Rang = s.Rang
                }).OrderBy(x => x.Name).ToList()
            };
        }
    }
}