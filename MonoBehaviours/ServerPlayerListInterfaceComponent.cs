using JoksterCube.ServerPlayerList.Common;
using JoksterCube.ServerPlayerList.Domain;
using JoksterCube.ServerPlayerList.Settings;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static JoksterCube.ServerPlayerList.Common.PluginUtils;
using static JoksterCube.ServerPlayerList.Settings.Constants;

namespace JoksterCube.ServerPlayerList.MonoBehaviours;

internal class ServerPlayerListInterfaceComponent : DragNDrop
{
    private const int Pad = 5;
    private static readonly Vector2 LegacyDefaultAnchorPosition = new(720.25f, 296.75f);
    private static readonly Vector2 DefaultAnchorPosition = new(-239.75f, -243.25f);

    private TMP_FontAsset _font = null!;
    private Color _color;

    private bool _applyingFromConfig;

    private RectTransform _mainRect = null!;

    private Image _background = null!;

    private TMP_Text _headerText = null!;
    private Transform _container = null!;
    private readonly List<PlayerInfoElement> _containerElements = [];

    private float lastRefresh;
    private bool IsShowing => _container.gameObject.activeSelf;

    protected override void Awake()
    {
        SetHudCanvasAsParent();

        base.Awake();

        GetStyles();

        InitGameObjects();
        ApplyFromConfig();

        RegisterEvents();
    }

