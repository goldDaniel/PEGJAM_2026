
using UnityEngine;

public class GameplayIndicator : MonoBehaviour
{
	[SerializeField] private UIAnimationClip _animationClip;
	[SerializeField] private UIAnimator _animator;

	private UIAnimHandle _animHandle;

	void OnEnable() => _animHandle = _animator.Play(_animationClip);
	void OnDisable() => _animHandle?.Stop();
}
