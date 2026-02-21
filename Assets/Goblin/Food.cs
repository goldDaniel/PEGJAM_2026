
using System;
using UnityEngine;

public class Food : MonoBehaviour
{
	[NonSerialized]
	public FoodTemplate template;

	public void Init(FoodTemplate template)
	{
		this.template = template;
		var sr = GetComponentInChildren<SpriteRenderer>();
		sr.sprite = template.sprite;
	}
}