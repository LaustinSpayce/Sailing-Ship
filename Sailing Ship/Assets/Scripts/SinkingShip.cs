using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SinkingShip : MonoBehaviour {

	public float m_SinkSpeed = 2.0f;
	public float m_SinkTimer = 5.0f;
	public float m_RotationSpeed = 2f;
	public float m_SinkSpinSpeed = 45f;

	void Awake ()
	{
		Destroy(this.gameObject, m_SinkTimer);
	}

	void Start ()
	{
		iTween.RotateTo(this.gameObject, iTween.Hash("x", -90.0f, "time", m_RotationSpeed, "z", 45.0f, "easetype", iTween.EaseType.easeInOutSine));
	}

	void Update ()
	{
		transform.Translate (-Vector3.forward * m_SinkSpeed * Time.deltaTime);
		transform.Rotate(0, 0, m_SinkSpinSpeed * Time.deltaTime);
	}
}
