using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoLShuffler.DAL.Models;

namespace LoLShuffler.Models
{
    public class Summoner
    {
        public string Name;
        public List<string> ChampionsNames { get; set; }
        public bool IsActiv { get; set; }
        public TeamColor TeamColor { get; set; }
    }
}
