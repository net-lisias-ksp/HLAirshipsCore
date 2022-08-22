/*
	This file is part of HLAirshipsCore Watch Dog
		©2022 Lisias T : http://lisias.net <support@lisias.net>

	HLAirshipsCore Watch Dog is licensed as follows:
		* SKL 1.0 : https://ksp.lisias.net/SKL-1_0.txt

	HLAirshipsCore Watch Dog is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.

	You should have received a copy of the SKL Standard License 1.0
	along with HLAirshipsCore Watch Dog. If not, see
	<https://ksp.lisias.net/SKL-1_0.txt>.

*/
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using KAssemblyLoader = KSPe.Util.AssemblyLoader;
using IO = KSPe.IO;

namespace HLAirshipsCore
{
	[KSPAddon(KSPAddon.Startup.Instantly, true)]
	internal class Startup:MonoBehaviour
	{
		private void Start()
		{
			Log.force("Version {0}", Version.Text);

			try
			{
				KSPe.Util.Installation.Check<Startup>("HLA.WatchDog", "HLAirshipsCore", null);

				{ 
					IEnumerable<AssemblyLoader.LoadedAssembly> assemblies = KAssemblyLoader.Search.ByName("HLAirships");
					int count = assemblies.Count();
					switch (count)
					{
						case 0: UI.ShowStopperAlertBox.Show("HLAirships' DLL is not installed!"); return;
						case 1: break;
						default:
							{
								string[] dirs = (from i in assemblies select string.Format("* {0}"
												 , IO.Path.GetDirectoryName(IO.Path.GetPath(i.assembly.Location))
												)
											).ToArray();
								UI.ShowStopperAlertBox.Show(string.Format(
									"Multiple HLAirships' DLLs were found, but there can be only one!\n{0}"
									, string.Join("\n", dirs)
								));
							}
							return;
					}
				}
				{
					// Survives the KSP 1.12.3 idiocy that prevented me from detecting multiple copies of an Assembly
					// At least I can be sure about the winning one being the wanted one.
					System.Reflection.Assembly assembly = KSPe.Util.SystemTools.Assembly.Finder.FindByName("HLAirships");
					System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
					string hisplace = IO.Path.GetDirectoryName(IO.Path.GetPath(assembly.Location));
					if (null == fvi || !fvi.FileVersion.Equals(System.Diagnostics.FileVersionInfo.GetVersionInfo(typeof(Startup).Assembly.Location).FileVersion))
						UI.ShowStopperAlertBox.Show(string.Format("An older version of HLAirships was found on {0}", hisplace));
					{
						string myplace = IO.Path.GetDirectoryName(IO.Path.GetPath(typeof(Startup).Assembly.Location));
						if (!myplace.Equals(hisplace))
							UI.ShowStopperAlertBox.Show(string.Format("HLAirships DLL is installed on the wrong directory! {0}", hisplace));
					}
				}
				{
					// Prevents the old TweakScale patch from screwing up the new ones!
					string airships = IO.Hierarchy.GAMEDATA.Solve("HLAirships");
					string oldpatch = IO.Path.Combine(airships, "Parts","TweakScale.cfg");
					Log.dbg("The old path path is {0}", oldpatch);
					if (System.IO.File.Exists(oldpatch))
						UI.ShowStopperAlertBox.Show(string.Format("Deprected TweakScale patch for HLAirships must be removed! {0}", oldpatch));
				}
			}
			catch (KSPe.Util.InstallmentException e)
			{
				Log.err(e.ToShortMessage());
				KSPe.Common.Dialogs.ShowStopperAlertBox.Show(e);
			}
		}
	}
}
