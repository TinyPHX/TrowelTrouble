using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBrick : MonoBehaviour
{
	private NavMeshAgent navMeshAgent;
	private BrickTower brickTower;
	
	// Use this for initialization
	void Start ()
	{
		navMeshAgent = GetComponent<NavMeshAgent>();
		brickTower = FindObjectOfType<BrickTower>();
	}
	
	// Update is called once per frame
	void Update ()
	{
		navMeshAgent.SetDestination(brickTower.transform.position);
	}
}
