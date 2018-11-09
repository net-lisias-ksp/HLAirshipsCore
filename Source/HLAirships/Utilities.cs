using System;
using KSPe.Util.Log;
using System.Diagnostics;

namespace KSPPluginFramework
{
	internal static class Constants
	{ 
		public const string MODID = "HLAirships";
		public const string MODNAME = "HooliganLabs Airships";
	}

	internal static class Log
	{
		private static readonly Logger LOG = Logger.CreateForType<HLAirships.HLEnvelopeControlWindow>();

		internal static void error(string msg, params object[] @params)
		{
			LOG.error(msg, @params);
		}

		internal static void info(string msg, params object[] @params)
		{
			LOG.info(msg, @params);
		}

		internal static void detail(string msg, params object[] @params)
		{
			LOG.detail(msg, @params);
		}

		[Conditional("DEBUG")]
		internal static void dbg(string msg, params object[] @params)
		{
			LOG.dbg(msg, @params);
		}

	}
}
