using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace LoLShuffler.Models
{
    public class TeamShufflerData
    {
        public string PrivateKey { get; set; }
        public string PublicKey { get; set; }
        public bool IsOwner { get; set; }
        public Filter Filter { get; set; }
        public List<Summoner> Summoners { get; set; }
        public int Version { get; set; }
    }
}
