
using UnityEngine;

public class Food : MonoBehaviour
{
	public void Init(FoodTemplate template)
	{
		var sr = GetComponentInChildren<SpriteRenderer>();
		sr.sprite = template.sprite;
	}
}