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
        // Even the slightest change could and would break a naive matcher.
        //
        // A safer way (probably) is to search for the call to `tryToPurchaseItem`, and see which local variable index
        // is used to populate the buy quantity (just before ldarg.1, expecting ldloc.s). Then, back up to the first
        // instruction that wrote to it (stloc.s with the same index). We can then insert an instruction after it that
        // simply overwrites the default, without having to know precisely how it was calculated.
        //
        // Note that we do _not_ want to overwrite it at (or near) the call site because some of the in-between checks
        // are supposed to override the requested stack size, e.g. throttling some that were increased by way of a
        // keyboard modifier key.
        var tryToPurchaseItemMethod = AccessTools.Method(typeof(ShopMenu), "tryToPurchaseItem");
        var forSaleField = AccessTools.Field(typeof(ShopMenu), nameof(ShopMenu.forSale));
        // We seem to be on an old version of Harmony without AccessTools.Indexer?
        var getSalableAtIndexMethod = AccessTools.Method(typeof(List<ISalable>), "get_Item");
        var adjustCountMethod = AccessTools.Method(typeof(ShopMenuPatch), nameof(AdjustPurchaseCount));
        var tryToPurchaseItemMatch = new CodeMatcher(instructions, gen)
            .MatchStartForward([
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Ldarg_1),
                new CodeMatch(OpCodes.Ldarg_2),
                new CodeMatch(OpCodes.Call, tryToPurchaseItemMethod),
            ]);
        var toBuyIndex = tryToPurchaseItemMatch.Operand;
        var forSaleIndex = tryToPurchaseItemMatch
            .MatchStartBackwards(
                new CodeMatch(OpCodes.Ldfld, name: nameof(ShopMenu.forSale)),
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Callvirt, getSalableAtIndexMethod))
            .Advance(1)
            .Operand;
        return new CodeMatcher(instructions, gen)
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
