using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CannonShoot : MonoBehaviour
{

	public Rigidbody m_CannonBall;					// Cannonball instance.
	public float m_FiringAngle;						// Upward angle of the cannons. I never bothered using this.
	public Transform[] m_CannonPortTransform;		// Array of Cannons on the port side.
	public Transform[] m_CannonStarboardTransform;	// Array of Cannons on the staboard side.
	// I quite regret using Port and Starboard, it'd be much easier to use left and right.
	public Slider m_AimPortSlider; // Port Aim Slider
	public Slider m_AimStarboardSlider; // Starboard Aim Slider
	public Slider m_CannonRechargeSlider; // Cannon Recharge Slider.
	public Image m_CannonReadyImage; // Cannon Ready Image.
	public float m_MinShotSpeed = 15f;        // The force given to the cannonball if the fire button is not held.
    public float m_MaxShotSpeed = 30f;        // The force given to the cannonball if the fire button is held for the max charge time.
    public float m_MaxChargeTime = 0.75f;       // How long the cannon can charge for before it is fired at max force.
	public float m_FiringDelay = 2.0f; 			// The Time between shots.
	public GameObject m_CameraBase; // The Camera Base, we get the angle from this object.
	public float m_MaxDelay = 0.25f; // Maximum delay from releasing the fire button until the cannon goes bang.
	public float m_CrewCompetence = 0.5f; // Multiplier for the max angle deviance. 1 is incompetent, 0 is perfect.
	public float m_MaxAngleDeviance = 15f; // Maximum angle, in degrees, in a cone around the cannon's direction.
	public bool m_isNPC = false; // normally not an NPC. Change to true if you are.
	public Transform m_Player; // The player
	public float m_MinimumDistanceToShoot;
	
	// Now for FMOD and Sound stuff
	[FMODUnity.EventRef]
	public string m_CannonSound = "event:/Input_1";
	[FMODUnity.EventRef]
	public string m_ChargeSound = "event:/Input_2";
	[FMODUnity.EventRef]
	public string m_ErrorSound = "Error Sound";	

	private bool m_Fired = false; // Has a ball been fired
	private float m_ShotSpeed = 15f;	// Speed of cannon ball when fired
	private bool m_Port = false;		// determine which side will be fired, is it port or not.
	private bool m_ReadyToCharge = true; // There is a delay between shots, when the delay is finished then this registers as true.
	private bool m_Charging = false;
	private float m_TimeSinceLastShot;
	private float m_ChargeSpeed;
	private float m_CameraAngle;
	private Slider m_ActiveSlider; // Switch between port and starboard once
	private FMOD.Studio.EventInstance m_ChargeSoundEvent;
	private Rigidbody m_BoatRigidBody;

	// Use this for initialization
	void Start () 
	{
		m_ShotSpeed = m_MinShotSpeed;

		m_ChargeSpeed = (m_MaxShotSpeed - m_MinShotSpeed) / m_MaxChargeTime;
		if(!m_isNPC)
		{
			m_AimPortSlider.value = m_MinShotSpeed;
			m_AimStarboardSlider.value = m_MinShotSpeed;
			m_CannonRechargeSlider.value = m_FiringDelay;
			m_CannonReadyImage.gameObject.SetActive(true);
			m_ChargeSoundEvent = FMODUnity.RuntimeManager.CreateInstance(m_ChargeSound);
			FMODUnity.RuntimeManager.AttachInstanceToGameObject(m_ChargeSoundEvent, this.transform, m_BoatRigidBody);
		}
	}

	// Update is called once per frame
	void Update () 
	{
		if (m_ShotSpeed >= m_MaxShotSpeed && !m_Fired && !m_isNPC) //When the charge reaches maximum, fire anyway.
		{
			m_ShotSpeed = m_MaxShotSpeed;
			if (m_Port) // Was the camera facing port?
				Fire(m_CannonPortTransform);
			else if (!m_Port) // If not Port it's gotta be Starboard.
				Fire(m_CannonStarboardTransform);
		}
		else if (Input.GetButtonDown("Fire1") && m_ReadyToCharge && !GameManager.m_Instance.m_Paused && !m_isNPC)
		{
			m_Fired = false;
			m_Charging = true;
			m_ShotSpeed = m_MinShotSpeed;		
			// Add charging sound
			m_ChargeSoundEvent.start();
			// We decide wether to fire port or starboard right now.
			var cameraAngle = m_CameraBase.GetComponent<CameraManager>().m_AngleFromFront; // Get the Camera Angle from the Camera Manager Script
			if (cameraAngle < 0.0f) // Determine wether it's going to be port or starboard (port is negative degrees)
				{
				m_Port = true;
				m_ActiveSlider = m_AimPortSlider;
				}
			else {
				m_Port = false;
				m_ActiveSlider = m_AimStarboardSlider;
			}
		}
		else if (Input.GetButton ("Fire1") && !m_Fired && !m_isNPC)
        {
            // Increment the launch force.
            m_ShotSpeed += m_ChargeSpeed * Time.deltaTime;
			// Update UI.
			m_ActiveSlider.value = m_ShotSpeed;
		}
		else if (Input.GetButtonDown("Fire1") && !m_ReadyToCharge && !m_isNPC)
		{
			if (!GameManager.m_Instance.m_Paused)
			{
				FMODUnity.RuntimeManager.PlayOneShot (m_ErrorSound, transform.position);
			}
		}
		else if (Input.GetButtonUp("Fire1") && m_Charging && !m_isNPC)
		{
			if (m_Port)
				Fire(m_CannonPortTransform);
			else if (!m_Port)
				Fire(m_CannonStarboardTransform);
		}
		
		if (!m_ReadyToCharge)
		{
			m_TimeSinceLastShot += Time.deltaTime;
			if (!m_isNPC)
			{
			m_CannonRechargeSlider.value = m_TimeSinceLastShot;
			}
		}

		if (m_TimeSinceLastShot >= m_FiringDelay)
		{
			m_ReadyToCharge = true;
			if (!m_isNPC)
			{
			m_CannonRechargeSlider.value = m_FiringDelay;
			m_CannonReadyImage.gameObject.SetActive(true);
			}
		}

		var distanceToPlayer = Vector3.Distance(this.transform.position, m_Player.position);

		if (m_isNPC && m_ReadyToCharge && distanceToPlayer < m_MinimumDistanceToShoot)
		{
			var playerAngle = Vector3.SignedAngle(this.transform.forward, m_Player.position - this.transform.position, Vector3.up);
			var NPCShotPower = Random.Range(m_MinShotSpeed, m_MaxShotSpeed);
			if (playerAngle < 0.0f)
			{
				m_Port = true;
				Fire(m_CannonPortTransform);
			}
			else
			{
				m_Port = false;
				Fire(m_CannonStarboardTransform);
			}
		}
	}

	void Fire (Transform[] cannonArray)
	{
		m_Fired = true;
		m_TimeSinceLastShot = 0.0f;
		m_Charging = false;
		m_ReadyToCharge = false;
		if (!m_isNPC)
		{		
			StopChargeSound();
			m_CannonRechargeSlider.value = m_TimeSinceLastShot;
			m_ActiveSlider.value = m_MinShotSpeed;
			m_CannonReadyImage.gameObject.SetActive(false);
		}
		for (int i = 0; i < cannonArray.Length; i++)
		{
		StartCoroutine(CannonBoom(cannonArray[i]));
		}
	}

	IEnumerator CannonBoom (Transform cannonTransform)
	{
		yield return new WaitForSeconds(Random.Range(0, m_MaxDelay * m_CrewCompetence));
		var xDeviance = Random.Range(-m_MaxAngleDeviance, m_MaxAngleDeviance) * m_CrewCompetence; // x degrees variation
		var zDeviance = Random.Range(-m_MaxAngleDeviance, m_MaxAngleDeviance) * m_CrewCompetence; // z degrees variation
		var cannonBallDirection = cannonTransform.transform;
		cannonBallDirection.Rotate(zDeviance, xDeviance, 0);
		Rigidbody cannonInstance = Instantiate (m_CannonBall, cannonTransform.position, cannonTransform.rotation) as Rigidbody;
		cannonInstance.velocity = m_ShotSpeed * cannonBallDirection.forward;
		FMODUnity.RuntimeManager.PlayOneShot (m_CannonSound, cannonTransform.position);
		cannonTransform.transform.GetComponent<ParticleSystem>().Play();
	}

	private void StopChargeSound ()
	{
		m_ChargeSoundEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
	}
}
