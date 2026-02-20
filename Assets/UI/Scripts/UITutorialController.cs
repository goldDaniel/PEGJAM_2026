using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using static UnityEngine.GraphicsBuffer;

public enum TutorialAdvanceMode
{
	AnyKey,
	SpecificKey,
	GameplayEvent,
	Manual,
}

[Serializable]
public sealed class UITutorialStep
{
	public string Id;
	public RectTransform HighlightTarget;
	public Vector2 HighlightPadding = new Vector2(16f, 16f);
	[TextArea(2, 6)] public string BodyText;
	public RectTransform TooltipAnchor;
	public TutorialAdvanceMode AdvanceMode = TutorialAdvanceMode.AnyKey;
	public Key RequiredKey = Key.Enter;
	public string RequiredEvent;
	public bool WaitForAnimationsBeforeAdvance = true;
}

public sealed class UITutorialController : MonoBehaviour
{
	private GameObject _tutorialRoot => gameObject;

	[Header("Visuals")]
	[SerializeField]
	private CanvasGroup _dimmerCanvasGroup;
	[SerializeField]
	private RectTransform _highlightFrame;
	[SerializeField]
	private RectTransform _tooltipPanel;
	[SerializeField]
	private TMP_Text _tooltipText;

	[Header("Animators")]
	[SerializeField]
	private UIAnimator _dimmerAnimator;
	[SerializeField]
	private UIAnimator _highlightAnimator;
	[SerializeField]
	private UIAnimator _tooltipAnimator;

	[Header("Clips")]
	[SerializeField]
	private UIAnimationClip _dimmerInClip;
	[SerializeField]
	private UIAnimationClip _dimmerOutClip;
	[SerializeField]
	private UIAnimationClip _tooltipInClip;
	[SerializeField]
	private UIAnimationClip _tooltipOutClip;
	[SerializeField]
	private UIAnimationClip _highlightPulseClip;

	[Header("Runtime Move Animation")]
	[SerializeField, Min(0.01f)]
	private float _moveDuration = 0.24f;
	[SerializeField]
	private UIEaseType _moveEase = UIEaseType.CubicOut;

	[SerializeField]
	private List<UITutorialStep> _steps = new List<UITutorialStep>();

	private int _currentStepIndex = -1;
	private string _lastReceivedEvent;
	private bool _isRunning;

	private UIAnimHandle _tooltipMoveHandle;
	private UIAnimHandle _highlightMoveHandle;

	public bool IsRunning => _isRunning;

	public void StartTutorial()
	{
		if (_steps.Count == 0)
			return;

		_isRunning = true;
		_currentStepIndex = 0;
		_lastReceivedEvent = null;

		if (_tutorialRoot != null)
			_tutorialRoot.SetActive(true);
		if (_dimmerAnimator != null && _dimmerInClip != null)
			_dimmerAnimator.Play(_dimmerInClip);

		ApplyStepVisuals(_steps[_currentStepIndex]);
	}

	public void StopTutorial()
	{
		if (!_isRunning)
			return;

		_isRunning = false;
		_currentStepIndex = -1;
		_lastReceivedEvent = null;

		if (_tooltipAnimator != null && _tooltipOutClip != null)
			_tooltipAnimator.Play(_tooltipOutClip);

		if (_dimmerAnimator != null && _dimmerOutClip != null)
		{
			_dimmerAnimator.Play(_dimmerOutClip, new UIAnimOptions
			{
				OnComplete = () =>
				{
					if (_tutorialRoot != null)
						_tutorialRoot.SetActive(false);
				},
			});
		}
		else if (_tutorialRoot != null)
		{
			_tutorialRoot.SetActive(false);
		}
	}

	public void NotifyEvent(string eventId)
	{
		_lastReceivedEvent = eventId;
	}

	public void AdvanceStep()
	{
		if (!_isRunning)
			return;

		_currentStepIndex++;
		if (_currentStepIndex >= _steps.Count)
		{
			StopTutorial();
			return;
		}

		ApplyStepVisuals(_steps[_currentStepIndex]);
	}

	private void Update()
	{
		if (!_isRunning)
			return;

		if (_currentStepIndex < 0 || _currentStepIndex >= _steps.Count)
			return;

		UITutorialStep step = _steps[_currentStepIndex];

		if (CanAdvance(step))
			AdvanceStep();
	}

	private bool CanAdvance(UITutorialStep step)
	{
		if (step.WaitForAnimationsBeforeAdvance)
		{
			if ((_tooltipMoveHandle != null && _tooltipMoveHandle.IsPlaying) ||
				(_highlightMoveHandle != null && _highlightMoveHandle.IsPlaying))
				return false;
		}

		Keyboard keyboard = Keyboard.current;
		switch (step.AdvanceMode)
		{
			case TutorialAdvanceMode.AnyKey:
				return keyboard != null && keyboard.anyKey.wasPressedThisFrame;
			case TutorialAdvanceMode.SpecificKey:
				if (keyboard == null)
					return false;
				KeyControl keyControl = keyboard[step.RequiredKey];
				return keyControl != null && keyControl.wasPressedThisFrame;
			case TutorialAdvanceMode.GameplayEvent:
				if (string.IsNullOrWhiteSpace(step.RequiredEvent))
					return false;
				if (_lastReceivedEvent != step.RequiredEvent)
					return false;
				_lastReceivedEvent = null;
				return true;
			case TutorialAdvanceMode.Manual:
			default:
				return false;
		}
	}

	private void ApplyStepVisuals(UITutorialStep step)
	{
		_tooltipText.text = step.BodyText;

		MoveHighlightToTarget(step.HighlightTarget, step.HighlightPadding);
		MoveTooltipToAnchor(step.TooltipAnchor);
	}

	private void MoveHighlightToTarget(RectTransform target, Vector2 padding)
	{
		Vector2 targetSize = target.rect.size;
		Vector2 newSize = targetSize + (padding * 2f);

		_highlightFrame.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newSize.x);
		_highlightFrame.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newSize.y);

		UIAnimationClip moveClip = UIAnimationClip.CreateRuntimeAnchoredMoveClip(_highlightFrame.position, target.position, _moveDuration, _moveEase);
		_highlightMoveHandle = _highlightAnimator.Play(moveClip);
		if (_highlightPulseClip != null)
			_highlightAnimator.Play(_highlightPulseClip);
	}

	private void MoveTooltipToAnchor(RectTransform target)
	{
		if (_tooltipAnimator == null)
			return;

		UIAnimationClip moveClip = UIAnimationClip.CreateRuntimeAnchoredMoveClip(_tooltipPanel.position, target.position, _moveDuration, _moveEase);
		_tooltipMoveHandle = _tooltipAnimator.Play(moveClip);
	}

#if UNITY_EDITOR
	private void OnValidate()
	{
		if (_tooltipText == null)
			_tooltipText = GetComponentInChildren<TMP_Text>(true);
		if (_dimmerCanvasGroup != null && _dimmerAnimator == null)
			_dimmerAnimator = _dimmerCanvasGroup.GetComponent<UIAnimator>();
	}
#endif
}
