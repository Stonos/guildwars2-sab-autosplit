# How to time map completion
The map completion category allows to subtract the time you spend in loading screens, therefore you should time those.
The easiest way to do this is to use two installations of LiveSplit simultaneously (so you can easily verify the runtime by comparing to character age).
One of which times your gameplay excluding loading screens and the other one times only your loading screens.
Using this method should produce times with accuracy down to a few seconds.
## Setup
0. You can watch this [video tutorial](https://youtu.be/s3Vweo2-Pcs) if you want.
1. Install LiveSplit + this Component in two different directories.
2. Edit both your layouts and add the component.
3. Load your Splits in one of the instances.
4. Edit the configurations in ``Components\GW2SAB\gw2sab_config.json``.  
The two versions should be (first for Splits, second for loading screens):
```json
{
    "LoadingScreens": "exclude",
    "StartCondition": "moving",
    "PauseOnExit": "true",
    "MaxSkippedTicks":  "3"
}
```
```json
{
    "LoadingScreens": "only",
    "StartCondition": "moving",
    "PauseOnExit": "true",
    "MaxSkippedTicks":  "3"
}
```
## Running
1. Open your game to character select.
2. Open both LiveSplit instances with your layouts and splits.
3. Start Recording.
3. Start Running :)  
The timers start once you move you character (maybe change ``StartCondition`` to ``notTransitioning``)
4. Take breaks to stay hydrated.  
While you are in a loading screen you can ``Alt+F4`` the game and the timers will pause.
5. Do not shut down your PC!  
If LiveSplit gets closed it's state is lost and you will need to re-time your run manually from video.
6. Resume your run by starting the game.  
After you it enter on character select the timers will resume.
7. Once you reach 100% and receive your gifts of exploration pause the timers and do a ``/age``.  
The sum of both timers should match the output.

## Timing single segments
Maybe you only want to practice one or a few maps to set up your personal bests.  
Do the following:
1. Start the Game and LiveSplit.
2. Look up the end time of the segment just before the one you want to time.
3. Set up the start time under ``Edit Splits``.
4. Enter the map you want to run.  
This will trigger a split.
5. Un-split.
6. Skip Splits until one before your segment.
7. Split manually and Run.
8. Reset to update your best segments.
