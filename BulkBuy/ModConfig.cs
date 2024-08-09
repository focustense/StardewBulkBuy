namespace BulkBuy;

/// <summary>
/// Configuration settings for BulkBuy.
/// </summary>
public class ModConfig
{
    /// <summary>
    /// The maximum quantity (stack) to buy in a single event.
    /// </summary>
    /// <remarks>
    /// <para>
    /// An "event" in this context means a registered click; the normal stack size for game controllers is always
    /// exactly 1. A value of 100 would mean that every repeated click (happening around 20x per second when counting
    /// the doubling due to both A and X being pressed) would purchase 200 of the specified item, filling up 2 entire
    /// slots per second, or fill up an entire empty inventory in about 18 seconds.
    /// </para>
    /// <para>
    /// The default being 111 rather than a round number like 100 is because 111 divides evenly into 999.
    /// </para>
    /// </remarks>
    public int MaxStack { get; set; } = 111;

    /// <summary>
    /// Amount of time between when acceleration starts - i.e. when both buttons are held simultaneously - to when the
    /// purchased stack size reaches <see cref="MaxStack"/>, assuming that the buttons continue to be held.
    /// </summary>
    /// <remarks>
    /// Acceleration is linear; the longer the buttons are held, the higher the purchase quantity.
    /// </remarks>
    public TimeSpan TimeToMaxStack { get; set; } = TimeSpan.FromSeconds(5);
}
