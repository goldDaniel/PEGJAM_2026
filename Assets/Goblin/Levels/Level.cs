using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Stage", menuName = "Scriptable Objects/Stage")]
public class Level : ScriptableObject
{
	public ContestantTemplate opponent;

	public List<FoodTemplate> foodItems;
}
