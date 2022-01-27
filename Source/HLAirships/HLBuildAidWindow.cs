/*
	This file is part of Hooligan Labs Airships /L Unleashed
		© 2018-2022 Lisias T : http://lisias.net <support@lisias.net>
		© 2013-2018 Jewel Shisen
		© 2012-2013 Hooligan Labs

	Hooligan Labs Airships /L Unleashed is double licensed as follows:
		* SKL 1.0 : https://ksp.lisias.net/SKL-1_0.txt
		* ARR (Pending agreement with the upstream for MIT)

	And you are allowed to choose the License that better suit your needs.

	Hooligan Labs Airships /L Unleashed is distributed in the hope that it will be
	useful, but WITHOUT ANY WARRANTY; without even the implied
	warranty of	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.

	You should have received a copy of the SKL Standard License 1.0
	along with Hooligan Labs Airships /L Unleashed.
	If not, see <https://ksp.lisias.net/SKL-1_0.txt>.
*/
using System;
using System.Collections.Generic;

using UnityEngine;

using KSP.UI.Screens;

using KSPPluginFramework;

using GUILayout = KSPe.UI.GUILayout;

namespace HLAirships
{
	[KSPAddon(KSPAddon.Startup.EditorAny, false)]
	public class HLBuildAidWindowEditor : HLBuildAidWindow
	{
		public override string MonoName { get { return this.name; } }
		//public override bool ViewAlarmsOnly { get { return false; } }
	}


	public class HLBuildAidWindow : MonoBehaviourExtended
	{

		public bool EditorWindowVisible { get; set; }
		public virtual String MonoName { get; set; }
		public static HLBuildAidWindow Instance { get; set; }
		public float TotalEnvelopeVolume { get; set; }
		public float TotalMass { get; set; }


		private Rect windowPos;
		private int airshipWindowID;
		private bool activeGUI;
		private float windowWidth;
		private int currentBody = REF_BODY_KERBIN;

		public const int REF_BODY_KERBOL = 0;
		public const int REF_BODY_KERBIN = 1;
		public List<string> selString = new List<string>();
		public string[] selStringArray;
		public List<int> BodyRef = new List<int>();
		public int currentSelection = 0;

		internal override void Awake()
		{
			Instance = this;

			GameEvents.onShowUI.Add(OnShowUI);
			GameEvents.onHideUI.Add(OnHideUI);
			InitVariables();

			ToolbarController.Instance.Register();
			ToolbarController.Instance.OnTrue += onAppLaunchToggleOn; 
			ToolbarController.Instance.OnFalse += onAppLaunchToggleOff; 
		}

		private void InitVariables()
		{
			airshipWindowID = UnityEngine.Random.Range(1000, 2000000) + _AssemblyName.GetHashCode();
			for(int i = 0; i < FlightGlobals.Bodies.Count; i++)
			{
				if(FlightGlobals.Bodies[i].atmosphere)
				{
					selString.Add(FlightGlobals.Bodies[i].name);
					BodyRef.Add(i);
				}
			}
			selStringArray = selString.ToArray();
			// default to the non-sun option
			if(selStringArray.Length > 1)
			{
				currentSelection = 1;
			}
			GameEvents.onEditorShipModified.Add(OnShipModified);
		}
		private void OnShipModified(ShipConstruct sc)
		{
			if (HLBuildAidWindow.Instance != null && sc.Parts != null)
			{
				if (sc.Parts.Count > 0)
				{
					float treeVolume = 0;
					TotalMass = HLEnvelopePartModule.FindTotalMass(sc.Parts[0], out treeVolume);
					HLBuildAidWindow.Instance.TotalEnvelopeVolume = treeVolume;
				}
				else
				{
					TotalMass = 0;
					TotalEnvelopeVolume = 0;
				}
			}
		}

		private void OnShowUI()
		{
			Log.dbg("OnShowGUI Fired");
		}
		private void OnHideUI()
		{
			Log.dbg("OnHideGUI Fired");
		}

		internal override void OnGUIEvery()
		{
			if (EditorWindowVisible)
			{
				if (!activeGUI)
				{
					initGUI();
					activeGUI = true;
				}

				drawGUI();
			}
			else
			{
				if (activeGUI)
				{
					activeGUI = false;
				}
			}
		}

		internal Boolean AppLauncherToBeSetTrue = false;
		internal DateTime AppLauncherToBeSetTrueAttemptDate;
		internal void SetAppButtonToTrue()
		{
			if (!ApplicationLauncher.Ready)
			{
				Log.dbg("not ready yet");
				AppLauncherToBeSetTrueAttemptDate = DateTime.Now;
				return;
			}
		}

		void onAppLaunchToggleOn()
		{
			Log.dbg("TOn");

			EditorWindowVisible = true;
			Log.dbg("{0}", EditorWindowVisible);
		}
		void onAppLaunchToggleOff()
		{
			Log.dbg("TOff");

			EditorWindowVisible = false;
			Log.dbg("{0}", EditorWindowVisible);
		}

		//Destroy Event - when the DLL is loaded
		internal override void OnDestroy()
		{
			Log.info("Destroying the KerbalAlarmClock-{0}", MonoName);

			//Hook the App Launcher
			ToolbarController.Instance.Unregister();

			GameEvents.onShowUI.Remove(OnShowUI);
			GameEvents.onHideUI.Remove(OnHideUI);
		}

