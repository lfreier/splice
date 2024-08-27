using System.Collections;
using UnityEngine;
using UnityEngine.U2D;


public class EnemyData : MonoBehaviour
{
	public Actor actor;

	public Sprite[] spriteList;

	private SpriteRenderer sprite;


	// Use this for initialization
	void Start()
	{
		sprite = GetComponent<SpriteRenderer>();

		int i = Random.Range(0, spriteList.Length);
		sprite.sprite = spriteList[i];
	}

	// Update is called once per frame
	void Update()
	{

	}
}