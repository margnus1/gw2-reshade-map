GW2ReshadeMap
=============
Polls the current map id from Guild Wars 2 using the Mumble Link API and writes
it to a small header file.

If given no command-line arguments, the file will be called `gw2map.h`. It looks
like this:

    #define GW2MapId 28
    #define GW2TOD 3
    #define GW2Active 1
    #define TimeZone 3600

`GW2TOD` is the time-of-day ingame (0-3 are Dawn, Day, Dusk, and Night), derived
from your system clock.

GW2Active is 0 if the game is not running, in the character select screen or a
loading screen, and 1 otherwise. It will only turn to 0 after a delay
(`ActivityTimeoutMs` in `GW2ReshadeMap.cs`), but might help prevent overbloomed
login screens, for example.

`TimeZone` is your time zone and daylight savings time in seconds of offset from
UTC. It is useful to make presets that change smoothly with in-game time, as
opposed to presets that use `GW2TOD` above, which change suddenly and cause
short game freezes as the change happens. Because of this, use of a macro like
`GW2DayNight` is heavily recommended in favour of using `GW2TOD`. See the
[GW2DayNight](https://gist.github.com/margnus1/901ebb2a7f5f25ebeb393334b1500f04)
macro, used in my simple
[Subtle Days, Immersive Nights](https://gist.github.com/margnus1/9055fadc0692219099bc)
MasterEffect preset for an example.

How to compile
--------------
Double-click `build.bat`. If the window instantly dissapears, that means
everything went went well and you should have `GW2ReshadeMap.exe` in the same
folder.

Or from the command line:

    csc GW2ReshadeMap.cs

How to use
----------
Easiest is to drop `GW2ReshadeMap.exe` into the directory you keep `ReShade.fx`
(or `MasterEffect.h`, if you're using that), and keep it running whenever you're
playing. It might need to "run as Administrator" to be able to modify its
file. You can avoid that by creating the file (`gw2map.h`) yourself, and then
assigning your user permission to change it.

In your Reshade preset, simply include the file:

    #include "gw2map.h"

###I want to start GW2ReshadeMap automatically

Change your Guild Wars 2 shortcut to point to `GW2ReshadeMap.exe`, and add
`/launch ..\Gw2.exe` to the program arguments (adjust the path `..\Gw2.exe` as
appropriate if you are not keeping `GW2ReshadeMap.exe` in the `bin` folder, it
should lead to `Gw2.exe`). If you already have arguments for `Gw2.exe`, replace
them with <code>/launchargs "<i>your old arguments here</i>"</code>, don't omit
the quotes ("").

###Example

For example, say that you are using MasterEffect and would like to turn off the
"Curves" effect when you're in Wayfarer Foothills (id = `28`) or Straits of
Devastation (id = `51`) and it is either day or dusk, you might insert something
like this at the top of `MasterEffect.h` to define the value `TooBright` to be
`1` in those circumstances:

    /*=================================================================*\
    |                         GW2 CONDITIONALS                          |
    \*=================================================================*/
    
    #include "gw2map.h"
    #define TooBright ((GW2MapId == 28 || GW2MapId == 51) \
                       && (GW2TOD == 1 || GW2TOD == 2))

If you're unfamiliar with the C preprocessor, note the `\` required if you want
to wrap lines staring with `#` (so called directives) over multiple lines.

And then, in the "ENABLE EFFECTS" section, you use that value to disable Curves
when TooBright is 1:

    ...
    #define USE_CURVES      	!TooBright
    ...

Or you might simply wish to change its parameters in the "EFFECT PARAMETERS"
section:

    #if TooBright
    #  define Curves_contrast 		0.90
    #else
    #  define Curves_contrast 		0.70
    #endif
