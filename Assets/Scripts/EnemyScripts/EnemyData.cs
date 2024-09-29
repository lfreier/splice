using System.Collections;
using UnityEngine;
using UnityEngine.U2D;


public class EnemyData : MonoBehaviour
{
	public Actor actor;

	public int cellDropMin = 1;
	public int cellDropMax = 1;

	public Sprite[] spriteList;
	public Sprite[] corpseSpriteList;

	private SpriteRenderer sprite;


	// Use this for initialization
	void Start()
	{
		sprite = GetComponent<SpriteRenderer>();

		int i = Random.Range(0, spriteList.Length);
		sprite.sprite = spriteList[i];

		if (i < corpseSpriteList.Length)
		{
			actor.corpseSprite = corpseSpriteList[i];
		}
	}
}