using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RankingManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] protected GameObject    rankingRowPrefab;
    [SerializeField] protected RectTransform rankingTableContent;
    [SerializeField] protected ScrollRect    scrollRect;

    [Header("Week Reset Information")]
    [SerializeField] private TMP_Text weekResetCountdownText;
    private WeekResetCountdown _resetCountdown;

    [Header("Loading Status")]
    [SerializeField] private GameObject loadingIndicator;
    [SerializeField] private TMP_Text   lastUpdateText;

    // ─── Estado interno ───────────────────────────────────────
    protected IRankingRepository _rankingRepository;
    protected List<Ranking>      _rankings;

    private INavigationService _navigation;
    private DateTime           _lastFetchTime = DateTime.MinValue;
    private bool               _isFetching    = false;

    // ─────────────────────────────────────────────────────────
    // Unity lifecycle
    // ─────────────────────────────────────────────────────────
    protected virtual void Start()
    {
        _navigation = AppContext.Navigation;

        if (rankingRowPrefab == null || rankingTableContent == null || scrollRect == null)
        {
            Debug.LogError("[RankingManager] Referências obrigatórias não configuradas!");
            return;
        }

        InitializeRepository();
        InitializeWeekResetCountdown();
    }

    // ─────────────────────────────────────────────────────────
    // Inicialização
    // ─────────────────────────────────────────────────────────
    private void InitializeWeekResetCountdown()
    {
        if (weekResetCountdownText != null)
        {
            _resetCountdown = gameObject.AddComponent<WeekResetCountdown>();
            _resetCountdown.Initialize(weekResetCountdownText);
        }
    }

    protected virtual void InitializeRepository()
    {
        bool debugMode = BioBlocksSettings.Instance != null
                    && BioBlocksSettings.Instance.IsDebugMode();

        _rankingRepository = debugMode
            ? (IRankingRepository) new FakeRankingRepository()
            : new RankingRepository();

        // Popula a tabela imediatamente com o cache — sem await, sem flash
        var syncService = AppContext.RankingSync;
        if (syncService != null)
        {
            _rankings = syncService.GetCachedRankings();
            if (_rankings != null && _rankings.Count > 0)
                UpdateRankingTable();
        }

        // Verifica staleness e atualiza em background
        _ = InitializeAsync();
    }

    protected virtual async Task InitializeAsync()
    {
        try
        {
            var syncService = AppContext.RankingSync;
            if (syncService != null)
            {
                var fresh = await syncService.GetRankingsWithFallback();
                if (fresh != null && fresh.Count > 0)
                {
                    _rankings = fresh;
                    UpdateRankingTable();
                }
            }
            else
                await FetchRankings();
        }
        catch (Exception e)
        {
            Debug.LogError($"[RankingManager] Falha na inicialização: {e.Message}");
        }
        finally
        {
            ShowLoadingIndicator(false);
        }
    }

    // ─────────────────────────────────────────────────────────
    // Fetch
    // ─────────────────────────────────────────────────────────
    public virtual async Task FetchRankings()
    {
        if (_isFetching)
        {
            Debug.LogWarning("[RankingManager] Busca de rankings já em andamento.");
            return;
        }

        try
        {
            _isFetching    = true;
            ShowLoadingIndicator(true);

            _rankings      = await _rankingRepository.GetRankingsAsync(limit: 50);
            _lastFetchTime = DateTime.UtcNow;

            if (_rankings != null && _rankings.Count > 0)
                UpdateRankingTable();
            else
                Debug.LogWarning("[RankingManager] Ranking retornou vazio.");
        }
        catch (Exception e)
        {
            Debug.LogError($"[RankingManager] Erro ao buscar rankings: {e.Message}\n{e.StackTrace}");
            _rankings = new List<Ranking>();
        }
        finally
        {
            _isFetching = false;
            ShowLoadingIndicator(false);
            UpdateLastFetchLabel();
        }
    }

    // ─────────────────────────────────────────────────────────
    // UI
    // ─────────────────────────────────────────────────────────
    protected virtual void UpdateRankingTable()
    {
        if (rankingTableContent == null) return;

        foreach (Transform child in rankingTableContent)
            Destroy(child.gameObject);

        var top20 = _rankings.Take(20).ToList();

        for (int i = 0; i < top20.Count; i++)
        {
            CreateRankingRow(i + 1, top20[i], false);
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rankingTableContent);

    }    
    
    protected virtual void CreateRankingRow(int rank, Ranking ranking, bool isCurrentUser)
    {
        GameObject rowGO = Instantiate(rankingRowPrefab, rankingTableContent);
        var rowUI = rowGO.GetComponent<RankingRowUI>();

        if (rowUI != null)
            rowUI.Setup(rank, ranking.userName,
                        ranking.userScore, ranking.userWeekScore,
                        ranking.profileImageUrl);
        else
            Debug.LogError("[RankingManager] RankingRowUI não encontrado no prefab!");
    }    private void ShowLoadingIndicator(bool show)
    {
        if (loadingIndicator != null)
            loadingIndicator.SetActive(show);
    }

    private void UpdateLastFetchLabel()
    {
        if (lastUpdateText == null) return;

        lastUpdateText.text = _lastFetchTime == DateTime.MinValue
            ? "Nunca atualizado"
            : FormatElapsedTime(DateTime.UtcNow - _lastFetchTime);
    }

    private string FormatElapsedTime(TimeSpan elapsed)
    {
        if (elapsed.TotalMinutes < 1)  return "Atualizado agora";
        if (elapsed.TotalMinutes < 60) return $"Atualizado há {(int)elapsed.TotalMinutes} min";
        if (elapsed.TotalHours   < 24) return $"Atualizado há {(int)elapsed.TotalHours}h";
        return                                $"Atualizado há {(int)elapsed.TotalDays}d";
    }

    // ─────────────────────────────────────────────────────────
    // Botões / navegação
    // ─────────────────────────────────────────────────────────
    public async void OnRefreshButtonClicked()
    {
        try   { await FetchRankings(); }
        catch (Exception e) { Debug.LogError($"[RankingManager] Refresh falhou: {e.Message}"); }
    }

    public virtual void Navigate(string sceneName)
        => _navigation.NavigateTo(sceneName);
}