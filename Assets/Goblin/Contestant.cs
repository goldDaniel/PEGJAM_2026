using System.Collections.Generic;
using System.Runtime.CompilerServices;
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

	// how long it takes the contestant to finish their plate. Only applicable to NPCS
	[Range(5f, 30f)] 
	[SerializeField] private float _eatingTime;

	private Dictionary<ContestantAnimations, Sprite> _sprites;

	private bool _isBiting;

	public bool IsHoldingFood => _currentFood != null;
	public bool HasEatenAllFood => !IsHoldingFood && _foodPile.Count == 0;

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
		}
		
		_foodPile.Capacity = level.foodItems.Count;
		for (int i = 0; i < level.foodItems.Count; ++i)
		{
			var food = Instantiate(foodPrefab, foodSpawn, true);
			food.transform.position = foodSpawn.position.xy() + Random.insideUnitCircle * new Vector2(0.8f, 0.5f);
			food.Init(level.foodItems[i]);
			_foodPile.Add(food);
		}
	}

	public Food GetHeldFood() => _currentFood;

	public void GrabNextFoodItem()
	{
		_currentFood = _foodPile[0];
		_foodPile.RemoveAt(0);
		_currentFood.SetPosition(_foodHoldingPosition);
	}

	public void Bite()
	{
		Eat();
		_currentFood.Bite();
	}

	public void FinishFood()
	{
		Destroy(_currentFood.gameObject);
		_currentFood = null;
	}

	public void Eat()
	{
		if (_bodyRenderer == null)
			return;
	

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
	}
}
