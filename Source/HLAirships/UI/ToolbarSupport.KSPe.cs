/*
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
#if KSPe
using UnityEngine;
using KSP.UI.Screens;

using KSPe.Annotations;
using Toolbar = KSPe.UI.Toolbar;
using System.Collections.Generic;

namespace HLAirships.UI
{
	internal partial class ToolbarController:MonoBehaviour
	{
		private Toolbar.Toolbar controller => Toolbar.Controller.Instance.Get<Startup>();

		private void InitToolbar()
		{
			Toolbar.Controller.Instance.Register<Startup>(Version.FriendlyName);
		}

		// State controller for the toobar button
		private class WindowState:KSPe.UI.Toolbar.State.Status<bool> { protected WindowState(bool v):base(v) { }  public static implicit operator WindowState(bool v) => new WindowState(v);   public static implicit operator bool(WindowState s) => s.v; }
		private Toolbar.State.Control windowState;
		private Toolbar.Button button = null;

		private void Do_Register_Editor()
		{
			this.button = Toolbar.Button.Create(this
					, ApplicationLauncher.AppScenes.SPH | ApplicationLauncher.AppScenes.VAB
					, Icons.launcherSupressed
					, Icons.toolbarSupressed
					, Version.FriendlyName
				);
			this.PostRegister();
		}

		private void Do_Register_Flight()
		{
			this.button = Toolbar.Button.Create(this
					, ApplicationLauncher.AppScenes.FLIGHT
					, Icons.launcherSupressed
					, Icons.toolbarSupressed
					, Version.FriendlyName
				);
			this.PostRegister();
		}

		private void PostRegister()
		{
			windowState = this.button.State.Controller.Create<WindowState>(
				new Dictionary<Toolbar.State.Status, Toolbar.State.Data> {
							{ (WindowState)false, Toolbar.State.Data.Create(Icons.launcherSupressed, Icons.toolbarSupressed) }
							,{ (WindowState)true, Toolbar.State.Data.Create(Icons.launcher, Icons.toolbar) }
				}
			);

			this.button.Mouse.Add(Toolbar.Button.MouseEvents.Kind.Left, this.Button_OnLeftClick);
			this.button.Mouse.Add(Toolbar.Button.MouseEvents.Kind.Right, this.Button_OnRightClick);
			this.controller.Add(this.button);
			ToolbarController.Instance.ButtonsActive(true, true);
			this.IsRegistered = true;
			this.state = false;
		}

		private void Do_Unregister()
		{
			this.controller.Destroy();
			this.button = null;
		}

		internal void ButtonsActive(bool enableBlizzy, bool enableStock)
		{
			this.controller.ButtonsActive(enableBlizzy, enableStock);
		}

		internal void ResetState()
		{
			this.state = false;
			if (this.IsRegistered) this.button.Status = (WindowState)this.state;
		}

		internal void ToggleState()
		{
			this.state = !this.state;
			if (this.IsRegistered) this.button.Status = (WindowState)this.state;
		}
	}
}
#endif
