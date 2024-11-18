
public class SceneDefs
{
	public static int MAIN_MENU_SCENE		= 0;
	public static int MANAGER_SCENE			= 1;
	public static int PLAYER_HUD_SCENE		= 2;
	public static int LOADING_SCENE			= 3;
	public static int PAUSE_SCENE			= 4;
	public static int GAME_OVER_SCENE		= 5;
	public static int MUTATION_SELECT_SCENE	= 6;
	public static int WIN_SCENE				= 7;
	public static int LEVEL_START_SCENE		= 8;
	public static int LEVEL_OFFICE_SCENE	= 9;

	public static int[] BACKGROUND_SCENES = {MANAGER_SCENE, PLAYER_HUD_SCENE, LOADING_SCENE};

	public static bool isLevelScene(int toCheck)
	{
		if (toCheck == LEVEL_START_SCENE || toCheck == LEVEL_OFFICE_SCENE)
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