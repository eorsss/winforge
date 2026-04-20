using System.Net.Http.Json;
using System.Text.Json;
using WinForge.Shared.DTOs;

namespace WinForge.Web.Services;

public interface IApiClient
{
    Task<AuthResponse?> RegisterAsync(RegisterRequest request);
    Task<AuthResponse?> LoginAsync(LoginRequest request);
    Task<CuttingOptimizationResult?> OptimizeProfileCuttingAsync(CuttingOptimizationRequest request);
    void SetAuthToken(string token);
}

public class ApiClient(HttpClient http) : IApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public void SetAuthToken(string token)
    {
        http.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
    {
        try
        {
            var response = await http.PostAsJsonAsync("/api/auth/register", request);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
            return null;
        }
        catch (Exception ex) { Console.WriteLine($"Register error: {ex.Message}"); return null; }
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        try
        {
            var response = await http.PostAsJsonAsync("/api/auth/login", request);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
            return null;
        }
        catch (Exception ex) { Console.WriteLine($"Login error: {ex.Message}"); return null; }
    }

    public async Task<CuttingOptimizationResult?> OptimizeProfileCuttingAsync(CuttingOptimizationRequest request)
    {
        try
        {
            var response = await http.PostAsJsonAsync("/api/optimization/profile-cutting", request);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<CuttingOptimizationResult>(JsonOptions);
            return null;
        }
        catch (Exception ex) { Console.WriteLine($"Optimizer error: {ex.Message}"); return null; }
    }
}
