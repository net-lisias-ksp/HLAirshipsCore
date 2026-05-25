/*
	This file is part of Hooligan Labs Airships Core
		© 2018-2026 LisiasT : http://lisias.net <support@lisias.net>
		© 2013-2021 Jewel Shisen
		© 2012-2013 Hooligan Labs

	Hooligan Labs Airships Core is licensed as follows:
		* MIT (Expat) : https://opensource.org/licenses/MIT

	Hooligan Labs Airships Core is distributed in the hope that it will be
	useful, but WITHOUT ANY WARRANTY; without even the implied
	warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
*/
using System;
namespace HLAirships.UI
{
	internal static class Util
	{
		static public void SetEventVisibility(PartModule owner, string name, bool visible)
		{
			BaseEvent ev = owner.Events[name];
			ev.guiActive = visible;
			ev.guiActiveUnfocused = visible;
		}

	}
}
