namespace deployer.Model;

public class Version
{
  public Version(string programVersion)
  {
    ProgramVersion = programVersion;
  }

  public string ProgramVersion { get; }


  public static bool operator <(Version x, Version y)
  {
    return VersionCompare(x.ProgramVersion, y.ProgramVersion) < 0;
  }


  public static bool operator >(Version x, Version y)
  {
    return VersionCompare(x.ProgramVersion, y.ProgramVersion) > 0;
  }


  public static bool operator <=(Version x, Version y)
  {
    return VersionCompare(x.ProgramVersion, y.ProgramVersion) <= 0;
  }


  public static bool operator >=(Version x, Version y)
  {
    return VersionCompare(x.ProgramVersion, y.ProgramVersion) >= 0;
  }

  public static bool operator ==(Version x, Version y)
  {
    return VersionCompare(x.ProgramVersion, y.ProgramVersion) == 0;
  }

  public static bool operator !=(Version x, Version y)
  {
    return VersionCompare(x.ProgramVersion, y.ProgramVersion) != 0;
  }

  private static int VersionCompare(string v1, string v2)
  {
    int vNum1 = 0, vNum2 = 0;
    for (int i = 0, j = 0; i < v1.Length || j < v2.Length;)
    {
      while (i < v1.Length && v1[i] != '.')
      {
        vNum1 = vNum1 * 10 + (v1[i] - '0');
        i++;
      }

      while (j < v2.Length && v2[j] != '.')
      {
        vNum2 = vNum2 * 10 + (v2[j] - '0');
        j++;
      }

      if (vNum1 > vNum2)
        return 1;
      if (vNum2 > vNum1)
        return -1;
      vNum1 = vNum2 = 0;
      i++;
      j++;
    }

    return 0;
  }
}