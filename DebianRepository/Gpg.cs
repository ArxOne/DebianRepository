
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace ArxOne.Debian;

public class Gpg : IDisposable
{
    private readonly string _gpgPath;

    private sealed record LocalDirectories(string Root, string Home, string Temp);

    private readonly object _directoriesLock = new();
    private LocalDirectories? _directories;

    private readonly HashSet<string> _armoredAsciiKeys = new();

    private LocalDirectories Directories
    {
        get
        {
            lock (_directoriesLock)
            {
                if (_directories is null)
                {
                    _directories = GetDirectories();
                    foreach (var armoredAsciiKey in _armoredAsciiKeys)
                        LoadPrivateKey(armoredAsciiKey);
                }

                return _directories;
            }
        }
    }

    private string HomeDir => Directories.Home;
    private string TempDir => Directories.Temp;

    public byte[] PublicKeyBytes
    {
        get
        {
            string? publicKey = null;
            Invoke("--export --armor", ref publicKey);
            return File.ReadAllBytes(publicKey);
        }
    }

    public Gpg(string gpgPath)
    {
        _gpgPath = gpgPath;
    }

    private static LocalDirectories GetDirectories()
    {
        var rootDirectory = Path.Combine(Path.GetTempPath(), $"GPG-{Guid.NewGuid():N}");
        Directory.CreateDirectory(rootDirectory);
        var homeDirectory = Path.Combine(rootDirectory, "gnupg");
        Directory.CreateDirectory(homeDirectory);
        var tempDirectory = Path.Combine(rootDirectory, "temp");
        Directory.CreateDirectory(tempDirectory);
        return new LocalDirectories(rootDirectory, homeDirectory, tempDirectory);
    }

    protected virtual void Dispose(bool disposing)
    {
        Cleanup();
    }

    public void Cleanup()
    {
        lock (_directoriesLock)
        {
            var directories = _directories;
            if (directories is null)
                return;
            Safe(() => Start("gpgconf", $"--homedir \"{HomeDir}\" --kill gpg-agent").WaitForExit(5000));
            Safe(() => Directory.Delete(directories.Root, true));
            _directories = null;
        }
    }

    private void Safe(Action action)
    {
        try
        {
            action();
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
        return Path.Combine(TempDir, Guid.NewGuid().ToString("N"));
    }

    public void AddPrivateKey(string armoredAsciiKey)
    {
        if (_directories is not null)
            LoadPrivateKey(armoredAsciiKey);
        _armoredAsciiKeys.Add(armoredAsciiKey);
    }

    private void LoadPrivateKey(string armoredAsciiKey)
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
        var gpg = Start(_gpgPath, $"--homedir \"{HomeDir}\" {arguments}");
        gpg.WaitForExit();
        return gpg.ExitCode;
    }

    public int Invoke(string arguments, ref string? outputPath)
    {
        outputPath ??= GetTemp();
        return Invoke($" --output={outputPath} {arguments}");
    }
}
