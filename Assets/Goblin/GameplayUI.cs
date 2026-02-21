using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

public class GameplayUI : MonoBehaviour
{
	[SerializeField] private UIAnimator _animator;
	[SerializeField] private UIAnimationTarget _target;
	[SerializeField] private Image _renderer;
	public RectTransform rectTransform;

	[SerializeField] private UIAnimationClip _bounce;

	[SerializeField] private string _sfxKey;

	private KeyControl _control;

	public Key GetKey() => _control.keyCode;

	public void Init(Key key)
	{
		_control = Keyboard.current[key];
		_animator.Play(_bounce);
	}

	public void OnValidPress()
	{
		AudioManager.Instance.Play(_sfxKey);
	}

	public void MoveToPosition(Vector2 position)
	{
		var clip = UIAnimationClip.CreateRuntimeAnchoredMoveClip(rectTransform.position, position, 0.1f, UIEaseType.SineInOut);
		_animator.Play(clip);
	}
}
