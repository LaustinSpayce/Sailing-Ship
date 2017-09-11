using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionInstantiations : MonoBehaviour
{
	[FMODUnity.EventRef]
	public string m_ExplosionEvent = "explosionevent";
	public GameObject m_Smoke;

	// Use this for initialization
	void Awake () {
		Instantiate(m_Smoke, this.gameObject.transform.position, Quaternion.identity);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
