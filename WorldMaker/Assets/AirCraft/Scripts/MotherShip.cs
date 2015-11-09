﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(StellarObjectCenter))]
[RequireComponent(typeof(Rigidbody))]
public class MotherShip : MonoBehaviour {
	
	private StellarObjectCenter truePos;
	public StellarObjectCenter TruePos {
		get {
			if (truePos == null) {
				this.truePos = this.GetComponent<StellarObjectCenter> ();
			}
			
			return truePos;
		}
	}

	private Rigidbody cRigidbody;
	private Rigidbody CRigidbody {
		get {
			if (cRigidbody == null) {
				this.cRigidbody = this.GetComponent<Rigidbody> ();
			}

			return cRigidbody;
		}
	}

	public Vector3 speed = Vector3.zero;
	public Vector3 rotationSpeed = Vector3.zero;
	public float enginePow = 0f;
	public float enginePowMin = -10f;
	public float enginePowMax = 10f;
	public float targetSpeed = 0f;
	public float maxSpeed = 500f;
	private float speedInc = 0f;

	public float cForward;
	public float cRight;
	public float cUp;

	public float yawSpeed;
	public float pitchSpeed;
	public float rollSpeed;

	public float cYaw;
	public float cPitch;
	public float cRoll;
	
	public float forwardVelocity;
	private float rightVelocity;
	private float upVelocity;
	
	private float yawInput;
	private float pitchInput;
	private float rollInput;
	
	public PilotState pilotMode = PilotState.Pilot;
	public Planet orbitPlanet = null;
	public float orbitalPlanetDist = 0f;

	public enum PilotState
	{
		NoPilot,
		Pilot,
		Orbit,
		OrbitAutoPilot
	};

	private List<KeyValuePair<Planet, float>> planets = null;
	public List<KeyValuePair<Planet, float>> Planets {
		get {
			if (this.planets == null) {
				this.planets = new List<KeyValuePair<Planet, float>> ();
				foreach (Planet p in FindObjectsOfType <Planet> ()) {
					this.planets.Add (new KeyValuePair<Planet, float> (p, float.MaxValue));
				}
			}

			return this.planets;
		}
	}

	public Planet closestPlanet {
		get {
			if (this.Planets.Count > 0) {
				return this.Planets [0].Key;
			}
			return null;
		}
	}
	public float closestPlanetDist {
		get {
			if (this.Planets.Count > 0) {
				return this.Planets [0].Value;
			}
			return float.MaxValue;
		}
	}

	private float localAtm = 0f;

	void Start () {
		this.enginePow = 0f;
		this.SwitchModeTo (this.pilotMode);
	}

	void Update () {
		if (Input.GetKeyDown (KeyCode.Keypad8)) {
			Time.timeScale += 10f;
		}
		if (Input.GetKeyDown (KeyCode.Keypad2)) {
			Time.timeScale -= 10f;
			Time.timeScale = Mathf.Max (Time.timeScale, 1f);
		}

		this.enginePow = 0f;
		if (this.forwardVelocity < this.targetSpeed * 0.99f) {
			this.enginePow = this.enginePowMax;
		}
		if (this.forwardVelocity > this.targetSpeed * 1.1f) {
			this.enginePow = this.enginePowMin;
		}

		if (this.pilotMode == PilotState.Pilot) {
			if (Input.GetKeyDown (KeyCode.W)) {
				this.targetSpeed += 1f;
			}
			if (Input.GetKey (KeyCode.W)) {
				this.speedInc += Time.deltaTime * 5f;
				this.targetSpeed += this.speedInc * Time.deltaTime;
			}
			if (Input.GetKeyUp (KeyCode.W)) {
				this.speedInc = 0f;
				this.targetSpeed = Mathf.Floor (this.targetSpeed);
			}
			
			if (Input.GetKeyDown (KeyCode.S)) {
				this.targetSpeed -= 1f;
			}
			if (Input.GetKey (KeyCode.S)) {
				this.speedInc += Time.deltaTime * 5f;
				this.targetSpeed -= this.speedInc * Time.deltaTime;
			}
			if (Input.GetKeyUp (KeyCode.S)) {
				this.speedInc = 0f;
				this.targetSpeed = Mathf.Floor (this.targetSpeed);
			}

			this.targetSpeed = Mathf.Max (this.targetSpeed, 0f);
			this.targetSpeed = Mathf.Min (this.targetSpeed, this.maxSpeed);
		}

		else if (this.pilotMode == PilotState.OrbitAutoPilot) {
			this.targetSpeed = Mathf.Sqrt (this.closestPlanet.mass / this.closestPlanetDist);
			this.SwitchModeTo (PilotState.Orbit);
		}
	}

