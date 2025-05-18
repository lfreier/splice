
public class SceneDefs
{
	public enum SCENE
	{
		MAIN_MENU = 0,
		MANAGER = 1,
		PLAYER_HUD = 2,
		LOADING = 3,
		PAUSE = 4,
		GAME_OVER = 5,
		INVENTORY_HUD = 6,
		MUTATION_SELECT = 7,
		WIN = 8,
		SAVING = 9,
		TUTORIAL = 10,
		STATION_MENU = 11,
		TEMP12 = 12,
		TEMP13 = 13,
		TEMP14 = 14,
		TEMP15 = 15,
		TEMP16 = 16,
		TEMP17 = 17,
		TEMP18 = 18,
		TEMP19 = 19,
		LEVEL_START = 20,
		LEVEL_OFFICE = 21,
		LEVEL_HUB = 22,
		LEVEL_WAREHOUSE = 23,
		LEVEL_ARCH = 24
	}

	//index is SCENE index
	//value is build index
	public static int[] SCENE_INDEX_MASK = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 12, 13, 14, 15, 24 };

	//index is build index
	//value is SCENE index
	public static int[] SCENE_BUILD_MASK = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 20, 21, 22, 23, 16, 17, 18, 19, 20, 21, 22, 23, 24 };

	public static int[] BACKGROUND_SCENES = { SCENE_INDEX_MASK[(int)SCENE.MANAGER], SCENE_INDEX_MASK[(int)SCENE.PLAYER_HUD], SCENE_INDEX_MASK[(int)SCENE.LOADING], SCENE_INDEX_MASK[(int)SCENE.INVENTORY_HUD] };

	public static int NUM_SCENES = (int)SCENE.LEVEL_ARCH + 1;

	public static bool isLevelScene(SCENE toCheck)
	{
		if (toCheck >= SCENE.LEVEL_START)
		{
			return true;
		}
		return false;
	}

	/* these scenes should normally not be unloaded unless in specific scenarios */
	public static bool isValidUnload(int toCheck)
	{
		for (int i = 0; i < BACKGROUND_SCENES.Length; i ++)
		{
			if (toCheck == BACKGROUND_SCENES[i])
			{
				return false;
			}
		}
		return true;
	}
}