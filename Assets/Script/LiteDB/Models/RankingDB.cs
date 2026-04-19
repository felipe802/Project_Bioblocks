using System;
using LiteDB;

public class RankingDB
{
    [BsonId]
    public string NickName       { get; set; }
    public int    Score          { get; set; }
    public int    WeekScore      { get; set; }
    public string ProfileImageUrl { get; set; }

    public static RankingDB FromDomain(Ranking ranking) => new RankingDB
    {
        NickName        = ranking.userName,
        Score           = ranking.userScore,
        WeekScore       = ranking.userWeekScore,
        ProfileImageUrl = ranking.profileImageUrl ?? ""
    };

    public Ranking ToDomain() => new Ranking(
        NickName, Score, WeekScore, ProfileImageUrl
    );
}