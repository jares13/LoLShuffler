using System.Collections.Generic;
using LoLShuffler.DAL.Models;

namespace LoLShuffler.Models
{
    public class Filter
    {
        public int ChampionsCount { get; set; }
        public int MinRang { get; set; }
        public int MaxRang { get; set; }
        public List<ChampionClass> IgnoredChampionClasses { get; set; }
    }
}
