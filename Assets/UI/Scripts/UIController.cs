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

	[Header("Goblin Bar")]
	public UIAnimator _goblinBarAnimator;
	public UIAnimationTarget _goblinTarget;
	private UIAnimHandle _goblinBarHandle;


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
		UpdateMeter();
	}

	private void UpdatePause()
	{
		if (Keyboard.current.escapeKey.wasPressedThisFrame)
		{
			if (_pauseHandle == null || !_pauseHandle.IsPlaying)
			{
				if (pauseAnimator.gameObject.activeSelf)
				{
					_pauseHandle = pauseAnimator.Play(pauseOutClip, new()
					{ 
						OnComplete = () =>
						{
							pauseAnimator.gameObject.SetActive(false);
							Game.Instance.ResumeGame();
						}
					});
				}
				else
				{
					Game.Instance.PauseGame();
					pauseAnimator.gameObject.SetActive(true);
					_pauseHandle = pauseAnimator.Play(pauseInClip);
				}
			}
		}
	}

	private void UpdateMeter()
	{
		int minKey = (int)Key.Digit1;
		int maxKey = (int)Key.Digit0;

		for(int i = minKey; i <= maxKey; i++)
		{
			KeyControl keyControl = Keyboard.current[(Key)i];
			if (keyControl != null && keyControl.wasPressedThisFrame)
			{
				float percentage = (i - minKey) / (float)(maxKey - minKey);
				if (_goblinBarHandle != null && _goblinBarHandle.IsPlaying)
					_goblinBarHandle.Stop();
				
				var clip = UIAnimationClip.CreateRuntimeScaleClip(_goblinTarget.GetScale(), new Vector2(percentage, 1f), 1.0f, UIEaseType.SineInOut);
				_goblinBarHandle = _goblinBarAnimator.Play(clip);
				break;
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
