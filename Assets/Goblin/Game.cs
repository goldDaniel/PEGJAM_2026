
using System.Collections;
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

	[SerializeField] private Transform[] _foodSpawnPosition;

	[SerializeField] private GameplayUI _leftArrowPrefab;
	[SerializeField] private GameplayUI _rightArrowPrefab;
	[SerializeField] private GameplayUI _upArrowPrefab;
	[SerializeField] private GameplayUI _downArrowPrefab;

	[SerializeField] private GameplayUI _luArrowPrefab;
	[SerializeField] private GameplayUI _ldArrowPrefab;
	[SerializeField] private GameplayUI _lrArrowPrefab;
	[SerializeField] private GameplayUI _duArrowPrefab;
	[SerializeField] private GameplayUI _drArrowPrefab;
	[SerializeField] private GameplayUI _urArrowPrefab;

	[SerializeField] private CurrentIndicator _indicator;

	private List<GameplayUI> _activeArrowCombos = new();
	private Dictionary<int, GameplayUI> _comboDict = new();

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

	IEnumerator Start()
	{
		InitArrowDict();
		_missCooldownTimer = 0;

		while (SceneTransitionManager.Instance.IsTransitioning)
			yield return null;

		UIController.Instance.ShowVersusPanel(_currentLevel);
	}

	private static int HashKeys(params Key[] keys)
	{
		System.Array.Sort(keys);
		int hash = 0;
		foreach (Key key in keys)
			hash = System.HashCode.Combine(hash, key);
		return hash;
	}

	private void InitArrowDict()
	{
		_comboDict[HashKeys(Key.RightArrow)] = _rightArrowPrefab;
		_comboDict[HashKeys(Key.LeftArrow)] = _leftArrowPrefab;
		_comboDict[HashKeys(Key.DownArrow)] = _downArrowPrefab;
		_comboDict[HashKeys(Key.UpArrow)] = _upArrowPrefab;

		_comboDict[HashKeys(Key.LeftArrow, Key.UpArrow)] = _urArrowPrefab;
		_comboDict[HashKeys(Key.LeftArrow, Key.DownArrow)] = _ldArrowPrefab;
		_comboDict[HashKeys(Key.LeftArrow, Key.RightArrow)] = _lrArrowPrefab;
		_comboDict[HashKeys(Key.DownArrow, Key.UpArrow)] = _duArrowPrefab;
		_comboDict[HashKeys(Key.DownArrow, Key.RightArrow)] = _drArrowPrefab;
		_comboDict[HashKeys(Key.UpArrow, Key.RightArrow)] = _urArrowPrefab;
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
		else if (IsReadyToEat && _activeArrowCombos.Count > 0 && Keyboard.current.anyKey.wasPressedThisFrame)
		{
			HandleEating();
		}		
	}

	public void PrepareLevel()
	{
		_currentLevel = Instantiate(_currentLevel);

		_foodPile.Capacity = _currentLevel.foodItems.Count;
		foreach (var spawnPos in _foodSpawnPosition)
		{
			for (int i = 0; i < _currentLevel.foodItems.Count; ++i)
			{
				var food = Instantiate(_foodPrefab, spawnPos, true);
				food.transform.position = spawnPos.position.xy() + Random.insideUnitCircle * new Vector2(0.8f, 0.5f);
				food.Init(_currentLevel.foodItems[i]);
				_foodPile.Add(food);
			}
		}
	}

	public void StartGameplayCountdown()
	{
		StartCoroutine(GameplayCountdown());
	}

	private IEnumerator GameplayCountdown()
	{
		// Display 3
		yield return new WaitForSeconds(1);
		// Display 2
		yield return new WaitForSeconds(1);
		// Display 1
		yield return new WaitForSeconds(1);
		// FEAST
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
		var desiredCombo = _activeArrowCombos[0].GetKeys();
		bool comboPressed = true;
		foreach (var key in desiredCombo)
			comboPressed &= Keyboard.current[key].wasPressedThisFrame;

		if (comboPressed)
		{
			_activeArrowCombos[0].OnValidPress();

			Destroy(_activeArrowCombos[0].gameObject);
			_activeArrowCombos.RemoveAt(0);

			for (int i = 0; i < _activeArrowCombos.Count; ++i)
			{
				var arrowCombo = _activeArrowCombos[i];
				arrowCombo.MoveToPosition(_arrowPath[i].transform.position);
			}

			if (_activeArrowCombos.Count == 0)
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
		_activeArrowCombos.Clear();

		var seq = food.template.GetKeySequence();
		int count = 0;
		for (int i = 0; i < seq.Length; ++i)
		{
			var keys = seq[i].keys;
			var prefab = _comboDict[HashKeys(keys)];

			var parent = _arrowPath[count];
			var instance = Instantiate(prefab, parent);
			instance.transform.position = parent.transform.position;
			instance.rectTransform.sizeDelta *= 1.5f;
			instance.Init(keys);
			_activeArrowCombos.Add(instance);
			count++;
		}
		_indicator.Stop();
		_indicator.Play();
	}
}
