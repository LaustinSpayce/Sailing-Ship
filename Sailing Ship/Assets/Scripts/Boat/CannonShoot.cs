using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonShoot : MonoBehaviour {

	public float m_ShotSpeed = 25.0f;
	public Rigidbody m_CannonBall;
	public float m_FiringAngle;
	public Transform m_CannonTransform;

	private bool m_Fired = false;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetButtonDown("Fire1") && m_Fired == false)
			Fire();
		else if (Input.GetButtonUp("Fire1") && m_Fired == true)
			m_Fired = false;
	}

	void Fire ()
	{
		m_Fired = true;
		Rigidbody cannonInstance = Instantiate (m_CannonBall, m_CannonTransform.position, m_CannonTransform.rotation) as Rigidbody;

		cannonInstance.velocity = m_ShotSpeed * m_CannonTransform.forward;
	}
}
