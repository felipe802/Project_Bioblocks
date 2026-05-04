using System.Threading.Tasks;

/// <summary>
/// Orquestra a seleção de avatar preset pelo usuário.
///
/// Fluxo esperado:
///   1. BeginSession()                — snapshot do avatar atual
///   2. PreviewSelection(avatarId)    — 0..N vezes conforme o usuário navega no catálogo
///   3. CommitSessionAsync()          — persiste o resultado final em LiteDB e Firestore
///
/// Preview atualiza apenas o estado in-memory (UserDataStore.CurrentUserData),
/// para que o UserHeaderManager e demais consumidores reajam imediatamente via
/// OnUserDataChanged. Nada é persistido em disco ou rede até o Commit.
///
/// LiteDB é a fonte da verdade local. Se o push ao Firestore falhar, o registro
/// local permanece atualizado e é marcado como dirty para reconciliação futura.
/// </summary>
public interface IAvatarSelectionService
{
    /// <summary>
    /// Inicia uma nova sessão de seleção. Captura o ProfileImageUrl atual como
    /// baseline para comparação no commit. Deve ser chamado toda vez que o modal
    /// abre, para refletir possíveis mudanças vindas de outras fontes.
    /// </summary>
    void BeginSession();

    /// <summary>
    /// Aplica uma pré-seleção in-memory. Atualiza
    /// <see cref="UserDataStore.CurrentUserData"/> com <c>preset:{avatarId}</c>
    /// e dispara <see cref="UserDataStore.OnUserDataChanged"/>. Não persiste.
    /// </summary>
    /// <param name="avatarId">Id válido do catálogo (ex.: <c>avatar_dna_03</c>).</param>
    void PreviewSelection(string avatarId);

    /// <summary>
    /// Finaliza a sessão persistindo o avatar atualmente em preview:
    ///   1) LiteDB (bloqueante, fonte da verdade local);
    ///   2) Firestore (async; marca registro como dirty em caso de falha).
    /// No-op se o preview não diverge do baseline capturado em BeginSession.
    /// </summary>
    Task CommitSessionAsync();
}
