
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
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
		tryAgainButton.gameObject.SetActive(false);
		continueButton.gameObject.SetActive(true);
		gameObject.SetActive(true);

		animator.Play(inClip);
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
		// TODO (danielg): Load next level
	}

	public void OnTryAgainPressed()
	{

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

