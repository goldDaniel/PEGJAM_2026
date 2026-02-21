
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Game : MonoSingleton<Game>
{
	private Food _currentFood = null;

	[SerializeField] private RectTransform[] _arrowPath;
	[SerializeField] private Level _currentLevel;
	[SerializeField] private Food _foodPrefab;
	[SerializeField] private Transform _foodHoldingPosition;

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
	private bool _isHoldingFood => _currentFood != null;

	void Awake()
	{
		_currentLevel = Instantiate(_currentLevel);
	}

	void Update()
	{
		if (IsPaused)
			return;

		bool stageComplete = _currentLevel.foodItems.Count == 0 && _currentFood == null;
		if (stageComplete)
			return;

		if (!_isHoldingFood)
		{
			HandleGrabbing();	
		}
		else if (_isHoldingFood && _activeArrows.Count > 0 && Keyboard.current.anyKey.wasPressedThisFrame)
		{
			HandleEating();
		}		
	}

	private void HandleGrabbing()
	{
		if (_handState == HandState.Empty || _handState == HandState.Holding)
		{
			if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
			{
				_handState = HandState.Reaching;
				if(!_isHoldingFood)
				{
					GrabNextFoodItem();
				}
			}
				
		}
		else if (_handState != HandState.Reaching)
		{
			if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
				_handState = HandState.Holding;
		}
	}

	private void GrabNextFoodItem()
	{
		var items = _currentLevel.foodItems;
		SetupArrowsForFood(items[0]);

		_currentFood = Instantiate(_foodPrefab, _foodHoldingPosition, true);
		_currentFood.transform.position = _foodHoldingPosition.position;
		_currentFood.Init(items[0]);

		items.RemoveAt(0);
	}

	private void HandleEating()
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

			if (_activeArrows.Count == 0)
			{
				Destroy(_currentFood.gameObject);
				_currentFood = null;
				_handState = HandState.Empty;
			}
		}
	}

	private void SetupArrowsForFood(FoodTemplate template)
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
}
