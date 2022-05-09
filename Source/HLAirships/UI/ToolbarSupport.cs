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

using UnityEngine;
using KSP.UI.Screens;

using KSPe.Annotations;
using System.Collections.Generic;

namespace HLAirships.UI
{
	[KSPAddon(KSPAddon.Startup.Instantly, true)]
	internal partial class ToolbarController:MonoBehaviour
	{
		private static ToolbarController instance;
		internal static ToolbarController Instance => instance;

		[UsedImplicitly]
		private void Awake()
		{
			instance = this;
			this.InitToolbar();
			DontDestroyOnLoad(this);
		}

		internal bool state = false;
		internal bool IsRegistered { get; private set; }

		internal void Register_Editor()
		{
			this.registerEvents();
			this.Do_Register_Editor();
		}

		internal void Register_Flight()
		{
			this.registerEvents();
			this.Do_Register_Flight();
			this.IsRegistered = true;
		}

		private void registerEvents()
		{
			GameEvents.onGameSceneLoadRequested.Add(this.OnGameSceneLoadRequested);
			GameEvents.OnMapEntered.Add(this.OnMapEntered);
			this.IsRegistered = true;
		}

		internal void Unregister()
		{
			GameEvents.OnMapEntered.Remove(this.OnMapEntered);
			GameEvents.onGameSceneLoadRequested.Remove(this.OnGameSceneLoadRequested);
			this.Do_Unregister();
			this.state = false;
			this.IsRegistered = false;
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

		private void OnGameSceneLoadRequested(GameScenes data) => ResetState();
		private void OnMapEntered() => this.ResetState();
	}
}