    private void Update()
    {
        var visible = ShouldBeVisible();

        if (!visible)
        {
            ToggleVisibility(false);
            return;
        }

        var showPlayers = PluginConfig.ShowPlayers.Value.IsOn();
        var showDetails = showPlayers && !RpcHandlers.ShouldWaitForInitialConfigSync;
        var layoutChanged = false;
        if (!_background.gameObject.activeSelf || (showDetails && !IsShowing)
            || Time.time - lastRefresh >= PluginConfig.RefreshDelay.Value)
        {
            lastRefresh = Time.time;
            Refresh(showDetails);
            layoutChanged = true;
        }

        layoutChanged |= SetListVisibility(showDetails);
        layoutChanged |= ToggleVisibility(true);
        if (layoutChanged)
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)_background.transform);
    }

    // The panel is only created once the HUD exists, so its hover text is always available.
    private void GetStyles()
    {
        var text = Hud.instance.m_hoverName;
        _font = text.font;
        _color = text.color;
    }

    private bool SetListVisibility(bool visible)
    {
        if (IsShowing == visible) return false;
        _container.gameObject.SetActive(visible);
        return true;
    }

    private void Refresh(bool showPlayers)
    {
        if (showPlayers)
        {
            ServerPlayerTracker.UpdatePlayerInfo();
            UpdateContainer();
        }
        else
        {
            ServerPlayerTracker.UpdatePlayerCount();
        }

        SetCurrentlyOnlineText();
    }

    private void SetCurrentlyOnlineText() =>
        _headerText.text = $"{PluginConfig.HeaderText.Value} <color=#{ColorUtility.ToHtmlStringRGB(PluginConfig.HeaderHighlightTextColor.Value)}>{ServerPlayerTracker.CurrentlyOnline}</color>";

    private void UpdateContainer()
    {
        var list = ServerPlayerTracker.GetCurrenlyOnlineList();
        var diff = list.Count - _containerElements.Count;
        if (diff > 0)
        {
            SpawnElements(diff);
        }
        else if (diff < 0)
        {
            DeleteElements(-diff);
        }
        UpdateElements(list);
    }

    private void UpdateElements(List<ServerPlayerInfo> infos)
    {
        for (int i = 0; i < _containerElements.Count; i++)
        {
            _containerElements[i].SetPlayerInfo(infos[i]);
        }
    }

    private void OnDestroy() => DeregisterEvents();

    private void RegisterEvents()
    {
        OnDragEnded += Save;

        PluginConfig.AnchorPosition.SettingChanged += OnConfigChanged;
        PluginConfig.Width.SettingChanged += OnConfigChanged;

        PluginConfig.BackgroundColor.SettingChanged += OnConfigChanged;
        PluginConfig.HeaderTextColor.SettingChanged += OnConfigChanged;

        PluginConfig.HeaderFontSize.SettingChanged += OnConfigChanged;
        PluginConfig.ListFontSize.SettingChanged += OnConfigChanged;
        PluginConfig.ShowPlayers.SettingChanged += OnShowPlayersChanged;
    }

    private void DeregisterEvents()
    {
        OnDragEnded -= Save;

        PluginConfig.AnchorPosition.SettingChanged -= OnConfigChanged;
        PluginConfig.Width.SettingChanged -= OnConfigChanged;

        PluginConfig.BackgroundColor.SettingChanged -= OnConfigChanged;
        PluginConfig.HeaderTextColor.SettingChanged -= OnConfigChanged;

        PluginConfig.HeaderFontSize.SettingChanged -= OnConfigChanged;
        PluginConfig.ListFontSize.SettingChanged -= OnConfigChanged;
        PluginConfig.ShowPlayers.SettingChanged -= OnShowPlayersChanged;
    }

    private void OnShowPlayersChanged(object sender, EventArgs e)
    {
        if (PluginConfig.ShowPlayers.Value.IsOn())
        {
            if (!IsShowing && ShouldBeVisible() && !RpcHandlers.ShouldWaitForInitialConfigSync)
            {
                lastRefresh = Time.time;
                Refresh(showPlayers: true);
                SetListVisibility(true);
                ToggleVisibility(true);
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)_background.transform);
            }
            return;
        }

        SetListVisibility(false);
    }

    private void OnConfigChanged(object sender, EventArgs e)
    {
        if (_applyingFromConfig) return;
        ApplyFromConfig();
    }

    private void ApplyFromConfig()
    {
        _applyingFromConfig = true;
        try
        {
            var anchorPosition = PluginConfig.AnchorPosition.Value;
            if (anchorPosition == LegacyDefaultAnchorPosition)
            {
                anchorPosition = DefaultAnchorPosition;
                PluginConfig.AnchorPosition.Value = anchorPosition;
            }

            SetAnchoredPosition(anchorPosition);
            SetMainFromConfig();
            SetBackgroundFromConfig();
            SetHeaderFromConfig();
            SetListFromConfig();
        }
        finally
        {
            _applyingFromConfig = false;
        }
    }

    private void SetMainFromConfig()
    {
        _mainRect.sizeDelta = new Vector2(PluginConfig.Width.Value, _mainRect.sizeDelta.y);
    }

    private void SetBackgroundFromConfig()
    {
        _background.color = PluginConfig.BackgroundColor.Value;
    }

    private void SetHeaderFromConfig()
    {
        _headerText.color = PluginConfig.HeaderTextColor.Value;
        _headerText.fontSize = PluginConfig.HeaderFontSize.Value;
    }

    private void SetListFromConfig()
    {
        foreach (var element in _containerElements)
        {
            element.Name.fontSize = PluginConfig.ListFontSize.Value;
            element.Distance.fontSize = PluginConfig.ListFontSize.Value;
            element.GetComponent<LayoutElement>().preferredHeight = GetPreferredRowHeight(element);
        }
    }

    private static float GetPreferredRowHeight(PlayerInfoElement element)
    {
        var nameHeight = element.Name.GetPreferredValues("M").y;
        var distanceHeight = element.Distance.GetPreferredValues("\u2191 999.99 km").y;
        return Mathf.Ceil(Mathf.Max(nameHeight, distanceHeight));
    }

    private void Save(Vector2 anchoredPosition)
    {
        if (_applyingFromConfig) return;
        if (PluginConfig.AnchorPosition.Value == anchoredPosition) return;

        PluginConfig.AnchorPosition.Value = anchoredPosition;
    }

    internal static bool ShouldBeVisible()
    {
        if (!Hud.instance) return false;
        if (!Hud.instance.m_rootObject || !Hud.instance.m_rootObject.activeInHierarchy) return false;

        if (Minimap.IsOpen()) return false;

        var me = Player.m_localPlayer;
        if (!me) return false;
        if (me.IsTeleporting()) return false;
        if (me.IsSleeping()) return false;
        if (me.IsDead()) return false;
        if (me.InCutscene()) return false;
        return true;
    }

    private bool ToggleVisibility(bool visible)
    {
        if (_background.gameObject.activeSelf == visible) return false;
        _background.gameObject.SetActive(visible);
        return true;
    }

    private void SetHudCanvasAsParent() =>
        transform.SetParent(Hud.instance.m_rootObject.GetComponentInParent<Canvas>().transform, false);

    private void InitGameObjects()
    {
        InitMain();
        InitBackground();
        InitHeader();
        InitListContainer();
    }

    private void InitMain()
    {
        _mainRect = (RectTransform)transform;

        _mainRect.anchorMin = Vector2.one;
        _mainRect.anchorMax = Vector2.one;
        _mainRect.pivot = Vector2.up;

        AddLayout<VerticalLayoutGroup>(gameObject);
        FitHeight(gameObject);

        SetMainFromConfig();
    }

    private void InitBackground()
    {
        var backgroundGameObject = CreateChild(GameObjectNames.ServerPlayerListBackground, transform);
        AddLayout<VerticalLayoutGroup>(backgroundGameObject, spacing: Pad, padding: Pad);
        FitHeight(backgroundGameObject);

        _background = backgroundGameObject.AddComponent<Image>();
        SetBackgroundFromConfig();
    }

    private void InitHeader()
    {
        _headerText = CreateText(GameObjectNames.ServerPlayerListHeader, _background.transform, TextAlignmentOptions.Top);
        SetHeaderFromConfig();
    }

    private void InitListContainer()
    {
        var containerGameObject = CreateChild(GameObjectNames.ServerPlayerListContainer, _background.transform);
        AddLayout<VerticalLayoutGroup>(containerGameObject);
        FitHeight(containerGameObject);

        _container = containerGameObject.transform;
    }

    private void SpawnElements(int number)
    {
        for (var i = 0; i < number; i++)
        {
            _containerElements.Add(SpawnElement());
        }
    }

    private PlayerInfoElement SpawnElement()
    {
        var elementGameObject = CreateChild(GameObjectNames.ServerPlayerListPlayerInfo, _container);
        AddLayout<HorizontalLayoutGroup>(elementGameObject);
        FitHeight(elementGameObject);
        var layoutElement = elementGameObject.AddComponent<LayoutElement>();

        var info = elementGameObject.AddComponent<PlayerInfoElement>();
        info.Name = CreateListText(GameObjectNames.ServerPlayerListPlayerName, info.transform, TextAlignmentOptions.Left);
        info.Name.color = _color;
        // Player names come from remote peers; never interpret them as markup.
        info.Name.richText = false;
        info.Name.gameObject.AddComponent<FavoritePlayerName>().Initialize(info);
        info.Distance = CreateListText(GameObjectNames.ServerPlayerListDistance, info.transform, TextAlignmentOptions.Right);

        info.FavoriteChanged += OnFavoriteChanged;
        layoutElement.preferredHeight = GetPreferredRowHeight(info);

        return info;
    }

    private void OnFavoriteChanged()
    {
        lastRefresh = Time.time;
        Refresh(showPlayers: true);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)_background.transform);
    }

    private TMP_Text CreateListText(string name, Transform parent, TextAlignmentOptions alignment)
    {
        var text = CreateText(name, parent, alignment);
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.fontSize = PluginConfig.ListFontSize.Value;
        return text;
    }

    private TMP_Text CreateText(string name, Transform parent, TextAlignmentOptions alignment)
    {
        var textGameObject = new GameObject(name, typeof(RectTransform));
        // Set the font before TextMeshPro wakes up, so it never falls back to a missing default font.
        textGameObject.SetActive(false);
        textGameObject.transform.SetParent(parent, false);
        FitHeight(textGameObject);

        var text = textGameObject.AddComponent<TextMeshProUGUI>();
        text.font = _font;
        text.fontStyle = FontStyles.Bold;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.alignment = alignment;

        textGameObject.SetActive(true);
        return text;
    }

    private static GameObject CreateChild(string name, Transform parent)
    {
        var child = new GameObject(name, typeof(RectTransform));
        child.transform.SetParent(parent, false);
        return child;
    }

    private static void AddLayout<T>(GameObject target, float spacing = 0f, int padding = 0) where T : HorizontalOrVerticalLayoutGroup
    {
        var layout = target.AddComponent<T>();
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.spacing = spacing;
        layout.padding = new RectOffset(padding, padding, padding, padding);
    }

    private static void FitHeight(GameObject target)
    {
        var fitter = target.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    private void DeleteElements(int number)
    {
        for (var i = 0; i < number && _containerElements.Count > 0; i++)
        {
            DeleteElement(_containerElements.Count - 1);
        }
    }

    private void DeleteElement(int index)
    {
        var element = _containerElements[index];
        Destroy(element.gameObject);
        _containerElements.RemoveAt(index);
    }
}
