using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Game : MonoSingleton<Game>
{
	public struct GameArrow
	{
		public GameplayUI gameElement;
		public MashAttack spider;
	};

	[SerializeField] private UITutorialController _tutorial;
	[SerializeField] private bool _isTutorial = false;

	[SerializeField] private LevelEndPanel _levelEndPanel;
	[SerializeField] private GameObject _countdown3;
	[SerializeField] private GameObject _countdown2;
	[SerializeField] private GameObject _countdown1;
	[SerializeField] private GameObject _countdownFeast;

	[SerializeField] private ComboSystem _comboSystem;
	[SerializeField] private Sayan _goblinMode;

	[SerializeField] private RectTransform[] _arrowPath;
	[SerializeField] private LevelLoader _levels;
	private Level _currentLevel => _levels.CurrentLevel;
	public bool IsLastLevel => _levels.IsLastLevel;

	[SerializeField] private Level _tutorialLevel;

	[SerializeField] private ContestantTemplate _playerTemplate;

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

	private MashAttack _fire = null;
	[SerializeField] private MashAttack _spiderPrefab;
	[SerializeField] private MashAttack _firePrefab;
	[SerializeField] private GameObject _rockPrefab;

	[SerializeField] private GameObject _grabFoodIndicator;
	[SerializeField] private GameObject _bringFoodIndicator;
	[SerializeField] private GameObject _eatIndicator;
	[SerializeField] private GameObject _mashIndicator;
	private GameObject _activeIndicator = null;
	private float _activeIndicatorCooldown = 1f;

	private int _inputSeqProgress;
	private int _inputSeqCount;
	private List<GameArrow> _activeArrowCombos = new();
	private Dictionary<int, GameplayUI> _comboDict = new();

	private bool _wasWaiting = false;

	private Camera _camera;
	private Vector3 _cameraOriginalPos;
	private float _cameraOriginalSize;
	[SerializeField] private float _swayAmount;
	[SerializeField] private float _swaySpeedX;
    [SerializeField] private float _swaySpeedY;
    [SerializeField] private float _zoomAmount;
	[SerializeField] private float _zoomSpeed;

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
		_comboSystem.AnimateGoblinMeter(0.1f);
		
		while (SceneTransitionManager.Instance.IsTransitioning)
			yield return null;

		if (_isTutorial)
		{
			for (int i = 0; i < 30; ++i)
				yield return null;

			yield return HandleTutorial();
			SceneTransitionManager.LoadScene("Gameplay");
		}
		else
		{
			AudioManager.Instance.PlayMusicCrossfade("GameplayMusic");
			UIController.Instance.ShowVersusPanel(_currentLevel, _playerTemplate.EntryImage);
		}
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

	private void SetupCameraData()
	{
        _camera = Camera.main;
        _cameraOriginalPos = _camera.transform.position;
        _cameraOriginalSize = _camera.orthographicSize;
    }

	void Update()
	{
		if (_isTutorial)
		{
			if (OnPunishmentCooldown)
				_missCooldownTimer = Mathf.Max(_missCooldownTimer - Time.deltaTime, 0f);
			return;
		}

		float swayX = Mathf.Sin(Time.time * _swaySpeedX) * _swayAmount;
        float swayY = Mathf.Sin(Time.time * _swaySpeedY) * _swayAmount;
        float zoom = Mathf.Sin(Time.time * _zoomSpeed) * _zoomAmount;

		if (_camera == null) 
			SetupCameraData();
		_camera.transform.position = _cameraOriginalPos + new Vector3(swayX, swayY, 0);
		_camera.orthographicSize = _cameraOriginalSize + zoom;

		if (IsPaused || !HasStarted)
			return;

		bool stageComplete = _player.HasEatenAllFood || _opponent.HasEatenAllFood;
		if (stageComplete)
		{
			if (!_levelEndPanel.IsDisplayed)
			{
				if (_player.HasEatenAllFood)
				{
					AudioManager.Instance.Play("CrowdClap");
					_levelEndPanel.OnWin();
				}
				else if (_opponent.HasEatenAllFood)
				{
					AudioManager.Instance.Play("CrowdBoo");
					_levelEndPanel.OnLose();
				}					
			}

			return;
		}

		_opponent.OpponentGameplayTick();

		if (OnPunishmentCooldown)
		{
			_missCooldownTimer = Mathf.Max(_missCooldownTimer - Time.deltaTime, 0f);
			return;
		}

		if (_handState == HandState.Empty)
		{
			SetIndicator(_grabFoodIndicator);
			_activeIndicatorCooldown = 1f;
		}
		else if (_handState == HandState.Reaching)
		{ 
			SetIndicator(_bringFoodIndicator);
			_activeIndicatorCooldown = 1f;
		}
		else if (!ActiveThreat())
		{
			if(_activeIndicatorCooldown > 0f)
			{
				SetIndicator(_eatIndicator);
				_activeIndicatorCooldown -= Time.deltaTime;
			}
			else 
			{
				SetIndicator(null);
				_activeIndicatorCooldown = 0f;
			}
		}
		else
		{
			SetIndicator(_mashIndicator);
			_activeIndicatorCooldown = 1f;
		}
			

		var inputs = InputController.Instance.GetInputs();
		bool waiting = InputController.Instance.inWindow();
		if (inputs.Contains(GameInput.Action))
		{
			HandleAttack();
		}
		else if (inputs.Count > 0 || _wasWaiting)
		{
			if (!IsReadyToEat)
			{
				HandleGrabbing(inputs);
				InputController.Instance.ClearInputBuffer();
			}
			else if (IsReadyToEat && _activeArrowCombos.Count > 0)
			{
				_wasWaiting = waiting;
				HandleEating(inputs, waiting);
			}
		}

		if (inputs.Contains(GameInput.Action))
			InputController.Instance.ClearInputBuffer();
	}

	private bool ActiveThreat()
	{
		return ActiveFire() || ActiveSpider();
	}

	private bool ActiveFire()
	{
		return _fire != null;
	}

	private bool ActiveSpider()
	{
		return _activeArrowCombos.Count > 0 && _activeArrowCombos[0].spider != null;
	}

	private void HandleAttack()
	{
		if (ActiveFire())
		{
			_fire.Damage();
			if (_fire.Finished())
			{
				Destroy(_fire.gameObject);
				_fire = null;
			}
		}
		else if (ActiveSpider())
		{
			var arrow = _activeArrowCombos[0];
			arrow.spider.Damage();
			if (arrow.spider.Finished())
			{
				Destroy(arrow.spider.gameObject);
				arrow.spider = null;
			}
		}
	}

	public void SetIndicator(GameObject indicator)
	{
		if (_activeIndicator == indicator)
			return;

		_grabFoodIndicator.gameObject.SetActive(false);
		_bringFoodIndicator.gameObject.SetActive(false);
		_eatIndicator.gameObject.SetActive(false);
		_mashIndicator.gameObject.SetActive(false);

		indicator?.gameObject.SetActive(true);
		_activeIndicator = indicator;
	}

	public void PrepareLevel()
	{
		var level = Instantiate(_currentLevel);

		_player.Setup(_playerTemplate, _foodPrefab, level, _playerFoodSpawn);
		_opponent.Setup(level.opponent, _foodPrefab, level, _opponentFoodSpawn);

		_goblinMode.SetContestant(_player);
	}

	public void StartGameplayCountdown()
	{
		StartCoroutine(GameplayCountdown());
	}

	private IEnumerator GameplayCountdown()
	{
		while(!_player.AllFoodSetup)
			yield return null;

		_countdown3.gameObject.SetActive(true);
		yield return new WaitForSeconds(1);

		_countdown3.gameObject.SetActive(false);
		_countdown2.gameObject.SetActive(true);
		yield return new WaitForSeconds(1);

		_countdown2.gameObject.SetActive(false);
		_countdown1.gameObject.SetActive(true);
		yield return new WaitForSeconds(1);

		_countdown1.gameObject.SetActive(false);
		_countdownFeast.gameObject.SetActive(true);
		yield return new WaitForSeconds(1);

		_countdownFeast.gameObject.SetActive(false);
		_hasStarted = true;
	}

	private void HandleGrabbing(List<GameInput> inputs)
	{
		if (_handState == HandState.Empty)
		{
			if (inputs.Contains(GameInput.Right))
			{
				_handState = HandState.Reaching;
				_player.Reach();
			}
		}
		else if (_handState == HandState.Reaching)
		{
			if (inputs.Contains(GameInput.Left))
			{
				_handState = HandState.Holding;
				_player.GrabNextFoodItem();
				_player.Hold();
				SetupArrowsForFood(_player.GetHeldFood());
			}
		}
	}

	private void HandleEating(List<GameInput> inputs, bool inWindow)
	{
		var element = _activeArrowCombos[0].gameElement;
		var desiredCombo = element.GetInputs();

		bool wrongCombo = false;
		foreach (var input in inputs)
			wrongCombo |= !desiredCombo.Contains(input);

		bool comboPressed = true;
		foreach (var input in desiredCombo)
			comboPressed &= inputs.Contains(input);

		bool threat = ActiveThreat();
		if (wrongCombo || (!comboPressed && !inWindow) || threat)
		{
			_missCooldownTimer = _missCooldownTime;
			_indicator.MissedInput();
			_comboSystem.ResetCombo();
			if (_goblinMode.IsGoblinMode)
				_goblinMode.ExitGoblinMode(_player);

			_wasWaiting = false;
			return;
		}
		else if(comboPressed)
		{ 
			_indicator.CorrectInput();
			_comboSystem.IncreaseCombo();
			_player.ToggleMouth();
			if (_comboSystem.IsMaxCombo && !_goblinMode.IsGoblinMode)
				_goblinMode.EnterGoblinMode(_player);
				

			_player.Bite((float)_inputSeqProgress++ / (_inputSeqCount - 1));
			element.OnValidPress();

			Destroy(element.gameObject);
			_activeArrowCombos.RemoveAt(0);

			for (int i = 0; i < _activeArrowCombos.Count; ++i)
			{
				var arrowCombo = _activeArrowCombos[i].gameElement;
				arrowCombo.MoveToPosition(_arrowPath[i].transform.position);
			}

			if (_activeArrowCombos.Count == 0)
			{
				_player.FinishFood();
				_handState = HandState.Empty;
			}

			InputController.Instance.ClearInputBuffer();
			_wasWaiting = false;

			if (!_isTutorial)
				OpponentAttack(_opponent.Attack());
        }
	}

	private void OpponentAttack(AttackType attack)
	{
		switch(attack)
		{
			case AttackType.Spider: SpawnSpider(); break;
			case AttackType.Fire: SpawnFire(); break;
			case AttackType.Rock: SpawnRock();  break;
			default: break;
		}
	}

	private void SpawnSpider()
	{
		List<int> validIndexes = new();
		for (int i = 1; i < _activeArrowCombos.Count; i++)
			if (_activeArrowCombos[i].spider == null)
				validIndexes.Add(i);

		if (validIndexes.Count > 0)
		{
			int index = validIndexes[Random.Range(0, validIndexes.Count - 1)];
			var arrow = _activeArrowCombos[index];
			arrow.spider = Instantiate(_spiderPrefab, _activeArrowCombos[index].gameElement.rectTransform);
			arrow.spider.transform.SetSiblingIndex(1);
			_activeArrowCombos[index] = arrow;
		}
	}

	private void SpawnFire()
	{
		if (_activeArrowCombos.Count > 0)
		{
			if (_fire == null)
			{
				_fire = Instantiate(_firePrefab, _activeArrowCombos[0].gameElement.rectTransform);
				_fire.transform.SetAsLastSibling();
			}
			else
				_fire.Reset();
		}
	}

	private void SpawnRock()
	{

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
			GameArrow arrow = new();
			arrow.gameElement = instance; 
			_activeArrowCombos.Add(arrow);
		}
		_inputSeqProgress = 0;
		_inputSeqCount = _activeArrowCombos.Count;
		_indicator.Stop();
		_indicator.Play();
	}

	public IEnumerator HandleTutorial()
	{
		_tutorial.StartTutorial();

		while (_tutorial.CurrentStepIndex != 2)
			yield return null;
		
		_player.Setup(_playerTemplate, _foodPrefab, _tutorialLevel, _playerFoodSpawn);
		while (!_player.AllFoodSetup)
			yield return null;

		_tutorial.AdvanceStep();

		while (_tutorial.CurrentStepIndex == 3)
			yield return null;

		SetIndicator(_grabFoodIndicator);

		while (_tutorial.CurrentStepIndex == 4)
			yield return null;

		_handState = HandState.Reaching;
		_player.Reach();
		SetIndicator(_bringFoodIndicator);

		while (_tutorial.CurrentStepIndex == 5)
			yield return null;

		_handState = HandState.Holding;
		_player.GrabNextFoodItem();
		_player.Hold();
		SetupArrowsForFood(_player.GetHeldFood());
		SetIndicator(_eatIndicator);

		while (_tutorial.CurrentStepIndex == 6)
			yield return null;

		SetIndicator(null);

		while (!_player.HasEatenAllFood)
		{
			var inputs = InputController.Instance.GetInputs();
			bool waiting = InputController.Instance.inWindow();
			if (inputs.Count > 0 || _wasWaiting)
			{
				if (!IsReadyToEat)
				{
					HandleGrabbing(inputs);
					InputController.Instance.ClearInputBuffer();
				}
				else if (IsReadyToEat && _activeArrowCombos.Count > 0)
				{
					_wasWaiting = waiting;
					HandleEating(inputs, waiting);
				}
			}

			if (inputs.Contains(GameInput.Action))
				InputController.Instance.ClearInputBuffer();

			yield return null;
		}

		_tutorial.AdvanceStep();

		while (_tutorial.CurrentStepIndex == 8)
			yield return null;


		var spider = Instantiate(_spiderPrefab);
		var rt = spider.GetComponent<RectTransform>();
		rt.SetParent(_tutorial.GetComponent<RectTransform>());
		rt.anchoredPosition = Vector2.zero;
		rt.sizeDelta = Vector2.one * 300;

		while (_tutorial.CurrentStepIndex == 9)
			yield return null;

		Destroy(spider.gameObject);

		while (_tutorial.CurrentStepIndex == 10)
			yield return null;
	}
}
