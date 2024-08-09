namespace BulkBuy;

/// <summary>
/// Tracks the current speed/acceleration of an ongoing purchase.
/// </summary>
/// <param name="configSelector">Function to retrieve the current mod configuration.</param>
internal class PurchaseAccelerator(Func<ModConfig> configSelector)
{
    /// <summary>
    /// Gets whether or not acceleration is currently active.
    /// </summary>
    public bool IsAccelerating => elapsedTotal > TimeSpan.Zero;

    private readonly Func<ModConfig> configSelector = configSelector;

    private float currentMultiplier = 1.0f;
    private TimeSpan elapsedTotal;

    /// <summary>
    /// Increases the purchasing speed based on how much time has elapsed since the previous call.
    /// </summary>
    /// <param name="elapsedDelta">Time elapsed since the last frame.</param>
    public void Accelerate(TimeSpan elapsedDelta)
    {
        var config = configSelector();
        if (config.MaxStack <= 1)
        {
            // Nothing useful to do here.
            currentMultiplier = 1.0f;
            return;
        }
        // Technically, it would be correct to ignore the first delta of an acceleration, since it was not accelerating
        // during the previous frame. We'd have to use an extra bool state to track this.
        // Practically, a difference of one frame isn't going to be noticeable at all.
        elapsedTotal += elapsedDelta;
        if (elapsedTotal > config.TimeToMaxStack)
        {
            elapsedTotal = config.TimeToMaxStack;
        }
        var progress = (float)(elapsedTotal / config.TimeToMaxStack);
        currentMultiplier = 1 + progress * (config.MaxStack - 1);
    }

    /// <summary>
    /// Gets the intended purchase count, scaled according to current acceleration/speed.
    /// </summary>
    /// <param name="item">The item being purchased.</param>
    /// <param name="stockInfo">Stock info with information on the price and available quantity.</param>
    /// <param name="availableFunds">Funds that the player has, in whatever currency the store accepts.</param>
    /// <returns>The quantity of <paramref name="item"/> to purchase, taking into account the current speed/acceleration
    /// as well as available funds and stock.</returns>
    public int GetScaledPurchaseCount(ISalable item, ItemStockInformation stockInfo, int availableFunds)
    {
        var count = (int)MathF.Round(currentMultiplier);
        // Some of these thresholds can be zero/negative/invalid, but it makes no difference to logic as these are
        // treated as 1 by the game, and we force it back to 1 if it sinks below.
        count = Math.Min(count, item.maximumStackSize());
        count = Math.Min(count, stockInfo.Stock);
        count = Math.Min(count, availableFunds / Math.Max(1, stockInfo.Price));
        count = Math.Min(count, Game1.player.Items.Sum(i => GetStackableQuantity(item, i)));
        return Math.Max(count, 1);
    }

    public void Reset()
    {
        currentMultiplier = 1.0f;
        elapsedTotal = TimeSpan.Zero;
    }

    private static int GetStackableQuantity(ISalable itemToPurchase, Item? itemInInventory)
    {
        if (itemInInventory is null)
        {
            return itemToPurchase.maximumStackSize();
        }
        if (!itemToPurchase.canStackWith(itemInInventory))
        {
            return 0;
        }
        return itemInInventory.maximumStackSize() - itemInInventory.Stack;
    }
}
