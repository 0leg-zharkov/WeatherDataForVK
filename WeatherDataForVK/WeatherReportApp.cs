using WeatherDataForVK.Models;
using WeatherDataForVK.Services;

namespace WeatherDataForVK;

public class WeatherReportApp
{
    private const string CitiesFile = "cities.txt";
    private const int DelayBetweenCitiesMs = 3000;

    public async Task<int> RunAsync()
    {
        if (!File.Exists(CitiesFile))
        {
            Console.WriteLine($"Файл {CitiesFile} не найден");
            return 1;
        }

        var cities = LoadCities();
        Console.WriteLine($"Загружено уникальных городов: {cities.Count}\n");

        var service = new WeatherService();
        await service.DetectProviderAsync();
        await Task.Delay(2000);  // чтобы часто не стучаться

        var results = new List<WeatherData>();

        foreach (var city in cities)
        {
            var data = await service.GetWeatherAsync(city);
            if (data == null)
            {
                Console.WriteLine($"[{city}] не удалось получить данные");
                continue;
            }

            results.Add(data);
            Console.WriteLine($"{data.City}, {data.Country} {data.TemperatureCelsius:+0;-0;0} °C");

            await Task.Delay(DelayBetweenCitiesMs);
        }

        PrintStatistics(results);
        return 0;
    }

    private static List<string> LoadCities()
    {
        return File.ReadAllLines(CitiesFile)
            .Select(line => line.Trim().TrimStart('\uFEFF')) 
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static void PrintStatistics(List<WeatherData> results)
    {
        Console.WriteLine("\n--- Статистика по странам ---");

        foreach (var group in results.GroupBy(r => r.Country).OrderBy(g => g.Key))
        {
            var temps = group.Select(g => g.TemperatureCelsius).ToList();
            string citiesWord = group.Count() == 1 ? "city" : "cities";

            Console.WriteLine(
                $"{group.Key} — {group.Count()} {citiesWord}, " +
                $"avg: {temps.Average():+0.0;-0.0;0.0} °C, " +
                $"min: {temps.Min():+0;-0;0} °C, " +
                $"max: {temps.Max():+0;-0;0} °C");
        }
    }
}