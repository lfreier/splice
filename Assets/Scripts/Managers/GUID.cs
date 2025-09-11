// Script for generating a unique but persistent string identifier belonging to this 
// component
//
// We construct the identifier from two parts, the scene name and a guid.
// 
// The guid is guaranteed to be unique across all components loaded at 
// any given time. In practice this means the ID is unique within this scene. We 
// then append the name of the scene to it. This ensures that the identifier will be 
// unique accross all scenes. (as long as your scene names are unique)
// 
// The identifier is serialised ensuring it will remaing the same when the level is 
// reloaded
//
// This code copes with copying the game object we are part of, using prefabs and 
// additive level loading
//
// Final point - After adding this Component to a prefab, you need to open all the 
// scenes that contain instances of this prefab and resave them (to save the newly 
// generated identifier). I recommend manually saving it rather than just closing it
// and waiting for Unity to prompt you to save it, as this automatic mechanism 
// doesn't always seem to know exactly what needs saving and you end up being re-asked
// incessantly
//
// Written by Diarmid Campbell 2017 - feel free to use and ammend as you like
//
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;


#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[ExecuteInEditMode]
public class GUID : MonoBehaviour
{

	// global lookup of IDs to Components - we can esnure at edit time that no two 
	// components which are loaded at the same time have the same ID. 
	static Dictionary<string, GUID> allGuids = new Dictionary<string, GUID>();

	public string guid;

	public int basicPrefabIndex;

	private async void Start()
	{
		if (System.String.IsNullOrEmpty(guid))
		{
			generate();
		}

		if (!System.String.IsNullOrEmpty(guid))
		{
			GameManager gm = GameManager.Instance;
			while (gm == null)
			{
				await Task.Delay(50);
				gm = GameManager.Instance;
			}

			if (!gm.levelManager.guidTable.ContainsKey(guid))
			{
				GUIDWatcher newWatcher = new GUIDWatcher();
				newWatcher.guid = this;
				if (gameObject.GetComponent<Actor>() != null)
				{
					newWatcher.idType = LevelManager.idType.ACTOR;
				}
				else if (gameObject.GetComponent<AutoDoor>() != null)
				{
					newWatcher.idType = LevelManager.idType.DOOR;
				}
				else if (gameObject.GetComponent<PickupInterface>() != null)
				{
					newWatcher.idType = LevelManager.idType.BASIC;
				}
				else if (gameObject.GetComponentInChildren<PickupBox>() != null)
				{
					newWatcher.idType = LevelManager.idType.BOX;
				}
				else if (gameObject.GetComponentInChildren<BasicWeapon>() != null)
				{
					newWatcher.idType = LevelManager.idType.WEAPON;
				}
				else if (gameObject.GetComponentInChildren<TutorialSceneLoader>() != null)
				{
					newWatcher.idType = LevelManager.idType.BASIC;
				}
				else if (gameObject.GetComponentInChildren<Corpse>() != null)
				{
					newWatcher.idType = LevelManager.idType.BASIC;
				}
				else
				{
					return;
				}
				gm.levelManager.guidTable.Add(guid, newWatcher);
			}
		}

	}

	//this is so dumb
	private void generate()
	{
		// Construct the name of the scene with an underscore to prefix to the Guid
		string sceneName = gameObject.scene.name + "_";

		// if we are not part of a scene then we are a prefab so do not attempt to set 
		// the id
		if (sceneName == null) return;

		// Test if we need to make a new id
		bool hasSceneNameAtBeginning = (guid != null &&
			guid.Length > sceneName.Length &&
			guid.Substring(0, sceneName.Length) == sceneName);

		bool anotherComponentAlreadyHasThisID = (guid != null &&
			allGuids.ContainsKey(guid) &&
			allGuids[guid] != this);

		if (!hasSceneNameAtBeginning || anotherComponentAlreadyHasThisID)
		{
			guid = sceneName + Guid.NewGuid();
		}
		// We can be sure that the key is unique - now make sure we have 
		// it in our list
		if (!allGuids.ContainsKey(guid))
		{
			allGuids.Add(guid, this);
		}
	}

	// Only compile the code in an editor build
#if UNITY_EDITOR

	// Whenever something changes in the editor (note the [ExecuteInEditMode])
	void Update()
	{
		// Don't do anything when running the game
		if (Application.isPlaying)
			return;

		generateEditor();
	}

	void generateEditor()
	{
		// Construct the name of the scene with an underscore to prefix to the Guid
		string sceneName = gameObject.scene.name + "_";

		// if we are not part of a scene then we are a prefab so do not attempt to set 
		// the id
		if (sceneName == null) return;

		// Test if we need to make a new id
		bool hasSceneNameAtBeginning = (guid != null &&
			guid.Length > sceneName.Length &&
			guid.Substring(0, sceneName.Length) == sceneName);

		bool anotherComponentAlreadyHasThisID = (guid != null &&
			allGuids.ContainsKey(guid) &&
			allGuids[guid] != this);

		if (!hasSceneNameAtBeginning || anotherComponentAlreadyHasThisID)
		{
			guid = sceneName + Guid.NewGuid();

			EditorUtility.SetDirty(this);
			EditorSceneManager.MarkSceneDirty(gameObject.scene);
		}
		// We can be sure that the key is unique - now make sure we have 
		// it in our list
		if (!allGuids.ContainsKey(guid))
		{
			allGuids.Add(guid, this);
		}
	}

#endif

	// When we get destroyed (which happens when unloading a level)
	// we must remove ourselves from the global list otherwise the
	// entry still hangs around when we reload the same level again
	// but now the THIS pointer has changed and end up changing 
	// our ID
	void OnDestroy()
	{
		if (GameManager.Instance != null)
		{
			GameManager.Instance.levelManager.guidTable.Remove(guid);
		}
		allGuids.Remove(guid);
	}
}