using UnityEngine;

public class CameraController : MonoSingleton<CameraController>
{
	public Transform cam;

	public Transform target;

	[Range(1f, 25f)]
	public float decay;

	[SerializeField][Range(0.1f, 2f)] private float screenShakeDecrement = 1f;
	[SerializeField][Range(1f, 100f)] private float screenShakeFrequency = 1f;
	private float screenShakeIntensity = 0;
	
	private Vector3 camPosition;

	private bool cameraLock = false;

	[Range(0.1f, 45f)]
	public float rotationNoiseScalar = 10f;
	[Range(0.1f, 20f)]
	public float translationNoiseScalar = 8f;


	public bool enableRotation = true;
	public bool enableTranslation = true;

#if UNITY_EDITOR
	public bool overrideScreenShakeIntensity;
	[Range(0f, 1f)] public float intensityOverride;
#endif

	public void LockCamera() => cameraLock = true;
	public void UnlockCamera() => cameraLock = false;

	public void Awake()
	{
		if(cam != null)
			camPosition = cam.position;
	}

	void Update()
	{
		if (target == null || !target)
			return;
		if (cam == null)
			return;

		float shakeRotation = !enableRotation? 0 : rotationNoiseScalar * (Mathf.PerlinNoise1D(123 + Time.time * screenShakeFrequency) * 2f - 1f);
		Vector3 shakeOffset = !enableTranslation ? Vector3.zero : translationNoiseScalar * 
												(new Vector2(
													Mathf.PerlinNoise1D(456 + Time.time * screenShakeFrequency), 
													Mathf.PerlinNoise1D(789 + Time.time * screenShakeFrequency)) * 2f - Vector2.one);

		if(!cameraLock)
			camPosition = MathUtils.ExpDecay(camPosition, target.position, decay, Time.deltaTime);
		
		
#if UNITY_EDITOR
		float intensity = overrideScreenShakeIntensity ? intensityOverride : screenShakeIntensity;
#else
		float intensity = screenShakeIntensity;
#endif

		cam.position = Vector3.Lerp(camPosition, camPosition + shakeOffset, intensity * intensity);
		cam.localEulerAngles = Vector3.Lerp(Vector3.zero, new Vector3(0, 0, shakeRotation), intensity * intensity);

#if UNITY_EDITOR
		if (!overrideScreenShakeIntensity)
#endif
		{
			screenShakeIntensity = Mathf.Clamp01(screenShakeIntensity - screenShakeDecrement * Time.deltaTime);
		}
	}

	public void ScreenShake(float intensity)
	{
		if (screenShakeIntensity < intensity)
			screenShakeIntensity = Mathf.Min(screenShakeIntensity + intensity, intensity);
	}
}
