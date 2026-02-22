
using UnityEngine;

[CreateAssetMenu(fileName = "FoodTemplate", menuName = "Scriptable Objects/Contestant")]
public class ContestantTemplate : ScriptableObject
{
	public string Name;
	public Sprite EntryImage;

	// how long it takes the contestant to finish their plate. Only applicable to NPCS
	[Range(5f, 30f)]
	public float EatingTime;

	// How long between switching eating animation frames
	[Range(0.01f, 1f)]
	public float BiteTime;

    [Range(0f, 1f)]
    public float attackRate;
    public int[] attackWeights;

    public Sprite EatingLeft;
	public Sprite EatingRight;
	public Sprite EatingUp;
	public Sprite EatingDown;
	public Sprite Empty;
	public Sprite Reaching;
	public Sprite Damage;
}

