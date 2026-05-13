using RPGCreationKit;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class AttributeInInventoryUI : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    ISelectHandler, IDeselectHandler
{
    [SerializeField]
    private bool usesTooltip = true;

    [SerializeField]
    public GameObject tooltipGameObject;

    [SerializeField]
    private TextMeshProUGUI tooltipText;

    [SerializeField, TextArea]
    string tooltipContent;

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        if (usesTooltip)
        {
            tooltipGameObject.SetActive(true);
            tooltipText.text = tooltipContent;
        }
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        if (usesTooltip)
            tooltipGameObject.SetActive(false);
    }

    public virtual void OnSelect(BaseEventData eventData)
    {
        if (usesTooltip)
        {
            tooltipGameObject.SetActive(true);
            tooltipText.text = tooltipContent;
        }
    }

    public virtual void OnDeselect(BaseEventData eventData)
    {
        if (usesTooltip)
            tooltipGameObject.SetActive(false);
    }
}
