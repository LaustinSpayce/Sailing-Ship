using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SinkingShip : MonoBehaviour
{

	public float m_SinkSpeed = 2.0f;
	public float m_SinkTimer = 5.0f;
	public float m_RotationSpeed = 2f;
	public float m_SinkSpinSpeed = 45f;

	private float randomX;
	private float randomY;

	void Awake ()
	{
		randomX = Mathf.Clamp(Random.Range(-1.0f, 1.0f)*5, -1.0f, 1.0f);
		randomY = Mathf.Clamp(Random.Range(-1.0f, 1.0f)*5, -1.0f, 1.0f);
		Destroy(this.gameObject, m_SinkTimer);
	}

	void Start ()
	{

	}

	void Update ()
	{
		transform.Translate (Vector3.down * m_SinkSpeed * Time.deltaTime, Space.World);
		transform.Rotate(0, m_SinkSpinSpeed/2 * Time.deltaTime * randomX, m_SinkSpinSpeed * Time.deltaTime * randomY);
	}
}
