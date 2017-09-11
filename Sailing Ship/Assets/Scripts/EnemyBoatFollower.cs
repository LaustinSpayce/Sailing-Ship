using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBoatFollower : MonoBehaviour {

	public Transform player;
	
	private NavMeshAgent m_Nav;

	// Use this for initialization
	void Start () {

		m_Nav = GetComponent<NavMeshAgent>();
		
	}
	
	// Update is called once per frame
	void Update () {

		m_Nav.destination = player.position;
		
	}
}
