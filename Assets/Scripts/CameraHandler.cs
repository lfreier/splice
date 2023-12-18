using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraHandler : MonoBehaviour
{
	GameObject player;
	bool followPlayer;
	Vector2 pointerPos;

	static float cameraSmoothing = 0.4F;

	float widthRatio = (float)Screen.width / (float)Screen.height;
	float heightRatio = (float)Screen.height / (float)Screen.width;
	float verExtent;
	float horExtent;

	[SerializeField]
	private InputActionReference pointerPosition;

	void Start()
	{
		followPlayer = true;
		player = GameObject.Find("Player");
		verExtent = Camera.main.orthographicSize;
		horExtent = verExtent * widthRatio;
		Update();
	}

	void Update()
	{
		Vector3 mousePos = pointerPosition.action.ReadValue<Vector2>();
		mousePos.z = Camera.main.nearClipPlane;
		pointerPos = Camera.main.ScreenToWorldPoint(mousePos);

		if (followPlayer)
		{
			camFollowPlayer();
		}
		else
		{
			camFollowPointer();
		}
	}

	public void setCamFollowPlayer(bool toSet)
	{
		followPlayer = toSet;
	}

	void camFollowPlayer()
	{
		float newXPos = (player.transform.position.x + pointerPos.x * 0.5F) / 2F;
		float newYPos = (player.transform.position.y + pointerPos.y * 0.5F) / 2F;
		//Keep camera's z position
		Vector3 newPos = new Vector3(
			Mathf.Clamp(newXPos, player.transform.position.x - (horExtent / 3F), player.transform.position.x + (horExtent / 3F)), 
			Mathf.Clamp(newYPos, player.transform.position.y - (verExtent / 3F), player.transform.position.y + (verExtent / 3F)), 
			this.transform.position.z);

		this.transform.position = Vector3.Lerp(this.transform.position, newPos, cameraSmoothing);
	}

	void camFollowPointer()
	{
		float newXPos = Mathf.Clamp((player.transform.position.x + pointerPos.x) / 2F, player.transform.position.x - horExtent, player.transform.position.x + horExtent);
		float newYPos = Mathf.Clamp((player.transform.position.y + pointerPos.y) / 2F, player.transform.position.y - verExtent, player.transform.position.y + verExtent);
		this.transform.position = Vector3.Lerp(this.transform.position, new Vector3(newXPos, newYPos, this.transform.position.z), cameraSmoothing);
	}
}