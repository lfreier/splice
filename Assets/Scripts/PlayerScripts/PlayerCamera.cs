using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using static GameManager;

public class PlayerCamera : MonoBehaviour
{
	public Actor player;
	private CameraHandler camHandler;
	public PlayerInput unityInput;

	//private bool rotateLocked;
	private bool init;

	private float moveCamInput;

	private float lastAimAngle;

	public PlayerInputs inputs;

	// Use this for initialization
	void Start()
	{
		init = false;
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
		if (!init && player.gameManager != null)
		{
			player.gameManager.rotationLockedEvent += lockRotation;
			player.gameManager.rotationUnlockedEvent += unlockRotation;
			init = true;
		}
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

		player.actorBody.MoveRotation(aimAngle);
		lastAimAngle = aimAngle;
	}

	private void lockRotation()
	{
		player.actorBody.MoveRotation(lastAimAngle);
		//rotateLocked = true;
	}

	private void unlockRotation()
	{
		//rotateLocked = false;
	}
}