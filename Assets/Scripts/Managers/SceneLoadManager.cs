using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using static SceneDefs;

public class SceneLoadManager
{
	public event Action<int> OnSceneLoaded = delegate { };
	public event Action<int> OnSceneUnloaded = delegate { };
	public event Action OnSceneGroupLoaded = delegate { };

	public int[] activeScenes;

	public async Task LoadScenes(int[] scenes, IProgress<float> progress, bool reloadDupScenes = false)
	{
		int i = 0;
		AsyncOperationGroup operationGroup = new AsyncOperationGroup(scenes.Length);

		for (i = 0; i < scenes.Length; i++)
		{
			operationGroup.operations.Add(SceneManager.LoadSceneAsync(scenes[i], LoadSceneMode.Additive)); 
			OnSceneLoaded.Invoke(scenes[i]);
		}

		while (!operationGroup.finished)
		{
			progress?.Report(operationGroup.progress);
			await Task.Delay(100);
		}

		for (i = 0; i < scenes.Length; i++)
		{
			if (isLevelScene(scenes[i]))
			{
				SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(scenes[i]));
				break;
			}
		}

		OnSceneGroupLoaded.Invoke();
	}

	public async Task UnloadScenes(bool unloadBgScenes)
	{
		int[] sceneArr = new int[SceneManager.sceneCount];
		for (int i = 0; i < sceneArr.Length; i ++)
		{
			sceneArr[i] = SceneManager.GetSceneAt(i).buildIndex;
		}
		await UnloadScenes(unloadBgScenes, sceneArr);
	}

	public async Task UnloadScenes(bool unloadBgScenes, int[] scenes)
	{
		List<int> sceneList = new List<int>();

		for (int i = scenes.Length - 1; i >= 0; i--)
		{
			Scene sceneAt = SceneManager.GetSceneByBuildIndex(scenes[i]);
			if (!sceneAt.isLoaded)
			{
				continue;
			}

			if (sceneAt.buildIndex == (int)SCENE.MANAGER || (!unloadBgScenes && !isValidUnload(sceneAt.buildIndex)))
			{
				continue;
			}

			sceneList.Add(sceneAt.buildIndex);
		}

		AsyncOperationGroup operationGroup = new AsyncOperationGroup(sceneList.Count);

		foreach (int scene in sceneList)
		{
			AsyncOperation op = SceneManager.UnloadSceneAsync(scene);
			if (op == null)
			{
				continue;
			}

			operationGroup.operations.Add(op);
			OnSceneUnloaded.Invoke(scene);
		}

		while (!operationGroup.finished)
		{
			await Task.Delay(100);
		}
	}

	public readonly struct AsyncOperationGroup
	{
		public readonly List<AsyncOperation> operations;

		public float progress => operations.Count == 0 ? 0 : operations.Average(o => o.progress);
		public bool finished => operations.All(o => o.isDone);

		public AsyncOperationGroup (int initialCapacity)
		{
			operations = new List<AsyncOperation> (initialCapacity);
		}
	}
}