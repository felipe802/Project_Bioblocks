using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class RankingRepository : IRankingRepository
{
    private IFirestoreRepository _firestore;

    public RankingRepository(IFirestoreRepository firestore)
    {
        _firestore = firestore ?? throw new ArgumentNullException(nameof(firestore));
    }

    // ─────────────────────────────────────────────────────────
    // IRankingRepository
    // ─────────────────────────────────────────────────────────
    public async Task<List<Ranking>> GetRankingsAsync(int limit = 50)
    {
        return await _firestore.GetRankingsAsync(limit);
    }

    public async Task<List<Ranking>> GetWeekRankingsAsync(int limit = 50)
    {
        return await _firestore.GetWeekRankingsAsync(limit);
    }
}