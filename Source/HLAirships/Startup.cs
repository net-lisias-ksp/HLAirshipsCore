/*
	This file is part of Hooligan Labs Airships /L Unleashed
		© 2018-2022 Lisias T : http://lisias.net <support@lisias.net>
		© 2013-2018 Jewel Shisen
		© 2012-2013 Hooligan Labs

	Hooligan Labs Airships /L Unleashed is double licensed as follows:
		* SKL 1.0 : https://ksp.lisias.net/SKL-1_0.txt
		* ARR (Pending agreement with the upstream for MIT)

	And you are allowed to choose the License that better suit your needs.

	Hooligan Labs Airships /L Unleashed is distributed in the hope that it will be
	useful, but WITHOUT ANY WARRANTY; without even the implied
	warranty of	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.

	You should have received a copy of the SKL Standard License 1.0
	along with Hooligan Labs Airships /L Unleashed.
	If not, see <https://ksp.lisias.net/SKL-1_0.txt>.
*/
using UnityEngine;

namespace HLAirships
{
	[KSPAddon(KSPAddon.Startup.Instantly, true)]
	internal class Startup : MonoBehaviour
	{
		private void Start()
		{
			Log.init();
			Log.log("Version {0} running on KSP {1}", Version.Text, KSPe.Util.KSP.Version.Current.ToString());

			try
			{
				KSPe.Util.Installation.Check<Startup>();
			}
			catch (KSPe.Util.InstallmentException e)
			{
				Log.err(e.ToShortMessage());
				KSPe.Common.Dialogs.ShowStopperAlertBox.Show(e);
			}
		}
	}
}
