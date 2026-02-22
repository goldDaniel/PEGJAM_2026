
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelEndPanel : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI endText;
	[SerializeField] private Button continueButton;
	[SerializeField] private Button tryAgainButton;
	[SerializeField] private Button quitButton;

	[SerializeField] private UIAnimationClip inClip;
	[SerializeField] private UIAnimationClip outClip;

	[SerializeField] private UIAnimator animator;

	public bool IsDisplayed => gameObject.activeSelf;

	public void OnWin()
	{
		endText.text = "you win";

		if (Game.Instance.IsLastLevel)
		{
			tryAgainButton.gameObject.SetActive(false);
			continueButton.gameObject.SetActive(false);
			quitButton.gameObject.SetActive(false);
		}
		else 
		{
			tryAgainButton.gameObject.SetActive(false);
			continueButton.gameObject.SetActive(true);
			quitButton.gameObject.SetActive(true);
		}

		gameObject.SetActive(true);
		animator.Play(inClip, new()
		{
			OnComplete = () =>
			{
				// TODO (danielg): Transition to game end / credits scene instead
				if (Game.Instance.IsLastLevel)
					SceneTransitionManager.LoadScene("Main Menu");
			}
		});
	}

	public void OnLose()
	{
		endText.text = "you lose";
		tryAgainButton.gameObject.SetActive(true);
		continueButton.gameObject.SetActive(false);
		gameObject.SetActive(true);

		animator.Play(inClip);
	}

	public void OnContinuePressed()
	{
		animator.Play(outClip, new()
		{
			OnComplete = () =>
			{
				LevelLoader.CurrentLevelIndex++;
				SceneTransitionManager.LoadScene("Gameplay");
			}
		});
	}

	public void OnTryAgainPressed()
	{
		animator.Play(outClip, new()
		{
			OnComplete = () =>
			{
				SceneTransitionManager.LoadScene("Gameplay");
			}
		});
	}

	public void OnQuitPressed()
	{
		animator.Play(outClip, new()
		{
			OnComplete = () =>
			{
				SceneTransitionManager.LoadScene("Main Menu");
			}
		});
	}
}

