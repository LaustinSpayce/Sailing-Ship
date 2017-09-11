using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoatMover : MonoBehaviour 
{

	public float m_MaxSpeed = 5.0f; // maximum speed
	public float m_MinSpeed = -1.0f;
	public float m_MaxTurnSpeed = 180f; // maximum turn rate in degrees per second
	public float m_MinTurnSpeed = 10f; // minimum turn speed
	public Slider m_TargetSpeedSlider;
	public Slider m_BoatSpeedSlider;
	public float m_SpeedMultiplier = 10.0f;
	public float m_BoatAcceleration = 0.1f;
	public TrailRenderer m_Trail;
	public float m_TrailThreshold;

	private Rigidbody m_Rigidbody; // rigidbody for the boat.
	private float m_CurrentSpeed; // current speed of the boat.
	private float m_TargetSpeed; // Target Speed of boat.
	private Rigidbody m_CameraTarget;

	void Awake ()
	{
		m_Rigidbody = GetComponent<Rigidbody>();
		// m_CameraTarget = GetComponentInChildren<Rigidbody>(); // Doesn't work like I hoped
		m_TargetSpeed = 0.0f;
		m_CurrentSpeed = 0.0f;
		m_Trail.gameObject.SetActive(false);
	}
	
	void Update () 
	{
		GetTargetSpeed();
		MoveBoat();
		TurnBoat();
		SetTrail();
	}

	// Move the boat forward
	void MoveBoat ()
	{
		if (m_CurrentSpeed >= m_TargetSpeed)
		{
			m_CurrentSpeed -= m_BoatAcceleration * Time.deltaTime;
		}
		else if (m_CurrentSpeed < m_TargetSpeed)
		{
			m_CurrentSpeed += m_BoatAcceleration * Time.deltaTime;
		}
		m_BoatSpeedSlider.value = m_CurrentSpeed;
		Vector3 movement = transform.forward * m_CurrentSpeed * Time.deltaTime;
		m_Rigidbody.MovePosition(m_Rigidbody.position + movement);
	}

	// Speed of the boat going forward.
	void GetTargetSpeed ()
	{
			var speedInput = Input.GetAxis("Vertical") * m_SpeedMultiplier * Time.deltaTime;
			m_TargetSpeed = Mathf.Clamp(m_TargetSpeed + speedInput, m_MinSpeed, m_MaxSpeed);
			if (Input.GetButton("Jump"))
			{
				m_TargetSpeed = 0;
			}
			m_TargetSpeedSlider.value = m_TargetSpeed;
	}

	// Turn the boat, the boat will only move the minimum when stationary, and the maximum at full speed.
	void TurnBoat ()
	{
		// turnvalue from the input.
		float turnValue = Input.GetAxis("Horizontal");

		float turnMultiplier = Mathf.Lerp(m_MinTurnSpeed, m_MaxTurnSpeed, m_CurrentSpeed / m_MaxSpeed);

		float turn =  turnValue * turnMultiplier * Time.deltaTime;

		// make it into a turn rotation on the y axis

		Quaternion turnRotation = Quaternion.Euler (0f, turn, 0f);

		// apply to rotation

		// Quaternion cameraRotation = m_Rigidbody.rotation;
		m_Rigidbody.MoveRotation (m_Rigidbody.rotation * turnRotation);
		// m_CameraTarget.MoveRotation (cameraRotation); // Doesn't work like I hoped.
		
	}

	void SetTrail ()
	{
		if (Mathf.Abs(m_CurrentSpeed) > m_TrailThreshold)
		{
			m_Trail.gameObject.SetActive(true);
		}
		else
		{
			m_Trail.gameObject.SetActive(false);
		}
	}
}
