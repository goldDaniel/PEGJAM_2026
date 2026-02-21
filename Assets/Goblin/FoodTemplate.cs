using System;
using UnityEngine;

[Serializable]
public struct InputCombo
{
	public GameInput[] inputs;
}

[CreateAssetMenu(fileName = "FoodTemplate", menuName = "Scriptable Objects/FoodTemplate")]
public class FoodTemplate : ScriptableObject
{
	[Header("Sprite")]
	public Sprite sprite;

	[Header("Input Sequence")]
	public InputCombo[] inputSequence;

	[Header("Cycles")]
	[Min(1)]
	public int cycles = 1;

	public InputCombo[] GetInputSequence()
	{
		InputCombo[] seq = new InputCombo[cycles * inputSequence.Length];
		for (int i = 0; i < seq.Length; i++)
		{
			seq[i] = inputSequence[i % inputSequence.Length];
		}

		return seq;
	}
}
