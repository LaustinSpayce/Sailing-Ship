using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour {

	public FMOD.Studio.Bus m_SfxBus;
	public FMOD.Studio.Bus m_MusicBus;
	public Slider m_SfxSlider;
	public Slider m_MusicSlider;
	public static SoundManager m_SoundManager;

	[FMODUnity.EventRef]
    public string m_Music = "event:/Tune";

	private float SFXVolume;
	private float MusicVolume;
	FMOD.Studio.EventInstance m_MusicEv;

	void Start () 
	{
		if (m_SoundManager != null)
		{
			Destroy(this);
			return;
		}
		m_SoundManager = this;
		m_SfxBus = FMODUnity.RuntimeManager.GetBus("bus:/SFX");
		m_MusicBus = FMODUnity.RuntimeManager.GetBus("bus:/Music");
		float SFXVolume = PlayerPrefs.GetFloat("SFXVolume", 1);
		float MusicVolume = PlayerPrefs.GetFloat("MusicVolume", 1);
		m_SfxBus.setVolume(SFXVolume);
		m_MusicBus.setVolume(MusicVolume);
		m_SfxSlider.value = SFXVolume;
		m_MusicSlider.value = MusicVolume;
		Debug.Log(SFXVolume);
		m_MusicEv = FMODUnity.RuntimeManager.CreateInstance(m_Music);
		m_MusicEv.start();
	}
	
	void Update () {
		
	}

	public void UpdateVolumeSettings ()
	{
		float SFXVolume = m_SfxSlider.value;
		float MusicVolume = m_MusicSlider.value;
		m_SfxBus.setVolume(SFXVolume);
		m_MusicBus.setVolume(MusicVolume);
	}

	public void MusicPlaybackState ()
	{
		FMOD.Studio.PLAYBACK_STATE play_state;
		m_MusicEv.getPlaybackState(out play_state);
		if (MusicVolume == 0.0f)
		{
			m_MusicEv.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
		}
		else if (MusicVolume > 0.0f && play_state != FMOD.Studio.PLAYBACK_STATE.PLAYING)
		{
			m_MusicEv.start();
		}
	}

	public void CommitVolumeSettings ()
	{
		SFXVolume = m_SfxSlider.value;
		MusicVolume = m_MusicSlider.value;
		PlayerPrefs.SetFloat("SFXVolume", SFXVolume);
		PlayerPrefs.SetFloat("MusicVolume", MusicVolume);
		Debug.Log(SFXVolume);
		// Debug.Log("Committing Volume Settings");
	}
}
