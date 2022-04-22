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
// Every Envelope is controlled by the Compressor.  These envelopes provide lift.

using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using KSP;
using System.Linq;


namespace HLAirships
{

	public enum AnimationState : int
	{
		AtBeginning = -2,
		AtEnd = 2,
		PlayForwards = 1,
		PlayBackwards = -1,
		UNK = 0
	}



	public class HLEnvelopePartModule : PartModule
	{
		// Private variables cannot be changed by the part.cfg file
		#region KSPFields
		// These public values can be overwritten in the part.cfg files.
		// The limit for buoyant force (0 to disable). Allows operation at low altitudes even with large envelope volumes, though perhaps gas could not be compressed this much.
		// The minimum atmospheric pressure the blimp can operate in.
		// Used for inflateable parachutes
		// The maximum speed at which the vessel can be made stationary

		[KSPField(isPersistant = true, guiActive = false)]
		public float limitBuoyantForce = 0f;

		[KSPField(isPersistant = true, guiActive = false)]
		public float minAtmPressure = -0.01f;

		[KSPField(isPersistant = true, guiActive = false)]
		public float dragDeployed = 0f;

		[KSPField(isPersistant = true, guiActive = false)]
		public float dragUndeployed = 0f;

		[KSPField(isPersistant = true, guiActive = false)]
		public float makeStationarySpeedMax = 1f;

		[KSPField(isPersistant = true, guiActive = false)]
		public float makeStationarySpeedClamp = 0.0f;

		// Compress and expand rate per second
		[KSPField(isPersistant = true, guiActive = false)]
		public float compressRate = 0.01f, expandRate = 0.01f;

		// The envelope volume. Not calculated from the size of the mesh because realistic values provide too much lift for stable operation, as said by Ludo
		// However, this has been recalculated based on model measurements done in Blender
		[KSPField(isPersistant = false, guiActive = true, guiName = "Max Envelope Volume", guiUnits = "m3", guiFormat = "F2")]
		public float envelopeVolume = 15f;

		// This multiplies the volume of the envelope.  Essentially, this is as if the envelope was X times larger when calculating buoyancy.  Note this does not affect mass.
		[KSPField(isPersistant = false, guiActive = false, guiName = "Envelope Volume Scale", guiFormat = "F2")]
		public float envelopeVolumeScale = 1f;

		// Target buoyant force (effectively specific volume) fraction of maximum possible, set by GUI
		[KSPField(isPersistant = true, guiActive = false, guiName = "Vessel Normal", guiFormat = "F2")]
		public float targetBuoyantVessel = 0.0f;
		// Target buoyant force (effectively specific volume) fraction of maximum possible, set by GUI
		[KSPField(isPersistant = false, guiActive = true, guiName = "Vessel %", guiFormat = "F2")]
		private float targetBuoyantVesselPercent = 0;

		// Is this the lead envelope?
		// Fraction of lifting gas specific volume the envelope
		[KSPField(isPersistant = true, guiActive = false, guiName = "Envelope Normal")]
		public float specificVolumeFractionEnvelope = 0.50f;
		[KSPField(isPersistant = false, guiActive = true, guiName = "Envelope %", guiFormat = "F2")]
		private float specificVolumeFractionEnvelopePercent = 0;

		[KSPField(isPersistant = true, guiActive = false, guiName = "Altitude Control")]
		private bool toggleAltitudeControl = false;
		[KSPField(isPersistant = true, guiActive = false, guiName = "Target Vertical Velocity", guiFormat = "F3")]
		public float targetVerticalVelocity = 0f;
		[KSPField(isPersistant = true, guiActive = false, guiName = "Automatic Pitch Control")]
		public bool toggleAutoPitch = false;

		[KSPField(isPersistant = true, guiActive = false, guiName = "Pitch Normal")]
		public float targetPitchBuoyancy = 0;
		[KSPField(isPersistant = false, guiActive = true, guiName = "Pitch %", guiFormat = "F2")]
		private float targetPitchBuoyancPercent = 0;

		[KSPField(isPersistant = true, guiActive = false, guiName = "Deploy Animation")]
		public bool animationDeploy = false;

		[KSPField(isPersistant = true, guiActive = true, guiName = "Lead Envelope")]
		public bool isLeadEnvelope = false;

		[KSPField(isPersistant = true, guiActive = false)]
		public float lineOffsetMultiplier = 2f;

		[KSPField(isPersistant = false, guiActive = false, guiName = "Display Hologram")]
		public bool displayHologram = false;

		// Animation
		[KSPField(isPersistant = true)]
		public bool envelopeHasAnimation = false;

