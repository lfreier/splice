using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraHandler : MonoBehaviour
{
	public GameObject player;
	bool followPlayer;
	bool stop;
	Vector2 pointerPos;

	Vector3 cameraTarget;

	public float sizeTarget = 7.7F;

	float widthRatio = (float)Screen.width / (float)Screen.height;
	float heightRatio = (float)Screen.height / (float)Screen.width;
	float defaultWidth;
	float verExtent;
	float horExtent;

	[SerializeField]
	private InputActionReference pointerPosition;

	static Camera _MainCamera;
	public static Camera MainCamera
	{
		get
		{
			if (_MainCamera == null)
				_MainCamera = Camera.main;

			return _MainCamera;
		}
	}

	void Start()
	{
		player = GameObject.Find("Player");
		followPlayer = true;
		verExtent = Camera.main.orthographicSize;
		horExtent = verExtent * widthRatio;
		stop = false;
		Update();
	}

	void Update()
	{
		Vector3 mousePos = pointerPosition.action.ReadValue<Vector2>();
		mousePos.z = Camera.main.nearClipPlane;
		pointerPos = Camera.main.ScreenToWorldPoint(mousePos);
		verExtent = Camera.main.orthographicSize;
		widthRatio = (float)Screen.width / (float)Screen.height;
		horExtent = verExtent * widthRatio;

		if (stop || player == null)
		{
			cameraTarget = transform.position;
		}
		else if (followPlayer)
		{
			camFollowPlayer();
		}
		else
		{
			camFollowPointer();
		}

		this.transform.position = cameraTarget;

	}

	public void stopCam(bool toSet)
	{
		stop = toSet;
	}

	public void setCamFollowPlayer(bool toSet)
	{
		followPlayer = toSet;
	}

	void camFollowPlayer()
	{
		float newXPos = (3 * player.transform.position.x + pointerPos.x) / 4F;
		float newYPos = (3 * player.transform.position.y + pointerPos.y) / 4F;
		//Keep camera's z position
		Vector3 newPos = new Vector3(
			Mathf.Clamp(newXPos, player.transform.position.x - horExtent, player.transform.position.x + horExtent), 
			Mathf.Clamp(newYPos, player.transform.position.y - verExtent, player.transform.position.y + verExtent), 
			this.transform.position.z);
		//this.transform.position = Vector3.Lerp(this.transform.position, newPos, cameraSmoothing);
		cameraTarget = newPos;
	}

	void camFollowPointer()
	{
		float newXPos = Mathf.Clamp((player.transform.position.x + pointerPos.x) / 2F, player.transform.position.x - horExtent, player.transform.position.x + horExtent);
		float newYPos = Mathf.Clamp((player.transform.position.y + pointerPos.y) / 2F, player.transform.position.y - verExtent, player.transform.position.y + verExtent);
		cameraTarget = new Vector3(newXPos, newYPos, this.transform.position.z);
	}
}