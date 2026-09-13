namespace Nation.Core.Localization
{
    /// <summary>
    /// Key-based text lookup. All player-facing text goes through this service; nothing in the UI is hardcoded.
    /// </summary>
    public interface ILocalizationService
    {
        string CurrentLocale { get; }

        bool Has(string key);

        /// <summary>Returns the localized string, falling back to the fallback locale, then to "[key]".</summary>
        string Get(string key);

        /// <summary>Returns the localized string with {0}, {1}, ... placeholders filled in.</summary>
        string Get(string key, params object[] args);
    }
}
