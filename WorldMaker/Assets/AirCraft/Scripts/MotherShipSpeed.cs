﻿using UnityEngine;
using System.Collections;

[RequireComponent(typeof(TextMesh))]
public class MotherShipSpeed : MonoBehaviour {


	public MotherShip targetMotherShip;
	private MotherShip TargetMotherShip {
		get {
			if (this.targetMotherShip == null) {
				this.targetMotherShip = this.GetComponentInParent<MotherShip> ();
			}
			return targetMotherShip;
		}
	}

	public TextMesh targetTextMesh;
	private TextMesh TargetTextMesh {
		get {
			if (this.targetTextMesh == null) {
				this.targetTextMesh = this.GetComponent<TextMesh> ();
			}
			return targetTextMesh;
		}
	}

	public void Update () {
		TargetTextMesh.text = "Current " + Mathf.RoundToInt (TargetMotherShip.forwardVelocity) + " m/s\n";
		TargetTextMesh.text += "Target " + Mathf.RoundToInt (TargetMotherShip.targetSpeed) + " m/s\n";
		if (TargetMotherShip.pilotMode == MotherShip.PilotState.Pilot) {
			TargetTextMesh.text += "(Z) : Speed Up\n";
			TargetTextMesh.text += "(S) : Slow Down";
		}
	}
}
