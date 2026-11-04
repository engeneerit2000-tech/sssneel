using System.Globalization;
using Microsoft.Extensions.Localization;

namespace SNEL.WalletPlatform.Application.Resources
{


    public static class StringLocalizerExtensions
    {
        public static string GetStringForCulture(this IStringLocalizer localizer, CultureInfo culture, string key)
        {
            var originalCulture = CultureInfo.CurrentUICulture;

            try
            {
                CultureInfo.CurrentUICulture = culture;
                return localizer[key];
            }
            finally
            {
                CultureInfo.CurrentUICulture = originalCulture;
            }
        }
    }
}