		[KSPField(isPersistant = true)]
		public string animationName = "Default Take";

		// Used if animation is triggered by key
		// [KSPField]
		// public string activationKey;

		[KSPField(isPersistant = false)]
		public float animationState = (float)AnimationState.UNK;
		#endregion

		#region Other Variables

		// Basic vectors
		private Vector3 heading;
		public Vector3 buoyantForce, maxBuoyancy;

		// For automatic pitch control
		float previousRotation;

		private float totalBuoyancy;
		private float totalBuoyancyMax;
		private float totalMass;
		private float totalGravityForce;
		private float atmosDensity;


		private List<HLEnvelopePartModule> Envelopes = new List<HLEnvelopePartModule>();
		private Part leadEnvelope;
		private HLEnvelopePartModule envelope;

		// Debug
		// private int activeVessel = 0;

		// Debug lines
		private LineRenderer lineGravity = null;
		private LineRenderer lineCorrect = null;
		private LineRenderer linePosition = null;
		private LineRenderer lineCorrectProjected = null;
		private LineRenderer linePositionProjected = null;
		private GameObject objGravity = null;
		private GameObject objUp = null;
		private GameObject objPosition = null;
		private GameObject objUpProjected = null;
		private GameObject objPositionProjected = null;

		// Animation
		private Animation m_Animation;

		// Hackish workaround to avoid having to convert from the float KSPField and integer enum values
		private AnimationState AniState
		{
			get
			{
				return (AnimationState)((int)animationState);
			}
			set
			{
				animationState = (float)((int)value);
			}
		}
		#endregion

		#region KSPEvents

		[KSPEvent(guiActive = true, guiName = "Envelope Buoyancy ++")]
		public void BuoyancyPP_Event()
		{
			foreach (HLEnvelopePartModule envelope in Envelopes)
			{
				toggleAutoPitch = false;
			}
			targetPitchBuoyancy += 0.01f;
		}

		[KSPEvent(active = true, guiActive = true, guiActiveUnfocused = true, unfocusedRange = 2000, guiName = "Envelope Buoyancy +")]
		public void BuoyancyP_Event()
		{
			foreach (HLEnvelopePartModule envelope in Envelopes)
			{
				toggleAutoPitch = false;
			}
			targetPitchBuoyancy += 0.001f;
		}

		[KSPEvent(active = true, guiActive = true, guiActiveUnfocused = true, unfocusedRange = 2000, guiName = "Envelope Buoyancy -")]
		public void BuoyancyN_Event()
		{
			foreach (HLEnvelopePartModule envelope in Envelopes)
			{
				toggleAutoPitch = false;
			}
			targetPitchBuoyancy -= 0.001f;
		}

		[KSPEvent(active = true, guiActive = true, guiActiveUnfocused = true, unfocusedRange = 2000, guiName = "Envelope Buoyancy --")]
		public void BuoyancyNN_Event()
		{
			foreach (HLEnvelopePartModule envelope in Envelopes)
			{
				toggleAutoPitch = false;
			}
			targetPitchBuoyancy -= 0.01f;
		}

		[KSPEvent(active = true, guiActive = true, guiActiveUnfocused = true, unfocusedRange = 2000, guiName = "Envelope Buoyancy Max")]
		public void BuoyancyM_Event()
		{
			foreach (HLEnvelopePartModule envelope in Envelopes)
			{
				toggleAutoPitch = false;
			}
			targetPitchBuoyancy = 1f;
		}

		[KSPEvent(active = true, guiActive = true, guiActiveUnfocused = true, unfocusedRange = 2000, guiName = "Envelope Buoyancy Min")]
		public void BuoyancyZ_Event()
		{
			foreach (HLEnvelopePartModule envelope in Envelopes)
			{
				toggleAutoPitch = false;
			}
			targetPitchBuoyancy = -1f;
		}

		[KSPEvent(active = true, guiActive = true, guiActiveUnfocused = true, unfocusedRange = 2000, guiName = "Envelope Clear Adjustments")]
		public void BuoyancyC_Event()
		{
			foreach (HLEnvelopePartModule envelope in Envelopes)
			{
				toggleAutoPitch = false;
			}
			targetPitchBuoyancy = 0f;
		}

		[KSPEvent(active = true, guiActive = true, guiName = "Toggle GUI")]
		public void guiToggle_Event()
		{
			if (HLEnvelopeControlWindow.Instance)
			{
				HLEnvelopeControlWindow.Instance.ControlWindowVisible = !HLEnvelopeControlWindow.Instance.ControlWindowVisible;
			}
		}

		#endregion

		#region KSPActions

		[KSPAction("GUI Toggle")]
		public void guiToggle_Action(KSPActionParam param)
		{
			guiToggle_Event();
		}

