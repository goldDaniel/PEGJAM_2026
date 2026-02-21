
using UnityEngine;

[CreateAssetMenu(fileName = "FoodTemplate", menuName = "Scriptable Objects/Contestant")]
public class ContestantTemplate : ScriptableObject
{
	public string Name;
	public Sprite EntryImage;

	public Sprite EatingLeft;
	public Sprite EatingRight;
	public Sprite EatingUp;
	public Sprite EatingDown;
	public Sprite Empty;
	public Sprite Reaching;
	public Sprite Damage;
}

