using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(ScrollRect))]
public class ScrollToSelected : MonoBehaviour
{
    //https://twitter.com/AsherVo/status/1141791964637147136
    //https://pastebin.com/HjcktEYv
    [Header("Required: Children must have a Y Pivot of 1!")]
    public float scrollSpeed = 10f;

    ScrollRect m_ScrollRect;
    RectTransform m_RectTransform;
    RectTransform m_ContentRectTransform;
    RectTransform m_SelectedRectTransform;

    void Awake()
    {
        m_ScrollRect = GetComponent<ScrollRect>();
        m_RectTransform = GetComponent<RectTransform>();
        m_ContentRectTransform = m_ScrollRect.content;
    }

    void Update()
    {
        UpdateScrollToSelected();
    }

    void UpdateScrollToSelected()
    {
        GameObject selected = EventSystem.current.currentSelectedGameObject;

        if (selected == null)
            return;


        m_SelectedRectTransform = selected.GetComponent<RectTransform>();

        float contentHeightDifference = GetContentHeightDifference();

        float selectedTop = m_SelectedRectTransform.anchoredPosition.y;
        float selectedBottom = selectedTop - m_SelectedRectTransform.rect.height;
        float viewportTop = NormalizedToPosition(m_ScrollRect.verticalNormalizedPosition, contentHeightDifference);
        float viewportBottom = viewportTop - m_ScrollRect.viewport.rect.height;

        if (selectedTop > viewportTop)
        {
            float goalY = selectedTop;

            m_ScrollRect.verticalNormalizedPosition = Mathf.Lerp(
                m_ScrollRect.verticalNormalizedPosition,
                PositionToNormalized(goalY, contentHeightDifference),
                scrollSpeed * Time.deltaTime
            );
        }
        else if (selectedBottom < viewportBottom && !Mathf.Approximately(selectedTop, selectedBottom))
        {
            float diff = selectedBottom - viewportBottom;
            float goalY = viewportTop + diff;

            m_ScrollRect.verticalNormalizedPosition = Mathf.Lerp(
                m_ScrollRect.verticalNormalizedPosition,
                PositionToNormalized(goalY, contentHeightDifference),
                scrollSpeed * Time.deltaTime
            );
        }
    }

    float GetContentHeightDifference()
    {
        return m_ContentRectTransform.rect.height - m_RectTransform.rect.height;
    }

    float NormalizedToPosition(float normPos, float contentHeightDifference)
    {
        return (normPos - 1.0f) * contentHeightDifference;
    }

    float PositionToNormalized(float pos, float contentHeightDifference)
    {
        return (pos / contentHeightDifference) + 1.0f;
    }

    public bool IsClippingTop()
    {
        return m_ScrollRect.verticalNormalizedPosition < 1.0f;
    }
}