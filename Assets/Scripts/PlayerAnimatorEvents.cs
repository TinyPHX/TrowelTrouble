using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimatorEvents : MonoBehaviour
{
	[SerializeField] private PlayerController player;
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	public void Slash() 
	{
		Debug.Log("slash");
		player.TrowelSlashStart();
	}
	
	public void SlashEnd() 
	{
		Debug.Log("slash");
		player.TrowelSlashEnd();
	}
}