		[KSPAction("Buoyancy ++")]
		public void BuoyancyP_Action(KSPActionParam param)
		{
			BuoyancyPP_Event();
		}

		[KSPAction("Buoyancy +")]
		public void BuoyancyPP_Action(KSPActionParam param)
		{
			BuoyancyP_Event();
		}

		[KSPAction("Buoyancy -")]
		public void BuoyancyN_Action(KSPActionParam param)
		{
			BuoyancyN_Event();
		}

		[KSPAction("Buoyancy --")]
		public void BuoyancyNN_Action(KSPActionParam param)
		{
			BuoyancyNN_Event();
		}

		[KSPAction("Buoyancy Max")]
		public void BuoyancyM_Action(KSPActionParam param)
		{
			BuoyancyM_Event();
		}

		[KSPAction("Buoyancy Zero")]
		public void BuoyancyZ_Action(KSPActionParam param)
		{
			BuoyancyZ_Event();
		}

		[KSPAction("Clear Adjustments")]
		public void BuoyancyC_Action(KSPActionParam param)
		{
			BuoyancyC_Event();
		}

		#endregion

		public override void OnAwake()
		{
			/// Constructor style setup. 
			/// Called in the Part\'s Awake method.  
			/// The model may not be built by this point.

			base.OnAwake();

			this.enabled = HighLogic.LoadedSceneIsEditor || HighLogic.LoadedSceneIsFlight;
			if (!this.enabled) return;

			this.objGravity = new GameObject();
			this.objUp = new GameObject();
			this.objPosition = new GameObject();
			this.objUpProjected = new GameObject();
			this.objPositionProjected = new GameObject();

			// Set starting animation state
			Log.trace("Set Envelope Animation");
			if (this.AniState == AnimationState.UNK || targetBuoyantVessel == 0)
			{
				// If it does not have animation start it "opened"
				this.AniState = this.envelopeHasAnimation ? AnimationState.AtBeginning : AnimationState.AtEnd;
			}
			else
				// Start opened if it is buoyant
				this.AniState = AnimationState.AtEnd;

		}

		public override void OnSave(ConfigNode node)
		{
			base.OnSave(node);
			if (HLBuildAidWindow.Instance != null && part.parent != null)
			{
				float treeVolume = 0;
				HLBuildAidWindow.Instance.TotalMass = FindTotalMass(this.part, out treeVolume);
				HLBuildAidWindow.Instance.TotalEnvelopeVolume = treeVolume;
			}
		}


		public override void OnUpdate()
		{
			base.OnUpdate();

			// Update % PAW
			this.targetBuoyantVesselPercent = this.targetBuoyantVessel * 100f;
			this.targetPitchBuoyancPercent = this.targetPitchBuoyancPercent * 100f;
			this.specificVolumeFractionEnvelopePercent = this.specificVolumeFractionEnvelope * 100f;

			/// Per-frame update 
			/// Called ONLY when Part is ACTIVE! 

			// Sets one envelope to run the control logic
			determineLeadEnvelope();


			// Try to animate if there is no animation
			if (envelopeHasAnimation)
				animateEnvelope();
		}


		internal static float FindTotalMass(Part part, out float envelopeVolume)
		{
			envelopeVolume = 0;
			Part rootPart = part;
			while(rootPart.parent != null)
			{
				rootPart = rootPart.parent;
			}
			return FindMassTree(rootPart, ref envelopeVolume);
		}

		private static float FindMassTree(Part part, ref float envelopeVolume)
		{
			float mass = part.mass + part.GetResourceMass();
			var envelope = part.Modules.OfType<HLEnvelopePartModule>().FirstOrDefault();
			if (envelope != null)
			{
				envelopeVolume += envelope.envelopeVolume * envelope.envelopeVolumeScale;
			}
			// recurse children
			foreach (Part childPart in part.children)
			{
				mass += FindMassTree(childPart, ref envelopeVolume);
			}
			return mass;
		}

