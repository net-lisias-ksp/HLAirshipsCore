// HLEnvelopePart
// by Hooligan Labs
// Every Envelope is controlled by the Compressor.  These envelopes provide lift.

// Update Log
// 2014-12-23: (by Khatharr) KSP 0.9 dropped support for Part.uid, so I changed it to Part.flightID. Needs more testing, but it's working at first glance.
// 2012-11-09: Each envelope now calculates its location relative to the center of mass, for use by the Compressor

using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using KSP.IO;
using Toolbar;

public enum AnimationState : int
{

    AtBeginning = -2,
    AtEnd = 2,
    PlayForwards = 1,
    PlayBackwards = -1

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
    public float limitBuoyantForce = 0f, minAtmPressure = -0.01f, dragDeployed = 0f, dragUndeployed = 0f, makeStationarySpeedMax = 1f, makeStationarySpeedClamp = 0.0f;

    [KSPField(isPersistant = true, guiActive = false, guiName = "GUI On")]
    public bool guiOn = true;

    [KSPField(isPersistant = true, guiActive = false, guiName = "Balance Envelope Adjusments")]
    public bool symmetricalPitch = false;

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
    [KSPField(isPersistant = true, guiActive = true, guiName = "Vessel %", guiFormat = "F2")]
    public float targetBuoyantVessel = 0.0f;
    // Is this the lead envelope?
    // Fraction of lifting gas specific volume the envelope
    [KSPField(isPersistant = true, guiActive = true, guiName = "Envelope %", guiFormat = "F3")]
    public float specificVolumeFractionEnvelope = 0.50f;
    [KSPField(isPersistant = true, guiActive = false, guiName = "Altitude Control")]
    private bool toggleAltitudeControl = false;
    [KSPField(isPersistant = true, guiActive = false, guiName = "Target Vertical Velocity", guiFormat = "F3")]
    public float targetVerticalVelocity = 0f;
    [KSPField(isPersistant = true, guiActive = false, guiName = "Make Stationary")]
    private bool togglePersistenceControl = false;
    // [KSPField(isPersistant = true, guiActive = true, guiName = "Manual Pitch Control")]
    // public bool toggleManualPitch = false;
    [KSPField(isPersistant = true, guiActive = false, guiName = "Automatic Pitch Control")]
    public bool toggleAutoPitch = false;
    [KSPField(isPersistant = true, guiActive = false, guiName = "Auto Pitch Adjust", guiFormat = "F5")]
    public float autoTarget = 0;
    [KSPField(isPersistant = true, guiActive = true, guiName = "Pitch %", guiFormat = "F3")]
    public float targetPitchBuoyancy = 0;
    [KSPField(isPersistant = true, guiActive = false, guiName = "Pitch Angle")]
    public float pitchAngle = 0;
    [KSPField(isPersistant = true, guiActive = false, guiName = "Deploy Animation")]
    public bool animationDeploy = false;

    [KSPField(isPersistant = true, guiActive = true, guiName = "Lead Envelope")]
    public bool isLeadEnvelope = false;

    public enum Direction { VesselRight, VesselForward, VesselUp };


    [KSPField(isPersistant = true, guiActive = false, guiName = "Auto Up")]
    public Direction stabilizeDirection = Direction.VesselUp;
    [KSPField(isPersistant = true, guiActive = false, guiName = "Stabilize Multi")]
    public float stabilizeMultiplier = 1f;
    [KSPField(isPersistant = true, guiActive = false, guiName = "Stabilize Invert")]
    public bool stabilizeInvert = false;

    [KSPField(isPersistant = true, guiActive = false)]
    public float targetBuoyancyP = 0f, targetBuoyancyN = 0f, lineOffsetMultiplier = 2f;

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
    public float animationState = 0;
    #endregion

    #region Other Variables

    // GUI
    public IButton button;
    public bool state1 = true;
    private Rect windowPos;
    private int airshipWindowID;
    public bool activeGUI = false;
    private float windowWidth;

    // Basic vectors
    private Vector3 heading;
    public Vector3 buoyantForce, maxBuoyancy;

    // For automatic pitch control
    float previousRotation;

    public float eDistanceFromCoM;

    private float totalBuoyancy;
    private float totalBuoyancyMax;
    private float totalMass;
    private float totalGravityForce;
    private float atmosDensity;

    public float totalTorqueP;
    public float totalTorqueN;
    public float totalBuoyancyMaxP;
    public float totalBuoyancyMaxN;
    public float totalTorqueMaxP;
    public float totalTorqueMaxN;

    private bool resetGUIsize = false;
    private bool willReset1 = false;
    private bool willReset2 = false;
    // private bool willReset3 = false;
    private bool willReset4 = false;

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
    private GameObject objGravity = new GameObject();
    private GameObject objUp = new GameObject();
    private GameObject objPosition = new GameObject();
    private GameObject objUpProjected = new GameObject();
    private GameObject objPositionProjected = new GameObject();

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

    [KSPEvent(guiActive = true, guiActiveUnfocused = true, unfocusedRange = 2000, guiName = "Envelope Buoyancy ++")]
    public void BuoyancyPP_Event()
    {
        foreach (HLEnvelopePartModule envelope in Envelopes)
        {
            toggleAutoPitch = false;
        }
        targetPitchBuoyancy += 0.01f;
    }

