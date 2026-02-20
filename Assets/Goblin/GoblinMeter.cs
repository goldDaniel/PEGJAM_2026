using UnityEngine;


public class GoblinMeter : MonoBehaviour
{
	[Header("Goblin Bar")]
	[SerializeField] private UIAnimator _animator;
	[SerializeField] private UIAnimationTarget _target;
	private UIAnimHandle _goblinBarHandle;

	public float GetPercentage() => _target.GetScale().x;

	public void SetPercentage(float value, float duration)
	{
		float percentage = Mathf.Clamp01(value);
		
		if (_goblinBarHandle != null && _goblinBarHandle.IsPlaying)
			_goblinBarHandle.Stop();

		var clip = UIAnimationClip.CreateRuntimeScaleClip(_target.GetScale(), new Vector2(percentage, 1f), duration, UIEaseType.SineInOut);
		_goblinBarHandle = _animator.Play(clip);
	}
}