		public void animateEnvelope()
		{
			// Beginning and end animations should be mostly removed
			switch (this.AniState)
			{
				case AnimationState.AtBeginning:
					this.m_Animation[this.animationName].speed = 0;
					if (this.m_Animation[this.animationName].time != 0.0f)
						this.m_Animation[this.animationName].time = 0.0f;
					if (animationDeploy)
					{
						this.AniState = AnimationState.PlayForwards;
						Log.dbg("Play Forwards");
					}
					break;

				case AnimationState.AtEnd:
					this.m_Animation[this.animationName].speed = 0;
					if (this.m_Animation[this.animationName].time !=
							this.m_Animation[this.animationName].length)
						this.m_Animation[this.animationName].time =
							this.m_Animation[this.animationName].length;
					if (!animationDeploy)
					{
						this.AniState = AnimationState.PlayBackwards;
						Log.dbg("Play Backwards");
					}
					break;

				case AnimationState.PlayForwards:
					this.m_Animation[this.animationName].speed = 1;
					/*
					if (!this.m_Animation.IsPlaying(this.animationName))
					{
						Log.dbg("At End");
						this.AniState = AnimationState.AtEnd;
					}
					 */
					if (!animationDeploy)
					{
						this.AniState = AnimationState.PlayBackwards;
						Log.dbg("Play Backwards");
					}
					// Should stop animation near end
					if (this.m_Animation[this.animationName].time >= this.m_Animation[this.animationName].length * 0.99) this.m_Animation[this.animationName].speed = 0;
					break;

				case AnimationState.PlayBackwards:
					this.m_Animation[this.animationName].speed = -1;
					/*
					if (!this.m_Animation.IsPlaying(this.animationName))
					{
						Log.dbg("At Beginning");
						this.AniState = AnimationState.AtBeginning;
					}
					 */
					if (animationDeploy)
					{
						this.AniState = AnimationState.PlayForwards;
						Log.dbg("Play Forwards");
					}
					// Should stop animation near beginning
					if (this.m_Animation[this.animationName].time <= this.m_Animation[this.animationName].length * 0.01) this.m_Animation[this.animationName].speed = 0;
					break;
			}
			Log.dbg("Animation Time = {0}, Animation State = {1}", this.m_Animation[this.animationName].time, this.AniState);
			this.m_Animation.Play(this.animationName);
		}

		public override void OnStart(StartState state)
		{
			base.OnStart(state);

			// OnFlightStart seems to have been removed
			/// Called during the Part startup. 
			/// StartState gives flag values of initial state
			Log.trace("HL Airship Plugin Start");
			if (!this.enabled) return;

			specificVolumeFractionEnvelope = Mathf.Clamp01(specificVolumeFractionEnvelope);

			Log.trace("Check for animation");
			if (this.GetComponentInChildren<Animation>() != null) m_Animation = this.GetComponentInChildren<Animation>();

			Log.trace("Set maximum drag");
			dragUndeployed = part.maximum_drag;

			// activeVessel = FlightGlobals.ActiveVessel.GetInstanceID();
			Log.trace("Set lead envelope");
			if (this.part == leadEnvelope) { targetBuoyantVessel = Mathf.Clamp01(targetBuoyantVessel); }

			// Debug lines
			Log.trace("Create Lines");
			lineCorrect = objUp.AddComponent<LineRenderer>();
			linePosition = objPosition.AddComponent<LineRenderer>();
			linePositionProjected = objPositionProjected.AddComponent<LineRenderer>();
			lineCorrectProjected = objUpProjected.AddComponent<LineRenderer>();
			lineGravity = objGravity.AddComponent<LineRenderer>();

			Log.trace("Parameterize Lines");
			try { makeLines(linePosition, Color.red); makeLines(linePositionProjected, Color.green); makeLines(lineCorrect, Color.blue); makeLines(lineCorrectProjected, Color.magenta); makeLines(lineGravity, Color.white); }
			catch (Exception ex) { print("makeLines Exception!"); print(ex.Message); }
		}

		public void makeLines(LineRenderer line, Color color)
		{
			line.transform.parent = null; // ...child to our part...
			line.useWorldSpace = true; // ...and moving along with it (rather 
									   // than staying in fixed world coordinates)
									   // line.transform.localPosition = Vector3.zero;
									   // line.transform.localEulerAngles = Vector3.zero;

			// Make it render a red to yellow triangle, 1 meter wide and 2 meters long
			line.material = new Material(Shader.Find("Particles/Additive"));
			line.SetColors(color, color);
			line.SetWidth(0.5f, 0);
			line.SetVertexCount(2);
			line.SetPosition(0, Vector3.zero);
			line.SetPosition(1, Vector3.zero);
		}

		public override void OnFixedUpdate()
		{
			// By some reason (had I understood this wrong?), OnFixedUpdate was being called even when this.enabled was false!
			if (!this.enabled) return;

			base.OnFixedUpdate();

			/// Per-physx-frame update 
			/// Called ONLY when Part is ACTIVE!
			/// 
			if (leadEnvelope == this.part) leadEnvelopeUpdate();

			// Update buoyancy properties
			envelopeUpdate();
		}

		private void determineLeadEnvelope()
		{
			// Finds all 
			try { findEnvelopes(); }
			catch (Exception ex) { print("findEnvelopes Exception!"); print(ex.Message); }
		}


