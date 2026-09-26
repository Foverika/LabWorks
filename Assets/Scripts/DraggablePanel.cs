using UnityEngine;
using UnityEngine.EventSystems;

public class DraggablePanel : MonoBehaviour, IDragHandler, IBeginDragHandler
{
    private RectTransform targetPanel;
    private Vector2 pointerOffset;

    void Awake()
    {
        targetPanel = transform.parent.GetComponent<RectTransform>();
        if (targetPanel == null)
            targetPanel = GetComponent<RectTransform>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            targetPanel, eventData.position, eventData.pressEventCamera, out pointerOffset);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (targetPanel == null) return;

        Vector2 localPointerPosition;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            targetPanel.parent.GetComponent<RectTransform>(), eventData.position, eventData.pressEventCamera, out localPointerPosition))
        {
            targetPanel.localPosition = localPointerPosition - pointerOffset;
        }
    }
}