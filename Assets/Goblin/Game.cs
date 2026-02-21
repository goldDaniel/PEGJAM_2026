
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Game : MonoSingleton<Game>
{
	private Food _currentFood = null;
	private List<Food> _foodPile = new();

	[SerializeField] private RectTransform[] _arrowPath;
	[SerializeField] private Level _currentLevel;
	[SerializeField] private Food _foodPrefab;
	[SerializeField] private Transform _foodHoldingPosition;

	[SerializeField] private Transform _foodSpawnPosition;

	[SerializeField] private GameplayUI _leftArrowPrefab;
	[SerializeField] private GameplayUI _rightArrowPrefab;
	[SerializeField] private GameplayUI _upArrowPrefab;
	[SerializeField] private GameplayUI _downArrowPrefab;

	[SerializeField] private CurrentIndicator _indicator;

	private List<GameplayUI> _activeArrows = new();

	[Range(0.01f, 1f)]
	[SerializeField] private float _missCooldownTime = 0.2f;
	private float _missCooldownTimer = 0;

	public enum GameInput
	{
		ArrowUp,
		ArrowDown,
		ArrowLeft,
		ArrowRight,
		Enter,
	}


	public enum HandState
	{
		Reaching,
		Holding,
		Empty,
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

	private HandState _handState = HandState.Empty;
	private bool IsReadyToEat => _currentFood != null && _handState == HandState.Holding;

	public bool OnPunishmentCooldown => _missCooldownTimer > 0;
	public float CooldownPercentage => _missCooldownTimer / _missCooldownTime;

	void Awake()
	{
		SpawnFoodOnTable();
		_missCooldownTimer = 0;
	}

	private void SpawnFoodOnTable()
	{
		_currentLevel = Instantiate(_currentLevel);
		_foodPile.Capacity = _currentLevel.foodItems.Count;
		for (int i = 0; i < _currentLevel.foodItems.Count; ++i)
		{
			var food = Instantiate(_foodPrefab, _foodSpawnPosition, true);
			food.transform.position = _foodSpawnPosition.position.xy() + Random.insideUnitCircle;
			food.Init(_currentLevel.foodItems[i]);
			_foodPile.Add(food);
		}
	}

	void Update()
	{
		if (IsPaused)
			return;

		bool stageComplete = _foodPile.Count == 0 && _currentFood == null;
		if (stageComplete)
			return;

		if(OnPunishmentCooldown)
		{
			_missCooldownTimer = Mathf.Max(_missCooldownTimer - Time.deltaTime, 0f);
			return;
		}
	
		if (!IsReadyToEat)
		{
			HandleGrabbing();
		}
		else if (IsReadyToEat && _activeArrows.Count > 0 && Keyboard.current.anyKey.wasPressedThisFrame)
		{
			HandleEating();
		}		
	}

	private void HandleGrabbing()
	{
		if (_handState == HandState.Empty)
		{
			if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
			{
				_handState = HandState.Reaching;
				if(!IsReadyToEat)
				{
					GrabNextFoodItem();
				}
			}
				
		}
		else if (_handState == HandState.Reaching)
		{
			if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
			{
				_handState = HandState.Holding;
				SetupArrowsForFood(_currentFood);
			}
		}
	}

	private void GrabNextFoodItem()
	{
		_currentFood = _foodPile[0];
		_foodPile.RemoveAt(0);
		_currentFood.transform.SetParent(_foodHoldingPosition, true);
		_currentFood.transform.position = _foodHoldingPosition.position;
	}

	private void HandleEating()
	{
		var desiredKey = _activeArrows[0].GetKey();
		if (Keyboard.current[desiredKey].wasPressedThisFrame)
		{
			_activeArrows[0].OnValidPress();

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
		else // miss input, punishment
		{
			_missCooldownTimer = _missCooldownTime;
			_indicator.MissedInput();
		}
	}

	private void SetupArrowsForFood(Food food)
	{
		_activeArrows.Clear();

		var seq = food.template.GetKeySequence();
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
		_indicator.Stop();
		_indicator.Play();
	}
}