		public void leadEnvelopeUpdate()
		{
			// Direction in which the ship currently points
			heading = (Vector3d)this.vessel.transform.up;

			// Finds all parts within the vessel (specifically mass)
			try { findParts(); }
			catch (Exception ex) { print("findParts Exception!"); print(ex.Message); }

			// Keeps the airship at a steady altitude
			try { altitudeControl(); }
			catch (Exception ex) { print("altitudeControl Exception!"); print(ex.Message); }

			// Tells each envelope what its buoyancy should be
			try { setBuoyancy(); }
			catch (Exception ex) { print("setBuoyancy Exception!"); print(ex.Message); }
		}

		private void SyncControlUIValues()
		{
			if (HLEnvelopeControlWindow.Instance && vessel == FlightGlobals.ActiveVessel)
			{
				HLEnvelopeControlWindow.Instance.MakeStationarySpeedMax = makeStationarySpeedMax;
				HLEnvelopeControlWindow.Instance.MaxBuoyancy = maxBuoyancy;
				HLEnvelopeControlWindow.Instance.TotalBuoyancy = totalBuoyancy;

				toggleAltitudeControl = HLEnvelopeControlWindow.Instance.ToggleAltitudeControl;
				if (toggleAltitudeControl)
				{
					// if altitude control is on, send calculated buoyancy value to GUI
					HLEnvelopeControlWindow.Instance.TargetBuoyantVessel = targetBuoyantVessel;
				}
				else
				{
					// otherwise we just read the setting
					targetBuoyantVessel = HLEnvelopeControlWindow.Instance.TargetBuoyantVessel;
				}
				targetVerticalVelocity = HLEnvelopeControlWindow.Instance.TargetVerticalVelocity;
				if(toggleAutoPitch != HLEnvelopeControlWindow.Instance.ToggleAutoPitch || 
					displayHologram != HLEnvelopeControlWindow.Instance.DisplayHologram)
				{
					lineGravity.SetPosition(0, Vector3.zero);
					lineGravity.SetPosition(1, Vector3.zero);

					linePosition.SetPosition(0, Vector3.zero);
					linePosition.SetPosition(1, Vector3.zero);

					linePositionProjected.SetPosition(0, Vector3.zero);
					linePositionProjected.SetPosition(1, Vector3.zero);

					lineCorrect.SetPosition(0, Vector3.zero);
					lineCorrect.SetPosition(1, Vector3.zero);

					lineCorrectProjected.SetPosition(0, Vector3.zero);
					lineCorrectProjected.SetPosition(1, Vector3.zero);
				}
				if (toggleAutoPitch != HLEnvelopeControlWindow.Instance.ToggleAutoPitch)
				{
					foreach(var envelope in Envelopes)
					{
						envelope.targetPitchBuoyancy = 0;
					}
				}
				toggleAutoPitch = HLEnvelopeControlWindow.Instance.ToggleAutoPitch;
				displayHologram = HLEnvelopeControlWindow.Instance.DisplayHologram;
				lineOffsetMultiplier = HLEnvelopeControlWindow.Instance.LineOffsetMultiplier;
			}
		}

		private void InitControlUIValues()
		{
			if (HLEnvelopeControlWindow.Instance && vessel == FlightGlobals.ActiveVessel)
			{
				HLEnvelopeControlWindow.Instance.MakeStationarySpeedMax = makeStationarySpeedMax;
				HLEnvelopeControlWindow.Instance.MaxBuoyancy = maxBuoyancy;
				HLEnvelopeControlWindow.Instance.TotalBuoyancy = totalBuoyancy;
				HLEnvelopeControlWindow.Instance.CurrentVessel = vessel;

				HLEnvelopeControlWindow.Instance.TargetBuoyantVessel = targetBuoyantVessel;
				HLEnvelopeControlWindow.Instance.TargetVerticalVelocity = targetVerticalVelocity;
				HLEnvelopeControlWindow.Instance.ToggleAltitudeControl = toggleAltitudeControl;
				HLEnvelopeControlWindow.Instance.ToggleAutoPitch = toggleAutoPitch;
				HLEnvelopeControlWindow.Instance.DisplayHologram = displayHologram;
				HLEnvelopeControlWindow.Instance.LineOffsetMultiplier = lineOffsetMultiplier;
			}
		}

