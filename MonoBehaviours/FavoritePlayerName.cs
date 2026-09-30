using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace JoksterCube.ServerPlayerList.MonoBehaviours;

internal class FavoritePlayerName : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private PlayerInfoElement _playerInfoElement = null!;
    private TMP_Text _text = null!;
    private FontStyles _defaultFontStyle;

    internal void Initialize(PlayerInfoElement playerInfoElement)
    {
        _playerInfoElement = playerInfoElement;
        _text = GetComponent<TMP_Text>();
        _defaultFontStyle = _text.fontStyle;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
            _playerInfoElement.ToggleFavorite();
    }

    public void OnPointerEnter(PointerEventData eventData) =>
        _text.fontStyle = _defaultFontStyle | FontStyles.Underline;

    public void OnPointerExit(PointerEventData eventData) =>
        _text.fontStyle = _defaultFontStyle;
}