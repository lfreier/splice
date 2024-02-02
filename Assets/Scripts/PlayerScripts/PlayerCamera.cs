using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
	public Actor player;
	public CameraHandler camHandler;

	private float moveCamInput;

	public PlayerInputs inputs;

	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{
	}

	private void FixedUpdate()
	{
		cameraInputs();
	}
	public void cameraInputs()
	{
		moveCamInput = inputs.moveCamInput();

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