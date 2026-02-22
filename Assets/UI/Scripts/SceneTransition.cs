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
		SceneTransitionManager.LoadScene(transition);
	}
}
