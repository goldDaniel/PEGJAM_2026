using UnityEngine;

public sealed class UIAnimationTarget : MonoBehaviour
{
	[SerializeField]
	private RectTransform _rectTransform;

	[SerializeField]
	private CanvasGroup _canvasGroup;

	private Vector2 _baseSize;

	private void Awake()
	{
		if (_rectTransform == null)
			_rectTransform = GetComponent<RectTransform>();

		if (_canvasGroup == null)
			_canvasGroup = GetComponent<CanvasGroup>();

		_baseSize = _rectTransform.rect.size;
	}

	public Vector2 GetAnchoredPosition() => _rectTransform.anchoredPosition;
	public Vector3 GetPosition() => _rectTransform.position;
	public Vector2 GetScale() => _rectTransform.rect.size / _baseSize;
	public float GetAlpha() => _canvasGroup != null ? _canvasGroup.alpha : 1f;

	public void SetAnchoredPosition(Vector2 value) => _rectTransform.anchoredPosition = value;
	public void SetPosition(Vector3 value) => _rectTransform.position = value;

	public void SetScale(Vector2 value)
	{
		_rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, value.x * _baseSize.x);
		_rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, value.y * _baseSize.y);
	}

	public void SetAlpha(float alpha)
	{
		if (_canvasGroup != null)
			_canvasGroup.alpha = Mathf.Clamp01(alpha);
	}
}