	public Action YawAndPitchInput;

	public void YawAndPitchPlayerInput () {
		if (Input.GetKey (KeyCode.LeftAlt)) {
			yawInput = Input.GetAxis ("Mouse X");
			pitchInput = Input.GetAxis ("Mouse Y");
		}
		if (Input.GetKey (KeyCode.A)) {
			rollInput = 1f;
		}
		if (Input.GetKey (KeyCode.D)) {
			rollInput = -1f;
		}
	}

	public void YawAndPitchAutoPilotInput () {
		pitchInput = PitchFor (orbitPlanet) / 360f;
		rollInput = RollFor (orbitPlanet) / 360f;
	}

	void FixedUpdate () {
		yawInput = 0f;
		pitchInput = 0f;
		rollInput = 0f;

		if (this.YawAndPitchInput != null) {
			this.YawAndPitchInput ();
		}

		forwardVelocity = Vector3.Dot (this.speed, this.transform.forward);
		rightVelocity = Vector3.Dot (this.speed, this.transform.right);
		upVelocity = Vector3.Dot (this.speed, this.transform.up);

		float sqrForwardVelocity = forwardVelocity * Mathf.Abs (forwardVelocity);
		float sqrRightVelocity = rightVelocity * Mathf.Abs (rightVelocity);
		float sqrUpVelocity = upVelocity * Mathf.Abs (upVelocity);

		this.speed += ((enginePow - sqrForwardVelocity * this.cForward * this.localAtm) * this.transform.forward) * Time.deltaTime;

		this.speed += (- sqrForwardVelocity * this.cForward * this.transform.forward * this.localAtm) * Time.deltaTime;
		this.speed += (- sqrRightVelocity * this.cRight * this.transform.right) * Time.deltaTime;
		this.speed += (- sqrUpVelocity * this.cUp * this.transform.up) * Time.deltaTime;

		this.rotationSpeed.x += - this.pitchSpeed * pitchInput * Time.deltaTime;
		this.rotationSpeed.y += this.yawSpeed * yawInput * Time.deltaTime;
		this.rotationSpeed.z += this.rollSpeed * rollInput * Time.deltaTime;

		this.rotationSpeed.x *= (1f - this.cPitch * Time.deltaTime);
		this.rotationSpeed.y *= (1f - this.cYaw * Time.deltaTime);
		this.rotationSpeed.z *= (1f - this.cRoll * Time.deltaTime);

		this.speed += (this.CRigidbody.mass * this.UpdatePlanets ()) * Time.deltaTime;

		if (this.pilotMode == PilotState.Orbit) {
			this.CRigidbody.MovePosition (this.transform.position + this.transform.up * (this.orbitalPlanetDist - this.DistFor (this.orbitPlanet)));
		}

		if (Input.GetKeyDown (KeyCode.O)) {
			if (this.pilotMode == PilotState.Pilot) {
				this.SwitchModeTo (PilotState.OrbitAutoPilot);
			}
			else if ((this.pilotMode == PilotState.OrbitAutoPilot) || (this.pilotMode == PilotState.Orbit)) {
				this.SwitchModeTo (PilotState.Pilot);
			}
		}

		this.transform.position += this.speed * Time.deltaTime;
		this.transform.RotateAround (this.transform.position, this.transform.right, this.rotationSpeed.x * Time.deltaTime);
		this.transform.RotateAround (this.transform.position, this.transform.up, this.rotationSpeed.y * Time.deltaTime);
		this.transform.RotateAround (this.transform.position, this.transform.forward, this.rotationSpeed.z * Time.deltaTime);
	}

//	void OnGUI () {
//		GUILayout.TextArea ("EnginePow = " + this.enginePow);
//		GUILayout.TextArea ("ForwardVelocity = " + this.forwardVelocity);
//		GUILayout.TextArea ("RightdVelocity = " + this.rightVelocity);
//		GUILayout.TextArea ("UpVelocity = " + this.upVelocity);
//		GUILayout.TextArea ("MouseX = " + this.yawInput);
//		GUILayout.TextArea ("MouseY = " + this.pitchInput);
//		GUILayout.TextArea ("Local Atm = " + this.localAtm);
//		foreach (KeyValuePair<Planet, float> p in this.Planets) {
//			GUILayout.TextArea (p.Key.planetName + " : " + p.Value + " m");
//		}
//		GUILayout.TextArea ("Closest = " + this.closestPlanet);
//		GUILayout.TextArea ("Dist = " + this.closestPlanetDist);
//		GUILayout.TextArea ("TimeScale = " + Time.timeScale);
//	}

