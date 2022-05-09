/*
	This file is part of Hooligan Labs Airships Core
		© 2018-2022 Lisias T : http://lisias.net <support@lisias.net>
		© 2013-2021 Jewel Shisen
		© 2012-2013 Hooligan Labs

	Hooligan Labs Airships Core is double licensed as follows:
		* SKL 1.0 : https://ksp.lisias.net/SKL-1_0.txt
		* ARR (Pending agreement with the upstream for MIT)

	And you are allowed to choose the License that better suit your needs.

	Hooligan Labs Airships Core is distributed in the hope that it will be
	useful, but WITHOUT ANY WARRANTY; without even the implied
	warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.

	You should have received a copy of the SKL Standard License 1.0
	along with Hooligan Labs Airships Core.
	If not, see <https://ksp.lisias.net/SKL-1_0.txt>.
*/
#if !KSPe
using UnityEngine;
using H = KSPe.IO.Hierarchy<HLAirships.Startup>;
using T = KSPe.Util.Image.Texture2D;
namespace HLAirships.UI
{
	internal static partial class Icons
	{
		private static Texture2D load(string name) => T.LoadFromFile(H.GAMEDATA.Solve("PluginData", ICONSDIR, name));
	}
}
#endif
