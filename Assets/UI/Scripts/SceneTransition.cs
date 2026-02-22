using UnityEngine;
using UnityEngine.InputSystem;

public class SceneTransition : MonoBehaviour
{
	[Header("Transition")]
	public string transition;

	void Awake()
	{ 
		InputSystem.actions.Enable();
		AudioManager.Instance.Play("MainMenu");
	}

	public void NextScene()
	{
		AudioManager.Instance.PlayMusicCrossfade("GameplayMusic", 5f);
		SceneTransitionManager.LoadScene(transition);
	}
}
