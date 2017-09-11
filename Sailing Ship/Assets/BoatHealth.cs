using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoatHealth : MonoBehaviour {

	public float m_StartingHealth = 100f;
	public Slider m_HealthSlider;

	[FMODUnity.EventRef]
	public string m_ShipDeathSound = "event:/Player Actions/Ship Death";

	private float m_CurrentHealth;
	private bool m_Dead;


	// Use this for initialization
	void OnEnable () {
		m_CurrentHealth = m_StartingHealth;
		m_Dead = false;

		SetHealthUI();		
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
		// Do some sinking animation or something
		m_Dead = true;
	}

}
