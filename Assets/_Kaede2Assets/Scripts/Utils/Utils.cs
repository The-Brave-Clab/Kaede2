namespace Kaede2.Utils
{
    public static class CommonUtils
    {
        public static string BytesToHumanReadable(double bytes)
        {
            string[] suffix = { "B", "KB", "MB", "GB", "TB" };
            int i = 0;
            double dblSByte = bytes;
            while (dblSByte >= 1024 * 0.8 && i < suffix.Length - 1)
            {
                dblSByte /= 1024;
                i++;
            }
            return $"{dblSByte:F2} {suffix[i]}";
        }
    }
}