using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour 
{
	public static GameManager m_Instance = null;
	public Text m_ScoreText;
	public Rigidbody m_Gate;
	public GameObject m_PausePanel;
	public float m_Gatedrop = 8.0f;
	public float m_GateSpeed = 1.0f;
	public SoundManager m_SoundManager;
	public GameObject[] m_EnemyShips;
	public int m_Threshold;

	private Vector3 m_GateStartPosition;
	private Vector3 m_GateEndPosition;
	private bool m_GateMoving = false;
	private float m_PlatformTolerance = 0.1f;

	// [FMODUnity.EventRef]
	// public string m_GateMovingSound = "event:/Player Actions/Gate Open";
	[FMODUnity.EventRef]
	public string m_ScoreSound = "ScoreSound";
	[FMODUnity.EventRef]
	public string m_WinnerSound = "WinnerSound";

	private int m_Score = 0;
	public bool m_Paused = false;

	private FMODUnity.StudioEventEmitter m_GateEmitter;

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
		m_GateStartPosition = m_Gate.position;
		m_GateEndPosition = m_Gate.position + (Vector3.down * m_Gatedrop);
		m_GateEmitter = m_Gate.GetComponent<FMODUnity.StudioEventEmitter>();
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
		if (m_Score >= m_Threshold)
		{
			OpenGate();
			SpawnEnemies();
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
		m_SoundManager.CommitVolumeSettings();
	}

	public static void GameOver()
	{
		SceneManager.LoadScene("MainScene");
	}

	private void OpenGate()
	{
		if (!m_GateMoving)
		{
			Debug.Log("Gate Dropping");
			m_GateMoving = true;
			StartCoroutine(GateOpen());
		}
	}

	private IEnumerator GateOpen()
	{
		m_GateEmitter.Play();
		m_GateEmitter.SetParameter("gateFinish", 0.0f);
		while (Vector3.Distance(m_Gate.position, m_GateEndPosition) > m_PlatformTolerance)
		{
			Vector3 targetPosition = m_Gate.position + Vector3.down * m_GateSpeed * Time.deltaTime;
			m_Gate.MovePosition(targetPosition);
			yield return null;
		}		
		m_GateEmitter.SetParameter("gateFinish", 1.0f);
		Debug.Log("Gate Finished");
	}

	private void SpawnEnemies ()
	{
		for (int i = 0; i < m_EnemyShips.Length; i++)
		{
			m_EnemyShips[i].SetActive(true);
		}
	}

	public void QuitGame()
	{
		Application.Quit();
	}

}
