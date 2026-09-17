using UnityEngine;
using UnityEngine.EventSystems;

// Efecto tactil de "hundirse" al presionar: no requiere sprites extra,
// solo desplaza y achica un poco el boton mientras se mantiene presionado.
[RequireComponent(typeof(RectTransform))]
public class ButtonPressEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [SerializeField] private float pressedScale = 0.94f;
    [SerializeField] private Vector2 pressedOffset = new Vector2(0f, -6f);

    private RectTransform _rectTransform;
    private Vector3 _originalScale;
    private Vector2 _originalPosition;
    private bool _isPressed;

    private void Awake()
    {
        _rectTransform = (RectTransform)transform;
        _originalScale = _rectTransform.localScale;
        _originalPosition = _rectTransform.anchoredPosition;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _isPressed = true;
        _rectTransform.localScale = _originalScale * pressedScale;
        _rectTransform.anchoredPosition = _originalPosition + pressedOffset;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ResetVisual();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_isPressed)
        {
            ResetVisual();
        }
    }

    private void ResetVisual()
    {
        _isPressed = false;
        _rectTransform.localScale = _originalScale;
        _rectTransform.anchoredPosition = _originalPosition;
    }
}
