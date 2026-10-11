using System.Text;

namespace WeatherDataForVK;

class Program
{
    static async Task<int> Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        try
        {
            WeatherReportApp app = new WeatherReportApp();
            return await app.RunAsync();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Критическая ошибка: {ex.Message}");
            return 1;
        }
    }
}