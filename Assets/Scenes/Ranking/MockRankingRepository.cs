using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class FakeRankingRepository : IRankingRepository
{
    private readonly List<Ranking> _mockRankings;

    public FakeRankingRepository()
    {
        _mockRankings = new List<Ranking>
        {
            new Ranking("Asoka",            1000, 300, ""),
            new Ranking("Zico",             850, 210, ""),
            new Ranking("Naruto",           700, 180, ""),
            new Ranking("Yoda",             600, 150, ""),
            new Ranking("Captain Kirk",     500, 120, ""),
            new Ranking("Hermione",         480, 115, ""),
            new Ranking("Tony Stark",       460, 100, ""),
            new Ranking("CurrentPlayer",    400,  90, ""),
        };
    }

    public Task<List<Ranking>> GetRankingsAsync(int limit = 50)
        => Task.FromResult(_mockRankings
            .OrderByDescending(r => r.userScore)
            .Take(limit)
            .ToList());

    public Task<List<Ranking>> GetWeekRankingsAsync(int limit = 50)
        => Task.FromResult(_mockRankings
            .OrderByDescending(r => r.userWeekScore)
            .Take(limit)
            .ToList());
}
