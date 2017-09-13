using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterSprayCollider : MonoBehaviour
{

	public Rigidbody m_SprayCube;
	public float m_SprayHeight = 9.0f;
	public float m_SpraySpeed = 1.0f;
	public float m_SprayTime = 2.0f;
	public ParticleSystem m_SprayParticles;
	public float m_SprayParticlesMax = 15.0f;
	public float m_PlatformTolerance = 0.1f;

	private bool m_IsSpraying = false;
	private Vector3 m_CubeStartPosition;
	private Vector3 m_CubeEndPosition;
	private FMODUnity.StudioEventEmitter m_WaterSpraySound;


	void Start () {
		m_IsSpraying = false;
		m_CubeStartPosition = m_SprayCube.position;
		m_CubeEndPosition = m_SprayCube.position + (Vector3.up * m_SprayHeight);
		m_SprayCube.MovePosition(m_SprayCube.position);
		m_SprayParticles.Stop();
		m_WaterSpraySound = m_SprayCube.GetComponent<FMODUnity.StudioEventEmitter>();
	}

	void OnCollisionEnter (Collision Collider)
	{
		if (Collider.gameObject.CompareTag("Projectile") && !m_IsSpraying)
		{
			m_IsSpraying = true;
			m_SprayParticles.startSpeed = 0.0f;
			m_SprayParticles.Play();
			StartCoroutine (SprayRoutine());
			Debug.Log("Start The Spray!");
		}

	}

	private IEnumerator SprayRoutine ()
	{
		m_WaterSpraySound.Play();
		yield return StartCoroutine (SprayRise());
		yield return StartCoroutine (SprayStay());
		yield return StartCoroutine (SprayFall());
		Debug.Log("Spray Finished");
	}

	private IEnumerator SprayRise ()
	{
		while (Vector3.Distance(m_SprayCube.position, m_CubeEndPosition) > m_PlatformTolerance)
		{
			Vector3 targetPosition = Vector3.Lerp(m_SprayCube.position, m_CubeEndPosition, m_SpraySpeed * Time.deltaTime);
			m_SprayParticles.startSpeed = Mathf.Lerp(m_SprayParticles.startSpeed, m_SprayParticlesMax, m_SpraySpeed * Time.deltaTime);
			m_SprayCube.MovePosition(targetPosition);
			yield return null;
		}
	}

	private IEnumerator SprayStay ()
	{
		Debug.Log("Spray Staying");
		yield return new WaitForSeconds(m_SprayTime);
	}

	private IEnumerator SprayFall ()
	{
		Debug.Log("Spray Falling");
		while (Vector3.Distance(m_SprayCube.position, m_CubeStartPosition) > m_PlatformTolerance)
		{
			Vector3 targetPosition = Vector3.Lerp(m_SprayCube.position, m_CubeStartPosition, m_SpraySpeed * Time.deltaTime);
			m_SprayParticles.startSpeed = Mathf.Lerp(m_SprayParticles.startSpeed, 0.0f, m_SpraySpeed * Time.deltaTime);
			m_SprayCube.MovePosition(targetPosition);
			yield return null;
		}
		m_IsSpraying = false;
		m_SprayParticles.Stop();
	}


}
