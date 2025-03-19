using System.Collections;
using UnityEngine;

public class FanRotate : MonoBehaviour
{
	public SpriteRenderer sprite;
	// Update is called once per frame
	void Update()
	{
		sprite.transform.Rotate(Vector3.forward);
	}
}