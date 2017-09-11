using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestruct : MonoBehaviour {

	public float m_SelfDestructTimer;
	public float m_StopAnimationTimer;

	private SpriteRenderer m_SpriteRenderer;

	void Awake () {
		m_SpriteRenderer = GetComponent<SpriteRenderer>();
		Destroy(this.gameObject, m_SelfDestructTimer);
		StartCoroutine(StopAnimation());
	}

	IEnumerator StopAnimation ()
	{
		yield return new WaitForSeconds(m_StopAnimationTimer);
		m_SpriteRenderer.enabled = false;
	}
}
