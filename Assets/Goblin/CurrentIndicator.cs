using UnityEngine;

public class CurrentIndicator : MonoBehaviour
{
	public UIAnimator animator;
	public UIAnimationClip clip;

	private UIAnimHandle _handle;

	void Awake() => Play();
	public void Play() => _handle = animator.Play(clip);
	public void Stop() => _handle?.Stop();
}