		private void drawGUI()
		{
			windowPos = GUILayout.Window﻿﻿(airshipWindowID, windowPos, WindowGUI, "HLAirships Build Aid", GUILayout.MinWidth(200));
		}

		protected void initGUI()
		{
			windowWidth = 300;

			if ((windowPos.x == 0) && (windowPos.y == 0))
			{
				windowPos = new Rect(Screen.width - windowWidth, Screen.height * 0.25f, 10, 10);
			}
		}

		// GUI
		private void WindowGUI(int windowID)
		{
			// General GUI window information
			GUIStyle mySty = new GUIStyle(GUI.skin.button);
			mySty.normal.textColor = mySty.focused.textColor = Color.white;
			mySty.hover.textColor = mySty.active.textColor = Color.yellow;
			mySty.onNormal.textColor = mySty.onFocused.textColor = mySty.onHover.textColor = mySty.onActive.textColor = Color.green;
			mySty.padding = new RectOffset(2, 2, 2, 2);


			GUILayout.BeginVertical();
			GUILayout.Label("Buoyancy on:");
			currentSelection = GUILayout.SelectionGrid(currentSelection, selStringArray, 1);

			currentBody = BodyRef[currentSelection];

			GUILayout.Label("Envelope Volume: " + TotalEnvelopeVolume);

			string mass = "?";
			double buoyancy = ComputeMaxBuoyancy();
			mass = TotalMass.ToString("00.00");
			// weight
			double weight = TotalMass * FlightGlobals.Bodies[currentBody].GeeASL * 9.81;
			double netForce = buoyancy - weight;

			GUILayout.Label("Vessel Mass: " + mass + "T");

			GUILayout.Label("Net Buoyancy: " + netForce.ToString("00.00") + "KN");

			try
			{
				if (netForce > 0)
				{
					float tippingPoint = 0f;
					Keyframe[] frames = null;
					float maxAltitude = 100000;
					int frameIndex = 0;
					bool useCurve = true;
					if(useCurve)
					{
						frames = FlightGlobals.Bodies[currentBody].atmosphereTemperatureCurve.Curve.keys;
						maxAltitude = frames[frames.Length - 1].time;
					}
					// compute equilibrium altitude
					for (float i = 100f; i < maxAltitude; i += 100f)
					{
						double pressure = FlightGlobals.Bodies[currentBody].GetPressure(i);

						// Compute the buoyant force

						// KSP provides the density based on local atmospheric pressure.
						double atmosDensity = pressure / (287.058 * FlightGlobals.Bodies[currentBody].atmosphereTemperatureSeaLevel);
						if(useCurve)
						{
							// walk the curve forward if needed
							if (frameIndex < frames.Length - 1 && i > frames[frameIndex + 1].time)
							{
								frameIndex++;
							}
							// linear interpolation is close enough for now
							float tempCurve = ((i - frames[frameIndex].time) / (frames[frameIndex + 1].time - frames[frameIndex].time)) * (frames[frameIndex + 1].value - frames[frameIndex].value) + frames[frameIndex].value;
							atmosDensity = pressure / (287.058 * tempCurve);
						}

						// The maximum buoyancy of the envelope is equal to the weight of the displaced air
						// Force (Newtons) = - Gravitational Acceleration (meters per second squared) * Density (kilograms / meter cubed) * Volume (meters cubed)
						double maxBuoyancy = FlightGlobals.Bodies[currentBody].GeeASL * 9.81 * atmosDensity * TotalEnvelopeVolume;

						if (maxBuoyancy < weight)
						{
							tippingPoint = i;
							break;
						}
					}
					if (tippingPoint > 0)
					{
						GUILayout.Label("Equilibrium Altitude : " + tippingPoint.ToString() + "m");
					}
				}
				else
				{
					GUILayout.Label("Equilibrium Altitude : Won't Fly");
				}
			}
			catch(Exception)
			{
				GUILayout.Label("Equilibrium Altitude : Unknown");
			}

			GUILayout.EndVertical();


			GUI.DragWindow(new Rect(0, 0, 500, 20));
		}

		private double ComputeMaxBuoyancy()
		{
			double pressure = FlightGlobals.Bodies[currentBody].atmospherePressureSeaLevel;

			// Compute the buoyant force

			// KSP provides the density based on local atmospheric pressure.
			double atmosDensity = FlightGlobals.Bodies[currentBody].atmDensityASL;  //FlightGlobals.getAtmDensity(pressure, FlightGlobals.Bodies[REF_BODY_KERBIN].atmosphereTemperatureSeaLevel);

			// The maximum buoyancy of the envelope is equal to the weight of the displaced air
			// Force (Newtons) = - Gravitational Acceleration (meters per second squared) * Density (kilograms / meter cubed) * Volume (meters cubed)
			double maxBuoyancy = FlightGlobals.Bodies[currentBody].GeeASL * 9.81 * atmosDensity * TotalEnvelopeVolume;
			// As force in KSP is measured in kilonewtons, we divide by 1000
			maxBuoyancy /= 1000;


			return maxBuoyancy;
		}
	}

}
