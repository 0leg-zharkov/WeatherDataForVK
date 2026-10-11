namespace WeatherDataForVK.Providers;

public interface IWeatherProvider
{
    string Name { get; }

    Task<string> FetchJsonAsync(string city);

    Task<bool> ProbeAsync();
}