using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatRocker : MonoBehaviour {


	public float m_ZDegrees;
	public float m_ZTime;

	void Awake ()
	{
		this.gameObject.transform.rotation = Quaternion.Euler(0, 0, -m_ZDegrees/2);
	}

	void Start () 
	{
		iTween.RotateAdd(this.gameObject, iTween.Hash("z", m_ZDegrees, "time", m_ZTime, "looptype", "pingpong", "easetype", iTween.EaseType.easeInOutSine));
	}
}
