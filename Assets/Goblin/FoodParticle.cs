using UnityEngine;

public class FoodParticle : MonoBehaviour
{
	[SerializeField]
	private FoodParticleTemplate template;

	private Vector2 _velocity;
	private float _lifetime;

	public void Init(Vector2 startPos, Vector2 initVel, float lifetime = 0.5f, float scale = 1f)
	{
		this._lifetime = lifetime;
		this._velocity = initVel;

		this.transform.position = startPos;
		transform.localScale = Vector3.one * scale;

		var sr = this.gameObject.GetComponent<SpriteRenderer>();
		sr.sprite = template.sprites[Random.Range(0, template.sprites.Length - 1)];
		sr.sortingOrder = 0;
	}

	void Update()
	{
		this.transform.position += (Vector3)_velocity * Time.deltaTime;
		_velocity += Vector2.down * 9.8f * Time.deltaTime;

		_lifetime -= Time.deltaTime;
		if (_lifetime <= 0)
		{
			Destroy(this.gameObject);
		}
	}
}
