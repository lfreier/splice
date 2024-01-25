using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
	private float moveCamInput;
	private Vector2 pointerPos;


	public Actor player;
	public CameraHandler camHandler;


	[SerializeField]
	private InputActionReference moveCam, pointer;

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
		Vector2 mousePos = pointer.action.ReadValue<Vector2>();
		pointerPos = Camera.main.ScreenToWorldPoint(mousePos);
		moveCamInput = moveCam.action.ReadValue<float>();

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
		Vector2 aimDir = pointerPos - player.actorBody.position;
		float aimAngle = (Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg) - 90F;
		player.actorBody.rotation = aimAngle;
	}
}