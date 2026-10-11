namespace WeatherDataForVK.Providers;

public class HttpWeatherProvider : IWeatherProvider
{
    private readonly HttpClient _http;

    public string Name => "HttpClient";

    public HttpWeatherProvider()
    {
        _http = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(15)
        };

        // wttr.in без User-Agent тормозит
        _http.DefaultRequestHeaders.UserAgent.ParseAdd("WeatherReportTool/1.0");
    }

    public async Task<string> FetchJsonAsync(string city)
    {
        string url = $"https://wttr.in/{Uri.EscapeDataString(city)}?format=j1";
        return await _http.GetStringAsync(url);
    }

    public async Task<bool> ProbeAsync()
    {
        try
        {
            string json = await FetchJsonAsync("Moscow");

            // попытка распарсить целиком
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            return doc.RootElement.TryGetProperty("current_condition", out _)
                && doc.RootElement.TryGetProperty("nearest_area", out _);
        }
        catch
        {
            return false;
        }
    }
}