using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetCollider : MonoBehaviour
{
	void OnCollisionEnter (Collision Collider)
	{
		if (Collider.gameObject.CompareTag("Projectile"))
		{
			Destroy(this.gameObject);
		}

	}
}
