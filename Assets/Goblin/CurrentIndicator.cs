using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CurrentIndicator : MonoBehaviour
{
	private RectTransform _rectTransform;
	private Vector2 _origin;

	public UIAnimator animator;
	public UIAnimationClip clip;

	private UIAnimHandle _handle;

	[SerializeField] private Image _image;
	[SerializeField] private float intensity;

	void Awake()
	{
		Play();
		_rectTransform = GetComponent<RectTransform>();
		_origin = _rectTransform.anchoredPosition;
	}

	public void Play() => _handle = animator.Play(clip);
	public void Stop() => _handle?.Stop();

	public void MissedInput() => StartCoroutine(AnimateMissedInput());
	public void CorrectInput() => StartCoroutine(AnimateCorrectInput());

	public void Update()
	{
		if (!Game.Instance.OnPunishmentCooldown)
		{
			_rectTransform.anchoredPosition = _origin;
			return;
		}
			
		float shakeIntensity = Game.Instance.CooldownPercentage;
		Vector2 shakeOffset = intensity * (new Vector2(
													Mathf.PerlinNoise1D(456 + Time.time * 16),
													Mathf.PerlinNoise1D(789 + Time.time * 16)) * 2f - Vector2.one);

		_rectTransform.anchoredPosition = Vector3.Lerp(_origin, _origin + shakeOffset, shakeIntensity * shakeIntensity);
	}

	private IEnumerator AnimateMissedInput()
	{
		while (Game.Instance.OnPunishmentCooldown)
		{
			_image.color = Color.Lerp(Color.white, Color.red, Game.Instance.CooldownPercentage);
			yield return null;
		}
		_image.color = Color.white;
	}

	private IEnumerator AnimateCorrectInput()
	{
		float tMax = 0.2f;
		float t = tMax;
		while (t > 0)
		{
			float interpolant = Mathf.Clamp01(t / tMax);
			_image.color = Color.Lerp(Color.white, Color.green, interpolant);
			yield return null;

			t -= Time.deltaTime;
		}
		_image.color = Color.white;
	}
}
