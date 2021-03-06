﻿using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class AirCraft : MonoBehaviour {

	public GUISkin UISkin;

	public MotherShip TargetMotherShip;
	public Transform CamTarget;

	private Rigidbody cRigidbody;
	private Rigidbody CRigidbody {
		get {
			if (cRigidbody == null) {
				this.cRigidbody = this.GetComponent<Rigidbody> ();
			}
			
			return cRigidbody;
		}
	}
	public void SetKinematic (bool k) {
		this.CRigidbody.isKinematic = k;
	}
	
	public bool engineBoost = false;
	public float engineBoostPow = 20f;
	public float engineInc = 5f;
	public float enginePow = 0f;
	public float enginePowMax = 0f;
	public float enginePowMin = 0f;
	
	public float lift;
	
	public float cForward;
	public float cRight;
	public float cUp;
	
	public float yawSpeed;
	public float pitchSpeed;
	public float rollSpeed;
		
	private float localAtm = 1f;

	public Player pilot;
	public GravitationalObject land;
	public PilotAirCraftState pilotMode;
	public MotherShipHangar hangar;

	public enum PilotAirCraftState
	{
		NoPilot,
		Pilot
	};

	void Start () {
		this.enginePow = 0f;
		this.SwitchModeTo (this.pilotMode);
	}
	
	void Update () {
		if (this.pilotMode == PilotAirCraftState.Pilot) {
			if (Input.GetKeyDown (KeyCode.W)) {
				this.enginePow += this.engineInc;
				if (this.enginePow > this.enginePowMax) {
					this.enginePow = this.enginePowMax;
				}
			}
			
			if (Input.GetKeyDown (KeyCode.S)) {
				this.enginePow -= this.engineInc;
				if (this.enginePow < this.enginePowMin) {
					this.enginePow = this.enginePowMin;
				}
			}
		}

		if (this.TargetMotherShip.pilotMode == MotherShip.PilotState.Orbit) {
			if (this.pilotMode == PilotAirCraftState.Pilot) {
				if ((this.transform.position - this.TargetMotherShip.transform.position).magnitude > 200f) {
					if (this.TargetMotherShip.TruePos.Lock == false) {
						this.TargetMotherShip.TruePos.Lock = true;
					}
				}
				else {
					if (this.TargetMotherShip.TruePos.Lock == true) {
						this.transform.position -= this.TargetMotherShip.transform.position;
						this.cRigidbody.velocity -= this.TargetMotherShip.speed;
						FindObjectOfType<CamManager> ().transform.position -= this.TargetMotherShip.transform.position;
						this.TargetMotherShip.TruePos.Lock = false;
					}
				}
			}
		}
	}
	
	private float forwardVelocity;
	private float rightVelocity;
	private float upVelocity;
	
	private float pitchInput;
	private float yawInput;
	private float rollInput;
	
	public Action YawAndPitchInput;

	public void YawAndPitchPlayerInput () {
		yawInput = Input.GetAxis ("Mouse X");
		pitchInput = Input.GetAxis ("Mouse Y");;
		
		if (Input.GetKey (KeyCode.A)) {
			rollInput ++;
		}
		if (Input.GetKey (KeyCode.D)) {
			rollInput --;
		}
	}

	void FixedUpdate () {
		if (this.pilotMode == PilotAirCraftState.Pilot) {
			pitchInput = 0f;
			yawInput = 0f;
			rollInput = 0f;
			
			if (this.YawAndPitchInput != null) {
				this.YawAndPitchInput ();
			}
			
			forwardVelocity = Vector3.Dot (this.CRigidbody.velocity, this.transform.forward);
			rightVelocity = Vector3.Dot (this.CRigidbody.velocity, this.transform.right);
			upVelocity = Vector3.Dot (this.CRigidbody.velocity, this.transform.up);
			
			float sqrForwardVelocity = forwardVelocity * Mathf.Abs (forwardVelocity);
			float sqrRightVelocity = rightVelocity * Mathf.Abs (rightVelocity);
			float sqrUpVelocity = upVelocity * Mathf.Abs (upVelocity);
			
			if (Input.GetKey (KeyCode.Space)) {
				this.engineBoost = true;
				this.CRigidbody.AddForce ((enginePow + engineBoostPow) * this.transform.forward);
			} 
			else {
				this.engineBoost = false;
				this.CRigidbody.AddForce (enginePow * this.transform.forward);
			}
			
			this.cRigidbody.AddForce (sqrForwardVelocity * this.lift * this.transform.up * this.localAtm);
			this.cRigidbody.AddForce (- sqrForwardVelocity * this.cForward * this.transform.forward * this.localAtm);
			this.cRigidbody.AddForce (- sqrRightVelocity * this.cRight * this.transform.right);
			this.cRigidbody.AddForce (- sqrUpVelocity * this.cUp * this.transform.up);
			
			this.CRigidbody.AddTorque (yawSpeed * yawInput * this.transform.up);
			this.CRigidbody.AddTorque (- pitchSpeed * pitchInput * this.transform.right);
			this.CRigidbody.AddTorque (rollSpeed * rollInput * this.transform.forward);
			
			this.CRigidbody.AddForce (this.CRigidbody.mass * this.ComputePlanetGravity ());
		}
	}
	
	void OnGUI () {
		if (this.pilotMode == PilotAirCraftState.Pilot) {
			GUI.skin = this.UISkin;
			if (this.engineBoost) {
				GUILayout.TextArea ("EnginePow = " + (this.enginePow + this.engineBoostPow));
			} 
			else {
				GUILayout.TextArea ("EnginePow = " + this.enginePow);
			}
			GUILayout.TextArea ("ForwardVelocity = " + Mathf.RoundToInt(this.forwardVelocity * 100) / 100f);
			GUILayout.TextArea ("Local Atm = " + Mathf.RoundToInt(this.localAtm * 100) / 100f);
			GUILayout.TextArea ("Land = " + (this.land != null));
			if (this.land != null) {
				GUILayout.TextArea ("(E) : Leave Aircraft");
			}
			GUILayout.TextArea ("Hangar = " + (this.hangar != null));
		}
	}
	
	public Vector3 ComputePlanetGravity () {
		this.localAtm = 0.05f;
		if (this.hangar != null) {
			this.localAtm = 2f;
		}
		Vector3 gravity = Vector3.zero;

		Planet p = this.TargetMotherShip.Planets [0].Key;
		float dist = (p.transform.position - this.transform.position).magnitude;
		dist = Mathf.Max (dist, 0f);
		
		gravity += p.Grav.GetAttractionFor (this.gameObject);
		gravity += this.TargetMotherShip.Grav.GetAttractionFor (this.gameObject);
		
		float a = (p.atmRange - Mathf.Max (dist - p.radius, 0f)) / p.atmRange * p.atmDensity;;
		if (a > 0) {
			this.localAtm = a;
		}
		
		return gravity;
	}

	public void SwitchModeTo (PilotAirCraftState newPilotMode) {
		if (newPilotMode == PilotAirCraftState.NoPilot) {
			this.YawAndPitchInput = null;
			this.pilotMode = newPilotMode;
		} 
		else if (newPilotMode == PilotAirCraftState.Pilot) {
			this.YawAndPitchInput = this.YawAndPitchPlayerInput;
			this.pilotMode = newPilotMode;
		}
	}

	public void TakeOff (Player p) {
		this.pilot = p;

		this.SwitchModeTo (PilotAirCraftState.Pilot);
		this.transform.parent = null;
		this.SetKinematic (false);
		FindObjectOfType<CamManager> ().GoAirCraftMode (this);
	}

	public void TryLand () {
		if (this.CRigidbody.velocity.sqrMagnitude > 0.01f) {
			Debug.Log ("Can't land, too fast !");
			return;
		}
		if (this.land == null) {
			Debug.Log ("Can't land, no ground !");
			return;
		}
		this.Land ();
	}

	private void Land () {
		this.SwitchModeTo (PilotAirCraftState.NoPilot);
		this.transform.parent = this.land.transform;
		this.SetKinematic (true);
		FindObjectOfType<CamManager> ().GoPlayerMode (this.pilot);

		this.pilot.DropAirCraftControl (this.land);
	}

	public void OnCollisionEnter (Collision c) {
		GravitationalObject g = SvenFranksonTools.GetComponentInAllParents<GravitationalObject> (c.collider.gameObject);
		if (g != null) {
			this.land = g;
		}
	}
	
	public void OnCollisionExit (Collision c) {
		GravitationalObject g = SvenFranksonTools.GetComponentInAllParents<GravitationalObject> (c.collider.gameObject);
		if (this.land == g) {
			this.land = null;
		}
	}

	public void OnTriggerEnter (Collider c) {		
		MotherShipHangar h = c.GetComponent<MotherShipHangar> ();
		if (h != null) {
			this.hangar = h;
		}
	}
	
	public void OnTriggerExit (Collider c) {		
		MotherShipHangar h = c.GetComponent<MotherShipHangar> ();
		if (this.hangar == h) {
			this.hangar = null;
		}
	}
}