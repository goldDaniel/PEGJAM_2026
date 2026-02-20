using UnityEngine;

public sealed class UIAnimationTarget : MonoBehaviour
{
	[SerializeField] private RectTransform _rectTransform;
	[SerializeField] private CanvasGroup _canvasGroup;

	private Vector2 _baseSizeDelta;
	private Vector2 _baseRectSize;
	private Vector2 _baseOffsetMin;
	private Vector2 _baseOffsetMax;

	private void Awake()
	{
		if (_rectTransform == null)
			_rectTransform = GetComponent<RectTransform>();

		if (_canvasGroup == null)
			_canvasGroup = GetComponent<CanvasGroup>();

		CacheBase();
	}

	// If you have layouts that change after Awake, call this after layout settles.
	// e.g. Start() or after enabling / rebuilding a layout group.
	public void CacheBase()
	{
		_baseSizeDelta = _rectTransform.sizeDelta;
		_baseRectSize = _rectTransform.rect.size;
		_baseOffsetMin = _rectTransform.offsetMin;
		_baseOffsetMax = _rectTransform.offsetMax;
	}

	public Vector2 GetAnchoredPosition() => _rectTransform.anchoredPosition;
	public Vector3 GetPosition() => _rectTransform.position;

	public Vector2 GetScale()
	{
		// For non-stretch this reads nicely as size relative to base rect size.
		// For stretch it’s not strictly meaningful (size depends on parent),
		// but this preserves your previous intent as best as possible.
		var s = _rectTransform.rect.size;
		return new Vector2(
			_baseRectSize.x != 0 ? s.x / _baseRectSize.x : 1f,
			_baseRectSize.y != 0 ? s.y / _baseRectSize.y : 1f
		);
	}

	public float GetAlpha() => _canvasGroup != null ? _canvasGroup.alpha : 1f;

	public void SetAnchoredPosition(Vector2 value) => _rectTransform.anchoredPosition = value;
	public void SetPosition(Vector3 value) => _rectTransform.position = value;

	public void SetScale(Vector2 value)
	{
		bool stretchX = !Mathf.Approximately(_rectTransform.anchorMin.x, _rectTransform.anchorMax.x);
		bool stretchY = !Mathf.Approximately(_rectTransform.anchorMin.y, _rectTransform.anchorMax.y);

		// If the rect is stretch-anchored, size is controlled by offsets, not sizeDelta.
		// We scale offsets around their midpoint so the rect stays centered relative to its anchors.
		if (stretchX)
		{
			float left = _baseOffsetMin.x;     // distance from left anchor edge
			float right = -_baseOffsetMax.x;    // distance from right anchor edge (note sign)
			float mid = (left - right) * 0.5f; // midpoint in "offset space"
			float half = (left + right) * 0.5f; // half-extent in "offset space"

			half *= value.x;

			float newLeft = mid + half;
			float newRight = -(mid - half);

			_rectTransform.offsetMin = new Vector2(newLeft, _rectTransform.offsetMin.y);
			_rectTransform.offsetMax = new Vector2(newRight, _rectTransform.offsetMax.y);
		}
		else
		{
			_rectTransform.sizeDelta = new Vector2(_baseSizeDelta.x * value.x, _rectTransform.sizeDelta.y);
		}

		if (stretchY)
		{
			float bottom = _baseOffsetMin.y;
			float top = -_baseOffsetMax.y;
			float mid = (bottom - top) * 0.5f;
			float half = (bottom + top) * 0.5f;

			half *= value.y;

			float newBottom = mid + half;
			float newTop = -(mid - half);

			_rectTransform.offsetMin = new Vector2(_rectTransform.offsetMin.x, newBottom);
			_rectTransform.offsetMax = new Vector2(_rectTransform.offsetMax.x, newTop);
		}
		else
		{
			_rectTransform.sizeDelta = new Vector2(_rectTransform.sizeDelta.x, _baseSizeDelta.y * value.y);
		}
	}

	public void SetAlpha(float alpha)
	{
		if (_canvasGroup != null)
			_canvasGroup.alpha = Mathf.Clamp01(alpha);
	}
}