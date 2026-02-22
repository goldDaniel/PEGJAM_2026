using System.Collections.Generic;
using UnityEngine;

public class CrowdGoblin : MonoBehaviour
{
	private enum Arm : int
	{
		Left = 0,
		Right = 1,
	}

	[Range(0.05f, 1f)][SerializeField] private float _headScaleMin;
	[Range(0.05f, 1f)][SerializeField] private float _headScaleMax;

	[SerializeField] private GameObject _leftShoulder;
	[SerializeField] private GameObject _rightShoulder;
	[SerializeField] private GameObject _neck;

	[SerializeField] private Sprite _headSprite;
	[SerializeField] private Sprite _bodySprite;
	[SerializeField] private Sprite _armSprite;

	[SerializeField] SpriteRenderer _headRenderer;
	[SerializeField] SpriteRenderer _bodyRenderer;
	[SerializeField] SpriteRenderer[] _armRenderers;

	private float _leftShoulderSeed;
	private float _rightShoulderSeed;
	private float _speed;

	private float _bodyBob;
	private Vector3 _basePos;
	private float _bobSpeed;

	void Start()
	{
		_headRenderer.sprite = _headSprite;
		this.transform.localScale = Vector2.one * Random.Range(_headScaleMin, _headScaleMax);

		_bodyRenderer.sprite = _bodySprite;
		_armRenderers[(int)Arm.Left].sprite = _armSprite;
		_armRenderers[(int)Arm.Right].sprite = _armSprite;

		_leftShoulderSeed = Random.value * 123f;
		_rightShoulderSeed = Random.value * 456f;

		_speed = Random.Range(0.8f, 1.2f);

		_bodyBob = Random.value * 789f;
		_bobSpeed = Random.Range(0.5f, 3f);

		List<SpriteRenderer> renderers = new List<SpriteRenderer>();
		renderers.Add(_headRenderer);
		renderers.Add(_bodyRenderer);
		renderers.AddRange(_armRenderers);

		float red = Random.Range(0.7f, 0.9f);
		float green = Random.Range(0.5f, 1f);
		float blue = Random.Range(0.5f, 0.8f);
		Color color = new Color(red, green, blue, 0.8f);
		foreach (var renderer in renderers )
			renderer.color = color;
	}


	void Update()
	{
		if (_basePos == Vector3.zero)
			_basePos = transform.position;

		_leftShoulderSeed += Time.deltaTime * _speed;
		_rightShoulderSeed += Time.deltaTime * _speed;

		float leftT = Mathf.Sin(_leftShoulderSeed);
		float rightT = Mathf.Sin(_rightShoulderSeed);

		float leftAngle = leftT * 15f;
		float rightAngle = rightT * 15f;

		_leftShoulder.transform.rotation = Quaternion.Euler(0, 0, leftAngle);
		_rightShoulder.transform.rotation = Quaternion.Euler(0, 0, rightAngle);

		_bodyBob += Time.deltaTime * _bobSpeed;
		transform.position = _basePos + Vector3.up * 0.5f * Mathf.Sin(_bodyBob);
	}
}
