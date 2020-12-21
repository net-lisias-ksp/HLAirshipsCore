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
