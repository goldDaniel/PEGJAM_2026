
using Mono.Cecil.Cil;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Game : MonoSingleton<Game>
{
	[SerializeField] private LevelEndPanel _levelEndPanel;
	[SerializeField] private TextMeshProUGUI _countdownText;

	[SerializeField] private RectTransform[] _arrowPath;
	[SerializeField] private LevelLoader _levels;
	private Level _currentLevel => _levels.CurrentLevel;
	public bool IsLastLevel => _levels.IsLastLevel;

	[SerializeField] private Food _foodPrefab;
	[SerializeField] private Transform _playerFoodHoldingPosition;
	[SerializeField] private Transform _opponentFoodHoldingPosition;

	[SerializeField] private Transform _playerFoodSpawn;
	[SerializeField] private Transform _opponentFoodSpawn;

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

	[SerializeField] private Contestant _player;
	[SerializeField] private Contestant _opponent;

	private int _inputSeqProgress;
	private int _inputSeqCount;
	private List<GameplayUI> _activeArrowCombos = new();
	private Dictionary<int, GameplayUI> _comboDict = new();

	[Range(0.01f, 1f)]
	[SerializeField] private float _missCooldownTime = 0.2f;
	private float _missCooldownTimer = 0;

	private bool _hasStarted = false;
	public bool HasStarted => _hasStarted;


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
	private bool IsReadyToEat => _player.IsHoldingFood && _handState == HandState.Holding;

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

	private static int HashInputs(params GameInput[] inputs)
	{
		System.Array.Sort(inputs);
		int hash = 0;
		foreach (var input in inputs)
			hash = System.HashCode.Combine(hash, input);
		return hash;
	}

	private void InitArrowDict()
	{
		_comboDict[HashInputs(GameInput.Right)] = _rightArrowPrefab;
		_comboDict[HashInputs(GameInput.Left)] = _leftArrowPrefab;
		_comboDict[HashInputs(GameInput.Down)] = _downArrowPrefab;
		_comboDict[HashInputs(GameInput.Up)] = _upArrowPrefab;

		_comboDict[HashInputs(GameInput.Left, GameInput.Up)] = _luArrowPrefab;
		_comboDict[HashInputs(GameInput.Left, GameInput.Down)] = _ldArrowPrefab;
		_comboDict[HashInputs(GameInput.Left, GameInput.Right)] = _lrArrowPrefab;
		_comboDict[HashInputs(GameInput.Down, GameInput.Up)] = _duArrowPrefab;
		_comboDict[HashInputs(GameInput.Down, GameInput.Right)] = _drArrowPrefab;
		_comboDict[HashInputs(GameInput.Up, GameInput.Right)] = _urArrowPrefab;
	}

	void Update()
	{
		if (IsPaused || !HasStarted)
			return;

		bool stageComplete = _player.HasEatenAllFood || _opponent.HasEatenAllFood;
		if (stageComplete)
		{
			if (!_levelEndPanel.IsDisplayed)
			{
				if (_player.HasEatenAllFood)
					_levelEndPanel.OnWin();
				else if (_opponent.HasEatenAllFood)
					_levelEndPanel.OnLose();
			}
			
			return;
		}

		_opponent.OpponentGameplayTick();

		if (OnPunishmentCooldown)
		{
			_missCooldownTimer = Mathf.Max(_missCooldownTimer - Time.deltaTime, 0f);
			return;
		}

		var inputs = InputController.Instance.GetInputs();
		if (inputs.Count > 0)
		{
			if (!IsReadyToEat)
			{
				HandleGrabbing(inputs);
				InputController.Instance.ClearInputBuffer();
			}
			else if (IsReadyToEat && _activeArrowCombos.Count > 0)
			{
				HandleEating(inputs, InputController.Instance.inWindow());
			}
		}
	}


	public void PrepareLevel()
	{
		var level = Instantiate(_currentLevel);

		_player.Setup(null, _foodPrefab, level, _playerFoodSpawn);
		_opponent.Setup(level.opponent, _foodPrefab, level, _opponentFoodSpawn);	
	}

	public void StartGameplayCountdown()
	{
		StartCoroutine(GameplayCountdown());
	}

	private IEnumerator GameplayCountdown()
	{
		_countdownText.gameObject.SetActive(true);

		_countdownText.text = $"{3}";
		yield return new WaitForSeconds(1);
		
		_countdownText.text = $"{2}";
		yield return new WaitForSeconds(1);
		
		_countdownText.text = $"{1}";
		yield return new WaitForSeconds(1);
		
		_countdownText.text = "feast";
		yield return new WaitForSeconds(1);

		_countdownText.gameObject.SetActive(false);
		_hasStarted = true;
	}

	private void HandleGrabbing(List<GameInput> inputs)
	{
		if (_handState == HandState.Empty)
		{
			if (inputs.Contains(GameInput.Right))
			{
				_handState = HandState.Reaching;
				if(!IsReadyToEat)
				{
					_player.GrabNextFoodItem();
				}
			}
		}
		else if (_handState == HandState.Reaching)
		{
			if (inputs.Contains(GameInput.Left))
			{
				_handState = HandState.Holding;
				SetupArrowsForFood(_player.GetHeldFood());
			}
		}
	}

	private void HandleEating(List<GameInput> inputs, bool inWindow)
	{
		var desiredCombo = _activeArrowCombos[0].GetInputs();
		
		bool wrongCombo = false;
		foreach (var input in inputs)
			wrongCombo |= !desiredCombo.Contains(input);

		bool comboPressed = true;
		foreach (var input in desiredCombo)
			comboPressed &= inputs.Contains(input);

		if (wrongCombo || (!comboPressed && !inWindow))
		{
			_missCooldownTimer = _missCooldownTime;
			_indicator.MissedInput();
		}
		else if(comboPressed)
		{ 
			_player.Bite((float)_inputSeqProgress++ / (_inputSeqCount - 1));
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
				_player.FinishFood();
				_handState = HandState.Empty;
			}

			InputController.Instance.ClearInputBuffer();
		}
	}

	private void SetupArrowsForFood(Food food)
	{
		_activeArrowCombos.Clear();

		var seq = food.template.GetInputSequence();
		for (int i = 0; i < seq.Length; ++i)
		{
			var inputs = seq[i].inputs;
			var prefab = _comboDict[HashInputs(inputs)];

			var parent = _arrowPath[i];
			var instance = Instantiate(prefab, parent);
			instance.transform.position = parent.transform.position;
			instance.rectTransform.sizeDelta *= 1.5f;
			instance.Init(inputs);
			_activeArrowCombos.Add(instance);
		}
		_inputSeqProgress = 0;
		_inputSeqCount = _activeArrowCombos.Count;
		_indicator.Stop();
		_indicator.Play();
	}
}
