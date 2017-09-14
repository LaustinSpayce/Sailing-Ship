using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour {


	public Slider m_SfxSlider;
	public Slider m_MusicSlider;
	[SerializeField]
	public static SoundManager s_SoundManager;

	[FMODUnity.EventRef]
    public string m_Music = "event:/Music/Music Track";


	private float m_SFXVolume;
	private float m_MusicVolume;
	private FMOD.Studio.EventInstance m_MusicEvent;
	private FMOD.Studio.Bus m_SfxBus;
	private FMOD.Studio.Bus m_MusicBus;

	void Start () 
	{
		if (s_SoundManager != null)
		{
			Destroy(this);
			return;
		}
		s_SoundManager = this;
		m_SfxBus = FMODUnity.RuntimeManager.GetBus("bus:/SFX");
		m_MusicBus = FMODUnity.RuntimeManager.GetBus("bus:/Music");
		m_SFXVolume = PlayerPrefs.GetFloat("SFXVolume", 1);
		m_MusicVolume = PlayerPrefs.GetFloat("MusicVolume", 1);
		m_SfxBus.setVolume(m_SFXVolume);
		m_MusicBus.setVolume(m_MusicVolume);
		m_SfxSlider.value = m_SFXVolume;
		m_MusicSlider.value = m_MusicVolume;
		m_MusicEvent = FMODUnity.RuntimeManager.CreateInstance(m_Music);
		MusicPlaybackState();
		DontDestroyOnLoad(this.gameObject);
		m_MusicEvent.setParameterValue("targetsLeft", 10);
	}
	
	void Update () {
		
	}

	public void UpdateVolumeSettings ()
	{
		m_SFXVolume = m_SfxSlider.value;
		m_MusicVolume = m_MusicSlider.value;
		m_SfxBus.setVolume(m_SFXVolume);
		m_MusicBus.setVolume(m_MusicVolume);
		MusicPlaybackState();
	}

	public void MusicPlaybackState ()
	{
		FMOD.Studio.PLAYBACK_STATE play_state;
		m_MusicEvent.getPlaybackState(out play_state);
		if (m_MusicVolume == 0.0f)
		{
			m_MusicEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
		}
		else if (m_MusicVolume > 0.0f && play_state != FMOD.Studio.PLAYBACK_STATE.PLAYING)
		{
			m_MusicEvent.start();
		}
	}

	public void CommitVolumeSettings ()
	{
		m_SFXVolume = m_SfxSlider.value;
		m_MusicVolume = m_MusicSlider.value;
		PlayerPrefs.SetFloat("SFXVolume", m_SFXVolume);
		PlayerPrefs.SetFloat("MusicVolume", m_MusicVolume);
		MusicPlaybackState();
	}

	public void SetMusicTargetsLeft(int targetsLeft)
	{
		m_MusicEvent.setParameterValue("targetsLeft", targetsLeft);
		Debug.Log(targetsLeft);
	}
}
