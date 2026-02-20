using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Game : MonoSingleton<Game>
{
	[SerializeField] private GoblinMeter _goblinMeter;
	[SerializeField] private Transform _foodContainer;
	[SerializeField] private Transform _foodPivot;
	private List<Food> _food = new();
	private Food _currentFood = null;

	[SerializeField] private GameObject _mouthOpen;
	[SerializeField] private GameObject _mouthClosed;

	public enum GameInput
	{
		ArrowUp,
		ArrowDown,
		ArrowLeft,
		ArrowRight,
		Enter,
	}

	public enum GameplayAction
	{
		ReachForFood,
		HoldFoodInFront,
		MouthOpen,
		MouthClose,
		Water,
	}

	public enum HandState
	{
		Reaching,
		Holding,
		Empty,
		Water
	}

	public enum EatingState
	{
		MouthOpen,
		MouthClosed,
	}

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

	private EatingState _eatingState = EatingState.MouthClosed;
	private HandState _handState = HandState.Empty;
	private bool _isHolding => _currentFood != null;

	private Dictionary<GameplayAction, GameplayUI> _ui = new();

	void Awake()
	{
		_food = _foodContainer.GetComponentsInChildren<Food>().ToList();
	}

	public void Register(GameplayAction gameplayAction, GameplayUI arrow)
	{
		if (_ui.ContainsKey(gameplayAction))
		{
			Debug.LogWarning($"UI for action {gameplayAction} is already registered.");
			return;
		}
		_ui[gameplayAction] = arrow;
	}

	public void Deregister(GameplayAction gameplayAction)
	{
		if (!_ui.ContainsKey(gameplayAction))
		{
			Debug.LogWarning($"No UI registered for action {gameplayAction} to deregister.");
			return;
		}
		_ui.Remove(gameplayAction);
	}

	public void Press(GameplayAction gameplayAction)
	{
		if (IsPaused)
			return;

		var uiElement = _ui[gameplayAction];

		switch (gameplayAction)
		{
			case GameplayAction.ReachForFood:
				HandleReachForFood();
				break;
			case GameplayAction.HoldFoodInFront:
				HandleHoldFoodInFront();
				break;
			case GameplayAction.MouthOpen:
				HandleMouthOpen();
				break;
			case GameplayAction.MouthClose:
				HandleMouthClosed();
				break;
			case GameplayAction.Water:
				_handState = HandState.Water;
				break;
		}
	}
	
	private void HandleReachForFood()
	{
		_handState = HandState.Reaching;
		if (!_isHolding)
		{
			if(_food.Count > 0)
			{
				_currentFood = _food[0];
				_food.RemoveAt(0);

				_currentFood.transform.SetParent(_foodPivot, true);
				_currentFood.transform.position = _foodPivot.position;
				_goblinMeter.SetPercentage(_currentFood.GetPercentage(), 0.5f);
			}
		}
	}
	private void HandleHoldFoodInFront()
	{
		if (_isHolding)
		{
			_handState = HandState.Holding;
		}
		else 
		{
			_handState = HandState.Empty;
		}
	}

	private void HandleMouthOpen()
	{
		if (_eatingState != EatingState.MouthOpen)
		{
			_eatingState = EatingState.MouthOpen;
			_mouthClosed.gameObject.SetActive(false);
			_mouthOpen.gameObject.SetActive(true);
		}
	}

	private void HandleMouthClosed()
	{
		if (_eatingState != EatingState.MouthClosed)
		{
			_eatingState = EatingState.MouthClosed;
			_mouthOpen.gameObject.SetActive(false);
			_mouthClosed.gameObject.SetActive(true);

			if (_isHolding && _handState == HandState.Holding)
			{
				float percentage = _currentFood.GetPercentage();
				if (_currentFood.TakeBite())
				{
					percentage = _currentFood.GetPercentage();
					Destroy(_currentFood.gameObject);
					_currentFood = null;
					_handState = HandState.Empty;
				}
				_goblinMeter.SetPercentage(percentage, 0.1f);
			}
		}
	}
}
