using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoLShuffler.DAL.Models
{
    public class Filter
    {
        [ForeignKey("Team")]
        public int Id { get; set; }
        public virtual Team Team { get; set; }

        public int ChampionsCount { get; set; }
        public int MinRang { get; set; }
        public int MaxRang { get; set; }
        public List<int> ChampionClasses { get; set; }

    }
}