	public bool CanEnterOrbitalAutoPilotMode () {
		if (this.closestPlanetDist > this.closestPlanet.radius * 5f) {
			return false;
		}

		return true;
	}

	public bool CanEnterOrbitalMode () {
		if (!this.CanEnterOrbitalAutoPilotMode ()) {
			return false;
		}
		if (Mathf.Abs(this.RollFor (this.closestPlanet)) > 10f) {
			return false;
		}
		if (Mathf.Abs(this.PitchFor (this.closestPlanet)) > 10f) {
			return false;
		}

		float orbitalSpeedClosest = Mathf.Sqrt (this.closestPlanet.mass / this.closestPlanetDist);

		if (Mathf.Abs ((this.forwardVelocity - orbitalSpeedClosest) / orbitalSpeedClosest) > 0.1f) {
			return false;
		}
		return true;
	}

	public void SwitchModeTo (PilotState newPilotMode) {
		if (newPilotMode == PilotState.NoPilot) {
			if (this.pilotMode == PilotState.Pilot) {
				this.YawAndPitchInput = null;
				this.orbitPlanet = null;
				this.orbitalPlanetDist = 0f;
				this.TruePos.Lock = false;
				this.pilotMode = newPilotMode;
			}
		} 
		else if (newPilotMode == PilotState.Pilot) {
			this.YawAndPitchInput = this.YawAndPitchPlayerInput;
			this.orbitPlanet = null;
			this.orbitalPlanetDist = 0f;
			this.TruePos.Lock = false;
			this.pilotMode = newPilotMode;
		}
		else if (newPilotMode == PilotState.OrbitAutoPilot) {
			if (this.CanEnterOrbitalAutoPilotMode ()) {
				this.YawAndPitchInput = this.YawAndPitchAutoPilotInput;
				this.orbitPlanet = this.closestPlanet;
				this.orbitalPlanetDist = 0f;
				this.TruePos.Lock = false;
				this.pilotMode = newPilotMode;
			}
		}
		else if (newPilotMode == PilotState.Orbit) {
			if (this.pilotMode == PilotState.OrbitAutoPilot) {
				if (this.CanEnterOrbitalMode ()) {
					this.orbitPlanet = this.closestPlanet;
					this.orbitalPlanetDist = this.closestPlanetDist;
					this.targetSpeed = Mathf.Sqrt (this.closestPlanet.mass / this.closestPlanetDist);
					this.TruePos.Lock = true;
					this.YawAndPitchInput = this.YawAndPitchAutoPilotInput;
					this.pilotMode = newPilotMode;
				}
			}
		}
	}

	public Vector3 UpdatePlanets () {
		for (int i = 0; i < this.Planets.Count; i++) {
			KeyValuePair<Planet, float> p = this.Planets [i];
			float dist = (p.Key.TruePos.TruePos - this.TruePos.TruePos).magnitude;
			this.Planets [i] = new KeyValuePair<Planet, float> (p.Key, dist);

			if (i - 1 >= 0) {
				KeyValuePair<Planet, float> pPrev = this.Planets [i - 1];
				if (p.Value < pPrev.Value) {
					this.Planets [i] = pPrev;
					this.Planets [i - 1] = p;
				}
			}
		}
		
		Vector3 gravity = this.closestPlanet.mass / (this.closestPlanetDist * this.closestPlanetDist) * (this.closestPlanet.TruePos.TruePos - this.TruePos.TruePos).normalized;

		float altitude = this.closestPlanetDist - this.closestPlanet.radius;
		float a = (this.closestPlanet.atmRange - altitude) / this.closestPlanet.atmRange * this.closestPlanet.atmDensity;;
		if (a > 0) {
			this.localAtm += a;
		}

		return gravity;
	}

	public float DistFor (Planet p) {
		foreach (KeyValuePair<Planet, float> pd in this.Planets) {
			if (p == pd.Key) {
				return pd.Value;
			}
		}
		
		return 0f;
	}

	public float RollFor (Planet p) {
		Vector3 zero = Vector3.Cross (this.transform.forward, (p.transform.position - this.transform.position));
		float rollAngle = Vector3.Angle (this.transform.right, zero);
		if (Vector3.Dot (this.transform.up, zero) < 0) {
			rollAngle = - rollAngle;
		}
		return rollAngle;
	}
	
	public float PitchFor (Planet p) {
		Vector3 zero = Vector3.Cross (this.transform.right, (this.transform.position - p.transform.position));
		float pitchAngle = Vector3.Angle (this.transform.forward, zero);
		if (Vector3.Dot (this.transform.up, zero) < 0) {
			pitchAngle = - pitchAngle;
		}
		return pitchAngle;
	}
}
