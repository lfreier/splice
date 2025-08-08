using System.Collections;
using UnityEngine;

public class StartEnding : MonoBehaviour
{
	public GameObject endingScreenPrefab;
	private GameObject endingScreen;
	private bool hasSetActive = false;

	public float initialTimerLength = 20F;
	private float initialTimer;

	public float zoomRatio = 5F;
	public float zoomTarget = 20F;
	private bool zoomOut = false;
	public float zoomTimerLength = 10F;
	private float zoomTimer;

	public SoundScriptable footstepDirtScriptable;

	public Collider2D attachedCollider;
	private Camera mainCam;

	private PlayerHUD playerHUD;

	private void Start()
	{
		mainCam = Camera.main;
		hasSetActive = false;
	}

	void Update()
	{
		if (initialTimer != 0)
		{
			initialTimer -= Time.deltaTime;
			if (initialTimer <= 0)
			{
				initialTimer = 0;
				startZoom();
			}
		}

		if (zoomTimer != 0)
		{
			zoomTimer -= Time.deltaTime;
			if (zoomTimer <= 0)
			{
				zoomTimer = 0;
				//fade in credits
				endingScreen = Instantiate(endingScreenPrefab, GameManager.Instance.playerStats.playerHUD.transform.parent);
				endingScreen.SetActive(true);
			}
		}

		if (zoomOut)
		{
			mainCam.orthographicSize += zoomRatio * Time.deltaTime;
			if (mainCam != null && mainCam.orthographicSize >= zoomTarget)
			{
				zoomOut = false;
			}
		}
	}

	private void startZoom()
	{
		if (mainCam != null)
		{
			CameraHandler cameraHandler = mainCam.GetComponent<CameraHandler>();
			if (cameraHandler != null)
			{
				cameraHandler.lockCameraPanAmount();
			}
			zoomOut = true;
		}

		zoomTimer = zoomTimerLength;
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		Actor actorHit = collision.GetComponentInChildren<Actor>();
		if (actorHit != null && actorHit.tag == ActorDefs.playerTag)
		{
			initialTimer = initialTimerLength;
			attachedCollider.enabled = false;
			PlayerMove move = actorHit.GetComponentInChildren<PlayerMove>();
			if (move != null)
			{
				move.footstepScriptable = footstepDirtScriptable;
			}
		}
	}
}