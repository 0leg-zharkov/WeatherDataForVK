using System.Diagnostics;
using System.IO;

namespace WeatherDataForVK.Providers;

public class CurlWeatherProvider : IWeatherProvider
{
    private const string CurlPath = @"C:\Windows\System32\curl.exe";

    public string Name => "curl";

    public async Task<string> FetchJsonAsync(string city)
    {
        string url = $"https://wttr.in/{Uri.EscapeDataString(city)}?format=j1";
        return await RunCurlAsync(url);
    }

    public Task<bool> ProbeAsync()
    {
        return Task.FromResult(File.Exists(CurlPath));
    }

    private async Task<string> RunCurlAsync(string url)
    {
        string tempFile = Path.Combine(Path.GetTempPath(), $"wttr_{Guid.NewGuid():N}.json");

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = CurlPath,
                /* нужны только первые ~2 КБ с полями temp_C и country
                 если curl не успеет закрыть соединение, файл всё равно будет с данными*/
                Arguments = $"--ssl-no-revoke --max-time 8 -s -o \"{tempFile}\" \"{url}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var proc = Process.Start(psi)
                ?? throw new Exception("Не удалось запустить curl.exe");

            var waitTask = proc.WaitForExitAsync();
            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(13));
            await Task.WhenAny(waitTask, timeoutTask);

            if (!proc.HasExited)
                try { proc.Kill(entireProcessTree: true); } catch { }

            if (!File.Exists(tempFile))
                throw new Exception($"curl не создал файл (exit code {proc.ExitCode})");

            return await File.ReadAllTextAsync(tempFile);
        }
        finally
        {
            try { if (File.Exists(tempFile)) File.Delete(tempFile); } catch { }
        }
    }
}