using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonBall : MonoBehaviour
{
	public LayerMask m_ShipMask; // Set to "Players"
	public float m_CannonBallLifeTime = 5.0f;
	public float m_CannonBallDamageDealt = 5.0f;
	public GameObject m_Explosion;
	[FMODUnity.EventRef]
	public string m_FMODHitShipRef = "event:/Player Actions/Ship Hit";

	// Use this for initialization
	void Start ()
	{
		Destroy(this.gameObject, m_CannonBallLifeTime);
	}

	private void OnDestroy ()
	{
		
	}

	private void OnTriggerEnter (Collider other)
	{
		if (other.tag == "Player")
		{
			Destroy(this.gameObject);
			Rigidbody targetRigidbody = other.GetComponent<Rigidbody>();
			if (!targetRigidbody)
			{
				return;
			}
			BoatHealth targetHealth = targetRigidbody.GetComponent<BoatHealth> ();
			if (!targetHealth)
			{
				return;
			}
			targetHealth.TakeDamage(m_CannonBallDamageDealt);
			Instantiate<GameObject>(m_Explosion, this.transform.position, Quaternion.identity);
			FMODUnity.RuntimeManager.PlayOneShot(m_FMODHitShipRef, this.gameObject.transform.position);
		}
	}
	

		
}
