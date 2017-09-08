using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetCollider : MonoBehaviour
{
	[FMODUnity.EventRef]
	public string m_BreakSound = "Break Sound";

	private 

	void OnCollisionEnter (Collision Collider)
	{
		if (Collider.gameObject.CompareTag("Projectile"))
		{
			Destroy(this.gameObject);
			GameManager.m_Instance.DestroyedTarget();
		}

	}
}
