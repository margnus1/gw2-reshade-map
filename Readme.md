GW2ReshadeMap
=============
Polls the current map id from Guild Wars 2 using the Mumble Link API and writes
it to a small header file.

If given no command-line arguments, the file will be called `gw2map.h`. It looks
like this:

    #define GW2MapId 28
    #define GW2TOD 3

`GW2TOD` is the time-of-day ingame (0-3 are Dawn, Day, Dusk, and Night), derived
from your system clock.

How to compile
--------------
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
    
#define USE_CURVES        	1 	//[0 or 1] Curves : Contrast adjustments using S-curves.
#define USE_SEPIA         	0 	//[0 or 1] Sepia : Sepia tones the image.


Or you might simply wish to change its parameters in the "EFFECT PARAMETERS"
section:

    #if TooBright
    #  define Curves_contrast 		0.90
    #else
    #  define Curves_contrast 		0.70
    #endif
