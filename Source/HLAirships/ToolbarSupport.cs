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
using KSP.UI.Screens;

using KSPe.Annotations;
using Toolbar = KSPe.UI.Toolbar;
using Asset = KSPe.IO.Asset<HLAirships.ToolbarController>;
using System.Collections.Generic;

namespace HLAirships
{
	[KSPAddon(KSPAddon.Startup.Instantly, true)]
	public class ToolbarController:MonoBehaviour
	{
		private static ToolbarController instance;
		internal static ToolbarController Instance => instance;
		private Toolbar.Toolbar controller => Toolbar.Controller.Instance.Get<ToolbarController>();

		[UsedImplicitly]
		private void Awake()
		{
			instance = this;
			DontDestroyOnLoad(this);
			GameEvents.onGameSceneLoadRequested.Add(this.OnGameSceneLoadRequested);
			GameEvents.OnMapEntered.Add(this.OnMapEntered);
		}

		[UsedImplicitly]
		private void OnDestroy()
		{
			GameEvents.OnMapEntered.Remove(this.OnMapEntered);
			GameEvents.onGameSceneLoadRequested.Remove(this.OnGameSceneLoadRequested);
		}

		[UsedImplicitly]
		private void Start()
		{
			Toolbar.Controller.Instance.Register<ToolbarController>(Version.FriendlyName);
		}

		// State controller for the toobar button
		private class WindowState:KSPe.UI.Toolbar.State.Status<bool> { protected WindowState(bool v):base(v) { }  public static implicit operator WindowState(bool v) => new WindowState(v);   public static implicit operator bool(WindowState s) => s.v; }
		private Toolbar.State.Control windowState;
		private Toolbar.Button button = null;

		internal bool state = false;
		internal bool IsRegistered { get; private set; }
		internal const string ICON_DIR = "Icons";
		private static UnityEngine.Texture2D launcher = null;
		private static UnityEngine.Texture2D launcherSupressed = null;
		private static UnityEngine.Texture2D toolbar = null;
		private static UnityEngine.Texture2D toolbarSupressed = null;

		private void PreloadAssets()
		{
			launcher = launcher ?? (launcher = Asset.Texture2D.LoadFromFile(ICON_DIR, "AirshipIconOn"));
			launcherSupressed = launcherSupressed ?? (launcherSupressed = Asset.Texture2D.LoadFromFile(ICON_DIR, "AirshipIcon"));
			toolbar = toolbar ?? (toolbar = Asset.Texture2D.LoadFromFile(ICON_DIR, "HLOnIcon"));
			toolbarSupressed = toolbarSupressed ?? (toolbarSupressed = Asset.Texture2D.LoadFromFile(ICON_DIR, "HLOffIcon"));
		}

		private void PostRegister()
		{
			windowState = this.button.State.Controller.Create<WindowState>(
				new Dictionary<Toolbar.State.Status, Toolbar.State.Data> {
							{ (WindowState)false, Toolbar.State.Data.Create(launcherSupressed, toolbarSupressed) }
							,{ (WindowState)true, Toolbar.State.Data.Create(launcher, toolbar) }
				}
			);

			this.button.Mouse.Add(Toolbar.Button.MouseEvents.Kind.Left, this.Button_OnLeftClick);
			this.button.Mouse.Add(Toolbar.Button.MouseEvents.Kind.Right, this.Button_OnRightClick);
			this.controller.Add(this.button);
			ToolbarController.Instance.ButtonsActive(true, true);
			this.IsRegistered = true;
			this.state = false;
		}

		internal void Register_Editor()
		{
			this.PreloadAssets();
			this.button = Toolbar.Button.Create(this
					, ApplicationLauncher.AppScenes.SPH | ApplicationLauncher.AppScenes.VAB
					, launcherSupressed
					, toolbarSupressed
					, Version.FriendlyName
				);
			this.PostRegister();
		}

		internal void Register_Flight()
		{
			this.PreloadAssets();
			this.button = Toolbar.Button.Create(this
					, ApplicationLauncher.AppScenes.FLIGHT
					, launcherSupressed
					, toolbarSupressed
					, Version.FriendlyName
				);
			this.PostRegister();
		}

		internal void ButtonsActive(bool enableBlizzy, bool enableStock)
		{
			this.controller.ButtonsActive(enableBlizzy, enableStock);
		}

		internal void Unregister()
		{
			this.controller.Destroy();
			this.button = null;
			this.state = false;
			this.IsRegistered = false;
		}

		internal void ResetState()
		{
			this.state = false;
			this.button.Status = (WindowState)this.state;
		}

		internal void ToggleState()
		{
			this.state = !this.state;
			this.button.Status = (WindowState)this.state;
		}

		private void Button_OnLeftClick()
		{
			Log.dbg("Left Click!!!");
			this.ToggleState();
		}

		private void Button_OnRightClick()
		{
			Log.dbg("Right Click!!!");
		}

		private void OnGameSceneLoadRequested(GameScenes data)
		{
			this.ResetState();
		}

		private void OnMapEntered()
		{
			this.ResetState();
		}
	}
}
