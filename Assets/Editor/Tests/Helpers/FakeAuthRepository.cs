using System.Threading.Tasks;

public class FakeAuthRepository : IAuthRepository
{
    private string _currentUserId;
    private bool _isLoggedIn;
    private bool _hasLocalSession;

    // Controle de falhas simuladas
    private bool _reloadShouldFail;
    private bool _checkAuthShouldFail;
    private bool _signInShouldFail;

    // Dados configuráveis
    private string   _userIdForNextRegistration;
    private UserData _userDataForSignIn;

    // Contadores
    public int    LogoutCallCount     { get; private set; }
    public int    ReloadCallCount     { get; private set; }
    public int    SignInCallCount     { get; private set; }
    public int    RegisterCallCount   { get; private set; }
    public string LastSignInEmail     { get; private set; }

    // -------------------------------------------------------
    // Configuração do fake
    // -------------------------------------------------------

    public void SetLoggedInUser(string userId)
    {
        _currentUserId   = userId;
        _isLoggedIn      = true;
        _hasLocalSession = true;
    }

    public void SetOfflineWithLocalSession(string userId)
    {
        _currentUserId    = userId;
        _isLoggedIn       = true;
        _hasLocalSession  = true;
        _reloadShouldFail = true;
    }

    public void SetLoggedOut()
    {
        _currentUserId    = null;
        _isLoggedIn       = false;
        _hasLocalSession  = false;
        _reloadShouldFail = false;
    }

    public void SetExpiredSession(string userId)
    {
        _currentUserId        = userId;
        _isLoggedIn           = false;
        _hasLocalSession      = true;
        _reloadShouldFail     = true;
        _checkAuthShouldFail  = true;
    }

    /// <summary>
    /// Define o userId que será atribuído após RegisterUserAsync.
    /// Necessário para que GetUserData encontre o usuário no FakeFirestoreRepository.
    /// </summary>
    public void SetUserIdForNextRegistration(string userId)
        => _userIdForNextRegistration = userId;

    /// <summary>
    /// Define o UserData retornado por SignInWithEmailAsync.
    /// </summary>
    public void SetUserDataForSignIn(UserData userData)
        => _userDataForSignIn = userData;

    /// <summary>
    /// Faz SignInWithEmailAsync lançar Exception (simula credenciais erradas).
    /// </summary>
    public void SetSignInShouldFail(bool shouldFail)
        => _signInShouldFail = shouldFail;

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
        SignInCallCount++;
        LastSignInEmail = email;

        if (_signInShouldFail)
            throw new System.Exception("Credenciais inválidas (simulado)");

        _isLoggedIn      = true;
        _hasLocalSession = true;

        var userData = _userDataForSignIn ?? new UserData
        {
            UserId   = _currentUserId ?? "fake-user-id",
            Email    = email,
            NickName = "FakeUser",
            Name     = "Fake User"
        };
        return Task.FromResult(userData);
    }

    public Task<UserData> RegisterUserAsync(string name, string nickName, string email, string password)
    {
        RegisterCallCount++;
        _currentUserId   = _userIdForNextRegistration ?? "new-fake-user-id";
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
        LogoutCallCount++;
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