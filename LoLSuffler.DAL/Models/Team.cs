using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace LoLShuffler.DAL.Models
{
    public class Team
    {
        public int Id { get; set; }
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }
        public int Version { get; set; }

        public virtual Filter Filter { get; set; }
        public virtual List<Summoner> Summoners { get; set; }
    }
}
