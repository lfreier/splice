using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using static PickupDefs;
using static UnityEngine.GraphicsBuffer;

public class Cell : MonoBehaviour, PickupInterface
{
	public int cellCount;
	public Rigidbody2D cellBody;
	public float maxSpeed = 10;
	public float attractForce = 100;
	public Animator animator;

	public AudioClip cellPopSound;
	public AudioSource cellAudioPlayer;

	[SerializeField]
	private Sprite icon;
	GameManager gameManager;

	// Use this for initialization
	void Start()
	{
		gameManager = GameManager.Instance;
		PickupDefs.setLayer(gameObject);
		init();
	}

	void FixedUpdate()
	{
		Actor target = null;
		//TODO: fly towards player if close

		RaycastHit2D[] nearbyActors = Physics2D.CircleCastAll(transform.position, CELL_ATTRACT_RANGE, transform.right, CELL_ATTRACT_RANGE, gameManager.actorLayers);
		foreach (RaycastHit2D nearby in nearbyActors)
		{
			if ((target = nearby.transform.GetComponent<Actor>()) != null)
			{
				if (target.tag == ActorDefs.playerTag)
				{
					break;
				}
				target = null;
			}
		}

		if (target != null)
		{
			Vector3 targetDiff = target.transform.position - transform.position;
			if (targetDiff.magnitude < CELL_ATTRACT_RANGE)
			{
				cellBody.velocity = (Vector2)(target.transform.position - (cellBody.transform.position + (Vector3)cellBody.centerOfMass)) * attractForce * Time.deltaTime;
			}
		}
		
		if (cellBody.velocity.magnitude != 0)
		{
			if (cellBody.velocity.magnitude > 0)
			{
				cellBody.velocity = cellBody.velocity / 1.005f;
			}
		}
	}

	public void destroyCell()
	{
		Destroy(this.gameObject);
	}

	public void generateCount(int min, int max)
	{
		cellCount = Random.Range(min, max);
	}

	public int getCount()
	{
		return cellCount;
	}

	public Sprite getIcon()
	{
		return icon;
	}


	public pickupType getPickupType()
	{
		return pickupType.CELL;
	}

	public void init()
	{

	}

	public void pickup(Actor actorTarget)
	{
		gameManager.playerStats.addItem(this);
		animator.SetTrigger(CELL_ANIM_TRIGGER);
	}
	
	private void OnTriggerEnter2D(Collider2D collision)
	{
		Actor target;
		if ((target = collision.transform.GetComponent<Actor>()) != null)
		{
			if (target.tag == ActorDefs.playerTag)
			{
				pickup(target);
				AudioClip toPlay;
				if (cellPopSound != null)
				{
					gameManager.audioManager.soundHash.TryGetValue(cellPopSound.name, out toPlay);
					if (toPlay != null)
					{
						cellAudioPlayer.PlayOneShot(toPlay);
					}
				}
			}
		}
	}
}