/*
	This file is part of HLAirshipsCore Watch Dog
		©2022 Lisias T : http://lisias.net <support@lisias.net>

	HLAirshipsCore Watch Dog is licensed as follows:
		* SKL 1.0 : https://ksp.lisias.net/SKL-1_0.txt

	HLAirshipsCore Watch Dog is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.

	You should have received a copy of the SKL Standard License 1.0
	along with HLAirshipsCore Watch Dog. If not, see
	<https://ksp.lisias.net/SKL-1_0.txt>.

*/
using System.Collections.Generic;
using IO = KSPe.IO;
using Directory = KSPe.IO.Directory;
using Path = KSPe.IO.Path;

namespace HLAirshipsCore
{
	public class ModuleManagerSupport : UnityEngine.MonoBehaviour
	{
		public static IEnumerable<string> ModuleManagerAddToModList()
		{
			List<string> list = new List<string>();

			string airships = IO.Hierarchy.GAMEDATA.Solve("HLAirships");
			if (Directory.Exists(airships))
			{
				// Prevents a PARTs hell if older HLAirships is installed.
				// Also prevents stomping on HLAirships' toes if it decides to publish updated versions of these parts.
				string aero = Path.Combine(airships, "Parts", "Aero");
				if (Directory.Exists(Path.Combine(aero, "AirshipCap")))						list.Add("HLAirships-AirshipCap-Installed");
				if (Directory.Exists(Path.Combine(aero, "HL_AirshipEnvelope")))				list.Add("HLAirships-HLAirshipEnvelope-Installed");
				if (Directory.Exists(Path.Combine(aero, "HL_AirshipEnvelope_Cirrus")))		list.Add("HLAirships-AirshipEnvelopeCirrus-Installed");
				if (Directory.Exists(Path.Combine(aero, "HL_AirshipEnvelope_Cirrus_Real")))	list.Add("HLAirships-AirshipEnvelopeCirrusReal-Installed");
				if (Directory.Exists(Path.Combine(aero, "HL_AirshipEnvelope_Dodec")))		list.Add("HLAirships-AirshipEnvelopeDodec-Installed");
				if (Directory.Exists(Path.Combine(aero, "HL_AirshipEnvelope_Hecto")))		list.Add("HLAirships-AirshipEnvelopeHecto-Installed");
				if (Directory.Exists(Path.Combine(aero, "HL_AirshipEnvelope_LudoBlimp")))	list.Add("HLAirships-Blimp-Installed");
				if (Directory.Exists(Path.Combine(aero, "HL_AirshipEnvelope_Octo")))		list.Add("HLAirships-AirshipEnvelopeOcto-Installed");
				if (Directory.Exists(Path.Combine(aero, "HL_AirshipEnvelope_Ray")))			list.Add("HLAirships-AirshipEnvelopeRay-Installed");
				if (Directory.Exists(Path.Combine(aero, "HL_AirshipEnvelope_Una")))			list.Add("HLAirships-AirshipEnvelopeUna-Installed");
				if (Directory.Exists(Path.Combine(aero, "OMG Airship")))					list.Add("HLAirships-DeathStarBattery-Installed");
				if (Directory.Exists(Path.Combine(aero, "Probe Envelope")))					list.Add("HLAirships-PLift-Installed");
				list.Add("HLAirships-Installed");
			}
			else
				// If HLAirships is not installed, fool everybody else into believing original HLAirhips is installed so eventual
				// clients would patch the Modules.
				list.Add("HLAirships");	
			string[] r = list.ToArray();
			return r;
		}
	}
}
