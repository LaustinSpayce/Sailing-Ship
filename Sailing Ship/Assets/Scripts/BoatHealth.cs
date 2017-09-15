using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoatHealth : MonoBehaviour {

	public float m_StartingHealth = 100f;
	public Slider m_HealthSlider;
	public GameObject m_DeadBoat;

	[FMODUnity.EventRef]
	public string m_ShipDeathSound = "event:/Player Actions/Ship Death";

	private float m_CurrentHealth;
	private bool m_Dead;
	private CannonShoot m_CannonShoot;

	// Use this for initialization
	void OnEnable () {
		m_CurrentHealth = m_StartingHealth;
		m_Dead = false;
		SetHealthUI();
		m_CannonShoot = GetComponent<CannonShoot>();
	}
	
	public void TakeDamage (float amount)
	{
		m_CurrentHealth -= amount;

		SetHealthUI();

		if (m_CurrentHealth <= 0.0f && !m_Dead)
		{
			OnDeath();
		}
	}

	private void SetHealthUI()
	{
		m_HealthSlider.value = m_CurrentHealth;
	}

	private void OnDeath()
	{
		m_Dead = true;
		Destroy(this.gameObject);
		GameObject deadBoat = Instantiate(m_DeadBoat, this.transform.position, this.transform.rotation);
		if (!m_CannonShoot.m_isNPC)
		{
			GameManager.m_Instance.GameOver();
		}
	}

}
