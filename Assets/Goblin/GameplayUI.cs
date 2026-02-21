using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

public class GameplayUI : MonoBehaviour
{
	[SerializeField] private UIAnimator _animator;
	[SerializeField] private UIAnimationTarget _target;
	public RectTransform rectTransform;

	[SerializeField] private UIAnimationClip _bounce;

	[SerializeField] private string _sfxKey;

	private KeyControl[] _controls;

	public IEnumerable<Key> GetKeys()
	{
		for (int i = 0; i < _controls.Length; i++)
			yield return _controls[i].keyCode;
	}

	public void Init(params Key[] keys)
	{
		_controls = new KeyControl[keys.Length];
		for (int i = 0; i < keys.Length; i++)
			_controls[i] = Keyboard.current[keys[i]];
		//_animator.Play(_bounce);
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
