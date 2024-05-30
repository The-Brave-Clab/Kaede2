using System.Globalization;
using System.Linq;
using Kaede2.Localization;

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

        public static bool BelongsTo(this CultureInfo thisCulture, CultureInfo thatCulture)
        {
            if (thatCulture == null)
                return false;
    
            if (Equals(thisCulture, CultureInfo.InvariantCulture))
                return Equals(thatCulture, CultureInfo.InvariantCulture);

            if (Equals(thatCulture, CultureInfo.InvariantCulture))
                return true;

            return thisCulture.Equals(thatCulture) || thisCulture.Parent.BelongsTo(thatCulture);
        }

        public static CultureInfo GetSystemLocaleOrDefault()
        {
            var locales = LocalizationManager.AllLocales;
            return locales.FirstOrDefault(l => CultureInfo.CurrentCulture.BelongsTo(l)) ??
                   locales[0];
        }

        // a mod function that works with negative numbers
        // Mod(-1, 3) == 2; Mod(1, 3) == 1
        public static int Mod(int x, int m)
        {
            if (m == 0) return x;
            return (x % m + m) % m;
        }
    }
}