    [KSPEvent(guiActive = true, guiActiveUnfocused = true, unfocusedRange = 2000, guiName = "Envelope Buoyancy +")]
    public void BuoyancyP_Event()
    {
        foreach (HLEnvelopePartModule envelope in Envelopes)
        {
            toggleAutoPitch = false;
        }
        targetPitchBuoyancy += 0.001f;
    }

    [KSPEvent(guiActive = true, guiActiveUnfocused = true, unfocusedRange = 2000, guiName = "Envelope Buoyancy -")]
    public void BuoyancyN_Event()
    {
        foreach (HLEnvelopePartModule envelope in Envelopes)
        {
            toggleAutoPitch = false;
        }
        targetPitchBuoyancy -= 0.001f;
    }

    [KSPEvent(guiActive = true, guiActiveUnfocused = true, unfocusedRange = 2000, guiName = "Envelope Buoyancy --")]
    public void BuoyancyNN_Event()
    {
        foreach (HLEnvelopePartModule envelope in Envelopes)
        {
            toggleAutoPitch = false;
        }
        targetPitchBuoyancy -= 0.01f;
    }

    [KSPEvent(guiActive = true, guiActiveUnfocused = true, unfocusedRange = 2000, guiName = "Envelope Buoyancy Max")]
    public void BuoyancyM_Event()
    {
        foreach (HLEnvelopePartModule envelope in Envelopes)
        {
            toggleAutoPitch = false;
        }
        targetPitchBuoyancy = 1f;
    }

    [KSPEvent(guiActive = true, guiActiveUnfocused = true, unfocusedRange = 2000, guiName = "Envelope Buoyancy Min")]
    public void BuoyancyZ_Event()
    {
        foreach (HLEnvelopePartModule envelope in Envelopes)
        {
            toggleAutoPitch = false;
        }
        targetPitchBuoyancy = -1f;
    }

    [KSPEvent(guiActive = true, guiActiveUnfocused = true, unfocusedRange = 2000, guiName = "Envelope Clear Adjustments")]
    public void BuoyancyC_Event()
    {
        foreach (HLEnvelopePartModule envelope in Envelopes)
        {
            toggleAutoPitch = false;
        }
        targetPitchBuoyancy = 0f;
    }

    [KSPEvent(guiActive = true, guiActiveUnfocused = true, unfocusedRange = 2000, guiName = "Toggle GUI")]
    public void guiToggle_Event()
    {
        Debug.Log("HLA: Sending guiToggle signal");
        part.SendEvent("guiToggle");
    }

