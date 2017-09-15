using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatSound : MonoBehaviour 
{

	[FMODUnity.EventRef]
	public string m_BoatAmbience = "event:/Environment/Boat Ambience";
	private BoatMover m_BoatMover;
	private FMOD.Studio.EventInstance m_BoatAmbienceEvent;                //rolling event
    private FMOD.Studio.ParameterInstance m_BoatSpeedParameter;    //speed param object
	private float m_BoatSpeedRatio;
	private Rigidbody m_BoatRigidBody;


	void Start () 
	{
		m_BoatMover = GetComponent<BoatMover>();
		m_BoatRigidBody = GetComponent<Rigidbody>();
		m_BoatAmbienceEvent = FMODUnity.RuntimeManager.CreateInstance(m_BoatAmbience);
		m_BoatAmbienceEvent.getParameter("boatSpeed", out m_BoatSpeedParameter);
		m_BoatAmbienceEvent.start();
		FMODUnity.RuntimeManager.AttachInstanceToGameObject(m_BoatAmbienceEvent, this.transform, m_BoatRigidBody);
		m_BoatSpeedParameter.setValue(0.0f);
	}
	
	void Update () 
	{
		m_BoatSpeedRatio = m_BoatMover.m_CurrentSpeed / m_BoatMover.m_MaxSpeed;
		m_BoatSpeedParameter.setValue(m_BoatSpeedRatio);
	}

	void OnDestroy ()
	{
		m_BoatAmbienceEvent.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
	}
}
