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
using UnityEngine;

namespace HLAirships
{
	[KSPAddon(KSPAddon.Startup.Instantly, true)]
	internal class Startup : MonoBehaviour
	{
		private void Start()
		{
			Log.force("Version {0}", Version.Text);

			try
			{
				KSPe.Util.Installation.Check<Startup>(typeof(Version).Namespace, Version.Namespace, null);
			}
			catch (KSPe.Util.InstallmentException e)
			{
				Log.err(e.ToShortMessage());
				KSPe.Common.Dialogs.ShowStopperAlertBox.Show(e);
			}
		}
	}
}
