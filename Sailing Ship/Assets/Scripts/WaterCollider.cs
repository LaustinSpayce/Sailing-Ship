using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterCollider : MonoBehaviour 
{
	[FMODUnity.EventRef]
	public string m_CannonBallSplash = "event:/Input_1";

	public GameObject m_SplashParticles;

	public float m_CannonBallSplashVelocity = 1.0f;

	private Rigidbody m_CannonBallRigidBody;

	void OnTriggerEnter (Collider Collider)
	{
		m_CannonBallRigidBody = Collider.gameObject.GetComponent<Rigidbody>(); // To calculate the velocity of the ball, hurrah.
		if (Collider.gameObject.CompareTag("Projectile") && m_CannonBallRigidBody.velocity.magnitude > m_CannonBallSplashVelocity) // I don't want a splash if the cannon ball is slow.
		{
			var position = Collider.gameObject.transform.position;
			FMODUnity.RuntimeManager.PlayOneShot (m_CannonBallSplash, position); // Play splashing sound effect.
			var angle = Quaternion.identity;
			angle *= Quaternion.Euler(-90, 0, 0);
			Instantiate(m_SplashParticles, position, angle); 
		}

	}
}
