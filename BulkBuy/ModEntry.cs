using GenericModConfigMenu;
using HarmonyLib;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley.Menus;

namespace BulkBuy;

internal sealed class ModEntry : Mod
{
    private readonly PerScreen<bool> accelerationActive = new();
    private readonly PerScreen<bool> shopMenuActive = new();

    // Initialized in Entry
    private ModConfig config = null!;

    public override void Entry(IModHelper helper)
    {
        I18n.Init(helper.Translation);
        config = helper.ReadConfig<ModConfig>();

        var harmony = new Harmony(ModManifest.UniqueID);
        try
        {
            harmony.PatchAll(typeof(ModEntry).Assembly);
        }
        catch (Exception ex)
        {
            Monitor.Log($"Failed to apply Harmony patches; mod functions are disabled.\n\n{ex}", LogLevel.Error);
            return;
        }
        ShopMenuPatch.Accelerator = new(() => new PurchaseAccelerator(() => config));
        ShopMenuPatch.Monitor = Monitor;

        helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
        helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
        helper.Events.Display.MenuChanged += Display_MenuChanged;
        helper.Events.Input.ButtonsChanged += Input_ButtonsChanged;
    }

    private void Display_MenuChanged(object? sender, MenuChangedEventArgs e)
    {
        shopMenuActive.Value = e.NewMenu is ShopMenu;
        if (!shopMenuActive.Value && e.OldMenu is ShopMenu)
        {
            ShopMenuPatch.Accelerator.Value.Reset();
            accelerationActive.Value = false;
        }
    }

    private void GameLoop_GameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        var gmcm = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
        if (gmcm is not null)
        {
            ConfigMenu.Register(
                gmcm,
                ModManifest,
                () => config,
                () => config = new(),
                () => Helper.WriteConfig(config));
        }
    }

    private void GameLoop_UpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        if (accelerationActive.Value)
        {
            ShopMenuPatch.Accelerator.Value.Accelerate(Game1.currentGameTime.ElapsedGameTime);
        }
    }

    private void Input_ButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        var nextActive = shopMenuActive.Value
            && Game1.options.gamepadControls
            && !Game1.lastCursorMotionWasMouse
            && e.Held.Count(b => b.IsActionButton() || b.IsUseToolButton()) == 2;
        if (accelerationActive.Value && !nextActive)
        {
            ShopMenuPatch.Accelerator.Value.Reset();
        }
        accelerationActive.Value = nextActive;
    }
}
