using System.Threading.Tasks;

public class FakeAuthRepository : IAuthRepository
{
    private string _currentUserId;
    private bool _isLoggedIn;
    private bool _hasLocalSession;

    // Controle de falhas simuladas
    private bool _reloadShouldFail;
    private bool _checkAuthShouldFail;

    // Contadores
    public int LogoutCallCount        { get; private set; }
    public int ReloadCallCount        { get; private set; }
    public string LastSignInEmail     { get; private set; }

    // -------------------------------------------------------
    // Configuração do fake
    // -------------------------------------------------------

    /// <summary>
    /// Usuário logado com sessão local válida (caso normal online).
    /// </summary>
    public void SetLoggedInUser(string userId)
    {
        _currentUserId   = userId;
        _isLoggedIn      = true;
        _hasLocalSession = true; // ← sincroniza
    }

    /// <summary>
    /// Tem token local mas está offline — ReloadAsync vai falhar.
    /// Simula o cenário principal que você quer testar.
    /// </summary>
    public void SetOfflineWithLocalSession(string userId)
    {
        _currentUserId    = userId;
        _isLoggedIn       = true;
        _hasLocalSession  = true;
        _reloadShouldFail = true; // ← simula sem internet
    }

    /// <summary>
    /// Sem sessão nenhuma — nunca logou neste dispositivo.
    /// </summary>
    public void SetLoggedOut()
    {
        _currentUserId    = null;
        _isLoggedIn       = false;
        _hasLocalSession  = false;
        _reloadShouldFail = false;
    }

    /// <summary>
    /// Tem sessão local mas o token expirou.
    /// </summary>
    public void SetExpiredSession(string userId)
    {
        _currentUserId        = userId;
        _isLoggedIn           = false;
        _hasLocalSession      = true;  // SDK ainda tem o token salvo...
        _reloadShouldFail     = true;  // ...mas não consegue renovar
        _checkAuthShouldFail  = true;
    }

    // -------------------------------------------------------
    // IAuthRepository
    // -------------------------------------------------------

    public bool   IsInitialized  => true;
    public string CurrentUserId  => _currentUserId;
    public bool   IsUserLoggedIn()   => _isLoggedIn;
    public bool   HasLocalSession()  => _hasLocalSession;

    public Task InitializeAsync() => Task.CompletedTask;

    public Task ReloadCurrentUserAsync()
    {
        ReloadCallCount++;
        if (_reloadShouldFail)
            throw new System.Exception("Sem internet (simulado)");
        return Task.CompletedTask;
    }

    public Task CheckAuthenticationStatus()
    {
        if (_checkAuthShouldFail)
            throw new System.Exception("Token expirado (simulado)");
        return Task.CompletedTask;
    }

    public Task<UserData> SignInWithEmailAsync(string email, string password)
    {
        LastSignInEmail  = email;
        _isLoggedIn      = true;
        _hasLocalSession = true;
        var fakeUser = new UserData
        {
            UserId   = _currentUserId ?? "fake-user-id",
            Email    = email,
            NickName = "FakeUser",
            Name     = "Fake User"
        };
        return Task.FromResult(fakeUser);
    }

    public Task<UserData> RegisterUserAsync(string name, string nickName, string email, string password)
    {
        _currentUserId   = "new-fake-user-id";
        _isLoggedIn      = true;
        _hasLocalSession = true;
        var fakeUser = new UserData
        {
            UserId   = _currentUserId,
            Email    = email,
            NickName = nickName,
            Name     = name
        };
        return Task.FromResult(fakeUser);
    }

    public Task LogoutAsync()
    {
        LogoutCallCount  ++;
        _currentUserId   = null;
        _isLoggedIn      = false;
        _hasLocalSession = false;
        return Task.CompletedTask;
    }

    public Task DeleteUser(string userId)
    {
        _currentUserId   = null;
        _isLoggedIn      = false;
        _hasLocalSession = false;
        return Task.CompletedTask;
    }

    public Task ReauthenticateUser(string email, string password) => Task.CompletedTask;
}