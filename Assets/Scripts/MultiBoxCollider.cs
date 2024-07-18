using System.Collections;
using UnityEngine;

public interface MultiBoxCollider
{
	public void colliderEnter(Collider2D collision, MultiBoxCollider childScript);
}