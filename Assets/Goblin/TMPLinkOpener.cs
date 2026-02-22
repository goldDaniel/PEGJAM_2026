using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TMPLinkOpener : MonoBehaviour, IPointerClickHandler
{
	private TextMeshProUGUI _text;

	private void Awake()
	{
		_text = GetComponent<TextMeshProUGUI>();
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		int linkIndex = TMP_TextUtilities.FindIntersectingLink(_text, eventData.position, eventData.pressEventCamera);
		if (linkIndex != -1)
		{
			TMP_LinkInfo linkInfo = _text.textInfo.linkInfo[linkIndex];
			string url = linkInfo.GetLinkID();

			Application.OpenURL(url);
		}
	}
}
