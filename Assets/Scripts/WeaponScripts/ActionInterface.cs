using System.Collections;
using UnityEngine;

public interface ActionInterface
{
	public void action();
	public bool toggleHitbox();

	public void setActorToHold(Actor actor);
}