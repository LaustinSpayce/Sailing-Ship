using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonBall : MonoBehaviour
{

	public float m_CannonBallLifeTime = 5.0f;

	// Use this for initialization
	void Start () {
		Destroy(this.gameObject, m_CannonBallLifeTime);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
