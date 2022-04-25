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
using IO = KSPe.IO;
using Directory = KSPe.IO.Directory;
using Path = KSPe.IO.Path;

namespace HLAirshipsCore
{
	public class ModuleManagerSupport : UnityEngine.MonoBehaviour
	{
		public static IEnumerable<string> ModuleManagerAddToModList()
		{
			List<string> list = new List<string>();

			string airships = IO.Hierarchy.GAMEDATA.Solve("HLAirships");
			if (Directory.Exists(airships))
			{
			}
			else
				// If HLAirships is not installed, fool everybody else into believing original HLAirhips is installed so eventual
				// clients would patch the Modules.
				list.Add("HLAirships");	
			string[] r = list.ToArray();
			return r;
		}
	}
}
