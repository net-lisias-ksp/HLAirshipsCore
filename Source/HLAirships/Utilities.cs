using System;
using System.Collections.Generic;
using UnityEngine;
using KSPPluginFramework;

namespace HLAirships
{
	internal static class LoadingUtilities
	{
		internal static readonly String PathApp = KSPUtil.ApplicationRootPath.Replace("\\", "/");
		internal static readonly String PathModInstall = string.Format("{0}GameData/HLAirships", PathApp);
 
		internal static readonly String PathPlugin = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
		internal static readonly String PathToolbarIcons = string.Format("{0}/Icons", PathModInstall);


		/// <summary>
		/// Loads a texture from the file system directly
		/// </summary>
		/// <param name="tex">Unity Texture to Load</param>
		/// <param name="FileName">Image file name</param>
		/// <param name="FolderPath">Optional folder path of image</param>
		/// <returns></returns>
		public static Boolean LoadImageFromFile(ref Texture2D tex, String FileName, String FolderPath = "")
		{
			//DebugLogFormatted("{0},{1}",FileName, FolderPath);
			Boolean blnReturn = false;
			try
			{
				if (FolderPath == "") FolderPath = PathToolbarIcons;

				//File Exists check
				if (System.IO.File.Exists(String.Format("{0}/{1}", FolderPath, FileName)))
				{
					try
					{
						//MonoBehaviourExtended.LogFormatted_DebugOnly("Loading: {0}", String.Format("{0}/{1}", FolderPath, FileName));
						tex.LoadImage(System.IO.File.ReadAllBytes(String.Format("{0}/{1}", FolderPath, FileName)));
						blnReturn = true;
					}
					catch (Exception ex)
					{
						MonoBehaviourExtended.LogFormatted("Failed to load the texture:{0} ({1})", String.Format("{0}/{1}", FolderPath, FileName), ex.Message);
					}
				}
				else
				{
					MonoBehaviourExtended.LogFormatted("Cannot find texture to load:{0}", String.Format("{0}/{1}", FolderPath, FileName));
				}


			}
			catch (Exception ex)
			{
				MonoBehaviourExtended.LogFormatted("Failed to load (are you missing a file):{0} ({1})", String.Format("{0}/{1}", FolderPath, FileName), ex.Message);
			}
			return blnReturn;
		}

	}
}
