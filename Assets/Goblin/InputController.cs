using NUnit.Framework;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
public enum GameInput
{
	Left,
	Right,
	Up,
	Down,
	Action,
	Menu
}

public class InputController : MonoSingleton<InputController>
{
	[Header("Input Window")]
	public float inputWindow;

	private float _pressedTime;
	private List<GameInput> _pressedInputs = new();

	void Awake()
	{
		SetInputCallbacks("Left", GameInput.Left);
		SetInputCallbacks("Right", GameInput.Right);
		SetInputCallbacks("Up", GameInput.Up);
		SetInputCallbacks("Down", GameInput.Down);
		SetInputCallbacks("Action", GameInput.Action);
		SetInputCallbacks("Menu", GameInput.Menu);
	}

	private void SetInputCallbacks(string actionName, GameInput dir)
	{
		InputSystem.actions.FindAction(actionName).started += (_) => OnKeyPress(dir);
		InputSystem.actions.FindAction(actionName).canceled += (_) => OnKeyReleased(dir);
	}

	private void OnKeyPress(GameInput input)
	{
		_pressedTime = Time.time;
		_pressedInputs.Add(input);
	}

	private void OnKeyReleased(GameInput input)
	{
		_pressedInputs.Remove(input);
	}

	public List<GameInput> GetInputs()
	{
		return _pressedInputs;
	}

	public bool inWindow()
	{
		return Time.time <= (_pressedTime + inputWindow);
	}

	public void ClearInputBuffer()
	{
		_pressedInputs.Clear();
	}
}
