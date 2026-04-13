using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class RankingSyncService : MonoBehaviour
{
    private IRankingRepository  _remoteRepo;
    private ILiteDBManager      _db;
    private ConnectivityMonitor _connectivity;
    private bool                _isSyncing  = false;
    private DateTime            _lastSyncAt = DateTime.MinValue;
    private const float         CACHE_VALID_MINUTES = 5f;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Não inicializa aqui — lazy-init no primeiro uso
        // garante que AppContext está pronto antes de acessar LiteDB
        Debug.Log("[RankingSyncService] Aguardando AppContext...");
    }

    // -------------------------------------------------------
    // Lazy-init — chamado antes de qualquer operação
    // -------------------------------------------------------
    private bool EnsureInitialized()
    {
        if (_db != null && _remoteRepo != null) return true;
        if (!AppContext.IsReady) return false;

        _db           = AppContext.LocalDatabase;
        _connectivity = AppContext.Connectivity;
        _remoteRepo   = new RankingRepository();

        if (_db == null || !_db.IsInitialized)
        {
            Debug.LogWarning("[RankingSyncService] LiteDB ainda não inicializado.");
            return false;
        }

        Debug.Log("[RankingSyncService] Inicializado.");
        return true;
    }

    // -------------------------------------------------------
    // API pública
    // -------------------------------------------------------

    /// <summary>
    /// Retorna rankings com fallback offline.
    /// Com internet: usa cache LiteDB + sync em background se stale.
    /// Sem internet: usa LiteDB diretamente.
    /// </summary>
    public async Task<List<Ranking>> GetRankingsWithFallback()
    {
        if (!EnsureInitialized())
        {
            Debug.LogWarning("[RankingSyncService] Não inicializado — retornando lista vazia.");
            return new List<Ranking>();
        }

        // Sem internet — usa LiteDB diretamente
        if (_connectivity == null || !_connectivity.IsOnline)
        {
            Debug.Log("[RankingSyncService] Offline — carregando rankings do cache local.");
            return GetCachedRankings();
        }

        // Com internet — verifica se cache é válido
        var cached = GetCachedRankings();
        if (cached.Count > 0 && !IsCacheStale())
        {
            Debug.Log("[RankingSyncService] Cache válido — usando LiteDB.");
            SyncInBackgroundAsync(); // atualiza em background se próximo de expirar
            return cached;
        }

        // Cache vazio ou stale — busca do Firestore
        Debug.Log("[RankingSyncService] Cache stale ou vazio — sincronizando do Firestore.");
        bool synced = await SyncFromFirestoreAsync();
        return synced ? GetCachedRankings() : cached;
    }

    /// <summary>
    /// Retorna o ranking do usuário atual com fallback offline.
    /// </summary>
    public async Task<Ranking> GetCurrentUserRankingWithFallback(string userId)
    {
        if (!EnsureInitialized() || string.IsNullOrEmpty(userId)) return null;

        // Com internet — busca do Firestore
        if (_connectivity != null && _connectivity.IsOnline)
        {
            try
            {
                var remote = await _remoteRepo.GetCurrentUserRankingAsync();
                if (remote != null)
                {
                    SaveRankingToCache(remote);
                    return remote;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[RankingSyncService] Firestore indisponível: {e.Message}");
            }
        }

        // Sem internet ou Firestore falhou — busca do LiteDB
        Debug.Log("[RankingSyncService] Buscando ranking do usuário no cache local.");
        var cached = _db?.Rankings.FindById(userId);
        return cached?.ToDomain();
    }

    /// <summary>
    /// Força sync do Firestore — chamado pelo botão refresh na RankingScene.
    /// </summary>
    public async Task<bool> ForceRefresh()
    {
        if (!EnsureInitialized()) return false;

        if (_connectivity == null || !_connectivity.IsOnline)
        {
            Debug.LogWarning("[RankingSyncService] Sem internet — refresh cancelado.");
            return false;
        }
        return await SyncFromFirestoreAsync();
    }

    // -------------------------------------------------------
    // Privados
    // -------------------------------------------------------
    private List<Ranking> GetCachedRankings()
    {
        if (!EnsureInitialized()) return new List<Ranking>();

        try
        {
            var all = _db?.Rankings.FindAll()
                          .OrderByDescending(r => r.Score)
                          .ToList();
            return all?.Select(r => r.ToDomain()).ToList()
                   ?? new List<Ranking>();
        }
        catch (Exception e)
        {
            Debug.LogError($"[RankingSyncService] Erro ao ler cache: {e.Message}");
            return new List<Ranking>();
        }
    }

    private async Task<bool> SyncFromFirestoreAsync()
    {
        if (_isSyncing) return false;
        _isSyncing = true;

        try
        {
            var rankings = await _remoteRepo.GetRankingsAsync(limit: 50);
            if (rankings == null || rankings.Count == 0)
            {
                Debug.LogWarning("[RankingSyncService] Firestore retornou ranking vazio.");
                return false;
            }

            // Salva no LiteDB — substitui tudo
            _db?.Rankings.DeleteAll();
            foreach (var ranking in rankings)
                SaveRankingToCache(ranking);

            _lastSyncAt = DateTime.UtcNow;
            Debug.Log($"[RankingSyncService] {rankings.Count} rankings sincronizados.");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[RankingSyncService] Sync falhou: {e.Message}");
            return false;
        }
        finally
        {
            _isSyncing = false;
        }
    }

    private void SaveRankingToCache(Ranking ranking)
    {
        if (ranking == null || string.IsNullOrEmpty(ranking.UserId)) return;
        try
        {
            _db?.Rankings.Upsert(RankingDB.FromDomain(ranking));
        }
        catch (Exception e)
        {
            Debug.LogError($"[RankingSyncService] Erro ao salvar no cache: {e.Message}");
        }
    }

    private bool IsCacheStale()
    {
        if (_lastSyncAt == DateTime.MinValue) return true;
        return (DateTime.UtcNow - _lastSyncAt).TotalMinutes > CACHE_VALID_MINUTES;
    }

    private async void SyncInBackgroundAsync()
    {
        try { await SyncFromFirestoreAsync(); }
        catch (Exception e)
        {
            Debug.LogWarning($"[RankingSyncService] Background sync falhou: {e.Message}");
        }
    }
}