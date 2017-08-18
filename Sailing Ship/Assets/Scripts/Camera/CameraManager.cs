using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour {

	public Transform m_Target;
	public float m_Smoothing;
	public float m_RotateSpeed;
	public float m_ClampTop = 80.0f;
	public float m_ClampBottom = 10.0f;
	public float m_InputSensititivty = 150.0f;
	public GameObject m_CameraObject;
	public GameObject m_PlayerObject;
	public float m_MouseX;
	public float m_MouseY;
	public float m_FinalInputX;
	public float m_FinalInputZ;

	private Vector3 m_Offset;
	private Transform m_RotateTarget;
	private float m_CamDistanceXToPlayer;
	private float m_CamDistanceYToPlayer;
	private float m_CamDistanceZToPlayer;
	private float m_rotY = 0.0f;
	private float m_rotX = 0.0f;


	void Start () 
	{
		m_Offset = transform.position - m_Target.position; // Get the position relative to the boat.
		m_RotateTarget = m_Target;

		Vector3 rot = transform.localRotation.eulerAngles;
		m_rotY = rot.y;
		m_rotX = rot.x;
		
		// Lock Cursor
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}
	
	void Update () 
	{
		// Enable Controller Support with Sticks
		float inputX = Input.GetAxis("RightStickHorizontal");
		float inputZ = Input.GetAxis("RightStickVertical");

		m_MouseX = Input.GetAxis("Mouse X");
		m_MouseY = Input.GetAxis("Mouse Y");

		m_FinalInputX = inputX + m_MouseX;
		m_FinalInputZ = inputZ + m_MouseY;

		m_rotY += m_FinalInputX * m_InputSensititivty * Time.deltaTime;
		m_rotX += m_FinalInputZ * m_InputSensititivty * Time.deltaTime;

		m_rotX = Mathf.Clamp(m_rotX, m_ClampBottom, m_ClampTop);

		Quaternion localRotation = Quaternion.Euler (m_rotX, m_rotY, 0.0f);
		transform.rotation = localRotation;




	}

	void LateUpdate ()	
	{
		CameraUpdater();
		// Vector3 targetCamPos = m_Target.position + m_Offset;
		// transform.position = Vector3.Lerp( transform.position, targetCamPos, m_Smoothing * Time.deltaTime);
	}

	void CameraUpdater ()
	{
		Transform target = m_PlayerObject.transform;

		float step = m_Smoothing * Time.deltaTime;
		transform.position = Vector3.Lerp(transform.position, target.position, m_Smoothing);
	}
}
