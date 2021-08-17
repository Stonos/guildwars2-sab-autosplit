# Guild Wars 2 SAB LiveSplit Auto Splitter

This is a LiveSplit Auto Splitter which splits the timer whenever you pass a checkpoint on Guild Wars 2 Super Adventure Box.

## Preview
[![Guild Wars 2 SAB LiveSplit Auto Splitter preview](https://img.youtube.com/vi/NjD6sSjyNsU/0.jpg)](https://www.youtube.com/watch?v=NjD6sSjyNsU)

## How to use
1. Download [LiveSplit](https://livesplit.org/downloads)
2. Download the [latest release](https://github.com/Stonos/guildwars2-sab-autosplit/releases) of this component
3. Extract all the files to LiveSplit's directory
4. Run LiveSplit
5. Open the Splits for the zone you intend to run by right clicking and going to **Open Splits - From File** and selecting the appropriate file from the `Components\GW2SAB\Splits` directory
6. Right click - Edit Layout - Add - Control - **Guild Wars 2 Super Adventure Box auto splitter**

### Adding custom checkpoints
You can modify [gw2sab_checkpoints.json](https://github.com/Stonos/guildwars2-sab-autosplit/blob/master/LiveSplit.GW2SAB/gw2sab_checkpoints.json) in order to add new checkpoints (you can use something like [Gw2MumbleLinkReader](https://github.com/Archomeda/Gw2MumbleLinkReader) to grab the `X` and `Z` coordinates).

Make sure that the checkpoints are sorted, otherwise they may not trigger correctly!

### Optional Features
Some features can be configured in [gw2sab_config.json](https://github.com/Atlan-G/guildwars2-sab-autosplit/blob/master/LiveSplit.GW2SAB/gw2sab_checkpoints.json)
- ``LoadingScreens`` control how loading-screens are timed. Options are: ``include``/``exclude`` from timer or ``only``, which discards normal playtime. Default is ``include``.
- ``StartCondition`` Allows the timer to start on ``moving`` (default), ``loading``-screens, anything ``notLoading``-screen (includes character select), ``notTransitioning`` (does not start on character select) or ``manual``.

## How it works
It works by reading the player's position using the [MumbleLink API](https://wiki.guildwars2.com/wiki/API:MumbleLink), and comparing it to a [list of known checkpoint locations](https://github.com/Stonos/guildwars2-sab-autosplit/blob/master/LiveSplit.GW2SAB/gw2sab_checkpoints.json).  
When detecting a loading-screen it also screenshots the game and counts the number of black pixels at the bottom.

## Limitations
It's not possible to detect when a boss dies through the MumbleLink API.
As a workaround, a set amount of time is subtracted after the level changes while you're in a boss area.
This means that in order to see your final time, you must wait until you get teleported to the next level.

## How to Develop (for .Net newbies)
1. Download [LiveSplit](https://livesplit.org/downloads)
2. Clone / Unzip this Repository inside the LiveSplit directory
3. Open the Project in Visual Studio and fetch dependencies with NuGet