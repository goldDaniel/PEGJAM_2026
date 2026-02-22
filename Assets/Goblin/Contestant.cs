using System.Collections.Generic;
using UnityEngine;

public enum ContestantAnimations
{
	EatingLeft,
	EatingRight,
	EatingUp,
	EatingDown,
	Empty, 
	Reaching,
	Damage, // Throwing up? Maybe name better
}

public enum AttackType
{
	Spider,
	Fire, 
	Rock,
	None
};

public class Contestant : MonoBehaviour
{
	[SerializeField] private SpriteRenderer _bodyRenderer;
	[SerializeField] private SpriteRenderer _armRenderer;

	[SerializeField] private Transform _foodHoldingPosition;

	private int[] _attackWeights;
	float _attackRate;

	private Food _currentFood = null;
	private List<Food> _foodPile = new();
	private int _initialFoodCount = 0;

	private float _timePerFood;
	private float _eatingTime;
	private float _currentEatingTimer = 0;

	private bool _isReaching = false;
	private float _reachTimer = 0;
	private float _reachTime = 0.5f;

	private Dictionary<ContestantAnimations, Sprite> _sprites;

	private bool _isBiting;
	private float _biteTime;
	private float _biteTimer = 0;

	public bool IsHoldingFood => _currentFood != null;
	public bool HasEatenAllFood => !IsHoldingFood && _foodPile.Count == 0;

	[SerializeField] private bool _isPlayer = false;
	private bool _isNPC => !_isPlayer;

	public bool AllFoodSetup
	{
		get
		{
			foreach (var food in _foodPile)
			{
				if (!food.FallComplete)
					return false;
			}
			return true;
		}
	}

	void Awake()
	{
		_sprites = new();
	}

	public void Reach()
	{
		_armRenderer.sprite = _sprites[ContestantAnimations.Reaching];
	}

	public void Hold()
	{
		_armRenderer.sprite = _sprites[ContestantAnimations.Empty];
	}

	public void ToggleMouth()
	{
		if (_bodyRenderer.sprite == _sprites[ContestantAnimations.EatingUp])
			_bodyRenderer.sprite = _sprites[ContestantAnimations.EatingDown];
		else
			_bodyRenderer.sprite = _sprites[ContestantAnimations.EatingUp];
	}

	public void Sad()
	{
		_bodyRenderer.sprite = _sprites[ContestantAnimations.Damage];
	}

	public Sprite GetSprite(ContestantAnimations anim) => _sprites[anim];

	public void UpdateBody(Sprite open, Sprite closed)
	{
		_sprites[ContestantAnimations.EatingUp] = closed;
		_sprites[ContestantAnimations.EatingDown] = open;

		_bodyRenderer.sprite = open;
	}


	public void Setup(ContestantTemplate template, Food foodPrefab, Level level, Transform foodSpawn)
	{
		if (template != null) // setup new NPC
		{
			_sprites.Clear();
			if (template.EatingLeft != null)
				_sprites[ContestantAnimations.EatingLeft] = template.EatingLeft;
			if (template.EatingRight != null)
				_sprites[ContestantAnimations.EatingRight] = template.EatingRight;
			if (template.EatingUp != null)
				_sprites[ContestantAnimations.EatingUp] = template.EatingUp;
			if (template.EatingDown != null)
				_sprites[ContestantAnimations.EatingDown] = template.EatingDown;
			if (template.Empty != null)
				_sprites[ContestantAnimations.Empty] = template.Empty;
			if (template.Reaching != null)
				_sprites[ContestantAnimations.Reaching] = template.Reaching;
			if (template.Damage != null)
				_sprites[ContestantAnimations.Damage] = template.Damage;

			_armRenderer.sprite = _sprites[ContestantAnimations.Empty];
			_bodyRenderer.sprite = _sprites[ContestantAnimations.EatingUp];

			_eatingTime = template.EatingTime;
			_biteTime = template.BiteTime;

            _attackRate = template.attackRate;
            _attackWeights = new int[template.attackWeights.Length];
            for (int i = 0; i < _attackWeights.Length; i++)
            {
                _attackWeights[i] = template.attackWeights[i];
            }
        }
		
		_foodPile.Capacity = level.foodItems.Count;
		_initialFoodCount = level.foodItems.Count;
		_timePerFood = _eatingTime / _initialFoodCount;
		_currentEatingTimer = _timePerFood;
		for (int i = 0; i < _initialFoodCount; ++i)
		{
			var food = Instantiate(foodPrefab, foodSpawn, true);
			var tablePosition = foodSpawn.position.xy() + Random.insideUnitCircle * new Vector2(0.8f, 0.5f);
			food.transform.position = tablePosition + Vector2.up * (15 + Random.value * 3f);
			food.Init(level.foodItems[i], tablePosition);
			_foodPile.Add(food);
		}
	}

