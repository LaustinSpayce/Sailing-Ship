using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetCollider : MonoBehaviour
{
	[FMODUnity.EventRef]
	public string m_BreakSound = "event:/Player Actions/Sign Hit";
	public GameObject m_Explosion;

	void OnCollisionEnter (Collision Collider)
	{
		if (Collider.gameObject.CompareTag("Projectile"))
		{
			Destroy(this.gameObject);
			GameManager.m_Instance.DestroyedTarget();
			Instantiate<GameObject>(m_Explosion, Collider.transform.position, Quaternion.identity);
			FMODUnity.RuntimeManager.PlayOneShot(m_BreakSound, Collider.transform.position);
		}

	}
}
