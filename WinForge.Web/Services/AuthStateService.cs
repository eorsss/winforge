using Microsoft.JSInterop;
using WinForge.Shared.DTOs;

namespace WinForge.Web.Services;

public interface IAuthStateService
{
    AuthResponse? CurrentUser { get; }
    bool IsLoggedIn { get; }
    event Action? OnChange;
    Task InitializeAsync();
    Task LoginAsync(AuthResponse response);
    Task LogoutAsync();
}

public class AuthStateService(IJSRuntime js, IApiClient apiClient) : IAuthStateService
{
    private const string StorageKey = "winforge_auth";

    public AuthResponse? CurrentUser { get; private set; }
    public bool IsLoggedIn => CurrentUser != null;
    public event Action? OnChange;

    public async Task InitializeAsync()
    {
        try
        {
            var json = await js.InvokeAsync<string?>("localStorage.getItem", StorageKey);
            if (!string.IsNullOrEmpty(json))
            {
                var auth = System.Text.Json.JsonSerializer.Deserialize<AuthResponse>(json);
                if (auth != null && auth.ExpiresAt > DateTime.UtcNow)
                {
                    CurrentUser = auth;
                    apiClient.SetAuthToken(auth.AccessToken);
                    NotifyStateChanged();
                }
            }
        }
        catch { /* ignore storage errors */ }
    }

    public async Task LoginAsync(AuthResponse response)
    {
        CurrentUser = response;
        apiClient.SetAuthToken(response.AccessToken);
        var json = System.Text.Json.JsonSerializer.Serialize(response);
        await js.InvokeVoidAsync("localStorage.setItem", StorageKey, json);
        NotifyStateChanged();
    }

    public async Task LogoutAsync()
    {
        CurrentUser = null;
        await js.InvokeVoidAsync("localStorage.removeItem", StorageKey);
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
