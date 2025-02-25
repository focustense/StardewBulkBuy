using HarmonyLib;
using StardewModdingAPI.Utilities;
using StardewValley.Menus;
using System.Reflection;
using System.Reflection.Emit;

namespace BulkBuy;

[HarmonyPatch(typeof(ShopMenu))]
internal static class ShopMenuPatch
{
    // Initialized by ModEntry.
    public static PerScreen<PurchaseAccelerator> Accelerator { get; set; } = null!;
    public static IMonitor Monitor { get; set; } = null!;

    private static int AdjustPurchaseCount(ShopMenu menu, ISalable item, int count)
    {
        // Default count with gamepad should always be 1. If it's something other than 1, then we probably do not want
        // it to be modified, as something else is already trying to modify it.
        if (count != 1 || !Accelerator.IsActiveForScreen() || !Accelerator.Value.IsAccelerating)
        {
            return count;
        }
        return Accelerator.Value.GetScaledPurchaseCount(
            item,
            menu.itemPriceAndStock[item],
            ShopMenu.getPlayerCurrencyAmount(Game1.player, menu.currency));
    }

    [HarmonyPatch(nameof(ShopMenu.receiveLeftClick))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction>? receiveLeftClick_Transpile(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator gen,
        MethodBase _)
    {
        // The actual code used to compute item quantity is - at least in the IL/decompilation - a giant one-line mess.
        // Even the slightest change could and would break a naive matcher, so we use some code analysis to try to
        // correctly identify the local indices instead of hardcoding them.
        var matcher = new CodeMatcher(instructions, gen);

        // The original heuristic for obtaining quantity was broken by a conflicting transpiler that overwrote the call
        // to tryToPurchaseItem: https://github.com/focustense/StardewBulkBuy/issues/1
        //
        // This alternative method tries to get the index from where the vanilla game clamps it to the actual quantity
        // available, on the assumption that mods are less likely to interfere with this (and the vanilla logic is
        // unlikely to change) since it would make little sense to allow purchasing more than the available quantity.
        var maximumStackSizeMethod = AccessTools.Method(typeof(ISalable), nameof(ISalable.maximumStackSize));
        var minMethod = AccessTools.Method(typeof(Math), nameof(Math.Min), [typeof(int), typeof(int)]);
        matcher
            .MatchEndForward(
                new CodeMatch(OpCodes.Callvirt, maximumStackSizeMethod),
                new CodeMatch(OpCodes.Call, minMethod),
                new CodeMatch(OpCodes.Stloc_S)
            )
            .ThrowIfNotMatch("Unable to determine the local index for purchase quantity.");
        // Actual local name in the method is "toBuy" so we use the same name here.
        var toBuyIndex = matcher.Operand;

        //
        var getSalableAtIndexMethod = AccessTools.Method(typeof(List<ISalable>), "get_Item");
        var forSaleIndex = matcher
            .MatchStartBackwards(
                new CodeMatch(OpCodes.Ldfld, name: nameof(ShopMenu.forSale)),
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Callvirt, getSalableAtIndexMethod))
            .ThrowIfNotMatch("Unable to determine the local index for the sale item position.")
            .Advance(1)
            .Operand;

        // Note that we do _not_ want to overwrite it at (or near) the call site because some of the in-between checks
        // are supposed to override the requested stack size, e.g. throttling some that were increased by way of a
        // keyboard modifier key.
        var forSaleField = AccessTools.Field(typeof(ShopMenu), nameof(ShopMenu.forSale));
        var adjustCountMethod = AccessTools.Method(typeof(ShopMenuPatch), nameof(AdjustPurchaseCount));
        return matcher
            .MatchEndForward(new CodeMatch(OpCodes.Stloc_S, toBuyIndex))
            .Advance(1)
            .Insert([
                // C#: toBuy = AdjustPurchaseCount(this, forSale[index], toBuy)
                // Arg 0: this
                new CodeInstruction(OpCodes.Ldarg_0),
                // Arg 1: this.forSale[index]
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, forSaleField),
                new CodeInstruction(OpCodes.Ldloc_S, forSaleIndex),
                new CodeInstruction(OpCodes.Callvirt, getSalableAtIndexMethod),
                // Arg 2: toBuy
                new CodeInstruction(OpCodes.Ldloc_S, toBuyIndex),
                // Call and store return value
                new CodeInstruction(OpCodes.Call, adjustCountMethod),
                new CodeInstruction(OpCodes.Stloc_S, toBuyIndex),
            ])
            .InstructionEnumeration();
    }
}
