using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeOut : MonoBehaviour {

	public float m_FadeOutTime = 2.0f;

	// OK it's not a fade I know. I just don't know how to do one.

	void Start () {
		Destroy(this.gameObject, m_FadeOutTime);
	}
	

}
