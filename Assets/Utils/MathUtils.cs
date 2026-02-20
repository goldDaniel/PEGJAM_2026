using UnityEngine;

public static class MathUtils
{
	// Freya Holmer: https://www.youtube.com/watch?v=LSNQuFEDOyQ at the 50min mark
	public static float ExpDecay(float a, float b, float decay, float dt)
	{
		return b + (a - b) * Mathf.Exp(-decay * dt);
	}

	public static Vector2 ExpDecay(Vector2 a, Vector2 b, float decay, float dt)
	{
		float x = ExpDecay(a.x, b.x, decay, dt);
		float y = ExpDecay(a.y, b.y, decay, dt);

		return new Vector2(x, y);
	}

	public static Vector3 ExpDecay(Vector3 a, Vector3 b, float decay, float dt)
	{
		float x = ExpDecay(a.x, b.x, decay, dt);
		float y = ExpDecay(a.y, b.y, decay, dt);
		float z = ExpDecay(a.z, b.z, decay, dt);

		return new Vector3(x, y, z);
	}

	public static float ExpDecayAngle(float a, float b, float decay, float dt)
	{
		float delta = Mathf.DeltaAngle(a, b);
		float factor = 1f - Mathf.Exp(-decay * dt); // Compute decay factor correctly
		return a + delta * factor;
	}

	public static float Dist2(Vector2 a, Vector2 b)
	{
		float x = (a.x - b.x);
		x *= x;

		float y = (a.y - b.y);
		y *= y;

		return x + y;
	}

	public static float Dist2(Vector3 a, Vector3 b)
	{
		float x = (a.x - b.x);
		x *= x;

		float y = (a.y - b.y);
		y *= y;

		float z = (a.z - b.z);
		z *= z;

		return x + y + z;
	}

	public static bool ApproximatelyZero(float value, float epsilon = 0.00001f)
	{
		return Mathf.Abs(value) < epsilon;
	}
}

// swizzles
public static class VectorExtensions
{
	public static Vector2 xx(this Vector3 v) => new Vector2(v.x, v.x);
	public static Vector2 xy(this Vector3 v) => new Vector2(v.x, v.y);
	public static Vector2 xz(this Vector3 v) => new Vector2(v.x, v.z);

	public static Vector2 yx(this Vector3 v) => new Vector2(v.y, v.x);
	public static Vector2 yy(this Vector3 v) => new Vector2(v.y, v.y);
	public static Vector2 yz(this Vector3 v) => new Vector2(v.y, v.z);

	public static Vector2 zx(this Vector3 v) => new Vector2(v.z, v.x);
	public static Vector2 zy(this Vector3 v) => new Vector2(v.z, v.y);
	public static Vector2 zz(this Vector3 v) => new Vector2(v.z, v.z);
}