		private void altitudeControl()
		{
			// Zero Net Force = Gravity / Max Possible Buoyancy
			if (toggleAltitudeControl)
			{
				// Tosh's PID Controller
				// Two attempts are used here, neither worked as well as our basic method.
				// float ControlAmount = brakePid.Control(targetVerticalVelocity - (float)this.vessel.verticalSpeed);
				// targetBoyantForceFractionCompressor = Mathf.Clamp01(brakePid.Control(this.vessel.verticalSpeed);

				// My method
				// Previous method, when used during onUpdate
				// targetBoyantForceFractionCompressor = Mathf.Clamp01(totalGravityForce / totalBuoyancyMax + (float)((targetVerticalVelocity - this.vessel.verticalSpeed) * 0.02));
				// New method
				targetBuoyantVessel = Mathf.Clamp01(totalGravityForce / totalBuoyancyMax + (float)((targetVerticalVelocity - this.vessel.verticalSpeed) * Time.deltaTime));

			}
		}

		// Gathers information on the envelopes
		public void findEnvelopes()
		{
			Envelopes.Clear();
			totalBuoyancy = 0;
			totalBuoyancyMax = 0;

			// More info: http://www.grc.nasa.gov/WWW/k-12/WindTunnel/Activities/balance_of_forces.html

			foreach (Part part in this.vessel.parts)
			{
				if (part.isAttached)
				{
					// Skip if not an envelope
					if (part.Modules.OfType<HLEnvelopePartModule>().FirstOrDefault() == null)
					{
						Log.dbg("Did not find an envelope on {0}", part.name);
						continue;
					}
					envelope = part.Modules.OfType<HLEnvelopePartModule>().FirstOrDefault();

					// Envelopes have to be active to work.
					if (part.State != PartStates.ACTIVE)
						part.force_activate();

					Envelopes.Add(envelope);
					totalBuoyancy += envelope.buoyantForce.magnitude;
					totalBuoyancyMax += envelope.maxBuoyancy.magnitude;					

				}

				// If this is the lead envelope...
				if (leadEnvelope == envelope.part)
				{
					Log.dbg("Adding GUI!");
					envelope.isLeadEnvelope = true;
				}// If not, then check to see if this envelope should be promoted to lead envelope
				else if (!vessel.parts.Contains(leadEnvelope))
				{
					leadEnvelope = envelope.part;
				}
				else envelope.isLeadEnvelope = false;
			}
			if(HLEnvelopeControlWindow.Instance)
			{
				HLEnvelopeControlWindow.Instance.Envelopes = Envelopes;
				if(FlightGlobals.ActiveVessel != HLEnvelopeControlWindow.Instance.CurrentVessel)
				{
					InitControlUIValues();
				}
				SyncControlUIValues();
			}
			if (torques == null || torques.Length != Envelopes.Count)
			{
				torques = new Vector3[Envelopes.Count];
				torqueArms = new Vector3[Envelopes.Count];
				percent = new float[Envelopes.Count];
			}
		}

		private void findParts()
		{
			// Hey, a method for finding mass!
			totalMass = vessel.GetTotalMass();

			totalGravityForce = (float)(totalMass * FlightGlobals.getGeeForceAtPosition(part.WCoM).magnitude);
		}

		public void setBuoyancy()
		{
			if (toggleAutoPitch)
			{
				NeutralizeEnvelopeTorque();
			}

			// The lead envelope sets the buoyancy for every envelope, including itself
			foreach (HLEnvelopePartModule envelope in Envelopes)
			{
				envelope.targetBuoyantVessel = this.targetBuoyantVessel;

				float adjustBy = envelope.targetPitchBuoyancy;

				// Use total
				envelope.updateTargetSpecificVolumeFraction(Mathf.Clamp01(adjustBy + targetBuoyantVessel), envelope);
			}
		}
		private Vector3[] torques;
		private Vector3[] torqueArms;
		private float[] percent;
		private float lastGuess = float.NaN;
		private float searchThreshold = 0.05f;

