using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatMover : MonoBehaviour {

	public float m_MaxSpeed = 5.0f; // maximum speed
	public float m_MaxTurnSpeed = 180f; // maximum turn rate in degrees per second
	public float m_MinTurnSpeed = 10f; // minimum turn speed

	private Rigidbody m_Rigidbody; // rigidbody for the boat.
	private float m_CurrentSpeed; // current speed of the boat.
	private float m_TargetSpeed; // Target Speed of boat.
	private Rigidbody m_CameraTarget;

	void Awake ()
	{
		m_Rigidbody = GetComponent<Rigidbody>();
		// m_CameraTarget = GetComponentInChildren<Rigidbody>(); // Doesn't work like I hoped
	}

	void Start () 
	{
		
	}
	
	// Update is called once per frame
	void Update () 
	{
		MoveBoat();
		TurnBoat();
	}

	// Move the boat forward
	void MoveBoat ()
	{
		GetTargetSpeed();
		
		Vector3 movement = transform.forward * m_TargetSpeed * Time.deltaTime;

		m_Rigidbody.MovePosition(m_Rigidbody.position + movement);
	}

	// Speed of the boat going forward.
	void GetTargetSpeed ()
	{
		if (Input.GetAxis("Vertical") >= 0) // The boat can only go forward so we ignore any reverse requests.
			m_TargetSpeed = Input.GetAxis("Vertical") * m_MaxSpeed;
		else m_TargetSpeed = 0; // if in reverse set the TargetSpeed to 0
	}

	// Turn the boat, the boat will only move the minimum when stationary, and the maximum at full speed.
	void TurnBoat ()
	{
		// turnvalue from the input.
		float turnValue = Input.GetAxis("Horizontal");

		float turnMultiplier = Mathf.Lerp(m_MinTurnSpeed, m_MaxTurnSpeed, m_TargetSpeed / m_MaxSpeed);

		float turn =  turnValue * turnMultiplier * Time.deltaTime;

		// make it into a turn rotation on the y axis

		Quaternion turnRotation = Quaternion.Euler (0f, turn, 0f);

		// apply to rotation

		// Quaternion cameraRotation = m_Rigidbody.rotation;
		m_Rigidbody.MoveRotation (m_Rigidbody.rotation * turnRotation);
		// m_CameraTarget.MoveRotation (cameraRotation); // Doesn't work like I hoped.
		
	}
}
