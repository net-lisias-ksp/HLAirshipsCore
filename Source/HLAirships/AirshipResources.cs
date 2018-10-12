using System;
using System.Collections.Generic;
using UnityEngine;
using KSPPluginFramework;

namespace HLAirships
{
	internal static class AirshipResources
	{

		//Clock Icons
		internal static Texture2D iconAirshipControlWindow = new Texture2D(32, 32, TextureFormat.ARGB32, false);
		internal static Texture2D iconAirshipControlWindowActive = new Texture2D(32, 32, TextureFormat.ARGB32, false);

		internal static void loadGUIAssets()
		{
			MonoBehaviourExtended.LogFormatted("Loading Textures");

			try
			{
				LoadingUtilities.LoadImageFromFile(ref iconAirshipControlWindow, "AirshipIcon.png", LoadingUtilities.PathToolbarIcons);
				LoadingUtilities.LoadImageFromFile(ref iconAirshipControlWindowActive, "AirshipIconOn.png", LoadingUtilities.PathToolbarIcons);


				MonoBehaviourExtended.LogFormatted("Loaded Textures");
			}
			catch (Exception)
			{
				MonoBehaviourExtended.LogFormatted("Failed to Load Textures - are you missing a file?");
			}


		}

	}
}
