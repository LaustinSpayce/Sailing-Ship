using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateHealthText : MonoBehaviour {

	public Text m_healthText;
	public float m_InputHealth = 100f;

	void Start () {
		UpdateText (m_InputHealth);
	}
	
	public void UpdateText (float Health)
	{
		m_healthText.text = (Mathf.Floor(Health)).ToString();
	}
}
