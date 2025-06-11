using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

public class UserSessionService
{
    private readonly ProtectedSessionStorage _sessionStorage;

    public string? Username { get; private set; }
    public bool IsAdmin { get; private set; }

    public event Action? OnChange;
    private void NotifyStateChanged() => OnChange?.Invoke();

    public UserSessionService(ProtectedSessionStorage sessionStorage)
    {
        _sessionStorage = sessionStorage;
    }

    public async Task InitializeAsync()
    {
        var usernameResult = await _sessionStorage.GetAsync<string>("Username");
        if (usernameResult.Success)
            Username = usernameResult.Value;

        var isAdminResult = await _sessionStorage.GetAsync<bool>("IsAdmin");
        if (isAdminResult.Success)
            IsAdmin = isAdminResult.Value;
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        if (username == "admin" && password == "admin")
        {
            Username = username;
            IsAdmin = true;
            await _sessionStorage.SetAsync("Username", username);
            await _sessionStorage.SetAsync("IsAdmin", true);
            NotifyStateChanged();
            return true;
        }
        return false;
    }

    public async Task LogoutAsync()
    {
        Username = null;
        IsAdmin = false;
        await _sessionStorage.DeleteAsync("Username");
        await _sessionStorage.DeleteAsync("IsAdmin");
        NotifyStateChanged();
    }
}
