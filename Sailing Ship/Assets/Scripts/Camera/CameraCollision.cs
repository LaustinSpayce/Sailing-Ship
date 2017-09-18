using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCollision : MonoBehaviour {

	public float m_MinDistance = 1.0f;
	public float m_MaxDistance = 4.0f;
	public float m_Smooth = 10.0f;
	private Vector3 m_DollyDir;
	public Vector3 m_DollyDirAdjusted;
	public float m_Distance;
	public LayerMask m_LayerMask;

	void Awake ()
	{
		m_DollyDir = transform.localPosition.normalized;
		m_Distance = transform.localPosition.magnitude;
	}
	
	void Update () {
		
		Vector3 DesiredCameraPos = transform.parent.TransformPoint (m_DollyDir * m_MaxDistance);
		RaycastHit hit;
		if (Physics.Linecast (transform.parent.position, DesiredCameraPos, out hit, m_LayerMask))
		{
			m_Distance = Mathf.Clamp ((hit.distance * 0.9f), m_MinDistance, m_MaxDistance);
		}
		else 
		{
			m_Distance = m_MaxDistance;
		}

		transform.localPosition = Vector3.Lerp (transform.localPosition, m_DollyDir * m_Distance, Time.deltaTime * m_Smooth);

	}
}
