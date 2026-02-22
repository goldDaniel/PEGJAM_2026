using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class SceneTransition : MonoBehaviour
{
	[Header("Transition")]
	public string transition;
	[Header("Scene Music")]
	public string music;

	void Awake()
	{ 
		InputSystem.actions.Enable();
	}

	public void NextScene()
	{
		if (music != "") 
			AudioManager.Instance.PlayMusicCrossfade(music, 5f);
		SceneTransitionManager.LoadScene(transition);
	}
}
