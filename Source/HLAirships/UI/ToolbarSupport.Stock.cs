﻿/*
	This file is part of Hooligan Labs Airships Core
		© 2018-2022 Lisias T : http://lisias.net <support@lisias.net>
		© 2013-2021 Jewel Shisen
		© 2012-2013 Hooligan Labs

	Hooligan Labs Airships Core is licensed as follows:
		* MIT (Expat) : https://opensource.org/licenses/MIT

	Hooligan Labs Airships Core is distributed in the hope that it will be
	useful, but WITHOUT ANY WARRANTY; without even the implied
	warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.

*/
#if !KSPe
using UnityEngine;

using KSP.UI.Screens;

using KSPe.Annotations;

namespace HLAirships.UI
{
	internal partial class ToolbarController
	{
		private ApplicationLauncherButton button;

		private void InitToolbar() {
			// Nothing to do...
		}

		private void Do_Register_Editor()
		{
			if (null == this.button)
				this.button = ApplicationLauncher.Instance.AddModApplication(
							this.OnTrue, this.OnFalse
							, this.Dummy, this.Dummy
							, this.Dummy, this.Dummy
							, ApplicationLauncher.AppScenes.SPH | ApplicationLauncher.AppScenes.VAB
							, UI.Icons.launcherSupressed
						)
					;
			this.ResetState();
		}

		private void Do_Register_Flight()
		{
			if (null == this.button)
				this.button = ApplicationLauncher.Instance.AddModApplication(
							this.OnTrue, this.OnFalse
							, this.Dummy, this.Dummy
							, this.Dummy, this.Dummy
							, ApplicationLauncher.AppScenes.FLIGHT
							, UI.Icons.launcherSupressed
						)
					;
			this.ResetState();
		}

		private void Do_Unregister() {
			if (null == this.button) return;
			ApplicationLauncher.Instance.RemoveModApplication(this.button);
			Destroy(this.button);
			this.button = null;
		}

		internal void ResetState()
		{
			this.state = false;
			if (this.IsRegistered) this.UpdateIcon();
		}

		internal void ToggleState()
		{
			this.state = !this.state;
			if (this.IsRegistered) this.UpdateIcon();
		}

		private void UpdateIcon() {
			if (this.IsRegistered) this.button.SetTexture(this.state ? UI.Icons.launcher : UI.Icons.launcherSupressed);
		}

		private void Dummy() {
			// Dummy Bears!!! :P
		}

		private void OnTrue() {
			this.state = true;
			if (this.IsRegistered) this.UpdateIcon();
		}

		private void OnFalse() {
			this.state = false;
			if (this.IsRegistered) this.UpdateIcon();
		}
	}
}
#endif
