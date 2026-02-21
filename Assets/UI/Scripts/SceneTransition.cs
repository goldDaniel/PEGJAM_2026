using UnityEngine;
using UnityEngine.InputSystem;

public class SceneTransition : MonoBehaviour
{
	[Header("Transition")]
	public string transition;

	void Awake()
	{ 
		InputSystem.actions.Enable(); 
	}

	public void NextScene()
	{
		SceneTransitionManager.LoadScene(transition);
	}
}
