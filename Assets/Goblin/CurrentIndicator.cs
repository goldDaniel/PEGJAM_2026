using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CurrentIndicator : MonoBehaviour
{
	private Vector2 _origin;

	public UIAnimator animator;
	public UIAnimationClip clip;

	private UIAnimHandle _handle;

	[SerializeField] private Image _image;
	[SerializeField] private float intensity;

	void Awake()
	{
		Play();
		_origin = transform.position;
	}

	public void Play() => _handle = animator.Play(clip);
	public void Stop() => _handle?.Stop();

	public void MissedInput()
	{
		StartCoroutine(AnimateColor());
	}

	public void Update()
	{
		if (!Game.Instance.OnPunishmentCooldown)
			return;

		float shakeIntensity = Game.Instance.CooldownPercentage;
		Vector2 shakeOffset = intensity * (new Vector2(
													Mathf.PerlinNoise1D(456 + Time.time * 16),
													Mathf.PerlinNoise1D(789 + Time.time * 16)) * 2f - Vector2.one);

		transform.position = Vector3.Lerp(_origin, _origin + shakeOffset, shakeIntensity * shakeIntensity);
	}

	private IEnumerator AnimateColor()
	{
		while (Game.Instance.OnPunishmentCooldown)
		{
			_image.color = Color.Lerp(Color.white, Color.red, Game.Instance.CooldownPercentage);
			yield return null;
		}
		_image.color = Color.white;
	}
}
