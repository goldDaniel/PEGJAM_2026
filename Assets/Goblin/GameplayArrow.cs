using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public enum ArrowDirection
{
	Up,
	Down,
	Left,
	Right,
}

public class GameplayArrow : MonoBehaviour
{
	[SerializeField] private ArrowDirection _direction;

	private KeyControl _control;

	void Awake()
	{
		switch (_direction)
		{
			case ArrowDirection.Up:
				_control = Keyboard.current.upArrowKey;
				break;
			case ArrowDirection.Down:
				_control = Keyboard.current.downArrowKey;
				break;
			case ArrowDirection.Left:
				_control = Keyboard.current.leftArrowKey;
				break;
			case ArrowDirection.Right:
				_control = Keyboard.current.rightArrowKey;
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	private void OnEnable() => Game.Instance.Register(_direction, this);

	private void OnDisable()
	{
		if(Game.HasInstance)
			Game.Instance.Deregister(_direction);
	}

	void Update()
	{
		if (_control != null && _control.wasPressedThisFrame)
			Game.Instance.Press(_direction);
	}
}
