using GenericModConfigMenu;

namespace BulkBuy;

/// <summary>
/// Helper for registering the mod's config menu with GMCM.
/// </summary>
internal class ConfigMenu
{
    /// <summary>
    /// Registers the mod's config menu page(s) with GMCM.
    /// </summary>
    /// <param name="gmcm">The GMCM API.</param>
    /// <param name="mod">The mod manifest.</param>
    /// <param name="translations">Translation helper for the current mod.</param>
    /// <param name="config">Function to retrieve the current configuration data.</param>
    /// <param name="reset">Delegate to reset/recreate the configuration data.</param>
    /// <param name="save">Delegate to save the current configuration data.</param>
    public static void Register(
        IGenericModConfigMenuApi gmcm,
        IManifest mod,
        Func<ModConfig> config,
        Action reset,
        Action save)
    {
        gmcm.Register(mod, reset, save);
        gmcm.AddParagraph(mod, I18n.ModDescription);
        gmcm.AddNumberOption(
            mod,
            name: I18n.Config_MaxStack_Title,
            tooltip: I18n.Config_MaxStack_Description,
            getValue: () => config().MaxStack,
            setValue: value => config().MaxStack = value,
            min: 1,
            max: 500,
            interval: 1);
        gmcm.AddNumberOption(
            mod,
            name: I18n.Config_TimeToMaxStack_Title,
            tooltip: I18n.Config_TimeToMaxStack_Description,
            getValue: () => (float)config().TimeToMaxStack.TotalSeconds,
            setValue: value => config().TimeToMaxStack = TimeSpan.FromSeconds(value),
            min: 1.0f,
            max: 10.0f,
            interval: 0.1f,
            formatValue: value => I18n.Config_TimeToMaxStack_Format(value));
    }
}
