using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrowelController : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	private void OnTriggerEnter(Collider other)
	{
		Rigidbody otherRigidbody = other.attachedRigidbody;

		if (otherRigidbody != null)
		{
			EnemyBrick enemyBrick = otherRigidbody.GetComponent<EnemyBrick>();

			if (enemyBrick != null)
			{
				enemyBrick.Hit();
			}
		}
	}
}
