
using System;
using System.Diagnostics;
using System.IO;

namespace ArxOne.Debian;

public class Gpg : IDisposable
{
    private readonly string _gpgPath;
    private readonly string _gpgDir;
    private readonly string _homeDir;
    private readonly string _tempDir;

    public Gpg(string gpgPath)
    {
        _gpgPath = gpgPath;
        _gpgDir = Path.Combine(Path.GetTempPath(), $"GPG-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_gpgDir);
        _homeDir = Path.Combine(_gpgDir, "gnupg");
        Directory.CreateDirectory(_homeDir);
        _tempDir = Path.Combine(_gpgDir, "temp");
        Directory.CreateDirectory(_tempDir);
    }

    protected virtual void Dispose(bool disposing)
    {
        try
        {
            Directory.Delete(_gpgDir, true);
        }
        // ReSharper disable once EmptyGeneralCatchClause
#pragma warning disable S2486
#pragma warning disable S108
        // ReSharper disable once CatchAllClause
        catch { }
#pragma warning restore S2486
#pragma warning restore S108
    }

    #region destructor
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Dispose(true);
    }

    ~Gpg()
    {
        Dispose(false);
    }
    #endregion

    private string GetTemp()
    {
        return Path.Combine(_tempDir, Guid.NewGuid().ToString("N"));
    }

    public void LoadPrivateKey(string armoredAsciiKey)
    {
        var path = GetTemp();
        File.WriteAllText(path, armoredAsciiKey);
        Invoke($"--import \"{path}\"");
        File.Delete(path);
    }

    private static Process Start(string path, string arguments)
    {
        var psi = new ProcessStartInfo(path, arguments) { CreateNoWindow = true, UseShellExecute = false };
        return Process.Start(psi);
    }

    public int Invoke(string arguments)
    {
        var gpg = Start(_gpgPath, $"--homedir \"{_homeDir}\" {arguments}");
        gpg.WaitForExit();
        return gpg.ExitCode;
    }

    public int Invoke(string arguments, ref string? outputPath)
    {
        outputPath ??= GetTemp();
        return Invoke($" --output={outputPath} {arguments}");
    }
}
