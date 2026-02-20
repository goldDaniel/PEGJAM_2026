using System.Collections.Generic;
using UnityEngine;

public class Game : MonoSingleton<Game>
{
	private bool _isPaused;
	public bool IsPaused
	{
		get => _isPaused;
		private set
		{
			if(_isPaused == value)
				return;

			if (value)
				Time.timeScale = 0f;
			else
				Time.timeScale = 1f;

			_isPaused = value;
		}
	}

	public void PauseGame() => IsPaused = true;
	public void ResumeGame() => IsPaused = false;

	private Dictionary<ArrowDirection, GameplayArrow> _arrows = new();

	public void Register(ArrowDirection direction, GameplayArrow arrow)
	{
		if (_arrows.ContainsKey(direction))
		{
			Debug.LogWarning($"Arrow for direction {direction} is already registered.");
			return;
		}
		_arrows[direction] = arrow;
	}

	public void Deregister(ArrowDirection direction)
	{
		if (!_arrows.ContainsKey(direction))
		{
			Debug.LogWarning($"No arrow registered for direction {direction} to deregister.");
			return;
		}
		_arrows.Remove(direction);
	}

	public void Press(ArrowDirection direction)
	{
		if (IsPaused)
			return;

		var arrow = _arrows[direction];

	}
}
