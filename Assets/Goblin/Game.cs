using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

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
	[SerializeField] private FoodTemplate _foodTemplate;

	[SerializeField] private GameplayUI _leftArrowPrefab;
	[SerializeField] private GameplayUI _rightArrowPrefab;
	[SerializeField] private GameplayUI _upArrowPrefab;
	[SerializeField] private GameplayUI _downArrowPrefab;

	private List<GameplayUI> _activeArrows = new();

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

	void Awake()
	{
		_food = _foodContainer.GetComponentsInChildren<Food>().ToList();
		SetupFood(_foodTemplate);
	}

	void Update()
	{
		if (IsPaused)
			return;

		if (_activeArrows.Count > 0 && Keyboard.current.anyKey.wasPressedThisFrame)
		{
			var desiredKey = _activeArrows[0].GetKey();
			if (Keyboard.current[desiredKey].wasPressedThisFrame)
			{
				Destroy(_activeArrows[0].gameObject);
				_activeArrows.RemoveAt(0);

				for (int i = 0; i < _activeArrows.Count; ++i)
				{
					var arrow = _activeArrows[i];
					arrow.MoveToPosition(_arrowPath[i].transform.position);
				}
			}
		}		
	}

	private void SetupFood(FoodTemplate template)
	{
		_activeArrows.Clear();

		var seq = template.GetKeySequence();
		for (int i = 0; i < seq.Length; ++i)
		{
			GameplayUI prefab = null;
			var keys = seq[i].keys;
			if (keys.Length == 1)
			{
				switch (keys[0])
				{
					case Key.LeftArrow:
						prefab = _leftArrowPrefab; break;
					case Key.RightArrow:
						prefab = _rightArrowPrefab; break;
					case Key.UpArrow:
						prefab = _upArrowPrefab; break;
					case Key.DownArrow:
						prefab = _downArrowPrefab; break;
				}
			}
			else 
			{
				// TODO (danielg): Handle multipress
			}

			var parent = _arrowPath[i];
			var instance = Instantiate(prefab, parent);
			instance.transform.position = parent.transform.position;
			instance.rectTransform.sizeDelta *= 1.5f;
			instance.Init(keys[0]);
			_activeArrows.Add(instance);
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
