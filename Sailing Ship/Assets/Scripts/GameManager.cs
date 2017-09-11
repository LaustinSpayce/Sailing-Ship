using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour 
{
	public static GameManager m_Instance = null;
	public Text m_ScoreText;
	public Rigidbody m_Gate;
	public GameObject m_PausePanel;

	private int m_Score = 0;
	private bool m_Paused = false;

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
		// if (m_PausePanel != null){
		// 	m_PausePanel = GameObject.Find("PausePanel");
		// }
	}

	private void Start ()
	{
		m_Score = 0;
		UpdateScore();
	}

	private void Update()
	{
		if (Input.GetButtonDown("Cancel"))
		{
			PauseToggle();
		}
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

	public void PauseToggle()
	{
		if (m_Paused)
		{
			ResumeGame();
		}
		else if (!m_Paused)
		{
			PauseGame();
		}
	}

	public void PauseGame()
	{
		Time.timeScale = 0.0f;
		m_PausePanel.SetActive(true);
		m_Paused = true;
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
	}

	public void ResumeGame()
	{
		Time.timeScale = 1.0f;
		m_PausePanel.SetActive(false);
		m_Paused = false;
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	private void OpenGate()
	{

	}

	public void QuitGame()
	{
		Application.Quit();
	}
}
