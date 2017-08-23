using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveController : MonoBehaviour {

	public float m_height;

	public float m_time;

	// Move the ocean plane up and down by this amount to simulate "Waves" - in a manner of speaking.
	void Start () {
		iTween.MoveBy(this.gameObject, iTween.Hash("y", m_height, "time", m_time, "looptype", "pingpong", "easetype", iTween.EaseType.easeInOutSine));		
	}
}
