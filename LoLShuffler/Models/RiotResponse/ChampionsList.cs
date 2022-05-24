using System.Collections.Generic;
using LoLShuffler.DAL.Models;

namespace LoLShuffler.Models.RiotResponse
{

    public class ChampionsList
    {
        public Dictionary<string, Champion> Data { get; set; }
    }

    public class Champion
    {
        public int Key { get; set; }
        public string Id { get; set; }
        public List<ChampionClass> Tags { get; set; }
    }
}