
using UnityEngine;

public class Sayan : MonoBehaviour
{
	[Range(0.01f, 1f)]
	[SerializeField]
	private float _flipInterval;

	[SerializeField]
	private SpriteRenderer[] _renderers;

	private int _currentRendererIndex;
	private float _flipTimer;

	[SerializeField] private Sprite _sayanOpen;
	[SerializeField] private Sprite _sayanClosed;

	Sprite _originalOpen;
	Sprite _originalClosed;

	public bool IsGoblinMode => _renderers[0].gameObject.activeSelf;

	void Awake()
	{
		_flipTimer = _flipInterval;
	}

	public void EnterGoblinMode(Contestant contestant)
	{
		contestant.UpdateBody(_sayanOpen, _sayanClosed);
		foreach(var renderer in _renderers) 
			renderer.gameObject.SetActive(true);

		AudioManager.Instance.Play("GoblinMode");
	}

	public void ExitGoblinMode(Contestant contestant)
	{
		contestant.UpdateBody(_originalOpen, _originalClosed);
		foreach (var renderer in _renderers)
			renderer.gameObject.SetActive(false);
	}


	public void SetContestant(Contestant contestant)
	{
		_originalOpen = contestant.GetSprite(ContestantAnimations.EatingDown);
		_originalClosed = contestant.GetSprite(ContestantAnimations.EatingUp);
	}

	void Update()
	{
		if( _flipTimer > 0)
			_flipTimer -= Time.deltaTime;
		else
		{
			_currentRendererIndex = (_currentRendererIndex + 1) % _renderers.Length;
			_renderers[_currentRendererIndex].flipX = !_renderers[_currentRendererIndex].flipX;
			_flipTimer = _flipInterval;	
		}
	}
}

