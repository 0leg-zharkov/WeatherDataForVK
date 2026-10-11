using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using WeatherDataForVK.Models;

namespace WeatherDataForVK.Services;

public static class WeatherResponseParser
{
    // регулярки на случай обрезанного JSON, полный парсер на неполном падает
    private static readonly Regex TempRegex = new(
        @"""temp_C""\s*:\s*""([^""]+)""", RegexOptions.Compiled);

    private static readonly Regex CountryRegex = new(
        @"""country""\s*:\s*\[\s*\{\s*""value""\s*:\s*""([^""]+)""", RegexOptions.Compiled);

    public static WeatherData? Parse(string city, string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        // wttr.in при перегрузке иногда отдаёт HTML вместо JSON
        if (json.TrimStart().StartsWith("<"))
            return null;

        try
        {
            return ParseFullJson(city, json);
        }
        catch (JsonException)
        {
            // curl прервался по таймауту, но успел выкачать нужные поля
            return ParseTruncatedJson(city, json);
        }
    }

    
    private static WeatherData ParseFullJson(string city, string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var current = root.GetProperty("current_condition")[0];
        double temp = double.Parse(
            current.GetProperty("temp_C").GetString() ?? "0",
            CultureInfo.InvariantCulture);

        var nearest = root.GetProperty("nearest_area")[0];
        string country = nearest.GetProperty("country")[0]
                                .GetProperty("value").GetString() ?? "Unknown";

        return new WeatherData
        {
            City = city,
            Country = country,
            TemperatureCelsius = temp
        };
    }

    // достать поля регулярками
    private static WeatherData? ParseTruncatedJson(string city, string json)
    {
        var tempMatch = TempRegex.Match(json);
        var countryMatch = CountryRegex.Match(json);

        if (!tempMatch.Success || !countryMatch.Success)
            return null;

        return new WeatherData
        {
            City = city,
            Country = countryMatch.Groups[1].Value,
            TemperatureCelsius = double.Parse(
                tempMatch.Groups[1].Value,
                CultureInfo.InvariantCulture)
        };
    }
}