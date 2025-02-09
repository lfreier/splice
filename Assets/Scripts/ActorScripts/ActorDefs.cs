using UnityEngine;
using static ActorDefs;

public class ActorDefs
{
	public static string actorLayer = "Actor";
	public static string npcTag = "NPC";
	public static string teamMonsterTag = "TeamMonster";
	public static string teamScienceTag = "TeamScience";
	public static string playerTag = "Player";

	public static float GLOBAL_PICKUP_RANGE = 1F;
	public static float GLOBAL_PICKUP_OFFSET = 0.6F;

	public static float NPC_TRY_PICKUP_RANGE = 2F;

	public static float PLAYER_WALK_SPEED = 0.4F;

	public static float MAX_PARRY_FORCE= 2000F;
	public static float MAX_HIT_FORCE = 3000F;
	public struct ActorData
	{
		public float armor;
		public float health;
		public float maxHealth;
		public float shield;

		public float maxSpeed;
		public float moveSpeed;

		public float acceleration;
		public float deceleration;

		public float hearingRange;
		public float sightAngle;
		public float sightRange;

		public float frightenedDistance;
	};

	public enum detectMode
	{
		nul = -1,
		idle = 0,
		suspicious = 1,
		seeking = 2,
		lost = 3,
		hostile = 4,
		frightened = 5,
		getWeapon = 6,
		wandering = 7,
		forced = 8
	};

	public static ActorData copyData(ActorData toCopy)
	{
		ActorData toReturn = new();

		toReturn.health = toCopy.health;
		toReturn.maxHealth = toCopy.maxHealth;
		toReturn.shield = 0;

		toReturn.maxSpeed = toCopy.maxSpeed;
		toReturn.moveSpeed = toCopy.moveSpeed;
		toReturn.acceleration = toCopy.acceleration;
		toReturn.deceleration = toCopy.deceleration;

		toReturn.hearingRange = toCopy.hearingRange;
		toReturn.sightAngle = toCopy.sightAngle;
		toReturn.sightRange = toCopy.sightRange;

		toReturn.frightenedDistance = toCopy.frightenedDistance;

		return toReturn;
	}
}