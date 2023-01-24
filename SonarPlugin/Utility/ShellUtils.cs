using System.Diagnostics;

namespace SonarPlugin.Utility
{
    public static class ShellUtils
    {
        public static void ShellExecute(string filename)
        {
            ProcessStartInfo startInfo = new(filename)
            {
                UseShellExecute = true,
            };
            Process.Start(startInfo)?.Dispose();
        }
    }
}
