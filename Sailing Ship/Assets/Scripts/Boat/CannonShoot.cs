using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonShoot : MonoBehaviour
{

	public Rigidbody m_CannonBall;
	public float m_FiringAngle;
	public Transform[] m_CannonPortTransform;
	public Transform[] m_CannonStarboardTransform;
	public float m_MinShotSpeed = 15f;        // The force given to the shell if the fire button is not held.
    public float m_MaxShotSpeed = 30f;        // The force given to the shell if the fire button is held for the max charge time.
    public float m_MaxChargeTime = 0.75f;       // How long the shell can charge for before it is fired at max force.
	public float m_FiringDelay = 2.0f; // The Time between shots.

	private bool m_Fired = false;
	private float m_ShotSpeed = 15f;
	private bool m_Port = false;
	private bool m_ReadyToCharge = true;
	private bool m_Charging = false;
	private float m_TimeSinceLastShot;
	private float m_ChargeSpeed;

	// Use this for initialization
	void Start () 
	{
		m_ShotSpeed = m_MinShotSpeed;

		m_ChargeSpeed = (m_MaxShotSpeed - m_MinShotSpeed) / m_MaxChargeTime;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (m_ShotSpeed >= m_MaxShotSpeed && !m_Fired) //
		{
			m_ShotSpeed = m_MaxShotSpeed;
			if (m_Port) // Was the camera facing port?
				Fire(m_CannonPortTransform);
			else if (!m_Port) // If not Port it's gotta be Starboard.
				Fire(m_CannonStarboardTransform);
		}
		else if (Input.GetButtonDown("Fire1") && m_ReadyToCharge)
		{
			m_Fired = false;
			m_Charging = true;
			m_ShotSpeed = m_MinShotSpeed;		
			// Add charging sound
			// Determine wether it's going to be port or starboard
		}
		else if (Input.GetButton ("Fire1") && !m_Fired)
        {
            // Increment the launch force and update the slider.
            m_ShotSpeed += m_ChargeSpeed * Time.deltaTime;
		}
		else if (Input.GetButtonDown("Fire1") && !m_ReadyToCharge)
		{
			Debug.Log("Not Ready to Fire!");
			// Play an error sound.
		}
		else if (Input.GetButtonUp("Fire1") && m_Charging)
		{
			if (m_Port)
				Fire(m_CannonPortTransform);
			else if (!m_Port)
				Fire(m_CannonStarboardTransform);
		}
		
		m_TimeSinceLastShot += Time.deltaTime;

		if (m_TimeSinceLastShot >= m_FiringDelay)
			m_ReadyToCharge = true;
	}

	void Fire (Transform[] cannonArray)
	{
		m_Fired = true;
		m_TimeSinceLastShot = 0.0f;
		m_Charging = false;
		m_ReadyToCharge = false;
		for (int i = 0; i < cannonArray.Length; i++)
		{
		Transform cannonTransform = cannonArray[i];
		Rigidbody cannonInstance = Instantiate (m_CannonBall, cannonTransform.position, cannonTransform.rotation) as Rigidbody;
		cannonInstance.velocity = m_ShotSpeed * cannonTransform.forward;
		}
	}
}
