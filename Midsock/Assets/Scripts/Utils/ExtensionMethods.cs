using System.IO;

public static class ExtensionMethods
{
    public static string PathToSceneName(this string path)
    {
        return Path.GetFileNameWithoutExtension(path);
    }
}