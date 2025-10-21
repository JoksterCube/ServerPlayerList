using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace JoksterCube.ServerPlayerList.Common;

internal class DragNDrop : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    private RectTransform _target;
    private RectTransform _clampWithin;
    private bool _shouldReturn;

    private Canvas _rootCanvas;
    private bool _isDragging;
    private Vector2 _dragOffset;
    private Vector2 _startAnchoredPosition;

    public event Action<Vector2> OnPositionChanged;
    public event Action<Vector2> OnDragEnded;

    protected virtual void Awake()
    {
        if (!_target) _target = (RectTransform)transform;
        if (!_clampWithin) _clampWithin = _target.parent as RectTransform;

        _rootCanvas = _target.GetComponentInParent<Canvas>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!_rootCanvas) return;

        _isDragging = true;
        _startAnchoredPosition = _target.anchoredPosition;

        var localPoint = ScreenToLocal(eventData.position);
        _dragOffset = _target.anchoredPosition - localPoint;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_isDragging || !_rootCanvas) return;

        var localPoint = ScreenToLocal(eventData.position);
        var desired = localPoint + _dragOffset;
        var clamped = ClampAnchoredPosition(desired);

        _target.anchoredPosition = clamped;
        OnPositionChanged?.Invoke(clamped);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!_isDragging) return;
        _isDragging = false;

        if (_shouldReturn)
        {
            _target.anchoredPosition = _startAnchoredPosition;
            OnPositionChanged?.Invoke(_target.anchoredPosition);
        }

        OnDragEnded?.Invoke(_target.anchoredPosition);
    }

    internal void SetShouldReturn(bool value) => _shouldReturn = value;

    internal void SetAnchoredPosition(Vector2 anchoredPos, bool clamp = true, bool notify = false)
    {
        if (clamp) anchoredPos = ClampAnchoredPosition(anchoredPos);
        if (_target.anchoredPosition == anchoredPos) return;

        _target.anchoredPosition = anchoredPos;
        if (notify) OnPositionChanged?.Invoke(anchoredPos);
    }

    private Vector2 ScreenToLocal(Vector2 screenPos)
    {
        var cam = _rootCanvas && _rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? _rootCanvas.worldCamera
            : null;

        var reference = _clampWithin ? _clampWithin : (RectTransform)_target.parent;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(reference, screenPos, cam, out var localPoint);
        return localPoint;
    }

    private Vector2 ClampAnchoredPosition(Vector2 desired)
    {
        if (!_clampWithin) return desired;

        var clampSize = _clampWithin.rect.size;
        var targetSize = _target.rect.size;

        var minX = -clampSize.x * 0.5f + targetSize.x * _target.pivot.x;
        var maxX = clampSize.x * 0.5f - targetSize.x * (1f - _target.pivot.x);
        var minY = -clampSize.y * 0.5f + targetSize.y * _target.pivot.y;
        var maxY = clampSize.y * 0.5f - targetSize.y * (1f - _target.pivot.y);

        desired.x = Mathf.Clamp(desired.x, minX, maxX);
        desired.y = Mathf.Clamp(desired.y, minY, maxY);
        return desired;
    }
}
