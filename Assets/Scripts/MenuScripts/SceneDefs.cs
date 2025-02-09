
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
		LEVEL_START = 10,
		LEVEL_OFFICE = 11
	}

	public static int[] BACKGROUND_SCENES = { (int)SCENE.MANAGER, (int)SCENE.PLAYER_HUD, (int)SCENE.LOADING, (int)SCENE.INVENTORY_HUD };

	public static int NUM_SCENES = (int)SCENE.LEVEL_OFFICE + 1;

	public static bool isLevelScene(int toCheck)
	{
		if (toCheck >= (int)SCENE.LEVEL_START)
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