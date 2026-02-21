
using UnityEngine;

[CreateAssetMenu(fileName = "LevelLoader", menuName = "Scriptable Objects/LevelLoader")]
public class LevelLoader : ScriptableObject
{
	public static int CurrentLevelIndex = 0;
	public Level CurrentLevel => allLevels[CurrentLevelIndex];
	public bool IsLastLevel => CurrentLevelIndex == (allLevels.Length - 1);

	public Level[] allLevels;
}

