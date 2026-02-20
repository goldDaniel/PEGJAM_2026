using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class GameplayUI : MonoBehaviour
{
	[SerializeField] private UIAnimator _animator;
	[SerializeField] private UIAnimationTarget _target;
	[SerializeField] private SpriteRenderer _renderer;
	

	[SerializeField] private Sprite _upSprite;
    [SerializeField] private Sprite _downSprite;
    [SerializeField] private Sprite _leftSprite;
    [SerializeField] private Sprite _rightSprite;

    private KeyControl _control;

    void Init(Key key)
	{
		_control = Keyboard.current[key];
		switch (key) 
		{
			case Key.UpArrow:
				_renderer.sprite = _upSprite;
				break;
			case Key.DownArrow:
				_renderer.sprite = _downSprite;
				break;
			case Key.LeftArrow:
				_renderer.sprite = _leftSprite;
				break;
			case Key.RightArrow:
				_renderer.sprite = _rightSprite;
				break;
		}
    }

	void MoveToPosition(Vector2 position)
	{

	}
}
