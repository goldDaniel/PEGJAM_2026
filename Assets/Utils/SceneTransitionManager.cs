using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class SceneTransitionManager : MonoSingleton<SceneTransitionManager>
{
	[Serializable]
	public struct Settings
	{
		public float FadeOutDuration;
		public float FadeInDuration;
		public float HoldDuration;
		public Color FadeColor;
		
		public static Settings Default => new Settings
		{
			FadeOutDuration = 1.5f,
			FadeInDuration = 1.5f,
			HoldDuration = 0.5f,
			FadeColor = Color.black,
		};
	}


	private Canvas _canvas;
	private CanvasGroup _canvasGroup;
	private Image _fadeImage;
	private Coroutine _transitionRoutine;

	public bool IsTransitioning => _transitionRoutine != null;

	private void Awake()
	{
		if (HasInstance)
		{
			Destroy(gameObject);
			return;
		}

		DontDestroyOnLoad(gameObject);
		EnsureVisuals();
		ApplyEffectVisuals(0f, Settings.Default.FadeColor);
		_canvas.enabled = false;
	}

	public static void LoadScene(string sceneName)
	{
		Instance.BeginTransition(sceneName, Settings.Default);
	}

	public static void LoadScene(int sceneIndex)
	{
		Instance.BeginTransition(sceneIndex, Settings.Default);
	}

	public static void LoadScene(string sceneName, Settings settings)
	{
		Instance.BeginTransition(sceneName, SanitizeSettings(settings));
	}

	public static void LoadScene(int sceneIndex, Settings settings)
	{
		Instance.BeginTransition(sceneIndex, SanitizeSettings(settings));
	}

	private void BeginTransition(string sceneName, Settings settings)
	{
		if (string.IsNullOrWhiteSpace(sceneName))
			throw new ArgumentException("Scene name cannot be empty.", nameof(sceneName));

		StartTransition(() => SceneManager.LoadSceneAsync(sceneName), settings);
	}

	private void BeginTransition(int sceneIndex, Settings settings)
	{
		StartTransition(() => SceneManager.LoadSceneAsync(sceneIndex), settings);
	}

	private void StartTransition(Func<AsyncOperation> loadOperationFactory, Settings settings)
	{
		if (_transitionRoutine != null)
			StopCoroutine(_transitionRoutine);

		_transitionRoutine = StartCoroutine(TransitionRoutine(loadOperationFactory, settings));
	}

	private IEnumerator TransitionRoutine(Func<AsyncOperation> loadOperationFactory, Settings settings)
	{
		InputSystem.actions.Disable();
		EnsureVisuals();
		_canvas.enabled = true;

		yield return AnimateTransition(settings, fadeOut: true);

		if (settings.HoldDuration > 0f)
			yield return new WaitForSecondsRealtime(settings.HoldDuration);

		AsyncOperation loadOperation = loadOperationFactory.Invoke();
		yield return WaitForSceneLoadComplete(loadOperation);
		if (loadOperation == null)
		{
			_canvas.enabled = false;
			_transitionRoutine = null;
			yield break;
		}

		// Let the new scene finish one rendered frame before revealing
		yield return null;
		yield return AnimateTransition(settings, fadeOut: false);
		_canvas.enabled = false;
		_transitionRoutine = null;

		InputSystem.actions.Enable();
	}

	private static IEnumerator WaitForSceneLoadComplete(AsyncOperation loadOperation)
	{
		if (loadOperation == null)
		{
			Debug.LogError("Scene transition failed because the load operation was null.");
			yield break;
		}

		while (!loadOperation.isDone)
		{
			// TODO (danielg): animate loading graphic here based on loadOperation.progress if desired
			yield return null;
		}
	}

	private IEnumerator AnimateTransition(Settings settings, bool fadeOut)
	{
		float duration = fadeOut ? settings.FadeOutDuration : settings.FadeInDuration;
		if (duration <= 0f)
		{
			ApplyEffectVisuals(fadeOut ? 1f : 0f, settings.FadeColor);
			yield break;
		}

		float elapsed = 0f;
		while (elapsed < duration)
		{
			elapsed += Time.unscaledDeltaTime;
			float t = Mathf.Pow(Mathf.Clamp01(elapsed / duration), 2.2f);
			float progress = fadeOut ? t : 1f - t;
			ApplyEffectVisuals(progress, settings.FadeColor);
			yield return null;
		}

		ApplyEffectVisuals(fadeOut ? 1f : 0f, settings.FadeColor);
	}

	private void ApplyEffectVisuals(float progress, Color color)
	{
		progress = Mathf.Clamp01(progress);
		_fadeImage.color = color;
		_fadeImage.raycastTarget = true;
		SetFadeModeVisual(progress);
	}

	private void SetFadeModeVisual(float alpha)
	{
		_fadeImage.type = Image.Type.Simple;
		SetAlpha(alpha);
	}

	private static Settings SanitizeSettings(Settings settings)
	{
		settings.FadeOutDuration = Mathf.Max(0f, settings.FadeOutDuration);
		settings.FadeInDuration = Mathf.Max(0f, settings.FadeInDuration);
		settings.HoldDuration = Mathf.Max(0f, settings.HoldDuration);
		return settings;
	}

	private void EnsureVisuals()
	{
		if (_canvas != null && _canvasGroup != null && _fadeImage != null)
			return;

		_canvas = GetComponentInChildren<Canvas>(true);
		if (_canvas == null)
		{
			GameObject canvasObject = new GameObject("TransitionCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
			canvasObject.transform.SetParent(transform, false);
			_canvas = canvasObject.GetComponent<Canvas>();
			_canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			_canvas.sortingOrder = short.MaxValue;
			CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
			scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
			scaler.referenceResolution = new Vector2(1920f, 1080f) * 1.1f; // 10% pad 
		}

		_canvasGroup = _canvas.GetComponent<CanvasGroup>();
		if (_canvasGroup == null)
			_canvasGroup = _canvas.gameObject.AddComponent<CanvasGroup>();
		_canvasGroup.blocksRaycasts = true;
		_canvasGroup.interactable = false;

		_fadeImage = _canvas.GetComponentInChildren<Image>(true);
		if (_fadeImage == null)
		{
			GameObject imageObject = new GameObject("FadeImage", typeof(RectTransform), typeof(Image));
			imageObject.transform.SetParent(_canvas.transform, false);
			RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
			rectTransform.anchorMin = Vector2.zero;
			rectTransform.anchorMax = Vector2.one;
			rectTransform.offsetMin = Vector2.zero;
			rectTransform.offsetMax = Vector2.zero;
			_fadeImage = imageObject.GetComponent<Image>();
		}
	}

	private void SetAlpha(float alpha)
	{
		if (_canvasGroup != null)
			_canvasGroup.alpha = Mathf.Clamp01(alpha);
	}
}
