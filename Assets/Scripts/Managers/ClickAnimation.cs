using System.Collections;
using UnityEngine;

public class ClickAnimation : MonoBehaviour
{
	public Animator anim;

	public void destroyClick()
	{
		Destroy(this.gameObject);
	}
}