		private void NeutralizeEnvelopeTorque()
		{
			Vector3 vCoM = vessel.GetWorldPos3D();
			Vector3 gravity = FlightGlobals.getGeeForceAtPosition(vCoM).normalized;

			// analyze system to determine net torque
			Vector3 sumTorque = new Vector3();
			int i = 0;
			foreach (HLEnvelopePartModule envelope in Envelopes)
			{
				envelope.targetBuoyantVessel = this.targetBuoyantVessel;
				torqueArms[i] = envelope.part.WCoM - vCoM;
				torques[i] = Vector3.Cross(torqueArms[i], envelope.maxBuoyancy * envelope.targetBuoyantVessel);
				sumTorque += torques[i];
				++i;
			}
			Vector3 correctTorque = sumTorque * -1;
			// if there's nothing to correct, we're done
			if(correctTorque.magnitude < 0.000001)
			{
				return;
			}
			i = 0;
			// compute percent each envelope contributes to that net torque
			float sumPercent = 0;
			foreach (HLEnvelopePartModule envelope in Envelopes)
			{
				percent[i] = Vector3.Dot(correctTorque, torques[i]);
				sumPercent += Math.Abs(percent[i]);
				++i;
			}
			for (int j = 0; j < percent.Length; ++j)
			{
				percent[j] /= sumPercent;
			}
			i = 0;
			// binary search on counter forces to apply at each envelope to achieve neutral torque
			Vector3 checkTorque = new Vector3();
			float baseline = Math.Abs(percent[0]);
			float currentGuess = lastGuess;
			float targetValue = correctTorque.magnitude;
			if (float.IsNaN(lastGuess))
			{
				currentGuess = targetValue;
			}
			bool stillSearching = true;
			float lSearch = 0;
			float rSearch = 0;
			float deltaSearch = 1.0f;
			do
			{
				checkTorque = TryTorqueValues(vCoM, gravity, baseline, currentGuess);
				float difference = (checkTorque - correctTorque).magnitude;
				if ( difference < searchThreshold )
				{
					stillSearching = false;
				}
				else
				{
					float currentMag = checkTorque.magnitude;
					if(currentMag < targetValue)
					{
						if(rSearch == 0)
						{
							lSearch = currentGuess;
							currentGuess *= 2.0f;
						}
						else
						{
							lSearch = currentGuess;
							currentGuess = (currentGuess + rSearch) / 2.0f;
						}
					}
					else
					{
						rSearch = currentGuess;
						currentGuess = (currentGuess + lSearch) / 2.0f;
					}
				}
				deltaSearch = rSearch - lSearch;
			}
			while (stillSearching && ++i < 20 && deltaSearch > 0.00001f);
			NeutralizeTorqueValues();

			lastGuess = currentGuess;
		}

		private void NeutralizeTorqueValues()
		{
			float total = 0f;
			foreach (HLEnvelopePartModule envelope in Envelopes)
			{
				total += envelope.targetPitchBuoyancy;
			}
			total /= -Envelopes.Count;
			foreach (HLEnvelopePartModule envelope in Envelopes)
			{
				envelope.targetPitchBuoyancy += total;
			}
		}

		private Vector3 TryTorqueValues(Vector3 vCoM, Vector3 gravity, float baseline, float baseCorrectForce)
		{
			Vector3 checkTorque = new Vector3();
			int i = 0;
			foreach (HLEnvelopePartModule envelope in Envelopes)
			{
				float correctForce = baseCorrectForce * percent[i] / baseline;
				envelope.targetPitchBuoyancy = Mathf.Clamp(correctForce / envelope.maxBuoyancy.magnitude, -1.0f, 1.0f);
				checkTorque += Vector3.Cross(torqueArms[i], envelope.maxBuoyancy * envelope.targetPitchBuoyancy);
				if (displayHologram)
				{
					Vector3 lineOffset = gravity * -lineOffsetMultiplier;

					envelope.lineGravity.SetPosition(0, vCoM + lineOffset);
					envelope.lineGravity.SetPosition(1, vCoM + gravity + lineOffset);

					envelope.linePosition.SetPosition(0, vCoM + lineOffset);
					envelope.linePosition.SetPosition(1, envelope.part.WCoM + lineOffset);

					envelope.linePositionProjected.SetPosition(0, envelope.part.WCoM + lineOffset);
					envelope.linePositionProjected.SetPosition(1, envelope.part.WCoM + (maxBuoyancy * envelope.targetPitchBuoyancy * 3) + lineOffset);

					envelope.lineCorrectProjected.SetPosition(0, envelope.part.WCoM + lineOffset);
					envelope.lineCorrectProjected.SetPosition(1, envelope.part.WCoM + torques[i] * 0.1f + lineOffset);
				}
				else
				{
					envelope.lineGravity.SetPosition(0, Vector3.zero);
					envelope.lineGravity.SetPosition(1, Vector3.zero);

					envelope.linePosition.SetPosition(0, Vector3.zero);
					envelope.linePosition.SetPosition(1, Vector3.zero);

					envelope.linePositionProjected.SetPosition(0, Vector3.zero);
					envelope.linePositionProjected.SetPosition(1, Vector3.zero);

					envelope.lineCorrect.SetPosition(0, vCoM + Vector3.zero);
					envelope.lineCorrect.SetPosition(1, vCoM + Vector3.zero);

					envelope.lineCorrectProjected.SetPosition(0, Vector3.zero);
					envelope.lineCorrectProjected.SetPosition(1, Vector3.zero);
				}
				i++;
			}
			return checkTorque;
		}


