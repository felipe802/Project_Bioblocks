using System;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Implementação padrão de <see cref="IAvatarSelectionService"/>.
/// MonoBehaviour registrado no GameObject do AppContext, injetado via
/// <see cref="InjectDependencies"/> em tempo de inicialização.
/// </summary>
public class AvatarSelectionService : MonoBehaviour, IAvatarSelectionService
{
    private const string PRESET_PREFIX = "preset:";

    private IFirestoreRepository     _firestore;
    private IUserDataLocalRepository _userDataLocal;

    private string _originalUrl;
    private string _pendingUrl;
    private bool   _sessionActive;

    public void InjectDependencies(
        IFirestoreRepository firestore,
        IUserDataLocalRepository userDataLocal)
    {
        _firestore     = firestore;
        _userDataLocal = userDataLocal;
    }

    public void BeginSession()
    {
        var user = UserDataStore.CurrentUserData;
        _originalUrl   = user?.ProfileImageUrl ?? string.Empty;
        _pendingUrl    = _originalUrl;
        _sessionActive = true;

        Debug.Log($"[AvatarSelection] Sessão iniciada. Baseline: '{_originalUrl}'");
    }

    public void PreviewSelection(string avatarId)
    {
        if (!_sessionActive)
        {
            Debug.LogWarning("[AvatarSelection] PreviewSelection chamado sem sessão ativa. Ignorando.");
            return;
        }

        if (string.IsNullOrEmpty(avatarId) || !AvatarCatalog.Contains(avatarId))
        {
            Debug.LogWarning($"[AvatarSelection] avatarId inválido: '{avatarId}'. Ignorando.");
            return;
        }

        var newUrl = PRESET_PREFIX + avatarId;
        _pendingUrl = newUrl;

        var user = UserDataStore.CurrentUserData;
        if (user == null)
        {
            Debug.LogWarning("[AvatarSelection] Preview sem usuário em memória. Ignorando.");
            return;
        }

        // Mutação + reatribuição para disparar OnUserDataChanged.
        user.ProfileImageUrl = newUrl;
        UserDataStore.CurrentUserData = user;

        Debug.Log($"[AvatarSelection] Preview aplicado: '{newUrl}'");
    }

    public async Task CommitSessionAsync()
    {
        if (!_sessionActive)
        {
            Debug.LogWarning("[AvatarSelection] CommitSessionAsync chamado sem sessão ativa. Ignorando.");
            return;
        }

        // Encerra a sessão antes de persistir para que chamadas subsequentes
        // (por ex. um tap tardio) não reabram o fluxo acidentalmente.
        _sessionActive = false;

        if (_pendingUrl == _originalUrl)
        {
            Debug.Log("[AvatarSelection] Avatar não mudou. Commit no-op.");
            return;
        }

        var user = UserDataStore.CurrentUserData;
        if (user == null || string.IsNullOrEmpty(user.UserId))
        {
            Debug.LogWarning("[AvatarSelection] Commit sem usuário válido em memória. Abortando persistência.");
            return;
        }

        // Garantia: objeto in-memory reflete o pending antes da persistência.
        if (user.ProfileImageUrl != _pendingUrl)
        {
            user.ProfileImageUrl = _pendingUrl;
            UserDataStore.CurrentUserData = user;
        }

        // 1) LiteDB — fonte da verdade local. Bloqueante.
        try
        {
            user.SavedAt = DateTime.Now;
            _userDataLocal.UpdateUser(user);
            Debug.Log($"[AvatarSelection] LiteDB atualizado: '{_pendingUrl}'");
        }
        catch (Exception e)
        {
            Debug.LogError($"[AvatarSelection] Falha ao atualizar LiteDB: {e.Message}. Abortando commit.");
            return;
        }

        // 2) Firestore — replica remota. Se falhar, marca dirty para retry posterior.
        try
        {
            await _firestore.UpdateUserProfileImageUrl(user.UserId, _pendingUrl);
            Debug.Log($"[AvatarSelection] Firestore atualizado: '{_pendingUrl}'");
        }
        catch (Exception e)
        {
            Debug.LogError($"[AvatarSelection] Falha ao atualizar Firestore: {e.Message}. " +
                           "LiteDB permanece atualizado e registro marcado como dirty.");
            try
            {
                _userDataLocal.MarkAsDirty(user.UserId);
            }
            catch (Exception markEx)
            {
                Debug.LogError($"[AvatarSelection] Falha ao marcar dirty: {markEx.Message}");
            }
        }
    }
}
