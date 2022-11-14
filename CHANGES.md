# HLAirships Core :: Changes

* 2022-1114: 7.0.1.0R3 (Lisias) for KSP >= 1.3.1
	+ Updates `KSPe.Light` to the latest version.
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
	+ Compatibility extended down to KSP 1.3.1!!! **#HURRAY!!**
	+ This is a **PRE RELEASE**, with controlled distribution.
		- Pending formal agreement from the upstream for relicensing. 
* 2022-0127: 7.0.0.0 (Lisias) for 1.4 <= KSP <= 1.11.2
	+ Cleaning up the distribution from ARR artefacts
		- Only parts and assets derived from the last known MIT release, `HooliganLabsAirships-3.0.0`, are available from now.
	+ Small code optimisation and updating support for KSPe.
	+ Support for KSP 1.12.x is WiP at this moment.
	+ This is a **PRE RELEASE**, not to be widely distributed.
		- Pending formal agreement from the upstream. 
