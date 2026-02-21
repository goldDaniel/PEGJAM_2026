
using System;
using System.Collections;
using UnityEngine;

public class Food : MonoBehaviour
{
	[NonSerialized]
	public FoodTemplate template;

	private Vector2 _origin;

    public void Init(FoodTemplate template)
	{
		this.template = template;
		var sr = GetComponentInChildren<SpriteRenderer>();
        sr.sprite = template.sprites[0];
	}

	private int GetSpriteIndex(float t)
	{
		float map = t * template.sprites.Length;
		return (int)MathF.Max(0, MathF.Min(template.sprites.Length - 1, map));
	}

	public void SetPosition(Transform holdLocation)
	{
		transform.SetParent(holdLocation, true);
		transform.position = holdLocation.position;
		_origin = transform.position;
	}

	public void Bite(float foodProgress)
	{
        var sr = GetComponentInChildren<SpriteRenderer>();
		sr.sprite = template.sprites[GetSpriteIndex(foodProgress)];
        StartCoroutine(AnimateBite(0.5f));
	}

	private IEnumerator AnimateBite(float duration)
	{
		float t = duration;
		while (t > 0)
		{
			float shakeIntensity = t / duration;
			Vector2 shakeOffset =	(new Vector2(
											Mathf.PerlinNoise1D(456 + Time.time * 16),
											Mathf.PerlinNoise1D(789 + Time.time * 16)) * 2f - Vector2.one);

			transform.position = Vector3.Lerp(_origin, _origin + shakeOffset, shakeIntensity * shakeIntensity);
			
			yield return null;
			t -= Time.deltaTime;
		}
		transform.position = _origin;
	}
}