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

	void Start()
	{
		_headRenderer.sprite = _headSprite;
        this.transform.localScale = Vector2.one * Random.Range(_headScaleMin, _headScaleMax);

		_bodyRenderer.sprite = _bodySprite;
		_armRenderers[(int)Arm.Left].sprite = _armSprite;
        _armRenderers[(int)Arm.Right].sprite = _armSprite;

		_leftShoulderSeed = Random.Range(0f, Mathf.PI / 2);
		_rightShoulderSeed = _leftShoulderSeed + Random.Range(0f, Mathf.PI / 2);
    }


	void Update()
	{
		_leftShoulderSeed += Time.deltaTime;
		_rightShoulderSeed += Time.deltaTime;

		float leftT = Mathf.Sin(_leftShoulderSeed);
		float rightT = Mathf.Sin(_rightShoulderSeed);

		float leftAngle = leftT * 15f;
		float rightAngle = rightT * 15f;

        _leftShoulder.transform.rotation = Quaternion.Euler(0, 0, leftAngle);
        _rightShoulder.transform.rotation = Quaternion.Euler(0, 0, rightAngle);
	}
}
