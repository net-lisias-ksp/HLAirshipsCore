/*
	This file is part of HLAirshipsCore Watch Dog
		©2022 Lisias T : http://lisias.net <support@lisias.net>

	HLAirshipsCore Watch Dog is licensed as follows:
		* SKL 1.0 : https://ksp.lisias.net/SKL-1_0.txt

	HLAirshipsCore Watch Dog is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.

	You should have received a copy of the SKL Standard License 1.0
	along with Module Manager Watch Dog. If not, see
	<https://ksp.lisias.net/SKL-1_0.txt>.

*/
using UnityEngine;

using Logger = KSPe.Util.Log.Logger;
using System.Diagnostics;

namespace HLAirshipsCore
{
    public static class Log
    {
        private static readonly Logger LOG = Logger.CreateForType<HLAirshipsCore.Startup>();

        public static void force(string format, params object[] @parms)
        {
            LOG.force(format, parms);
        }

        public static void detail(string format, params object[] @parms)
        {
            LOG.detail(format, parms);
        }

        public static void info(string format, params object[] @parms)
        {
            LOG.info(format, parms);
        }

        public static void warn(string format, params object[] @parms)
        {
            LOG.warn(format, parms);
        }

        public static void err(string format, params object[] parms)
        {
            LOG.error(format, parms);
        }

        public static void err(System.Exception ex, string format, params object[] parms)
        {
            LOG.error(ex, format, parms);
        }

        public static void ex(MonoBehaviour offended, System.Exception e)
        {
            LOG.error(offended, e);
        }

        public static void trace(string format, params object[] @parms)
        {
            LOG.dbg(format, parms);
        }

        [Conditional("DEBUG")]
        public static void dbg(string format, params object[] @parms)
        {
            LOG.dbg(format, parms);
        }
    }
}
