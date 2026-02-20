using UnityEngine;

public class CurrentIndicator : MonoBehaviour
{
	public UIAnimator animator;
	public UIAnimationClip clip;

	void Awake()
	{
		animator.Play(clip);
	}
}
