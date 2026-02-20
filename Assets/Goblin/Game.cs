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

	[SerializeField] private RectTransform[] _arrowPath;

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
		Eat,
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

	public void Press(GameplayAction gameplayAction)
	{
		if (IsPaused)
			return;

		switch (gameplayAction)
		{
			case GameplayAction.ReachForFood:
				HandleReachForFood();
				break;
			case GameplayAction.HoldFoodInFront:
				HandleHoldFoodInFront();
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
}
