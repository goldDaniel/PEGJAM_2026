using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using static Game;


public class GameplayUI : MonoBehaviour
{
	[SerializeField] private UIAnimator _animator;
	[SerializeField] private UIAnimationTarget _target;

	[SerializeField] private GameplayAction _action;
	[SerializeField] private GameInput _input;

	private KeyControl _control;

	void Awake()
	{
		switch (_action)
		{
			case GameplayAction.ReachForFood:
				_control = Keyboard.current.rightArrowKey;
				break;
			case GameplayAction.HoldFoodInFront:
				_control = Keyboard.current.leftArrowKey;
				break;
			case GameplayAction.MouthClose:
				_control = Keyboard.current.upArrowKey;
				break;
			case GameplayAction.MouthOpen:
				_control = Keyboard.current.downArrowKey;
				break;
			case GameplayAction.Water:
				_control = Keyboard.current.enterKey;
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	private void OnEnable() => Game.Instance.Register(_action, this);

	private void OnDisable()
	{
		if(Game.HasInstance)
			Game.Instance.Deregister(_action);
	}

	void Update()
	{
		if (_control != null && _control.wasPressedThisFrame)
			Game.Instance.Press(_action);
	}
}
