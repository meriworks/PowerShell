namespace Meriworks.PowerShell.Tests
{
    public static class StringUtil
    {
        public static string FixLineEndings(string s)
        {
            if (s == null) return null;
            return s.Replace("\r\n", "\n");
        }
    }
}