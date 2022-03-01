using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace deployer;

public class Application
{
  private const string ManifestFileName = "manifest.txt";
  private const string AppKeyName = "name";
  private const string AppKeyNameDefault = "MyApp";
  private const string AppKeyDir = "main_dir";
  private const string AppKeyDirDefault = "MyApp";
  private const string AppKeyVersion = "version";
  private const string AppKeyExe = "exe";
  private const string AppKeyVersionDefault = "1.0";
  private const string AppKeyCreateLink = "create_link";
  private const string ResourceNameManifest = "deployer.source.manifest.txt";
  private const string ResourceNameZip = "deployer.source.data.zip";

  /// <summary>
  /// </summary>
  public Application()
  {
    var localManifest = GetInternalFileAsText(ResourceNameManifest)
      .Replace("\r\n", "\n")
      .Split('\n')
      .ToList()
      .Select(x => x.Split('='))
      .ToDictionary(x => x[0], y => y[1]);

    var appDir = localManifest.ContainsKey(AppKeyDir) ? localManifest[AppKeyDir] : AppKeyDirDefault;
    AppName = localManifest.ContainsKey(AppKeyName) ? localManifest[AppKeyName] : AppKeyNameDefault;
    AppVersion = new Version(localManifest.ContainsKey(AppKeyVersion) ? localManifest[AppKeyVersion] : AppKeyVersionDefault);
    AppExecutable = localManifest[AppKeyExe];
    AppPath = appDir.Contains(":") ? appDir : Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\" + appDir;
    AppZip = GetInternalFileAsMemoryStream(ResourceNameZip);
    CreateLink = localManifest.ContainsKey(AppKeyCreateLink) && localManifest[AppKeyCreateLink] == "true";
  }


  private string AppName { get; }
  private string AppPath { get; }
  private Version AppVersion { get; }
  private bool CreateLink { get; }
  private Stream AppZip { get; }
  private string AppExecutable { get; }

  /// <summary>
  /// </summary>
  public void Copy()
  {
    var remoteManifestPath = new FileInfo(AppPath + @$"\{ManifestFileName}");
    var fullPathAppExecutable = AppPath + @$"\{AppExecutable}";
    if (remoteManifestPath.Exists)
    {
      var oldManifest = File.ReadAllText(remoteManifestPath.FullName)
        .Replace("\r\n", "\n")
        .Split('\n')
        .ToList()
        .Select(x => x.Split('='))
        .ToDictionary(x => x[0], y => y[1]);

      var oldVersion = new Version(oldManifest.ContainsKey(AppKeyVersion) ? oldManifest[AppKeyVersion] : AppKeyVersionDefault);
      if (AppVersion > oldVersion)
      {
        Directory.Delete(AppPath, true);
        UpdateDir();
      }
    }
    else
    {
      UpdateDir();
    }


    Process.Start(fullPathAppExecutable);
  }

  private static void AppShortcutToDesktop(string linkName, string app)
  {
    var deskDir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
    var appName = app.Replace('\\', '/');

    var t = Type.GetTypeFromCLSID(new Guid("72C24DD5-D70A-438B-8A42-98424B88AFB8")); //Windows Script Host Shell Object
    dynamic shell = Activator.CreateInstance(t);
    try
    {
      var lnk = shell.CreateShortcut(deskDir + "\\" + linkName + ".lnk");
      try
      {
        lnk.TargetPath = appName;
        lnk.IconLocation = appName;
        lnk.Save();
      }
      finally
      {
        Marshal.FinalReleaseComObject(lnk);
      }
    }
    finally
    {
      Marshal.FinalReleaseComObject(shell);
    }
  }

  private void UpdateDir()
  {
    var fullPathAppExecutable = AppPath + @$"\{AppExecutable}";
    var remoteManifestPath = new FileInfo(AppPath + @$"\{ManifestFileName}");
    var archive = new ZipArchive(AppZip);
    archive.ExtractToDirectory(AppPath);
    File.WriteAllText(remoteManifestPath.FullName, GetInternalFileAsText(ResourceNameManifest));
    if (CreateLink)
      AppShortcutToDesktop(AppName, fullPathAppExecutable);
  }


  private static string GetInternalFileAsText(string name)
  {
    var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
    if (stream == null)
      throw new Exception("Internal file does not exist");
    return new StreamReader(stream).ReadToEnd();
  }

  private static Stream GetInternalFileAsMemoryStream(string name)
  {
    var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
    if (stream == null)
      throw new Exception("Internal file does not exist");
    return stream;
  }
}