using System.Security.Cryptography;
using System.Text.Json;

namespace MTUpdater;

/// <summary>
/// Stand-alone updater — never modifies the running POS process files directly.
/// Commands: launch | download | apply | status
/// </summary>
internal static class Program
{
    private const string ExeName = "mt_softs.exe";
    private const string ActiveVersionFile = "active_version.json";
    private const string PendingUpdateFile = "pending_update.json";

    static int Main(string[] args)
    {
        try
        {
            if (args.Length == 0)
            {
                PrintUsage();
                return 1;
            }

            var installDir = ResolveInstallDir(args);
            var command = args[0].ToLowerInvariant();

            return command switch
            {
                "launch" => Launch(installDir),
                "download" => Download(installDir, args),
                "apply" => Apply(installDir),
                "status" => Status(installDir),
                _ => Unknown(command),
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"MTUpdater error: {ex.Message}");
            return 1;
        }
    }

    private static int Unknown(string command)
    {
        Console.Error.WriteLine($"Unknown command: {command}");
        PrintUsage();
        return 1;
    }

    private static void PrintUsage()
    {
        Console.WriteLine("MTUpdater — safe side-by-side desktop updates");
        Console.WriteLine("  MTUpdater launch   [--install-dir path]");
        Console.WriteLine("  MTUpdater download --version 1.0.11 --build 101 --url <zip> --sha256 <hex> [--install-dir path]");
        Console.WriteLine("  MTUpdater apply    [--install-dir path]");
        Console.WriteLine("  MTUpdater status   [--install-dir path]");
    }

    private static string ResolveInstallDir(string[] args)
    {
        for (var i = 0; i < args.Length - 1; i++)
        {
            if (args[i].Equals("--install-dir", StringComparison.OrdinalIgnoreCase))
                return Path.GetFullPath(args[i + 1]);
        }
        return AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    private static string VersionsDir(string installDir) => Path.Combine(installDir, "Versions");

    private static string FolderName(string version, int build) => $"{version}_{build}";

    private static string VersionPath(string installDir, string folderName) =>
        Path.Combine(VersionsDir(installDir), folderName);

    private static ActiveVersionState LoadActive(string installDir)
    {
        var path = Path.Combine(installDir, ActiveVersionFile);
        if (!File.Exists(path))
        {
            return new ActiveVersionState { Active = ".", Previous = null };
        }
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<ActiveVersionState>(json) ?? new ActiveVersionState { Active = "." };
    }

    private static void SaveActive(string installDir, ActiveVersionState state)
    {
        var path = Path.Combine(installDir, ActiveVersionFile);
        var json = JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, json);
    }

    private static string ResolveExePath(string installDir, string activeFolder)
    {
        if (activeFolder == "." || string.IsNullOrWhiteSpace(activeFolder))
            return Path.Combine(installDir, ExeName);
        return Path.Combine(VersionPath(installDir, activeFolder), ExeName);
    }

    private static int Launch(string installDir)
    {
        var active = LoadActive(installDir);
        var exe = ResolveExePath(installDir, active.Active);
        if (!File.Exists(exe))
        {
            // Fallback: root exe (legacy install before side-by-side).
            exe = Path.Combine(installDir, ExeName);
        }
        if (!File.Exists(exe))
        {
            Console.Error.WriteLine($"POS executable not found: {exe}");
            return 1;
        }

        var psi = new System.Diagnostics.ProcessStartInfo
        {
            FileName = exe,
            WorkingDirectory = Path.GetDirectoryName(exe)!,
            UseShellExecute = true,
        };
        System.Diagnostics.Process.Start(psi);
        return 0;
    }

