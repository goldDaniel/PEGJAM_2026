using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

[AddComponentMenu("UI (Canvas)/Inverted Mask", 13)]
[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
[DisallowMultipleComponent]
public class InvertedMask : Mask
{
	protected override void OnEnable()
	{
		base.OnEnable();
		SyncChildModifiers();
	}

	protected override void OnDisable()
	{
		ClearChildModifiers();
		base.OnDisable();
	}

	protected void OnTransformChildrenChanged()
	{
		SyncChildModifiers();
	}

#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		SyncChildModifiers();
	}
#endif

	/// Keep native mask write/pop behavior (for stencil stack correctness),
	/// but suppress rendering of the mask source graphic itself.
	/// Inversion is applied on child graphics via InvertedMaskMaterialModifier.
	public override Material GetModifiedMaterial(Material baseMaterial)
	{
		if (!MaskEnabled())
			return baseMaterial;

		var rootSortCanvas = MaskUtilities.FindRootSortOverrideCanvas(transform);
		var stencilDepth = MaskUtilities.GetStencilDepth(transform, rootSortCanvas);
		if (stencilDepth >= 8)
		{
			Debug.LogWarning("Attempting to use a stencil mask with depth > 8", gameObject);
			return baseMaterial;
		}

		int desiredStencilBit = 1 << stencilDepth;

		if (desiredStencilBit == 1)
			return StencilMaterial.Add(baseMaterial, 1, StencilOp.Replace, CompareFunction.Always, 0);

		return StencilMaterial.Add(
			baseMaterial,
			desiredStencilBit | (desiredStencilBit - 1),
			StencilOp.Replace,
			CompareFunction.Equal,
			0,
			desiredStencilBit - 1,
			desiredStencilBit | (desiredStencilBit - 1));
	}

	private void SyncChildModifiers()
	{
		if (!isActiveAndEnabled)
			return;

		var graphics = GetComponentsInChildren<MaskableGraphic>(true);
		foreach (var childGraphic in graphics)
		{
			if (childGraphic == null || childGraphic == graphic)
				continue;

			var modifier = childGraphic.GetComponent<InvertedMaskMaterialModifier>();
			if (modifier == null)
				modifier = childGraphic.gameObject.AddComponent<InvertedMaskMaterialModifier>();

			modifier.SetOwner(this);
			childGraphic.SetMaterialDirty();
		}
	}

	private void ClearChildModifiers()
	{
		var modifiers = GetComponentsInChildren<InvertedMaskMaterialModifier>(true);
		foreach (var modifier in modifiers)
		{
			if (modifier == null || !modifier.IsOwnedBy(this))
				continue;

			var ownerGraphic = modifier.GetComponent<Graphic>();
			if (Application.isPlaying)
				Destroy(modifier);
			else
				DestroyImmediate(modifier);
			if (ownerGraphic != null)
				ownerGraphic.SetMaterialDirty();
		}
	}
}

[DisallowMultipleComponent]
[RequireComponent(typeof(MaskableGraphic))]
public class InvertedMaskMaterialModifier : MonoBehaviour, IMaterialModifier
{
	private InvertedMask m_Owner;
	private Material m_ModifiedMaterial;

	public bool IsOwnedBy(InvertedMask owner) => m_Owner == owner;

	public void SetOwner(InvertedMask owner)
	{
		if (m_Owner == owner)
			return;

		m_Owner = owner;
		if (TryGetComponent<Graphic>(out var graphic))
			graphic.SetMaterialDirty();
	}

	public Material GetModifiedMaterial(Material baseMaterial)
	{
		if (m_Owner == null || !m_Owner.isActiveAndEnabled || baseMaterial == null)
			return baseMaterial;

		if (!baseMaterial.HasProperty("_Stencil") ||
			!baseMaterial.HasProperty("_StencilComp") ||
			!baseMaterial.HasProperty("_StencilOp") ||
			!baseMaterial.HasProperty("_StencilReadMask") ||
			!baseMaterial.HasProperty("_StencilWriteMask") ||
			!baseMaterial.HasProperty("_ColorMask"))
			return baseMaterial;

		int stencil = baseMaterial.GetInt("_Stencil");
		var stencilOp = (StencilOp)baseMaterial.GetInt("_StencilOp");
		int readMask = baseMaterial.GetInt("_StencilReadMask");
		int writeMask = baseMaterial.GetInt("_StencilWriteMask");
		var colorMask = (ColorWriteMask)baseMaterial.GetInt("_ColorMask");

		StencilMaterial.Remove(m_ModifiedMaterial);
		m_ModifiedMaterial = StencilMaterial.Add(
			baseMaterial,
			stencil,
			stencilOp,
			CompareFunction.NotEqual,
			colorMask,
			readMask,
			writeMask);

		return m_ModifiedMaterial;
	}

	private void OnDisable()
	{
		StencilMaterial.Remove(m_ModifiedMaterial);
		m_ModifiedMaterial = null;
	}

	private void OnDestroy()
	{
		StencilMaterial.Remove(m_ModifiedMaterial);
		m_ModifiedMaterial = null;
	}
}
