using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoLShuffler.DAL.Models
{
    public class Summoner
    {
        public int Id { get; set; }

        [ForeignKey("Team")]
        public int TeamId { get; set; }
        public virtual Team Team { get; set; }

        public string Name { get; set; }
        public string RiotId { get; set; }
        public List<string> ChampionsNames { get; set; }
        public bool IsActiv { get; set; }
        public TeamColor TeamColor { get; set; }
        public Rang Rang { get; set; }
    }

    public enum TeamColor
    {
        Blue,
        Red
    }

    public enum Rang
    {
        First,
        Second,
        Third
    }
}