using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Stage", menuName = "Scriptable Objects/Stage")]
public class Level : ScriptableObject
{
	public string opponentName;	
	public Sprite opponentImage;

	public List<FoodTemplate> foodItems;
}
