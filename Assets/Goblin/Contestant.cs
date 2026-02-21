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

public class Contestant : MonoBehaviour
{
	[SerializeField] private SpriteRenderer _bodyRenderer;
	[SerializeField] private SpriteRenderer _armRenderer;

	[SerializeField] private Transform _foodHoldingPosition;

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

	void Awake()
	{
		_sprites = new();
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
		}
		
		_foodPile.Capacity = level.foodItems.Count;
		_initialFoodCount = level.foodItems.Count;
		_timePerFood = _eatingTime / _initialFoodCount;
		_currentEatingTimer = _timePerFood;
		for (int i = 0; i < _initialFoodCount; ++i)
		{
			var food = Instantiate(foodPrefab, foodSpawn, true);
			food.transform.position = foodSpawn.position.xy() + Random.insideUnitCircle * new Vector2(0.8f, 0.5f);
			food.Init(level.foodItems[i]);
			_foodPile.Add(food);
		}
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
