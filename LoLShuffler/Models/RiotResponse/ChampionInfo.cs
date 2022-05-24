using LoLShuffler.DAL.Models;

namespace LoLShuffler.Models.RiotResponse;

public class ChampionInfo
{
    public int ChampionId { get; set; }
    public int ChampionLevel { get; set; }
    public ChampionClass Class { get; set; }
    public string Name { get; set; }
}