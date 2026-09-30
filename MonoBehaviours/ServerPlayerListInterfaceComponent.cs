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
    private readonly List<PlayerInfoElement> _containerElements = new();

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
        if (!_background.gameObject.activeSelf || (showDetails && !IsShowing)
            || Time.time - lastRefresh >= PluginConfig.RefreshDelay.Value)
        {
            lastRefresh = Time.time;
            Refresh(showDetails);
        }

        SetListVisibility(showDetails);
        ToggleVisibility(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)_background.transform);
    }

    private void GetStyles()
    {
        TMP_Text? text = null;
        if (EnemyHud.instance && EnemyHud.instance.m_baseHudPlayer)
            text = FindTextWithFont(EnemyHud.instance.m_baseHudPlayer);

        if (!text && Hud.instance && Hud.instance.m_rootObject)
            text = FindTextWithFont(Hud.instance.m_rootObject);

        if (!text)
        {
            foreach (var candidate in Resources.FindObjectsOfTypeAll<TMP_Text>())
            {
                if (candidate && candidate.font)
                {
                    text = candidate;
                    break;
                }
            }
        }

        if (!text)
        {
            Plugin.ModLogger.LogWarning("Could not find a loaded TextMeshPro font asset for the player list.");
            _font = TMP_Settings.defaultFontAsset;
            _color = Color.white;
            return;
        }

        _font = text.font;
        _color = text.color;
    }

    private static TMP_Text? FindTextWithFont(GameObject root)
    {
        foreach (var text in root.GetComponentsInChildren<TMP_Text>(true))
        {
            if (text && text.font)
                return text;
        }

        return null;
    }

    private void SetListVisibility(bool visible)
    {
        if (IsShowing == visible) return;
        _container.gameObject.SetActive(visible);
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
        OnPositionChanged += Save;
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
        OnPositionChanged -= Save;
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

    private void ToggleVisibility(bool visible)
    {
        if (_background.gameObject.activeSelf == visible) return;
        _background.gameObject.SetActive(visible);
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

        var layout = gameObject.AddComponent<VerticalLayoutGroup>();
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        var fitter = gameObject.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        SetMainFromConfig();
    }

    private void InitBackground()
    {
        var backgroundGameObject = new GameObject(GameObjectNames.ServerPlayerListBackground, typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter), typeof(Image));
        backgroundGameObject.transform.SetParent(transform, false);

        var layout = backgroundGameObject.GetComponent<VerticalLayoutGroup>();
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.padding = new RectOffset(Pad, Pad, Pad, Pad);
        layout.spacing = Pad;

        var fitter = backgroundGameObject.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        _background = backgroundGameObject.GetComponent<Image>();
        SetBackgroundFromConfig();
    }

    private void InitHeader()
    {
        var headerGameObject = new GameObject(GameObjectNames.ServerPlayerListHeader, typeof(RectTransform));
        headerGameObject.SetActive(false);
        headerGameObject.AddComponent<ContentSizeFitter>();
        headerGameObject.AddComponent<TextMeshProUGUI>();
        headerGameObject.transform.SetParent(_background.transform, false);

        var fitter = headerGameObject.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        _headerText = headerGameObject.GetComponent<TextMeshProUGUI>();
        _headerText.font = _font;
        _headerText.fontStyle = FontStyles.Bold;
        _headerText.textWrappingMode = TextWrappingModes.NoWrap;
        _headerText.alignment = TextAlignmentOptions.Top;

        SetHeaderFromConfig();
        headerGameObject.SetActive(true);
    }

    private void InitListContainer()
    {
        var containerGameObject = new GameObject(GameObjectNames.ServerPlayerListContainer, typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        _container = containerGameObject.transform;
        _container.SetParent(_background.transform, false);

        var layout = containerGameObject.GetComponent<VerticalLayoutGroup>();
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.spacing = 0f;

        var fitter = containerGameObject.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
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
        var info = SpawnElementMain();
        info.Name = SpawnElementName(info.transform);
        info.Distance = SpawnElementDistance(info.transform);
        info.Name.gameObject.GetComponent<FavoritePlayerName>().Initialize(info);
        info.FavoriteChanged += OnFavoriteChanged;
        info.GetComponent<LayoutElement>().preferredHeight = GetPreferredRowHeight(info);

        return info;
    }

    private PlayerInfoElement SpawnElementMain()
    {
        var elementGameObject = new GameObject(GameObjectNames.ServerPlayerListPlayerInfo, typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter), typeof(LayoutElement), typeof(PlayerInfoElement));
        elementGameObject.transform.SetParent(_container, false);

        var layout = elementGameObject.GetComponent<HorizontalLayoutGroup>();
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        var fitter = elementGameObject.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        return elementGameObject.GetComponent<PlayerInfoElement>();
    }

    private TMP_Text SpawnElementName(Transform element)
    {
        var playerNameGameObject = new GameObject(GameObjectNames.ServerPlayerListPlayerName, typeof(RectTransform));
        playerNameGameObject.SetActive(false);
        playerNameGameObject.AddComponent<ContentSizeFitter>();
        playerNameGameObject.AddComponent<TextMeshProUGUI>();
        playerNameGameObject.AddComponent<FavoritePlayerName>();
        playerNameGameObject.transform.SetParent(element, false);

        var fitter = playerNameGameObject.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var text = playerNameGameObject.GetComponent<TextMeshProUGUI>();
        text.font = _font;
        text.color = _color;
        text.fontStyle = FontStyles.Bold;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.alignment = TextAlignmentOptions.Left;

        text.fontSize = PluginConfig.ListFontSize.Value;
        playerNameGameObject.SetActive(true);

        return text;
    }

    private void OnFavoriteChanged()
    {
        lastRefresh = Time.time;
        Refresh(showPlayers: true);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)_background.transform);
    }

    private TMP_Text SpawnElementDistance(Transform element)
    {
        var playerNameGameObject = new GameObject(GameObjectNames.ServerPlayerListDistance, typeof(RectTransform));
        playerNameGameObject.SetActive(false);
        playerNameGameObject.AddComponent<ContentSizeFitter>();
        playerNameGameObject.AddComponent<TextMeshProUGUI>();
        playerNameGameObject.transform.SetParent(element, false);

        var fitter = playerNameGameObject.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var text = playerNameGameObject.GetComponent<TextMeshProUGUI>();
        text.font = _font;
        text.fontStyle = FontStyles.Bold;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.alignment = TextAlignmentOptions.Right;

        text.fontSize = PluginConfig.ListFontSize.Value;
        playerNameGameObject.SetActive(true);

        return text;
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
