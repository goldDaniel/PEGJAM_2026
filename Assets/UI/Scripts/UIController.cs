using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

public class UIController : MonoSingleton<UIController>
{
	[Header("Pause")]
	public UIAnimationClip pauseInClip;
	public UIAnimationClip pauseOutClip;
	public UIAnimator pauseAnimator;
	private UIAnimHandle _pauseHandle;


	public UIAnimator versusAnimator;
	private UIAnimHandle _versusHandle;

	public TextMeshProUGUI opponentName;
	public Image opponentImage;

	public Image playerImage;

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
	}

	public void ShowVersusPanel(Level level, Sprite playerImage)
	{
		_versusHandle?.Stop();
		opponentName.text = level.opponent.Name.ToLower();
		opponentImage.sprite = level.opponent.EntryImage;
		this.playerImage.sprite = playerImage;

		versusAnimator.gameObject.SetActive(true);
		_versusHandle = versusAnimator.Play(pauseInClip, new()
		{
			OnComplete = () => StartCoroutine(DelayBeforeHideVersus(3))
		});
	}

	private IEnumerator DelayBeforeHideVersus(float seconds)
	{
		Game.Instance.PrepareLevel();

		yield return new WaitForSeconds(seconds);
		versusAnimator.Play(pauseOutClip, new()
		{
			OnComplete = () => 
			{ 
				Game.Instance.StartGameplayCountdown();
				versusAnimator.gameObject.SetActive(false);
			}
		});
	}

	public void OnUnPause()
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
		}
	}

	public void OnQuit()
	{
		LevelLoader.CurrentLevelIndex = 0;
		AudioManager.Instance.PlayMusicCrossfade("MainMenu");
		SceneTransitionManager.LoadScene("Main Menu");
	}

	private void UpdatePause()
	{
		if (Keyboard.current.pKey.wasPressedThisFrame)
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
