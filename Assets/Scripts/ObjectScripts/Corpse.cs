using System.Collections;
using UnityEngine;
using static ActorDefs;

public class Corpse : MonoBehaviour
{
	public ActorData actorData;

	public SpriteRenderer corpseSprite;

	public corpseType type;

	public Collider2D pickupCollider;
}