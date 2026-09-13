using System.Globalization;
using Nation.Core.Localization;
using Nation.Core.Models;
using Nation.Core.Utilities;

namespace Nation.Game.UI.Formatting
{
    /// <summary>Player-facing number and date formatting. Every suffix, symbol and pattern comes from the string table.</summary>
    public sealed class UiFormat
    {
        private readonly ILocalizationService _loc;
        private CompactSuffixes _suffixes;
        private string _currency;
        private string _locale;

        public UiFormat(ILocalizationService loc)
        {
            _loc = loc;
            Refresh();
        }

        private void Refresh()
        {
            if (_locale == _loc.CurrentLocale && _currency != null)
            {
                return;
            }

            _locale = _loc.CurrentLocale;
            _suffixes = new CompactSuffixes(
                _loc.Get("number.suffix.thousand"),
                _loc.Get("number.suffix.million"),
                _loc.Get("number.suffix.billion"),
                _loc.Get("number.suffix.trillion"));
            _currency = _loc.Get("currency.symbol");
        }

        public string Money(double value, int decimals = 1)
        {
            Refresh();
            return _currency + NumberFormatting.FormatCompact(value, _suffixes, decimals);
        }

        public string SignedMoney(double value, int decimals = 1)
        {
            Refresh();
            var sign = value >= 0 ? "+" : "-";
            return sign + _currency + NumberFormatting.FormatCompact(System.Math.Abs(value), _suffixes, decimals);
        }

        public string Compact(double value, int decimals = 1)
        {
            Refresh();
            return NumberFormatting.FormatCompact(value, _suffixes, decimals);
        }

        public string Population(long value)
        {
            Refresh();
            return NumberFormatting.FormatCompact(value, _suffixes, value >= 100000000L ? 0 : 1);
        }

        public string Percent(double value, int decimals = 0)
        {
            return value.ToString("F" + decimals, CultureInfo.InvariantCulture) + "%";
        }

        public string SignedPercent(double value, int decimals = 1)
        {
            var sign = value >= 0 ? "+" : "";
            return sign + value.ToString("F" + decimals, CultureInfo.InvariantCulture) + "%";
        }

        public string Index(float value)
        {
            return ((int)System.Math.Round(value)).ToString(CultureInfo.InvariantCulture);
        }

        public string Area(double km2)
        {
            Refresh();
            return _loc.Get("unit.area_km2", NumberFormatting.FormatCompact(km2, _suffixes, 1));
        }

        public string Days(int days)
        {
            return _loc.Get("unit.days", days);
        }

        public string Date(GameDate date)
        {
            var monthName = _loc.Get("month." + date.Month);
            return _loc.Get("date.long", monthName, date.Day, date.Year);
        }

        public string ShortDate(GameDate date)
        {
            var monthName = _loc.Get("month.short." + date.Month);
            return _loc.Get("date.short", monthName, date.Day, date.Year);
        }
    }
}
