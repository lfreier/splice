using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MutationSelect : MonoBehaviour
{
	public GameObject[] mutationPrefabs;

	private PlayerInteract interact;

	private CameraHandler camHandler;

	private AsyncOperation op;

	private void Start()
	{
	}

	private void Update()
	{
		if (op != null && op.isDone)
		{
			foreach (MutationSelectHUD selectHud in FindObjectsByType<MutationSelectHUD>(FindObjectsSortMode.None))
			{
				selectHud.showMutationSelect(this);
				op = null;
				break;
			}
		}
	}

	public void activateSelectMenu(PlayerInteract player)
	{
		interact = player;
		camHandler = Camera.main.transform.GetComponent<CameraHandler>();
		if (camHandler != null)
		{
			camHandler.stopCam(true);
		}
		op = SceneManager.LoadSceneAsync(SceneDefs.MUTATION_SELECT_SCENE, LoadSceneMode.Additive);
		op.allowSceneActivation = true;
	}

	public void makeSelection(MutationInterface selected)
	{
		foreach (GameObject mutObj in mutationPrefabs)
		{
			MutationInterface mut = mutObj.GetComponentInChildren<MutationInterface>();
			if (mut != null && mut.getId() == selected.getId())
			{
				interact.equipMutation(mut);
				break;
			}
		}

		if (camHandler != null)
		{
			camHandler.stopCam(false);
		}

		Destroy(this.gameObject);
	}
}