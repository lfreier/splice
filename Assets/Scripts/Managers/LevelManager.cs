using System.Collections;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
	private bool hasPower;

	private void Awake()
	{
		//set starting power
	}

	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{

	}

	void setPower(bool powerToSet)
	{
		hasPower = powerToSet;
	}
}