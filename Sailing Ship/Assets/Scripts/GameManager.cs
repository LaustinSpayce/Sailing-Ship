using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour 
{
	public static GameManager m_Instance = null;
	public Text m_ScoreText;
	public Rigidbody m_Gate;

	private int m_Score = 0;

	private void Awake()
	{
		if (m_Instance == null)
		{
			m_Instance = this;
		}
		else if (m_Instance != this)
		{
			Destroy(gameObject);
		}
	}

	private void Start ()
	{
		m_Score = 0;
		UpdateScore();
	}

	public void DestroyedTarget()
	{
		m_Score++;
		UpdateScore();
	}

	private void UpdateScore()
	{
		m_ScoreText.text = "Score: " + m_Score;
		if (m_Score >= 4)
		{
			OpenGate();
		}
	}

	private void OpenGate()
	{

	}
}
