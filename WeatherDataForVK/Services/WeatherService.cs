using WeatherDataForVK.Models;
using WeatherDataForVK.Providers;

namespace WeatherDataForVK.Services;

public class WeatherService
{
    private readonly IWeatherProvider _httpProvider;
    private readonly IWeatherProvider _curlProvider;
    private IWeatherProvider _activeProvider;

    public WeatherService()
    {
        _httpProvider = new HttpWeatherProvider();
        _curlProvider = new CurlWeatherProvider();
        _activeProvider = _httpProvider;
    }

    public string ActiveProviderName => _activeProvider.Name;

    public async Task DetectProviderAsync()
    {
        Console.WriteLine("[Transport] Проверка HttpClient...");

        if (await _httpProvider.ProbeAsync())
        {
            _activeProvider = _httpProvider;
            Console.WriteLine("[Transport] Используем HttpClient");
            return;
        }

        _activeProvider = _curlProvider;
        Console.WriteLine("[Transport] HttpClient недоступен — работаем через curl");
    }

    public async Task<WeatherData?> GetWeatherAsync(string city, int maxAttempts = 3)
    {
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                string json = await _activeProvider.FetchJsonAsync(city);
                var data = WeatherResponseParser.Parse(city, json);

                if (data != null)
                    return data;

                Console.WriteLine($"[{city}] Попытка {attempt}/{maxAttempts}: не удалось разобрать");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{city}] Попытка {attempt}/{maxAttempts}: {ex.Message}");
            }

            if (attempt < maxAttempts)
                await Task.Delay(2000);
        }

        return null;
    }
}