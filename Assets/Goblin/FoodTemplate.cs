using UnityEngine;
using UnityEngine.InputSystem;

public struct KeyCombo
{
    public Key[] keys;
}

[CreateAssetMenu(fileName = "FoodTemplate", menuName = "Scriptable Objects/FoodTemplate")]
public class FoodTemplate : ScriptableObject
{
    [Header("Sprite")]
    public Sprite sprite;

    [Header("Key Sequence")]
    public KeyCombo[] keySequence;
}
