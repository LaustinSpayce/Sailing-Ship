using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetCollider : MonoBehaviour
{
	void OnCollisionEnter (Collision Collider)
	{
		if (Collider.gameObject.CompareTag("Projectile"))
		{
			// Debug.Log("Ow I'm hit!");
			transform.DetachChildren();
			Destroy(this.gameObject);
		}

	}
}
