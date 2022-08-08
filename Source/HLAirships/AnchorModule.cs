/*
	This file is part of Hooligan Labs Airships Core
		© 2018-2022 Lisias T : http://lisias.net <support@lisias.net>
		© 2013-2021 Jewel Shisen
		© 2012-2013 Hooligan Labs

	Hooligan Labs Airships Core is licensed as follows:
		* MIT (Expat) : https://opensource.org/licenses/MIT

	Hooligan Labs Airships Core is distributed in the hope that it will be
	useful, but WITHOUT ANY WARRANTY; without even the implied
	warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.

*/
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HLAirships
{
	class AnchorModule : PartModule
	{
		[KSPField(isPersistant = true, guiActive = true, guiName = "Anchored")]
		Boolean Anchored;
		[KSPField(isPersistant = true, guiActive = true, guiName = "Auto UnAnchor")]
		Boolean autoAnchor;

		[KSPField(isPersistant = true, guiActive = false)]
		private Vector3 AnchorPosition = new Vector3(0f, 0f, 0f);
		[KSPField(isPersistant = true, guiActive = false)]
		private Vector3 AnchorVelocity = new Vector3(0f, 0f, 0f);
		[KSPField(isPersistant = true, guiActive = false)]
		private Vector3 AnchorAcceleration = new Vector3(0f, 0f, 0f);
		[KSPField(isPersistant = true, guiActive = false)]
		private Vector3 AnchorAngularVelocity = new Vector3(0f, 0f, 0f);

		[KSPField(isPersistant = true, guiActive = false)]
		Vessel.Situations previousState = Vessel.Situations.LANDED;

		//have you ever clicked "AirAnchored"? Rember to keep interesting things from happening
		[KSPField(isPersistant = true, guiActive = false)]
		public bool isActive = false;

		private static Vector3 zeroVector = new Vector3(0f, 0f, 0f);

		private bool inactivePart = true;

		public override void OnStart(StartState state)
		{
			base.OnStart(state);

			if (state != StartState.Editor)
			{
				InitBaseState();
			}
		}

		private void InitBaseState()
		{
			if (vessel != null)
			{
				AnchorPosition = vessel.transform.position;
				part.force_activate();
			}
		}

		public override void OnSave(ConfigNode node)
		{
			base.OnSave(node);
			if (vessel != null)
			{
				AnchorPosition = GetVesselPostion();
			}
		}

		private void RememberPreviousState()
		{
			if (!Anchored && previousState != Vessel.Situations.LANDED)
			{
				previousState = vessel.situation;
				SetUpToolbarUI();
			}
		}

		private void SetUpToolbarUI()
		{
			if (HLEnvelopeControlWindow.Instance != null && vessel.isActiveVessel)
			{
				HLEnvelopeControlWindow.Instance.AnchorPresent = true;
				HLEnvelopeControlWindow.Instance.AnchorOn = Anchored;
				HLEnvelopeControlWindow.Instance.AutoAnchor = autoAnchor;
				inactivePart = false;
			}
			else
			{
				inactivePart = true;
			}
		}

		public override void OnFixedUpdate()
		{
			base.OnFixedUpdate();

			// if we were initialized as inactive and are now active
			if (inactivePart && vessel.isActiveVessel)
			{
				SetUpToolbarUI();
			}
			// if we were active, but are now inactive
			if (!inactivePart && !vessel.isActiveVessel)
			{
				inactivePart = true;
			}

			// can't Anchor if we're orbiting
			if (vessel.situation == Vessel.Situations.SUB_ORBITAL || vessel.situation == Vessel.Situations.ORBITING)
			{
				autoAnchor = false;
				Anchored = false;
			}
			// if we're inactive, and autoAnchor is set
			if (!vessel.isActiveVessel && autoAnchor)
			{
				// if we're less than 1.5km from the active vessel and Anchored, then wake up
				if ((vessel.GetWorldPos3D() - FlightGlobals.ActiveVessel.GetWorldPos3D()).magnitude < 1500.0f && Anchored)
				{
					vessel.GoOffRails();
					RestoreVesselState();
				}
				// if we're farther than 2km, auto Anchor if needed
				if ((vessel.GetWorldPos3D() - FlightGlobals.ActiveVessel.GetWorldPos3D()).magnitude > 2000.0f && (!Anchored))
				{
					AnchorVessel();
				}
			}
			if (Anchored)
			{
				AnchorVessel();
			}
			if (HLEnvelopeControlWindow.Instance != null && vessel.isActiveVessel)
			{
				Anchored = HLEnvelopeControlWindow.Instance.AnchorOn;
				autoAnchor = HLEnvelopeControlWindow.Instance.AutoAnchor;
			}
		}

		private void RestoreVesselState()
		{
			if (isActive == false) { return; } //we only want to restore the state if you have Anchored somewhere intentionally
			vessel.situation = previousState;
			if (vessel.situation != Vessel.Situations.LANDED) { vessel.Landed = false; }
			if (Anchored) { Anchored = false; }

			FreezeVesselInPlace();

			//Restore Velocity and Accleration
			vessel.SetWorldVelocity(AnchorVelocity);
			vessel.acceleration = AnchorAcceleration;
			vessel.angularVelocity = AnchorAngularVelocity;
		}

		[KSPEvent(guiActive = true, guiName = "Toggle Anchor")]
		public void ToggleAnchor()
		{
			// cannot Anchor in orbit or sub-orbit
			if (vessel.situation != Vessel.Situations.SUB_ORBITAL && vessel.situation != Vessel.Situations.ORBITING)
			{
				if (!Anchored)
				{
					AnchorPosition = GetVesselPostion();

					//we only want to remember the initial velocity, not subseqent updates by onFixedUpdate()
					AnchorVelocity = vessel.GetSrfVelocity();
					AnchorAcceleration = vessel.acceleration;
					AnchorAngularVelocity = vessel.angularVelocity;

					AnchorVessel();
				}
				else
				{
					RestoreVesselState();
				}
				isActive = true;
				if(HLEnvelopeControlWindow.Instance != null)
				{
					HLEnvelopeControlWindow.Instance.AnchorOn = Anchored;
				}
			}
		}


		[KSPEvent(guiActive = true, guiName = "Toggle AutoAnchor")]
		public void ToggleAutoAnchor()
		{
			autoAnchor = !autoAnchor;
			if (HLEnvelopeControlWindow.Instance != null)
			{
				HLEnvelopeControlWindow.Instance.AutoAnchor = autoAnchor;
			}
		}

		private void AnchorVessel()
		{
			RememberPreviousState();
			FreezeVesselInPlace();

			vessel.situation = Vessel.Situations.LANDED;
			vessel.Landed = true;
			Anchored = true;
		}

		private void FreezeVesselInPlace()
		{
			vessel.SetWorldVelocity(zeroVector);
			vessel.acceleration = zeroVector;
			vessel.angularVelocity = zeroVector;
			vessel.geeForce = 0.0;
			SetVesselPosition();

		}

		//Code Adapted from Hyperedit landing functions 
		//https://github.com/Ezriilc/HyperEdit


		private Vector3d GetVesselPostion()
		{
			double latitude = 0, longitude = 0, altitude = 0;
			var pqs = vessel.mainBody.pqsController;
			if (pqs == null)
			{
				Destroy(this);
				return zeroVector;
			}

			altitude = pqs.GetSurfaceHeight(vessel.mainBody.GetRelSurfaceNVector(latitude, longitude)) - vessel.mainBody.Radius;

			return vessel.mainBody.GetRelSurfacePosition(latitude, longitude, altitude);
		}

		private void SetVesselPosition()
		{
			vessel.orbitDriver.pos = AnchorPosition;
		}

	}
}
