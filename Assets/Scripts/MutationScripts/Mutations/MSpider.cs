using System.Collections;
using UnityEngine;
using static UnityEngine.UI.Image;

public class MSpider : MonoBehaviour, MutationInterface
{
	public Animator anim;

	public Actor actorWielder;

	public Sprite icon;

	public MutationScriptable mutationScriptable;

	public GameObject projectilePrefab;

	public Vector3 shootOffset;

	private PlayerInputs playerIn;

	private GameManager gameManager;

	private void OnDestroy()
	{
		GameManager gm = GameManager.Instance;
		gm.playerAbilityEvent -= abilityInputPressed;
	}

	private void abilityInputPressed()
	{
		if (mutationScriptable.mutCost <= gameManager.playerStats.getMutationBar())
		{
			anim.SetTrigger(MutationDefs.TRIGGER_SPIDER_SHOOT);
			gameManager.playerStats.changeMutationBar(-mutationScriptable.mutCost);
		}
	}

	public void shootLeft()
	{
		Vector2 origin = new Vector2(-shootOffset.x, shootOffset.y);
		shoot(this.transform.TransformPoint(origin));
	}

	public void shootRight()
	{
		Vector2 origin = new Vector2(shootOffset.x, shootOffset.y);
		shoot(this.transform.TransformPoint(origin));
	}

	public void shoot(Vector2 origin)
	{
		Vector2 point = origin;
		if (playerIn != null)
		{
			point = playerIn.pointerPos();
		}
		GameObject projectileObj = Instantiate(projectilePrefab, origin, Quaternion.identity, null);
		Projectile projectile = projectileObj.GetComponentInChildren<Projectile>();
		if (projectile != null)
		{
			projectile.launch(origin, point, actorWielder);
		}
	}

	public string getDisplayName()
	{
		return MutationDefs.NAME_SPIDER;
	}
	public Sprite getIcon()
	{
		return icon;
	}
	public string getId()
	{
		return "MSpider";
	}

	public mutationTrigger getMutationType()
	{
		return mutationTrigger.ACTIVE_SLOT;
	}

	public void trigger(Actor actorTarget)
	{
	}

	public MutationInterface mEquip(Actor actor)
	{
		gameManager = GameManager.Instance;
		gameManager.playerAbilityEvent += abilityInputPressed;
		setWielder(actor);

		return this;
	}
	public void setStartingPosition()
	{
	}

	public void setWielder(Actor wielder)
	{
		actorWielder = wielder;
		playerIn = actorWielder.GetComponentInChildren<PlayerInputs>();
	}

	Sprite[] MutationInterface.getTutorialSprites()
	{
		return null;
	}
}