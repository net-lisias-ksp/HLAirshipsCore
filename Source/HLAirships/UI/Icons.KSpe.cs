/*
	This file is part of Hooligan Labs Airships Core
		© 2018-2022 Lisias T : http://lisias.net <support@lisias.net>
		© 2013-2021 Jewel Shisen
		© 2012-2013 Hooligan Labs

	Hooligan Labs Airships Core is licensed as follows:
		* MIT (Expat) : https://opensource.org/licenses/MIT

	And you are allowed to choose the License that better suit your needs.

	Hooligan Labs Airships Core is distributed in the hope that it will be
	useful, but WITHOUT ANY WARRANTY; without even the implied
	warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
*/
#if KSPe
using UnityEngine;

using Asset = KSPe.IO.Asset<HLAirships.Startup>;

namespace HLAirships.UI
{
	internal static partial class Icons
	{
		private static Texture2D load(string name) => Asset.Texture2D.LoadFromFile(ICONSDIR, name);
	}
}
#endif
