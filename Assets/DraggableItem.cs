using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    public System.Action OnDropCallback;

    private Transform _parentToReturn;
    private GameObject _placeholder;
    private LayoutElement _layoutElement;
    private CanvasGroup _canvasGroup;

    private RectTransform Rect => transform as RectTransform;

    private void Awake()
    {
        _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        _layoutElement = gameObject.GetComponent<LayoutElement>();
        if (_layoutElement == null) _layoutElement = gameObject.AddComponent<LayoutElement>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _placeholder = new GameObject("Placeholder", typeof(LayoutElement));
        var phLE = _placeholder.GetComponent<LayoutElement>();
        phLE.preferredHeight = _layoutElement.preferredHeight;
        phLE.preferredWidth = _layoutElement.preferredWidth;
        phLE.flexibleHeight = 0;
        phLE.flexibleWidth = 0;

        _parentToReturn = transform.parent;
        int index = transform.GetSiblingIndex();
        _placeholder.transform.SetParent(_parentToReturn);
        _placeholder.transform.SetSiblingIndex(index);

        transform.SetParent(_parentToReturn.parent); // move to root canvas so it can overlay
        _canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (Rect != null)
        {
            Rect.position = eventData.position;
        }

        // Move placeholder to appropriate position
        for (int i = 0; i < _parentToReturn.childCount; i++)
        {
            var child = _parentToReturn.GetChild(i);
            if (child == _placeholder.transform) continue;
            if (Rect.position.y > child.position.y)
            {
                _placeholder.transform.SetSiblingIndex(i);
                break;
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(_parentToReturn);
        transform.SetSiblingIndex(_placeholder.transform.GetSiblingIndex());
        Destroy(_placeholder);

        _canvasGroup.blocksRaycasts = true;

        OnDropCallback?.Invoke();
    }
} 