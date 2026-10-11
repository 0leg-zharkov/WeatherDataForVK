namespace WeatherDataForVK.Models;

public class WeatherData
{
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public double TemperatureCelsius { get; set; }
}
