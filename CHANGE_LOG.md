# HLAirships Core :: Change Log

* 2022-0810: 7.0.1.0R2 (Lisias) for KSP >= 1.3.1
	+ Fixes a mishap on the `AssemblyInfo.cs` file.
		- Thanks to [Reylan](https://forum.kerbalspaceprogram.com/?app=core&module=members&controller=profile&id=222459) and [luna_cat](https://forum.kerbalspaceprogram.com/index.php?/profile/212171-luna_cat/) for the [reports](https://forum.kerbalspaceprogram.com/index.php?/topic/207891-ksp-131-hooligan-labs-airships-core-development-thread-7010-2022-0509/&do=findComment&comment=4162735)! 
* 2022-0509: 7.0.1.0 (Lisias) for KSP >= 1.3.1
	+ HLAirshipsCore goes gold!
	+ Officially licensed under the MIT (Expat) *#HURRAY**
* 2022-0503: 7.0.0.4 (Lisias) for KSP >= 1.3.1 PRERELEASE
	+ Fixing the classic HL_Envelopers to allow reusing crafts made on legacy
	+ New Icon
	+ **Finally** fixing the GUI lingering between Scenes or Map
	+ Works issues:
		- [#7](https://github.com/net-lisias-ksp/HLAirshipsCore/issues/7) Implement MODCAT & CUSTOM_PARTLIST_CATEGORY
		- [#2](https://github.com/net-lisias-ksp/HLAirshipsCore/issues/2) Some Parts needs World Stabiliser on [1.4.0 <= KSP <= 1.11.2], and blow up on KSP 1.12.x
* 2022-0502: 7.0.0.3 (Lisias) for KSP >= 1.3.1 PRERELEASE
	+ Let's pretend this never happened! :P
* 2022-0426: 7.0.0.2 (Lisias) for KSP >= 1.3.1 PRERELEASE
	+ Revamping parts:
		- `HL_AirshipEnvelop`
		- `HL_AirshipEnvelop_Octp`
		- `HL_AirshipEnvelop_Hecto`
		- `HL_AirshipEnvelop_LudoBlimp`
		- `OMG_Airhips (DeathStarBattery)`
		- No more need for World Stabiliser and the parts are fully functional on KSP 1.12.3
	+ Added `HLAirships Watch Dog` to prevent installations mishaps and some other problems plaguing KSP.
	+ New Sample Craft (Panorama Blimp)
	+ This is a **PRE RELEASE**, with controlled distribution.
		- Pending formal agreement from the upstream for relicensing. 
* 2022-0422: 7.0.0.1 (Lisias) for KSP >= 1.3.1 PRERELEASE
	+ Enhanced TweakScale Support
	+ Part fixing
		- `bulkheadProfiles` on the parts
		- descriptions
		- AirshipCap texture now on the right orientation!
	+ Code
		- Better resilience on some key features
		- Somewhat better KSP 1.12.x survivability
		- Small improvements on the PAW
	+ Caveats
		- Some parts (temporarily) deactivated on KSP 1.12.x
		- Some parts needs World Stabiliser on [1.4.0 <= KSP <= 1.11.3]
		- See [Issue #2](https://github.com/net-lisias-ksp/HLAirshipsCore/issues/2) for details 
	+ Compatibility extended down to KSP 1.3.1!!! **#HURRAY!!**
	+ This is a **PRE RELEASE**, with controlled distribution.
		- Pending formal agreement from the upstream for relicensing. 
* 2022-0127: 7.0.0.0 (Lisias) for 1.4 <= KSP <= 1.11.2 PRERELEASE
	+ Cleaning up the distribution from ARR artefacts
		- Only parts and assets derived from the last known MIT release, `HooliganLabsAirships-3.0.0`, are available from now.
	+ Small code optimisation and updating support for KSPe.
	+ Support for KSP 1.12.x is WiP at this moment.
	+ This is a **PRE RELEASE**, not to be widely distributed.
		- Pending formal agreement from the upstream. 
* 2020-1221: 6.4.0.1 (Lisias) for KSP >= 1.4
	+ Making the thing work fine on KSP >= 1.8 the proper way
	+ Using some convenience methods from ToolbarControl /LU
	+ Bumping version to catch up with upstream 
* 2020-0516: 6.4 (jewelshisen) for KSP 1.9.1
	+ Updating to KSP >= 1.8 
* 2018-1115: 5.6.1.4 (Lisias) for KSP 1.4.1+; 1.5
	+ **Properly** respecting <F2> and hiding the U.I.
* 2018-1109: 5.6.1.3 (Lisias) for KSP 1.4.1+; 1.5
	+ Using KSPe Facilities
		- Logging
		- Config Data
		- Assets
* 2018-0801: 5.6.1.2 (lisias) for KSP 1.4.1
	+ Recompile for KSP 1.4
	+ Added ClickThroughBlocker support
	+ Respecting <F2> and hiding the U.I. as expected.
* 2017-0529: 6.3.1 (jewelshisen) for KSP 1.3.0
	+ Updated a few parts to correct node alignments
* 2017-0527: 6.3 (jewelshisen) for KSP 1.3.0
	+ No changelog provided
* 2016-1113: 6.2 (jewelshisen) for KSP 1.2.1
	+ No changelog provided
* 2016-1015: 6.1 (jewelshisen) for KSP 1.2
	+ No changelog provided
* 2016-1014: 6.0 (jewelshisen) for KSP 1.2
	+ No changelog provided
* 2016-1014: 5.6.1 (smce) for KSP 1.2
	+ 5.6.1 HotFix
		- Fix hatch collider issues on 5mtr gondola
		- Add missing internals for both Airship pods
		- Add Spaces folder for easy reference
* 2016-1014: 5.6 (dunclaw) for KSP 1.2
	+ Update to work with KSP 1.2
* 2016-0813: 5.5 (jewelshisen) for KSP 1.1.3
	+ Fix for autopitch crash bug
* 2016-0701: 5.4 (jewelshisen) for KSP 1.1.3
	+ No changelog provided
* 2016-0523: 5.3 (jewelshisen) for KSP 1.1.2
	+ 15m Parts have now been added 
* 2016-0515: 5.2 (jewelshisen) for KSP 1.1.2
	+ Removed parts that should not be there.
* 2016-0514: 5.2 (dunclaw) for KSP 1.1.2
	+ Adds support for a build aid window in the editor
* 2016-0513: 5.2 (jewelshisen) for KSP 1.1.2
	+ Added Build Aid for SPH and VAB. Builds dynamically for all planets with an atmosphere (including the SUN for some reason...) 
* 2016-0502: 1.1.2.1 (dunclaw) for KSP 1.1.2
	+ Added Support for the AppLauncher Bar 
* 2016-0503: 5.1 (jewelshisen) for KSP 1.1.2
	+ Removed some parts that were not supposed to be in here. 
* 2016-0502: 1.1.2 (dunclaw) for KSP 1.1.2 (no binary)
	+ Updated the plugin to work with KSP 1.1.2
	+ Removed explicit dependency on Toolbar
	+ Refactored UI code
	+ Removed several bits of dead code
* 2014-1016: 3.0.0 (Hooligan Labs)
	+ Licensed under MIT
