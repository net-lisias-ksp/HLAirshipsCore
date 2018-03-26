# LibreLabs Airships

*A Free-as-in-Freedom airship mod for KSP 1.1+*

## Features

LibreLabs Airships includes envelope parts and a buoyancy controller for setting float/sink rate.  It will also attempt to balance an airship, but it works best if you do most of the balancing yourself in the editor.

## Dependencies

At present, LLA depends on blizzy's toolbar mod.

## Recommended addons

## Download and install

Ordinarily, I'd direct you someplace to get a zip file to unzip into GameData, but HooliganLabs is still in active development.  If you really care enough to use LLA over HLA, grab the source from [**GitHub**](https://github.com/Kerbas-ad-astra/LibreLabsAirships) and compile it with your favorite C# development tools, then plop the DLL and files in a "LibreLabsAirships" folder in GameData.

## Known and anticipated issues

## Version history and changelog

* 2016 May XX:
	* Fixes to compile and run in KSP 1.1.2.
	* Changed folder name, assembly name, and part names to avoid collisions with HooliganLabs 5.
* 2016 Sep XX:
	* Fixes to compile and run in KSP 1.2.
	* Added part tags.
	* Added editor info.
* 2016 Oct XX:
	* Renaming to bring in line with my other Libre resurrections.
* 2018 Mar XX:
	* Updated line rendering for KSP 1.4.1.

## Roadmap

Keep LLA up-to-date with KSP updates.  Make a snazzy logo.

I'm studying KAC and Landertron to figure out how to make blizzy's toolbar optional and to excise the "lead envelope" stuff.  It would also be nice to get a non-magic-number for atmospheric density (i.e. for a generic homeworld, as opposed to Earth and Kerbin specifically), but since those are the primary homeworlds, that takes a back burner.  I might study KerbalEngineer to see how to get atmospheric data from the in-game solar system itself to make a design helper in the editor, but that is a long way off.

A fully-compiled release will happen when JewelShisen gets hit by a bus, and not before.

When I've learned how to make parts with animations, I'll create a set of envelopes that more closely resemble NASA's Venus exploration concepts, such as HAVOC and VISE, but that won't be part of this mod.

## Credits

LibreLabs Airships is based on HooliganLabs Airships, release 3.0.1b, copyright 2014 HooliganLabs, JewelShisen, and Khatharr.

Many thanks to TriggerAu for Kerbal Alarm Clock (which I used as a guide to make the GUI independent of blizzy's toolbar) and charfa for the Landertron overhaul (which I used as a guide to remove the "lead envelope" business).

Finally, thanks are owed to JewelShisen for giving me a reason to perform my own resurrection of HL Airships.

## License

LibreLabs Airships is copyright 2016-2018 Kerbas-ad-astra.

Part configuration files, source and DLLs are copyright 2016 Kerbas_ad_astra and released under the GNU GPL v3 (or any later version).  If you make and redistribute a fork (unless it's intended to be merged with the master or if I'm handing over central control to someone else), you must give it a different name and use a different GameData folder in addition to the other anti-user-confusion provisions of the GPL (see sections 5a and 7).

Models and textures are released under the Creative Commons CC-BY-SA 4.0 International license.

All other rights (e.g. the LibreLabs Airships logo) reserved.