    [KSPEvent(guiActive = false, guiActiveUnfocused = true, unfocusedRange = 2000)]
    public void guiToggle()
    {
        Debug.Log("HLA: Executing guiToggle method");
        if (part == leadEnvelope && guiOn)
        {
            guiOn = false;
            removeGUI();
        }
        else if (part == leadEnvelope)
        {
            guiOn = true;
            addGUI();
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

        button = ToolbarManager.Instance.add("HooliganLabs", "HooliganLabsGUI");
        button.TexturePath = "HooliganLabs/Icons/HLOffIcon";
        button.ToolTip = "HooliganLabs";
        //button.OnClick += (e) => guiToggle_Event();// guiToggle_Event() guiToggle()
        button.OnClick += (e) =>
        {
            guiToggle_Event();
            button.TexturePath = state1 ? "HooliganLabs/Icons/HLOffIcon" : "HooliganLabs/Icons/HLOnIcon";
            state1 = !state1;
        };

        // Set starting animation state
        Debug.Log("Set Envelope Animation");
        if (animationState == 0 || targetBuoyantVessel == 0)
        {
            // If it does not have animation start it "opened"
            if (!envelopeHasAnimation)
                animationState = 2;
            else
                animationState = -2;
        }
        else
            // Start opened if it is buoyant
            animationState = 2;
    }

    public override void OnUpdate()
    {
        /// Per-frame update 
        /// Called ONLY when Part is ACTIVE! 

        try { AirshipPIDcontrol(); }
        catch (Exception ex) { print("Airship PID Control Exception!"); print(ex.Message); }

        // Freeze in place if requested
        if (togglePersistenceControl)
            persistentParts();

        // Sets one envelope to run the GUI and control logic
        determineLeadEnvelope();

        if (this.part == leadEnvelope && guiOn)
        {
            addGUI();
            // Events["guiToggle_Event"].active = true;
        }
        else if (!guiOn) removeGUI();

        // if (this.part != leadEnvelope) Events["guiToggle_Event"].active = false;

        // Try to animate if there is no animation
        if (envelopeHasAnimation)
            animateEnvelope();
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
                    // Debug.Log("Play Forwards");
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
                    // Debug.Log("Play Backwards");
                }
                break;

            case AnimationState.PlayForwards:
                this.m_Animation[this.animationName].speed = 1;
                /*
                if (!this.m_Animation.IsPlaying(this.animationName))
                {
                    //Debug.Log("At End");
                    this.AniState = AnimationState.AtEnd;
                }
                 */
                if (!animationDeploy)
                {
                    this.AniState = AnimationState.PlayBackwards;
                    //Debug.Log("Play Backwards");
                }
                // Should stop animation near end
                if (this.m_Animation[this.animationName].time >= this.m_Animation[this.animationName].length * 0.99) this.m_Animation[this.animationName].speed = 0;
                break;

            case AnimationState.PlayBackwards:
                this.m_Animation[this.animationName].speed = -1;
                /*
                if (!this.m_Animation.IsPlaying(this.animationName))
                {
                    //Debug.Log("At Beginning");
                    this.AniState = AnimationState.AtBeginning;
                }
                 */
                if (animationDeploy)
                {
                    this.AniState = AnimationState.PlayForwards;
                    //Debug.Log("Play Forwards");
                }
                // Should stop animation near beginning
                if (this.m_Animation[this.animationName].time <= this.m_Animation[this.animationName].length * 0.01) this.m_Animation[this.animationName].speed = 0;
                break;
        }
        // Debug.Log("Animation Time = " + this.m_Animation[this.animationName].time.ToString() + ", Animation State = " + animationState);
        this.m_Animation.Play(this.animationName);
    }

    public override void OnStart(StartState state)
    {
        // OnFlightStart seems to have been removed
        /// Called during the Part startup. 
        /// StartState gives flag values of initial state
        Debug.Log("HL Airship Plugin Start");
        specificVolumeFractionEnvelope = Mathf.Clamp01(specificVolumeFractionEnvelope);

        Debug.Log("Check for animation");
        if (this.GetComponentInChildren<Animation>() != null) m_Animation = this.GetComponentInChildren<Animation>();

        Debug.Log("Set maximum drag");
        dragUndeployed = part.maximum_drag;

        // activeVessel = FlightGlobals.ActiveVessel.GetInstanceID();
        Debug.Log("Set lead envelope");
        if (this.part == leadEnvelope) { targetBuoyantVessel = Mathf.Clamp01(targetBuoyantVessel); }

        // Debug lines
        Debug.Log("Create Lines");
        lineCorrect = objUp.AddComponent<LineRenderer>();
        linePosition = objPosition.AddComponent<LineRenderer>();
        linePositionProjected = objPositionProjected.AddComponent<LineRenderer>();
        lineCorrectProjected = objUpProjected.AddComponent<LineRenderer>();
        lineGravity = objGravity.AddComponent<LineRenderer>();

        Debug.Log("Parameterize Lines");
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

    public void OnDestroy()
    {
        button.Destroy();
        if (this.part == leadEnvelope)
        {
            removeGUI();
        }
    }

    public void onDisconnect()
    {
        if (this.part == leadEnvelope)
        {
            removeGUI();
        }
    }

    public override void OnFixedUpdate()
    {
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

    private void AirshipPIDcontrol()
    {
        // public PidController( float Kp, float Ki, float Kd, int integrationBuffer, float clamp )
        // brakePid = new PidController((float)(totalGravityForce / maxBuoyancy.magnitude / 5), (float)(totalGravityForce / maxBuoyancy.magnitude * expandRate * 100), (float)(totalGravityForce / maxBuoyancy.magnitude * expandRate * 0.0625), 50, 1);
    }

    public void leadEnvelopeUpdate()
    {
        // Direction in which the ship currently points
        heading = (Vector3d)this.vessel.transform.up;

        // Up, relative to the center of the nearest celestial body
        Vector3d up = (this.vessel.rigidbody.position - this.vessel.mainBody.position).normalized;

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
        else
            togglePersistenceControl = false;
    }

    // Gathers information on the envelopes
    public void findEnvelopes()
    {
        Envelopes.Clear();
        totalBuoyancy = 0;
        totalTorqueP = 0;
        totalTorqueN = 0;
        totalTorqueMaxP = 0;
        totalTorqueMaxN = 0;
        totalBuoyancyMax = 0;
        totalBuoyancyMaxP = 0;
        totalBuoyancyMaxN = 0;

        // More info: http://www.grc.nasa.gov/WWW/k-12/WindTunnel/Activities/balance_of_forces.html

        foreach (Part part in this.vessel.parts)
        {
            if (part.isAttached)
            {
                // Skip if not an envelope
                if (part.Modules.OfType<HLEnvelopePartModule>().FirstOrDefault() == null)
                {
                    // Debug.Log("Did not find an envelope on " + part.name);
                    continue;
                }
                envelope = part.Modules.OfType<HLEnvelopePartModule>().FirstOrDefault();

                // Envelopes have to be active to work.
                if (part.State != PartStates.ACTIVE)
                    part.force_activate();

                Envelopes.Add(envelope);
                totalBuoyancy += envelope.buoyantForce.magnitude;
                totalBuoyancyMax += envelope.maxBuoyancy.magnitude;

                /*
                // Legacy pitch control
                if (envelope.eDistanceFromCoM > 0)
                {
                    totalTorqueP += envelope.buoyantForce.magnitude * envelope.eDistanceFromCoM;
                    totalBuoyancyMaxP += envelope.maxBuoyancy.magnitude;
                    totalTorqueMaxP += envelope.maxBuoyancy.magnitude * envelope.eDistanceFromCoM;
                }
                if (envelope.eDistanceFromCoM < 0)
                {
                    totalTorqueN += envelope.buoyantForce.magnitude * envelope.eDistanceFromCoM;
                    totalBuoyancyMaxN += envelope.maxBuoyancy.magnitude;
                    totalTorqueMaxN += envelope.maxBuoyancy.magnitude * envelope.eDistanceFromCoM * -1;
                }
                 */

            }

            // If this is the lead envelope...
            if (leadEnvelope == envelope.part)
            {
                Debug.Log("HLA: Adding GUI!");
                envelope.isLeadEnvelope = true;

                // Make sure all envelopes have the same logic
                /*
                envelope.toggleAltitudeControl = this.toggleAltitudeControl;
                envelope.targetVerticalVelocity = this.targetVerticalVelocity;
                envelope.togglePersistenceControl = this.togglePersistenceControl;
                // envelope.toggleManualPitch = this.toggleManualPitch;
                envelope.toggleAutoPitch = this.toggleAutoPitch;
                envelope.pitchAngle = this.pitchAngle;
                envelope.symmetricalPitch = this.symmetricalPitch;
                envelope.displayHologram = this.displayHologram;
                 */

            }// If not, then check to see if this envelope should be promoted to lead envelope
            else if (!vessel.parts.Contains(leadEnvelope))
            {
                leadEnvelope = envelope.part;
            }
            else envelope.isLeadEnvelope = false;
        }
    }

    private void findParts()
    {
        // Hey, a method for finding mass!
        totalMass = vessel.GetTotalMass();

        totalGravityForce = (float)(totalMass * FlightGlobals.getGeeForceAtPosition(part.rigidbody.worldCenterOfMass).magnitude);
    }

    public void setBuoyancy()
    {
        float sumPitch = 0;

        // Find out the sum of all the pitches
        foreach (HLEnvelopePartModule envelope in Envelopes)
        {
            sumPitch += envelope.targetPitchBuoyancy;
        }

        // The lead envelope sets the buoyancy for every envelope, including itself
        foreach (HLEnvelopePartModule envelope in Envelopes)
        {
            envelope.targetBuoyantVessel = this.targetBuoyantVessel;

            // Balance out pitches to 0
            if (symmetricalPitch) envelope.targetPitchBuoyancy -= sumPitch / Envelopes.Count;

            // if(toggleManualPitch)
            // envelope.targetPitchBuoyancy = manualPitchControl(envelope.targetBoyantForceFractionCompressor, envelope.eDistanceFromCoM);
            // else
            // if (toggleAutoPitch) envelope.targetPitchBuoyancy += autoPitchControl(part.rigidbody.worldCenterOfMass, envelope);
            /*
            else
            {
                envelope.targetPitchBuoyancy = 0;
                envelope.autoTarget = 0;
            }
             */

            Mathf.Clamp01(envelope.targetPitchBuoyancy);

            // Use total
            envelope.updateTargetSpecificVolumeFraction(Mathf.Clamp01(envelope.targetPitchBuoyancy + targetBuoyantVessel), envelope);
        }
    }

    public float autoPitchControl(Vector3 position, HLEnvelopePartModule envelope)
    {
        Vector3 stabilizeVector = Vector3.one;
        Vector3 pitchVector = Vector3.one;
        switch (stabilizeDirection)
        {
            case Direction.VesselForward:
                stabilizeVector = vessel.transform.forward;
                pitchVector = -1 * vessel.transform.up;
                // rotation = vessel.ctrlState.pitch;
                break;
            case Direction.VesselRight:
                stabilizeVector = vessel.transform.right;
                pitchVector = -1 * vessel.transform.up;
                // rotation = vessel.ctrlState.roll;
                break;
            case Direction.VesselUp:
                stabilizeVector = vessel.transform.up;
                pitchVector = -1 * vessel.transform.forward;
                // rotation = vessel.ctrlState.pitch;
                break;
        }

        // Factor in multiplier
        stabilizeVector *= stabilizeMultiplier;
        if (stabilizeInvert)
        {
            stabilizeVector *= -1;
            pitchVector *= -1;
        }

        // Pitch angle
        if (pitchAngle < 0)
            pitchVector *= -1;

        // Slerp (yummy!) is a spherical rotation of a vector towards something
        stabilizeVector = Vector3.Slerp(stabilizeVector, pitchVector, Mathf.Abs(pitchAngle) / 90);

        // Center of Mass
        Vector3 theCoM = vessel.findWorldCenterOfMass();

        // Removes vertical component from the vector we are stabilizing to and the envelope's position
        Vector3 correctVector = ProjectVectorOnPlane(FlightGlobals.getGeeForceAtPosition(theCoM).normalized, stabilizeVector);

        Vector3 positionVector = ProjectVectorOnPlane(FlightGlobals.getGeeForceAtPosition(theCoM).normalized, position - vessel.CoM);

        Vector3 lineOffset = FlightGlobals.getGeeForceAtPosition(theCoM).normalized * -lineOffsetMultiplier;

        // makeLines(linePosition, Color.red);
        // makeLines(linePositionProjected, Color.green);
        // makeLines(lineUp, Color.blue);
        // makeLines(lineUpProjected, Color.magenta);
        // makeLines(lineGravity, Color.white); }

        // Are we already rotating towards the correct position?
        float rotation = Vector3.Angle(stabilizeVector, -1 * FlightGlobals.getGeeForceAtPosition(theCoM).normalized);
        if ((previousRotation - rotation) < 0.001)
        {
            // Adjust for rotation
            // envelope.autoTarget -= (rotation - previousRotation) / 10;

            // Project correction onto relative position to get correction amount
            envelope.autoTarget = Vector3.Dot(correctVector / 1000, positionVector);
        }
        else envelope.autoTarget = 0;
        // Save this rotation
        previousRotation = rotation;

        // Do not send small adjustments
        if (envelope.autoTarget < 0.0001)
            envelope.autoTarget = 0;

        if (displayHologram)
        {
            envelope.lineGravity.SetPosition(0, theCoM + lineOffset);
            envelope.lineGravity.SetPosition(1, theCoM + FlightGlobals.getGeeForceAtPosition(theCoM).normalized + lineOffset);

            envelope.linePosition.SetPosition(0, theCoM + lineOffset);
            envelope.linePosition.SetPosition(1, position + lineOffset);

            envelope.linePositionProjected.SetPosition(0, theCoM + lineOffset);
            envelope.linePositionProjected.SetPosition(1, theCoM + positionVector + lineOffset);

            envelope.lineCorrect.SetPosition(0, theCoM + lineOffset);
            envelope.lineCorrect.SetPosition(1, theCoM + stabilizeVector + lineOffset);

            envelope.lineCorrectProjected.SetPosition(0, theCoM + lineOffset);
            envelope.lineCorrectProjected.SetPosition(1, theCoM + correctVector + lineOffset);
        }
        else
        {
            envelope.lineGravity.SetPosition(0, Vector3.zero);
            envelope.lineGravity.SetPosition(1, Vector3.zero);

            envelope.linePosition.SetPosition(0, Vector3.zero);
            envelope.linePosition.SetPosition(1, Vector3.zero);

            envelope.linePositionProjected.SetPosition(0, Vector3.zero);
            envelope.linePositionProjected.SetPosition(1, Vector3.zero);

            envelope.lineCorrect.SetPosition(0, theCoM + Vector3.zero);
            envelope.lineCorrect.SetPosition(1, theCoM + Vector3.zero);

            envelope.lineCorrectProjected.SetPosition(0, Vector3.zero);
            envelope.lineCorrectProjected.SetPosition(1, Vector3.zero);
        }

        // Return result
        return envelope.autoTarget;
    }

    // From http://wiki.unity3d.com/index.php?title=3d_Math_functions
    //Projects a vector onto a plane. The output is not normalized.
    public static Vector3 ProjectVectorOnPlane(Vector3 planeNormal, Vector3 vector)
    {
        return vector - (Vector3.Dot(vector, planeNormal) * planeNormal);
    }

    // I talked to Mu and it sounds like saving and loading is going to be redone in 0.19 or 0.20, so I'm not going to worry about mid-air saving until then.
    // Update:  0.19 and 0.20 have come and gone an no word on this being redone still.  :P
    private void persistentParts()
    {
        // this.vessel.SetWorldVelocity(new Vector3d(0,0,0));

        if (this.vessel.srf_velocity.magnitude > makeStationarySpeedClamp)
        {
            this.vessel.SetWorldVelocity(vessel.srf_velocity.normalized * makeStationarySpeedClamp * 0.9);
        }

        vessel.Landed = true;
        // vessel.ctrlState.mainThrottle = 0;
    }

    // Envelopes determine their positions and effect buoyant force on themselves

    public void envelopeUpdate()
    {
        Vector3 stabilizeVector = Vector3.one;
        switch (stabilizeDirection)
        {
            case Direction.VesselForward:
                stabilizeVector = vessel.transform.forward;
                break;
            case Direction.VesselRight:
                stabilizeVector = vessel.transform.right;
                break;
            case Direction.VesselUp:
                stabilizeVector = vessel.transform.up;
                break;
        }
        try { eDistanceFromCoM = DistanceFromCoM(rigidbody.position, vessel.findWorldCenterOfMass(), vessel.mainBody.position, stabilizeVector); }
        catch (Exception ex) { print("eDistanceFromCoM Exception!"); print(ex.Message); }

        float pressure = (float)FlightGlobals.getStaticPressure();

        // Compute the buoyant force

        // KSP provides the density based on local atmospheric pressure.
        atmosDensity = (float)FlightGlobals.getAtmDensity(pressure);
        // The maximum buoyancy of the envelope is equal to the weight of the displaced air
        // Force (Newtons) = - Gravitational Acceleration (meters per second squared) * Density (kilograms / meter cubed) * Volume (meters cubed)
        maxBuoyancy = -FlightGlobals.getGeeForceAtPosition(part.rigidbody.worldCenterOfMass) * atmosDensity * envelopeVolume;
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

        // New method is to add force based on render bounds.
        this.part.Rigidbody.AddForceAtPosition(buoyantForce, part.rigidbody.worldCenterOfMass, ForceMode.Force);

    }

    // Changes envelope buoyancy targets
    public void updateTargetSpecificVolumeFraction(float fraction, HLEnvelopePartModule envelope)
    {
        // Difference between specific volume and target specific volume
        float delta = fraction - specificVolumeFractionEnvelope;

        // Clamp the change to compress and expand rates
        delta = Mathf.Clamp(delta, -compressRate, expandRate);

        envelope.specificVolumeFractionEnvelope += delta * Time.deltaTime;
    }

    // Distance from center of mass
    public float DistanceFromCoM(Vector3 position, Vector3 CoM, Vector3 mainBody, Vector3 direction)
    {
        // project the vectors on the plane with up as a normal
        return Vector3.Dot((position - mainBody) - (CoM - mainBody), direction);
    }

    // "Continuous" angle calculations
    private float ContAngle(Vector3 fwd, Vector3 targetDir, Vector3 upDir)
    {
        float angle = Vector3.Angle(fwd, targetDir);

        return angle;
    }

    // Direction of angle
    private float AngleDir(Vector3 fwd, Vector3 targetDir, Vector3 up)
    {
        Vector3 perp = Vector3.Cross(fwd, targetDir);
        return Math.Sign(Vector3.Dot(perp, up));
    }

    // This was recommended by Kreuzung
    public void OnCenterOfLiftQuery(CenterOfLiftQuery q)
    {
        q.dir = Vector3.up;
        q.lift = (float)envelopeVolume;
        q.pos = transform.position;
    }
    //////////// GUI ///////////////////////
    private void WindowGUI(int windowID)
    {
        float targetBoyantForceFractionCompressorTemp;
        targetBoyantForceFractionCompressorTemp = targetBuoyantVessel;

        float targetVerticalVelocityTemp;
        targetVerticalVelocityTemp = targetVerticalVelocity;

        /*GUILayout.BeginHorizontal();
        if (GUILayout.Button("Collapse", GUILayout.Height(30f)))
        {
            state1 = !state1;
            guiToggle();
        }
        GUILayout.EndHorizontal();*/

        #region General GUI
        // General GUI window information
        GUIStyle mySty = new GUIStyle(GUI.skin.button);
        mySty.normal.textColor = mySty.focused.textColor = Color.white;
        mySty.hover.textColor = mySty.active.textColor = Color.yellow;
        mySty.onNormal.textColor = mySty.onFocused.textColor = mySty.onHover.textColor = mySty.onActive.textColor = Color.green;
        mySty.padding = new RectOffset(2, 2, 2, 2);

        // Buoyancy -, current %, and + buttons
        GUILayout.BeginHorizontal();
        if (GUILayout.RepeatButton("-", mySty))
        {
            targetBuoyantVessel -= 0.002f;
            toggleAltitudeControl = false;
        }
        targetBuoyantVessel = Mathf.Clamp01(targetBuoyantVessel);

        GUILayout.Label("        " + Mathf.RoundToInt(targetBuoyantVessel * 100) + "%");

        if (GUILayout.RepeatButton("+", mySty))
        {
            targetBuoyantVessel += 0.002f;
            toggleAltitudeControl = false;
        }
        GUILayout.EndHorizontal();

        // Slider control.  Also is set by the other controls.
        GUILayout.BeginHorizontal();
        float temp = targetBuoyantVessel;
        targetBuoyantVessel = GUILayout.HorizontalSlider(targetBuoyantVessel, 0f, 1f);
        if (temp != targetBuoyantVessel)
        {
            toggleAltitudeControl = false;
        }
        GUILayout.EndHorizontal();

        targetBuoyantVessel = Mathf.Clamp01(targetBuoyantVessel);
        #endregion

        #region Toggle Altitude
        // Altitude control.  Should be deactivated when pressing any other unrelated control.
        GUILayout.BeginHorizontal();
        string toggleAltitudeControlString = "Altitude Control Off";
        if (toggleAltitudeControl) toggleAltitudeControlString = "Altitude Control On";
        toggleAltitudeControl = GUILayout.Toggle(toggleAltitudeControl, toggleAltitudeControlString);
        GUILayout.EndHorizontal();
        #endregion

        if (toggleAltitudeControl)
        {
            #region Altitude Control
            willReset1 = true;

            // Vertical Velocity -, target velocity, and + buttons
            GUILayout.BeginHorizontal();
            GUILayout.Label("Target Vertical Velocity");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.RepeatButton("--", mySty)) targetVerticalVelocity -= 0.1f;
            if (GUILayout.Button("-", mySty)) targetVerticalVelocity -= 0.1f;
            if (GUILayout.Button(targetVerticalVelocity.ToString("00.0") + " m/s", mySty)) targetVerticalVelocity = 0;
            if (GUILayout.Button("+", mySty)) targetVerticalVelocity += 0.1f;
            if (GUILayout.RepeatButton("++", mySty)) targetVerticalVelocity += 0.1f;
            GUILayout.EndHorizontal();
            #endregion

            #region Persistence
            // Allows "landing" (persistence) by slowing the vehicle, as long as the this.vessel has buoyancy
            if (totalBuoyancy > 0f)
            {
                if (Mathf.Abs((int)vessel.horizontalSrfSpeed) < makeStationarySpeedMax && Mathf.Abs((int)vessel.verticalSpeed) < makeStationarySpeedMax && (vessel.Landed || vessel.Splashed))
                {
                    GUILayout.BeginHorizontal();
                    togglePersistenceControl = GUILayout.Toggle(togglePersistenceControl, "Make Slow to Save", mySty);
                    GUILayout.EndHorizontal();
                }
                else
                {
                    GUILayout.BeginHorizontal();
                    if (this.vessel.Landed || this.vessel.Splashed)
                    {
                        GUILayout.Label("Reduce Speed to Save");
                    }
                    else if (maxBuoyancy.magnitude > 0)
                    {
                        GUILayout.Label("Touch Ground to Save");
                        togglePersistenceControl = false;
                    }
                    GUILayout.EndHorizontal();
                }
            }
            #endregion
        }
        else
        {
            targetVerticalVelocity = 0;
            if (willReset1)
            {
                resetGUIsize = true;
                willReset1 = false;
            }
        }

        #region Toggle Pitch
        if (Envelopes.Count > 1)
        {
            /*
            GUILayout.BeginHorizontal();
            // if (toggleAutoPitch) toggleManualPitch = false;
            string toggleManualPitchString = "Manual Pitch Off";
            if (toggleManualPitch) toggleManualPitchString = "Manual Pitch On";
            toggleManualPitch = GUILayout.Toggle(toggleManualPitch, toggleManualPitchString);
            GUILayout.EndHorizontal();
            */

            // As Auto Pitch is not working, it has been commented out

            /*
            GUILayout.BeginHorizontal();
            // if (toggleManualPitch) toggleAutoPitch = false;
            string symmetricalPitchString = "Symetrical Pitch Off";
            if (symmetricalPitch) symmetricalPitchString = "Symetrical Pitch On";
            symmetricalPitch = GUILayout.Toggle(symmetricalPitch, symmetricalPitchString);
            GUILayout.EndHorizontal();

            if (symmetricalPitch)
            {
                GUILayout.BeginHorizontal();
                // if (toggleManualPitch) toggleAutoPitch = false;
                string toggleAutoPitchString = "Auto Pitch Off";
                if (toggleAutoPitch) toggleAutoPitchString = "Auto Pitch On";
                toggleAutoPitch = GUILayout.Toggle(toggleAutoPitch, toggleAutoPitchString);
                GUILayout.EndHorizontal();
            }
            else toggleAutoPitch = false;
             */
        }
        #endregion

        if (toggleAutoPitch)
        {
            willReset4 = true;
            if (GUILayout.Button("Up is " + stabilizeDirection))
            {
                switch (stabilizeDirection)
                {
                    case Direction.VesselUp:
                        stabilizeDirection = Direction.VesselForward;
                        stabilizeInvert = true;
                        break;
                    case Direction.VesselForward:
                        stabilizeDirection = Direction.VesselRight;
                        stabilizeInvert = false;
                        break;
                    case Direction.VesselRight:
                        stabilizeDirection = Direction.VesselUp;
                        stabilizeInvert = false;
                        break;
                }
            }
        }
        else
        {
            if (willReset4)
            {
                resetGUIsize = true;
                willReset4 = false;
            }
        }

        if (toggleAutoPitch)
        {
            willReset2 = true;
            /*
            string stabilizeStrengthString = stabilizeMultiplier.ToString("0.00");
            if (stabilizeInvert) stabilizeStrengthString = "-" + stabilizeMultiplier.ToString("0.00");
            if (GUILayout.Button("Stabilizing Strength " + stabilizeStrengthString))
                stabilizeMultiplier = 1f;
            stabilizeMultiplier = GUILayout.HorizontalSlider(stabilizeMultiplier, 0.1f, 10f);
            stabilizeInvert = GUILayout.Toggle(stabilizeInvert, "Invert Direction");
             */

            // Pitch angle setting
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("-")) pitchAngle -= 1f;
            if (GUILayout.Button("Pitch Angle " + pitchAngle.ToString("0"))) pitchAngle = 0f;
            if (GUILayout.Button("+")) pitchAngle += 1f;
            GUILayout.EndHorizontal();

            pitchAngle = GUILayout.HorizontalSlider(pitchAngle, -90f, 90f);

            displayHologram = GUILayout.Toggle(displayHologram, "Display Hologram at " + lineOffsetMultiplier.ToString("F1"));
            if (displayHologram)
            {
                lineOffsetMultiplier = GUILayout.HorizontalSlider(lineOffsetMultiplier, -20f, 20f);
            }
        }
        else
        {
            displayHologram = false;
            if (willReset2)
            {
                resetGUIsize = true;
                willReset2 = false;
            }
        }

        #region Manual Pitch Control Buttons
        /*
        // Manual pitch control.  Works alongside other controls.
        if(toggleManualPitch)
        {
            willReset3 = true;
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Pitch Up", mySty))
            {
                targetBuoyancyP += 0.01f;
                targetBuoyancyN -= 0.01f;
            }
            if (GUILayout.Button("Pitch Down", mySty))
            {
                targetBuoyancyP -= 0.01f;
                targetBuoyancyN += 0.01f;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Forward \t\t\t Rear");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("-", mySty))
            {
                targetBuoyancyP -= 0.01f;
            }
            targetBuoyancyP = Mathf.Clamp(targetBuoyancyP, -1f, 1f);
            GUILayout.Label(Mathf.RoundToInt(targetBuoyancyP * 100) + "%");
            if (GUILayout.Button("+", mySty))
            {
                targetBuoyancyP += 0.01f;
            }
            if (GUILayout.Button("-", mySty))
            {
                targetBuoyancyN -= 0.01f;
            }
            targetBuoyancyN = Mathf.Clamp(targetBuoyancyN, -1f, 1f); 
            GUILayout.Label(Mathf.RoundToInt(targetBuoyancyN * 100) + "%");
            if (GUILayout.Button("+", mySty))
            {
                targetBuoyancyN += 0.01f;
            }
            GUILayout.EndHorizontal();

            foreach (HLEnvelopePartModule envelope in Envelopes)
            {
                envelope.targetBuoyancyN = targetBuoyancyN;
                envelope.targetBuoyancyP = targetBuoyancyP;
            }
        }
        else if (willReset3)
        {
            resetGUIsize = true;
            willReset3 = false;
        }
        */
        #endregion


        if (resetGUIsize)
        {
            // Reset window size
            windowPos.Set(windowPos.x, windowPos.y, 10, 10);
            resetGUIsize = false;
        }

        #region Debug
        // Debug info

        // if (Mathf.Abs(targetBoyantForceFractionCompressorTemp - targetBoyantForceFractionCompressor) > 0.002)
        // debugHLAirship("targetBoyantForceFractionCompressor", targetBoyantForceFractionCompressor.ToString("0.000"));

        // if (targetVerticalVelocityTemp != targetVerticalVelocity)
        // debugHLAirship("targetVerticalVelocityTemp", targetVerticalVelocity.ToString("00.0"));

        //GUILayout.BeginHorizontal();
        //GUILayout.Label("Buoyancy: " + totalBuoyancy.ToString("0.0"));
        //GUILayout.EndHorizontal();

        //GUILayout.BeginHorizontal();
        //GUILayout.Label("Gravity: " + totalGravityForce.ToString("0.0"));
        //GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Buoyancy - Weight: " + (totalBuoyancy - (vessel.GetTotalMass() * FlightGlobals.getGeeForceAtPosition(vessel.findWorldCenterOfMass()).magnitude)).ToString("0.00"));
        GUILayout.EndHorizontal();

        //GUILayout.BeginHorizontal();
        //GUILayout.Label("Angle from Up: " + (ContAngle(heading, up, up)).ToString("0.0"));
        //GUILayout.EndHorizontal();

        //GUILayout.BeginHorizontal();
        //GUILayout.Label("Front Torque: " + (totalTorqueP).ToString("0.0"));
        //GUILayout.EndHorizontal();

        //GUILayout.BeginHorizontal();
        //GUILayout.Label("Rear Torque: " + (totalTorqueN).ToString("0.0"));
        //GUILayout.EndHorizontal();

        //GUILayout.BeginHorizontal();
        //GUILayout.Label("Front B: " + (targetBuoyancyP).ToString("0.00"));
        //GUILayout.EndHorizontal();

        //GUILayout.BeginHorizontal();
        //GUILayout.Label("Rear B: " + (targetBuoyancyN).ToString("0.00"));
        //GUILayout.EndHorizontal();

        //int x = 0;
        //foreach (HLEnvelopePart envelope in Envelopes)
        //{
        //    GUILayout.BeginHorizontal();
        //    GUILayout.Label("Env" + x + " Location: " + (envelope.eDistanceFromCoM).ToString("0.00"));
        //    GUILayout.EndHorizontal();
        //    GUILayout.BeginHorizontal();
        //    GUILayout.Label("Env" + x + " Buoyancy: " + (envelope.buoyantForce.magnitude).ToString("0.00"));
        //    GUILayout.EndHorizontal();
        //    GUILayout.BeginHorizontal();
        //    GUILayout.Label("Env" + x + " Specific Volume: " + (envelope.specificVolumeFractionEnvelope).ToString("0.00"));
        //    GUILayout.EndHorizontal();
        //    GUILayout.BeginHorizontal();
        //    GUILayout.Label("Env" + x + " targetPitchBuoyancy: " + (envelope.targetPitchBuoyancy).ToString("0.00"));
        //    GUILayout.EndHorizontal();
        //    GUILayout.BeginHorizontal();
        //    GUILayout.Label("Env" + x + " targetBoyantForceFractionCompressor: " + (envelope.targetBoyantForceFractionCompressor).ToString("0.00"));
        //    GUILayout.EndHorizontal();

        //    x += 1;
        //}

        GUI.DragWindow(new Rect(0, 0, 500, 20));
        #endregion
    }

    public void OnDrawStats()
    {
        GUILayout.TextArea("Envelope volume: " + envelopeVolume + "\nInitial Gas Specific Volume: " + Mathf.RoundToInt(specificVolumeFractionEnvelope * 100) + "%");
    }

    //More GUI stuff
    private void drawGUI() {
      Debug.Log("HLA: entering drawGUI()");
      if(this.vessel != FlightGlobals.ActiveVessel) {return;}
      airshipWindowID = (int)part.flightID;
      windowPos = GUILayout.Window(airshipWindowID, windowPos, WindowGUI, "HooliganLabs", GUILayout.MinWidth(200));
      Debug.Log("HLA: leaving drawGUI()");
    }

    protected void initGUI()
    {
        windowWidth = 300;

        if ((windowPos.x == 0) && (windowPos.y == 0))
        {
            windowPos = new Rect(Screen.width - windowWidth, Screen.height * 0.25f, 10, 10);
        }
    }

    private void addGUI()
    {
        Debug.Log("HLA: entering addGUI()");
        //button.TexturePath = "HooliganLabs/Icons/HLOnIcon";
        if (!activeGUI && guiOn)
        {
            initGUI();
            RenderingManager.AddToPostDrawQueue(3, new Callback(drawGUI)); //start the GUI

            foreach (HLEnvelopePartModule envelope in Envelopes)
                envelope.activeGUI = true;
        }

        Debug.Log("HLA: leaving addGUI()");
    }

    protected void removeGUI()
    {
        Debug.Log("HLA: entering removeGUI()");
        //button.TexturePath = "HooliganLabs/Icons/HLOffIcon";
        if (activeGUI)
        {
            RenderingManager.RemoveFromPostDrawQueue(3, new Callback(drawGUI)); //close the GUI
            foreach (HLEnvelopePartModule envelope in Envelopes)
                envelope.activeGUI = false;
        }
        Debug.Log("HLA: leaving removeGUI()");
    }

}