		// From http://wiki.unity3d.com/index.php?title=3d_Math_functions
		//Projects a vector onto a plane. The output is not normalized.
		public static Vector3 ProjectVectorOnPlane(Vector3 planeNormal, Vector3 vector)
		{
			return vector - (Vector3.Dot(vector, planeNormal) * planeNormal);
		}


		// Envelopes determine their positions and effect buoyant force on themselves
		public void envelopeUpdate()
		{
			float pressure = (float)FlightGlobals.getStaticPressure();

			// Compute the buoyant force

			// KSP provides the density based on local atmospheric pressure.
			atmosDensity = (float)FlightGlobals.getAtmDensity(pressure, FlightGlobals.getExternalTemperature());

			// The maximum buoyancy of the envelope is equal to the weight of the displaced air
			// Force (Newtons) = - Gravitational Acceleration (meters per second squared) * Density (kilograms / meter cubed) * Volume (meters cubed)
			maxBuoyancy = -FlightGlobals.getGeeForceAtPosition(part.WCoM) * atmosDensity * envelopeVolume;
			// As force in KSP is measured in kilonewtons, we divide by 1000
			maxBuoyancy /= 1000;
			// Then we apply the "volume scale", for gameplay purposes if needed.
			maxBuoyancy *= envelopeVolumeScale;
			// The final force is dependent on how filled the envelope is.
			buoyantForce = maxBuoyancy * specificVolumeFractionEnvelope;

			// If set, clamp the magnitude of the buoyant force to perhaps prevent overly straining the vessel at low altitude...
			// Written by Ludo, not tested by Hooligan Labs.
			if (limitBuoyantForce > 0)
				if (buoyantForce.magnitude > limitBuoyantForce)
					buoyantForce = buoyantForce.normalized * limitBuoyantForce;

			if (specificVolumeFractionEnvelope < 0.0001 || pressure < minAtmPressure)
			{
				animationDeploy = false;
				if (dragDeployed != 0)
				{
					part.minimum_drag = dragUndeployed;
					part.maximum_drag = dragUndeployed;
				}
			}
			else
			{
				animationDeploy = true;
				// If deployed, the drag is a function of how deployed the balloon is
				if (dragDeployed != 0 && this.m_Animation[this.animationName].length != 0)
				{
					part.minimum_drag = dragDeployed * (this.m_Animation[this.animationName].time / this.m_Animation[this.animationName].length);
					part.maximum_drag = dragDeployed * (this.m_Animation[this.animationName].time / this.m_Animation[this.animationName].length);
				}
			}

			// TODO - scale anguglar drag based on volume
			this.part.angularDrag = envelopeVolume / 10.0f;
			// New method is to add force based on render bounds.
			this.part.Rigidbody.AddForceAtPosition(buoyantForce, part.WCoM, ForceMode.Force);
			// limit angular velocity to avoid dangerous occilations
			if (buoyantForce.magnitude > 30)
			{
				this.part.Rigidbody.angularDrag = 1.0f;
				this.part.Rigidbody.maxAngularVelocity = 1.0f;
			}
			if (part.Splashed || part.WaterContact)
			{
				//part.waterAngularDragMultiplier
				this.part.Rigidbody.angularDrag = 5.1f;
				this.part.Rigidbody.maxDepenetrationVelocity = 10;
				this.part.Rigidbody.maxAngularVelocity = 0.5f;
				// TODO - reduce force based on water penetration to avoid double counting buoyancy
				float reducedMagnitude = buoyantForce.magnitude - part.buoyancy;
			}

		}

		// Changes envelope buoyancy targets
		public void updateTargetSpecificVolumeFraction(float fraction, HLEnvelopePartModule envelope)
		{
			// Difference between specific volume and target specific volume
			float delta = fraction - envelope.specificVolumeFractionEnvelope;

			// Clamp the change to compress and expand rates
			delta = Mathf.Clamp(delta, -compressRate, expandRate);

			envelope.specificVolumeFractionEnvelope += delta * Time.deltaTime;
		}

		// Distance from center of mass
		public static float DistanceFromCoM(Vector3 position, Vector3 CoM, Vector3 mainBody, Vector3 direction)
		{
			// project the vectors on the plane with up as a normal
			return Vector3.Dot((position - mainBody) - (CoM - mainBody), direction);
		}

		public float eDistanceFromCoM {
			get {
				return DistanceFromCoM(this.part.attPos, this.vessel.CoM, this.vessel.mainBody.position, this.vessel.GetFwdVector());
			}
			set {
				throw new NotSupportedException("eDistanceFromCoM");
			}
		}

	}


}