﻿/*
	This file is part of Module Manager Watch Dog
		©2020-22 Lisias T : http://lisias.net <support@lisias.net>

	Module Manager Watch Dog is licensed as follows:
		* SKL 1.0 : https://ksp.lisias.net/SKL-1_0.txt

	Module Manager Watchdog is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.

	You should have received a copy of the SKL Standard License 1.0
	along with Module Manager Watch Dog. If not, see
	<https://ksp.lisias.net/SKL-1_0.txt>.

*/
using UnityEngine;

namespace HLAirshipsCore.UI
{
	internal static class ShowStopperAlertBox
	{
		private static readonly string AMSG = @"to get instructions about how to Download and Install HLAirships. KSP will close";

		internal static void Show(string msg)
		{
			KSPe.Common.Dialogs.ShowStopperAlertBox.Show(
				msg,
				AMSG,
				() => { Application.OpenURL("https://github.com/net-lisias-ksp/HLAirshipsCore/discussions/6"); Application.Quit(); }
			);
			Log.detail("\"Houston, we have a Problem!\" was displayed about : {0}", msg);
		}
	}
}