    private static int Download(string installDir, string[] args)
    {
        string? version = null, url = null, sha256 = null;
        var build = 0;
        for (var i = 1; i < args.Length - 1; i++)
        {
            switch (args[i].ToLowerInvariant())
            {
                case "--version": version = args[++i]; break;
                case "--build": build = int.Parse(args[++i]); break;
                case "--url": url = args[++i]; break;
                case "--sha256": sha256 = args[++i]; break;
            }
        }

        if (string.IsNullOrWhiteSpace(version) || build <= 0 ||
            string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(sha256))
        {
            Console.Error.WriteLine("download requires --version --build --url --sha256");
            return 1;
        }

        var folder = FolderName(version, build);
        var targetDir = VersionPath(installDir, folder);
        var versionsRoot = VersionsDir(installDir);
        Directory.CreateDirectory(versionsRoot);

        if (Directory.Exists(targetDir) && File.Exists(Path.Combine(targetDir, ExeName)))
        {
            Console.WriteLine($"Version already present: {folder}");
            WritePending(installDir, folder, version, build, sha256);
            return 0;
        }

        var tempZip = Path.Combine(Path.GetTempPath(), $"mtsofts-{folder}-{Guid.NewGuid():N}.zip");
        var tempExtract = Path.Combine(Path.GetTempPath(), $"mtsofts-extract-{Guid.NewGuid():N}");

        try
        {
            Console.WriteLine($"Downloading {url} ...");
            using (var client = new HttpClient { Timeout = TimeSpan.FromMinutes(30) })
            using (var response = client.GetAsync(url).GetAwaiter().GetResult())
            {
                response.EnsureSuccessStatusCode();
                using var fs = File.Create(tempZip);
                response.Content.CopyToAsync(fs).GetAwaiter().GetResult();
            }

            var hash = ComputeSha256(tempZip);
            if (!hash.Equals(sha256, StringComparison.OrdinalIgnoreCase))
            {
                Console.Error.WriteLine($"SHA-256 mismatch. Expected {sha256}, got {hash}");
                return 1;
            }

            Console.WriteLine("SHA-256 verified.");

            if (Directory.Exists(targetDir))
                Directory.Delete(targetDir, true);
            Directory.CreateDirectory(tempExtract);
            System.IO.Compression.ZipFile.ExtractToDirectory(tempZip, tempExtract);

            // Zip may contain files at root or in a single subfolder.
            var sourceDir = tempExtract;
            var children = Directory.GetDirectories(tempExtract);
            if (children.Length == 1 && !File.Exists(Path.Combine(tempExtract, ExeName)))
                sourceDir = children[0];

            Directory.CreateDirectory(targetDir);
            CopyDirectory(sourceDir, targetDir);

            if (!File.Exists(Path.Combine(targetDir, ExeName)))
            {
                Console.Error.WriteLine("Downloaded package does not contain mt_softs.exe");
                return 1;
            }

            WritePending(installDir, folder, version, build, sha256);
            Console.WriteLine($"Ready for restart: {folder}");
            return 0;
        }
        finally
        {
            try { if (File.Exists(tempZip)) File.Delete(tempZip); } catch { }
            try { if (Directory.Exists(tempExtract)) Directory.Delete(tempExtract, true); } catch { }
        }
    }

    private static void WritePending(string installDir, string folder, string version, int build, string sha256)
    {
        var pending = new PendingUpdateState
        {
            FolderName = folder,
            AppVersion = version,
            BuildNumber = build,
            Sha256 = sha256,
            DownloadedAt = DateTime.UtcNow,
        };
        var path = Path.Combine(installDir, PendingUpdateFile);
        File.WriteAllText(path, JsonSerializer.Serialize(pending, new JsonSerializerOptions { WriteIndented = true }));
    }

    private static int Apply(string installDir)
    {
        var pendingPath = Path.Combine(installDir, PendingUpdateFile);
        if (!File.Exists(pendingPath))
        {
            Console.WriteLine("No pending update.");
            return 0;
        }

        var pending = JsonSerializer.Deserialize<PendingUpdateState>(File.ReadAllText(pendingPath));
        if (pending == null || string.IsNullOrWhiteSpace(pending.FolderName))
        {
            Console.Error.WriteLine("Invalid pending_update.json");
            return 1;
        }

        var newDir = VersionPath(installDir, pending.FolderName);
        var newExe = Path.Combine(newDir, ExeName);
        if (!File.Exists(newExe))
        {
            Console.Error.WriteLine($"Target version missing: {newDir}");
            return 1;
        }

        var active = LoadActive(installDir);
        var previous = active.Active;
        if (previous == pending.FolderName)
        {
            File.Delete(pendingPath);
            return 0;
        }

        active.Previous = previous;
        active.Active = pending.FolderName;
        active.AppVersion = pending.AppVersion;
        active.BuildNumber = pending.BuildNumber;
        active.SwitchedAt = DateTime.UtcNow;
        SaveActive(installDir, active);
        File.Delete(pendingPath);

        Console.WriteLine($"Switched active version to {pending.FolderName} (previous: {previous ?? "none"})");
        return 0;
    }

    private static int Status(string installDir)
    {
        var active = LoadActive(installDir);
        Console.WriteLine(JsonSerializer.Serialize(new
        {
            active.Active,
            active.Previous,
            active.AppVersion,
            active.BuildNumber,
            pending = File.Exists(Path.Combine(installDir, PendingUpdateFile)),
        }, new JsonSerializerOptions { WriteIndented = true }));
        return 0;
    }

    private static string ComputeSha256(string filePath)
    {
        using var sha = SHA256.Create();
        using var stream = File.OpenRead(filePath);
        var hash = sha.ComputeHash(stream);
        return Convert.ToHexString(hash);
    }

    private static void CopyDirectory(string source, string dest)
    {
        Directory.CreateDirectory(dest);
        foreach (var file in Directory.GetFiles(source))
        {
            var name = Path.GetFileName(file);
            File.Copy(file, Path.Combine(dest, name), true);
        }
        foreach (var dir in Directory.GetDirectories(source))
        {
            CopyDirectory(dir, Path.Combine(dest, Path.GetFileName(dir)));
        }
    }
}

internal sealed class ActiveVersionState
{
    public string Active { get; set; } = ".";
    public string? Previous { get; set; }
    public string? AppVersion { get; set; }
    public int? BuildNumber { get; set; }
    public DateTime? SwitchedAt { get; set; }
}

internal sealed class PendingUpdateState
{
    public string FolderName { get; set; } = "";
    public string AppVersion { get; set; } = "";
    public int BuildNumber { get; set; }
    public string Sha256 { get; set; } = "";
    public DateTime DownloadedAt { get; set; }
}
