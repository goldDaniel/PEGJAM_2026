using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class UIController : MonoBehaviour
{
	[Header("Pause")]
	public UIAnimationClip pauseInClip;
	public UIAnimationClip pauseOutClip;
	public UIAnimator pauseAnimator;
	private UIAnimHandle _pauseHandle;

	[Header("Right Side Banner")]
	public UIAnimationClip rightSideInClip;
	public UIAnimationClip rightSideOutClip;
	public UIAnimator rightSideAnimator;
	private UIAnimHandle _rightSideHandle;


	[Header("Tutorial")]
	public UITutorialController tutorialController;
	public Key tutorialStartKey = Key.F1;


	void Awake()
	{
		var uiAnimations = FindObjectsByType<UIAnimator>(FindObjectsInactive.Include, FindObjectsSortMode.None);
		Dictionary<UIAnimator, bool> activeInitial = new();

		foreach (var anim in uiAnimations)
		{
			activeInitial[anim] = anim.gameObject.activeSelf;
			if(!anim.gameObject.activeSelf)
				anim.gameObject.SetActive(true);
		}

		Canvas.ForceUpdateCanvases();

		foreach (var anim in uiAnimations)
		{
			if (!activeInitial[anim])
				anim.gameObject.SetActive(false);
		}
	}

	void Update()
	{
		UpdatePause();
		UpdateBanner();
		UpdateTutorial();

		if (Keyboard.current.f2Key.wasPressedThisFrame)
			SceneTransitionManager.LoadScene("SampleScene");
	}

	private void UpdatePause()
	{
		if (Keyboard.current.escapeKey.wasPressedThisFrame)
		{
			if (_pauseHandle == null || !_pauseHandle.IsPlaying)
			{
				if (pauseAnimator.gameObject.activeSelf)
				{
					_pauseHandle = pauseAnimator.Play(pauseOutClip, new() { OnComplete = () => pauseAnimator.gameObject.SetActive(false) });
				}
				else
				{
					pauseAnimator.gameObject.SetActive(true);
					_pauseHandle = pauseAnimator.Play(pauseInClip);
				}
			}
		}
	}

	private void UpdateBanner()
	{
		if(Keyboard.current.tabKey.wasPressedThisFrame)
		{
			if (_rightSideHandle == null || !_rightSideHandle.IsPlaying)
			{
				if (rightSideAnimator.gameObject.activeSelf)
				{
					_rightSideHandle = rightSideAnimator.Play(rightSideOutClip, new() { OnComplete = () => rightSideAnimator.gameObject.SetActive(false) });
				}
				else
				{
					rightSideAnimator.gameObject.SetActive(true);
					_rightSideHandle = rightSideAnimator.Play(rightSideInClip);
				}
			}
		}
	}

	private void UpdateTutorial()
	{
		if (tutorialController == null || Keyboard.current == null)
			return;

		KeyControl keyControl = Keyboard.current[tutorialStartKey];
		if (keyControl == null || !keyControl.wasPressedThisFrame)
			return;

		if (tutorialController.IsRunning)
			tutorialController.StopTutorial();
		else
			tutorialController.StartTutorial();
	}
}
