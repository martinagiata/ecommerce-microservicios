using Orders.Models;

namespace Orders.Services;

public class UsersService
{
    private readonly HttpClient _httpClient;
    private readonly string _usersBaseUrl;

    public UsersService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _usersBaseUrl = config["ServicesUrls:Users"] ?? "http://localhost:5161";
    }

    public async Task<UserInfo?> GetUserByIdAsync(long userId)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<UserInfo>(
                $"{_usersBaseUrl}/users/{userId}");
            return response;
        }
        catch
        {
            return null;
        }
    }
}