	public AttackType Attack()
	{
		float chance = Random.Range(0f, 1f);
		if (chance > _attackRate) return AttackType.None;

		int totalWeight = 0;
		for (int i = 0; i < (int)AttackType.Fire; i++) // hardcode out the fire and rock attacks
			totalWeight += _attackWeights[i];

		float[] probabilities  = new float[_attackWeights.Length];
		for (int i = 0; i < probabilities.Length; i++)
			probabilities[i] = (float)_attackWeights[i] / totalWeight;

		float rand = Random.Range(0f, 1f);
		for (int i = 0; i < probabilities.Length; i++)
			if (probabilities[i] > rand)
				return (AttackType)i;
			else
				rand -= probabilities[i];

		return AttackType.None;
	}

	public Food GetHeldFood() => _currentFood;

	public bool GrabNextFoodItem()
	{
		if (_foodPile.Count == 0)
			return false;

		_currentFood = _foodPile[0];
		_foodPile.RemoveAt(0);
		_currentFood.SetPosition(_foodHoldingPosition);
		return true;
	}

	public void Bite(float foodProgress)
	{
		_currentFood.Bite(foodProgress);
	}

	public void FinishFood()
	{
		if (_currentFood != null)
		{
			Destroy(_currentFood.gameObject);
			_currentFood = null;
		}
		_reachTimer = _reachTime;
	}

	public void Eat()
	{
		if (_bodyRenderer == null)
			return;

		_biteTimer -= Time.deltaTime;
		if (_biteTimer <= 0)
		{
			_currentFood.Bite((_timePerFood - _currentEatingTimer) / _timePerFood);
			if (_isBiting)
			{
				_bodyRenderer.sprite = _sprites[ContestantAnimations.EatingUp];
				_isBiting = false;
			}
			else
			{
				_bodyRenderer.sprite = _sprites[ContestantAnimations.EatingDown];
				_isBiting = true;
			}
			_biteTimer = _biteTime;
		}
	}

	public void OpponentGameplayTick()
	{
		if (_isPlayer)
			return;

		// no food currently, reach and grab food
		if (_currentFood == null)
		{
			if (!_isReaching)
			{
				if (_reachTimer > 0)
					_reachTimer -= Time.deltaTime;
				else
				{
					_isReaching = true;
					_reachTimer = _reachTime;
					_armRenderer.sprite = _sprites[ContestantAnimations.Reaching];
				}
			}
			else
			{
				if (_reachTimer > 0)
					_reachTimer -= Time.deltaTime;
				else
				{
					_armRenderer.sprite = _sprites[ContestantAnimations.Empty];
					GrabNextFoodItem();
					_isReaching = false;
				}
			}
		}
		else
		{
			if (_currentEatingTimer <= 0)
			{
				FinishFood();
				_currentEatingTimer = _timePerFood;
			}
			else
			{
				_currentEatingTimer -= Time.deltaTime;
				Eat();
			}
		}
	}
}
