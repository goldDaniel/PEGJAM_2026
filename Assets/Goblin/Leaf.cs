using UnityEngine;

public class Leaf : MonoBehaviour
{
    [SerializeField] private GameObject _leaf;

    [SerializeField] private float _fallSpeed = 1.5f;
    [SerializeField] private float _horizontalDrift = 0.5f;
    [SerializeField] private float settleSpeed = 2f;
    [SerializeField] private float flutterAmplitude = 15f;
    [SerializeField, Range(0f, 1f)] private float autumnProbability = 0.4f;
    [SerializeField, Range(0f, 1f)] private float maxAutumnStrength = 0.7f;

    private float _baseAngle;
    private float _targetAngle;
    private float _angleSeed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _angleSeed = Random.Range(0f, 2 * Mathf.PI);

        _baseAngle = Random.Range(0f, 360f);
        _targetAngle = Random.value > 0.5f ? 0f : 180f;

        TryApplyAutumnColour();
    }

    private void TryApplyAutumnColour()
    {
        if (Random.value > autumnProbability)
            return;

        var spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            return;

        Color[] autumnColors = new Color[]
        {
            new Color(0.75f, 0.15f, 0.1f),  // deep red
            new Color(0.9f, 0.4f, 0.1f),    // orange
            new Color(0.8f, 0.7f, 0.15f),   // yellow
            new Color(0.6f, 0.2f, 0.05f)    // brown-red
        };

        Color original = spriteRenderer.color;
        Color target = autumnColors[Random.Range(0, autumnColors.Length)];

        float strength = Random.Range(0f, maxAutumnStrength);

        spriteRenderer.color = Color.Lerp(original, target, strength);
    }

    // Update is called once per frame
    void Update()
    {
        _angleSeed += Time.deltaTime;
        float t = 1f - Mathf.Exp(-settleSpeed * Time.deltaTime); // settle slower as it rotates
        _baseAngle = Mathf.LerpAngle(_baseAngle, _targetAngle, t);

        float flutter = Mathf.Sin(_angleSeed) * flutterAmplitude;
        float finalAngle = _baseAngle + flutter;
        transform.rotation = Quaternion.Euler(0, 0, finalAngle);

        float drift = (Mathf.PerlinNoise(_angleSeed, 0f) - 0.5f) * 2f * _horizontalDrift;
        transform.position += new Vector3(drift, -_fallSpeed, 0f) * Time.deltaTime;

        CheckIfOffscreen();
    }

    private void CheckIfOffscreen()
    {
        Vector3 viewportPos = Camera.main.WorldToViewportPoint(transform.position);

        if (viewportPos.y < -0.1f)
        {
            Destroy(gameObject);
        }
    }
}
