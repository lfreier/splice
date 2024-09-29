using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
	public Actor player;
	private CameraHandler camHandler;
	public PlayerInput unityInput;

	private float moveCamInput;

	public PlayerInputs inputs;

	// Use this for initialization
	void Start()
	{
		foreach (Camera cam in FindObjectsByType<Camera>(FindObjectsSortMode.None))
		{
			CameraHandler script = cam.gameObject.GetComponent<CameraHandler>();
			if (script != null)
			{
				camHandler = script;
				unityInput.camera = cam;
				break;
			}
		}
	}

	// Update is called once per frame
	void Update()
	{
		moveCamInput = inputs.moveCamInput();
		cameraInputs();
	}

	private void FixedUpdate()
	{
	}
	public void cameraInputs()
	{
		// camera logic for 'look' input
		if (moveCamInput > 0)
		{
			camHandler.setCamFollowPlayer(false);
		}
		else
		{
			camHandler.setCamFollowPlayer(true);
		}

		// aim at pointer
		Vector2 aimDir = inputs.pointerPos() - player.actorBody.position;
		float aimAngle = (Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg) - 90F;
		player.actorBody.rotation = aimAngle;
	}
}