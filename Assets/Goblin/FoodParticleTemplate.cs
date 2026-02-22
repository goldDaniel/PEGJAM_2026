using System;
using UnityEngine;


[CreateAssetMenu(fileName = "FoodParticleTemplate", menuName = "Scriptable Objects/FoodParticleTemplate")]
public class FoodParticleTemplate : ScriptableObject
{
    [Header("Sprites")]
    public Sprite[] sprites;
}
