# Bulk Buy

_"Yes, I **did** just spend $2 million on lumber. You got a problem with that?"_

## Introduction

This is a simple utility mod made especially for controller users, designed to solve the common mid- to late-game problem: "how can I buy huge amounts of [some resource/item] without dying of boredom?"

Most players probably know how to speed up purchasing using a keyboard/mouse by holding <kbd>Ctrl</kbd> + <kbd>Shift</kbd> to buy 25x. Many even know the super-secret <kbd>Ctrl</kbd> + <kbd>Shift</kbd> + <kbd>1</kbd> combo to buy an entire stack of 999. And gamepad users probably know that holding `A` + `X` is the best option, giving you a whopping multiplier of... 2x.

Once again, controller users get hosed, and have to either switch to the mouse/keyboard or fall asleep listening to repetitive clinking. But fear not, fellow ergos! There is, in fact, a solution to this problem that has been known in the design world for many years: **acceleration.**

If you've ever wanted to skip halfway through a 2-hour video using a d-pad remote, e.g. on YouTube or Netflix, then you've seen it before. Navigation starts slowly, but quickly speeds up as you hold the button down. The same concept is used here, with a Stardew-flavored twist:

- Holding _either_ the A (action) or X (tool) buttons will buy at normal speed.

- Holding *both* buttons will start at 2x speed, same as vanilla, but accelerate to a much higher speed (about[^1] 100x with default configuration) over the next several seconds, allowing you to fill up your entire inventory in even *less* time than it takes with a mouse/keyboard.

- If you need to hit a precise target, just release one of the buttons as you get close to it, and the speed will return to normal (1x).

- You can configure both the maximum speed and acceleration duration, if you find the default too aggressive... or not aggressive enough.

That's all there is to it! Install the mod, tweak your speeds if you like, and never have to interrupt your ergonomic tranquility for a shop menu ever again.

[^1]: Technically, it's 111, because the maximum stack size is 999, not 1000, and a whole-number divisor makes new stacks start at a consistent count. It's hard to notice the subtle shifts happening at speed 100, but once you see it, you can't un-see it.

## Installation

Bulk Buy follows the standard installation procedure for most mods:

1. Install SMAPI and set up your game for mods, per the [Modding: Player's Guide](https://stardewvalleywiki.com/Modding:Player_Guide/Getting_Started) instructions.
2. Download the [latest release](https://github.com/focustense/StardewBulkBuy/releases). Make sure you download the `BulkBuy x.y.z.zip` file, **not** the source code.
3. Open the .zip file and extract the `BulkBuy` folder into your `Stardew Valley\Mods` folder.
4. Launch the game!

## Configuration

All of this mod's options are configurable through GMCM. If you prefer to edit the `config.json` or are having any trouble with the in-game settings, read on for a detailed explanation of what the settings do.

<details>
<summary>Example Configuration</summary>

```json
{
  "MaxStack": "111",
  "TimeToMaxStack": "00:00:05"
}
```
</details>

### Settings

* `MaxStack`: The maximum stack per "purchase event", which effectively acts as a speed multiplier. When holding both the A and X buttons, each repeated purchase will buy more items than the last, until this maximum is reached.
* `TimeToMaxStack`: The amount of time between the start of acceleration - when both the A and X buttons are initially pressed - until the maximum speed (`MaxStack`) is reached, after which the speed stays constant and does not continue to accelerate.

## See Also

* [Changelog](CHANGELOG.md)
