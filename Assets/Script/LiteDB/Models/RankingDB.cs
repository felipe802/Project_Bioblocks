using System;
using LiteDB;

public class RankingDB
{
    [BsonId]
    public string UserId         { get; set; }
    public string NickName       { get; set; }
    public int    Score          { get; set; }
    public int    WeekScore      { get; set; }
    public string ProfileImageUrl { get; set; }
    public DateTime CachedAt    { get; set; }

    public static RankingDB FromDomain(Ranking ranking) => new RankingDB
    {
        UserId          = ranking.UserId,
        NickName        = ranking.userName,
        Score           = ranking.userScore,
        WeekScore       = ranking.userWeekScore,
        ProfileImageUrl = ranking.profileImageUrl ?? "",
        CachedAt        = DateTime.UtcNow
    };

    public Ranking ToDomain() => new Ranking(
        UserId, NickName, Score, WeekScore, ProfileImageUrl
    );
}