using System;
using System.Collections.Generic;
using System.Globalization;
using Restory.UI.Presenters.DevicePaintingTool;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace RestoryReColor;

internal static class BoardParts
{
    private const string MainPanelName = "Main Panel";
    private const string BrushPageName = "GUI_BrushModePanel";
    private const string ColumnOneName = "LeftSide";
    private const string ColumnTwoName = "RightSide";
    private const string DropdownRowName = "PaletteName";
    private const string SliderRowName = "BrushSize";
    private const string CommandStripName = "CommandButtons";
    private const string CloseButtonName = "CloseButton";
    private const string CaptionName = "Text (TMP)";
    private const string SwatchFaceName = "Base";
    private const string ButtonFaceName = "Main";
    private const string ButtonIconName = "Image";

    private const float TabHeight = 100f;
    private const float TabTextHeight = 30f;

    private const float TabVisibleWidth = 84f;

    private const float PresetLabelWidth = 74f;
    private const string ItemLabelName = "Item Label";
    private const string ItemSwatchName = "MiniPalette";

    private const float ItemLabelPadding = 24f;

    private const int BoardSortingOrder = 500;

    internal const float RowWidth = 624f;
    internal const float SliderWidth = 600f;

    internal const float DropdownHeight = 100f;
    internal const float SliderHeight = 80f;

    internal const float RowPaintedWidth = 683f;

    internal const int RowsPerColumn = 3;

    private static readonly TabFace[] TabFaces = TabFace.Defaults();
    private static readonly Color SelectedTabTint = new Color(0.70f, 0.66f, 0.62f, 1f);
    internal static readonly LayoutSettings Layout = LayoutSettings.Defaults();

    private static readonly HashSet<string> KeptScripts = new HashSet<string>
    {
        "Canvas", "CanvasScaler", "GraphicRaycaster", "CanvasGroup",
        "Image", "RawImage", "Button", "Slider", "Toggle", "Scrollbar", "ScrollRect",
        "Mask", "RectMask2D", "GridLayoutGroup", "LayoutElement", "ContentSizeFitter",
        "HorizontalLayoutGroup", "VerticalLayoutGroup",
        "TextMeshProUGUI", "TMP_SubMeshUI"
    };

    private static readonly Dictionary<TabSlot, GameObject> Tabs = new Dictionary<TabSlot, GameObject>();
    private static readonly List<PresetCell> Cells = new List<PresetCell>();

    internal static float DropdownRowHeight => Layout.HeaderHeight + Layout.HeaderGap + DropdownHeight;

    internal static float SliderRowHeight => Layout.HeaderHeight + SliderHeight;

    internal enum TabSlot
    {
        Settings,
        Presets,
        Reset
    }

    private struct TabFace
    {
        internal string SourceName;
        internal Func<string> Caption;
        internal int Order;

        internal float IconSize;

        internal float IconX;
        internal float TextX;

        internal float IconTop;
        internal float TextCentre;

        internal float FontSize;

        internal static TabFace[] Defaults()
        {
            return new[]
            {
                DefineFace("UnDoButton", () => Strings.PaintTab, 0, 44f, -24f, 7f, -24f, 67f),
                DefineFace("ReDoButton", () => Strings.PresetTab, 1, 46f, -24f, 9f, -28f, 69f),
                DefineFace("ResetButton", () => Strings.ResetTab, 2, 28f, -27f, 18f, -29f, 70f)
            };
        }

        private static TabFace DefineFace(string sourceName, Func<string> caption, int order,
            float iconSize, float iconX, float iconTop, float textX, float textCentre)
        {
            return new TabFace
            {
                SourceName = sourceName,
                Caption = caption,
                Order = order,
                IconSize = iconSize,
                IconX = iconX,
                IconTop = iconTop,
                TextX = textX,
                TextCentre = textCentre,
                FontSize = 20f
            };
        }
    }

    internal struct LayoutSettings
    {
        internal float ContentTop;
        internal float ContentBottom;
        internal float ContentLeft;
        internal float ContentRight;

        internal float HeaderHeight;
        internal float HeaderGap;

        internal float RowScale;

        internal float SlotMargin;

        internal float BandToRows;
        internal float RowGap;

        internal static LayoutSettings Defaults()
        {
            return new LayoutSettings
            {
                ContentTop = 320f,
                ContentBottom = -320f,
                ContentLeft = -590f,
                ContentRight = 590f,
                HeaderHeight = 52f,
                HeaderGap = 8f,
                RowScale = 0.82f,
                SlotMargin = 20f,
                BandToRows = 0f,
                RowGap = 32f
            };
        }
    }

    private sealed class PresetCell
    {
        internal GameObject Root;
    }

    internal sealed class Panel
    {
        internal GameObject Root;
        internal GameObject SettingsPage;
        internal Transform Board;

        internal Transform Content;
        internal Transform GameColumnOne;
        internal Transform GameColumnTwo;

        internal GameObject DropdownTemplate;
        internal GameObject SliderTemplate;
        internal GameObject SwatchTemplate;
        internal GridLayoutGroup GridTemplate;

        internal GameObject PresetPage { get; set; }

        internal RectTransform PresetCells { get; set; }

        internal SliderRow CloneSliderRow(string name)
        {
            GameObject row = Spawn(SliderTemplate, name, Content);

            if (row == null)
                return null;

            Slider slider = row.GetComponentInChildren<Slider>(true);

            return slider == null ? null : new SliderRow(row, slider, FindCaption(row));
        }

        internal GameObject BuildPresetGrid(int columns)
        {
            if (SwatchTemplate == null || Board == null)
            {
                Log.Warning("Board parts: no swatch template, so the presets tab is unavailable.");
                return null;
            }

            var page = new GameObject("PresetsPage", typeof(RectTransform));
            var pageRect = page.GetComponent<RectTransform>();

            pageRect.SetParent(Board, false);
            pageRect.anchorMin = Vector2.zero;
            pageRect.anchorMax = Vector2.one;
            pageRect.offsetMin = Vector2.zero;
            pageRect.offsetMax = Vector2.zero;
            pageRect.localScale = Vector3.one;

            var cells = new GameObject("Cells", typeof(RectTransform), typeof(GridLayoutGroup));
            var cellsRect = cells.GetComponent<RectTransform>();

            float top = BandBottom();

            cellsRect.SetParent(pageRect, false);
            cellsRect.anchorMin = cellsRect.anchorMax = new Vector2(0.5f, 0.5f);
            cellsRect.pivot = new Vector2(0.5f, 1f);
            cellsRect.sizeDelta = new Vector2(Layout.ContentRight - Layout.ContentLeft, top - Layout.ContentBottom);
            cellsRect.anchoredPosition = new Vector2((Layout.ContentLeft + Layout.ContentRight) * 0.5f, top);
            cellsRect.localScale = Vector3.one;

            var grid = cells.GetComponent<GridLayoutGroup>();

            if (GridTemplate != null)
            {
                grid.cellSize = GridTemplate.cellSize;
                grid.spacing = GridTemplate.spacing;
                grid.startCorner = GridTemplate.startCorner;
                grid.startAxis = GridTemplate.startAxis;
            }

            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = columns;
            grid.childAlignment = TextAnchor.MiddleCenter;

            PresetPage = page;
            PresetCells = cellsRect;

            return page;
        }

        internal void FillPresetGrid(int count, Action<int> onClicked, Func<int, Color> swatchOf,
            Func<int, string> labelOf, Func<int, bool> isVisible)
        {
            if (PresetPage == null)
                return;

            Cells.Clear();

            for (int index = 0; index < count; index++)
            {
                GameObject cell = Object.Instantiate(SwatchTemplate, PresetCells);
                cell.name = $"Preset{index}";
                cell.SetActive(true);

                Transform face = cell.transform.Find(SwatchFaceName);
                Image image = face == null ? null : face.GetComponent<Image>();
                Button button = face == null ? null : face.GetComponent<Button>();

                if (image != null)
                {
                    image.color = swatchOf(index);
                }

                if (button != null)
                {
                    int presetIndex = index;
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => onClicked(presetIndex));
                }

                SetPresetLabel(cell, labelOf(index));

                Cells.Add(new PresetCell { Root = cell });
            }

            FillPresetVisibility(isVisible);
        }

        internal DropdownRow CloneDropdownSlot(string name, int slot, int slots)
        {
            return CloneDropdownSlot(name, Content, slot, slots);
        }

        internal DropdownRow CloneDropdownSlot(string name, Transform parent, int slot, int slots)
        {
            GameObject row = Spawn(DropdownTemplate, name, parent);

            if (row == null)
                return null;

            TMP_Dropdown dropdown = ConvertDropdown(row);

            if (dropdown == null)
                return null;

            var dropdownRow = new DropdownRow(row, dropdown, FindCaption(row));
            dropdownRow.SetSlot(slot, slots);

            return dropdownRow;
        }

        private static GameObject Spawn(GameObject template, string name, Transform parent)
        {
            if (template == null || parent == null)
            {
                Log.Warning($"Board parts: cannot place \"{name}\"; its template or parent is missing.");
                return null;
            }

            GameObject row = Object.Instantiate(template, parent, false);
            row.name = name;
            row.SetActive(true);

            return row;
        }
    }

    internal static void CopyTextLook(TMP_Text from, TMP_Text to)
    {
        if (from == null || to == null)
            return;

        to.color = from.color;
        to.enableVertexGradient = from.enableVertexGradient;
        to.colorGradient = from.colorGradient;
    }

    internal static void CopyTextFont(TMP_Text from, TMP_Text to)
    {
        if (from == null || to == null)
            return;

        to.font = from.font;
    }

    internal static void SetTextColor(TMP_Text text, Color color)
    {
        if (text == null)
            return;

        text.enableVertexGradient = false;
        text.color = color;
    }

    internal static float SlotWidth(int slots)
    {
        return (Layout.ContentRight - Layout.ContentLeft) / slots;
    }

    internal static float SlotCentre(int slot, int slots)
    {
        return Layout.ContentLeft + SlotWidth(slots) * (slot + 0.5f);
    }

    internal static float RowPitch()
    {
        return SliderRowHeight * Layout.RowScale + Layout.RowGap;
    }

    internal static float BandBottom()
    {
        return Layout.ContentTop - DropdownRowHeight * Layout.RowScale - Layout.BandToRows;
    }

    internal static float RowsTop(int rowsInColumn)
    {
        float top = BandBottom();
        float usedHeight = rowsInColumn <= 0 ? 0f : rowsInColumn * RowPitch() - Layout.RowGap;

        return top - (top - Layout.ContentBottom - usedHeight) * 0.5f;
    }

    internal static bool TryClonePanel(out Panel panel)
    {
        panel = null;

        GUI_DevicePainterPanel source = FindInScene<GUI_DevicePainterPanel>();

        if (source == null)
        {
            Log.Warning("Board parts: the painting tool panel is not in this scene, so the board "
                        + "cannot be built. The surface panel is unaffected.");

            return false;
        }

        Transform home = source.transform.parent;

        var holder = new GameObject("ReColorHolder");
        holder.SetActive(false);

        GameObject clone = Object.Instantiate(source.gameObject, holder.transform);
        clone.name = "ReColorBoard";

        Transform board = clone.transform.Find(MainPanelName);

        if (board == null)
        {
            Log.Warning("Board parts: the painting panel has no \"" + MainPanelName + "\" child, most "
                        + "likely renamed by a game update. The board is unavailable.");

            Object.Destroy(holder);
            return false;
        }

        CollectTabs(board);
        panel = ReadPanel(clone, board);

        StripForeignScripts(clone);
        ClearGameColumns(panel);

        clone.transform.SetParent(home, false);
        Object.Destroy(holder);

        Reveal(clone, board);

        clone.SetActive(false);
        panel.Root = clone;

        Log.Debug("Board parts: painting panel cloned and stripped.");

        return true;
    }

    internal static void WireTab(this Panel panel, TabSlot slot, Action onClicked)
    {
        if (!Tabs.TryGetValue(slot, out GameObject button))
            return;

        Button click = button.GetComponent<Button>();

        if (click == null)
            return;

        click.onClick.RemoveAllListeners();
        click.onClick.AddListener(() => onClicked());

        SetCaption(button, TabCaption(slot));
    }

    internal static void WireClose(this Panel panel, Action onClosed)
    {
        Transform close = panel.Board == null ? null : panel.Board.Find(CloseButtonName);
        Button click = close == null ? null : close.GetComponent<Button>();

        if (click == null)
            return;

        click.onClick.RemoveAllListeners();
        click.onClick.AddListener(() => onClosed());
    }

    internal static void HighlightTab(TabSlot slot)
    {
        foreach (KeyValuePair<TabSlot, GameObject> tab in Tabs)
        {
            if (tab.Key == TabSlot.Reset)
                continue;

            Transform face = tab.Value.transform.Find(ButtonFaceName);
            Image wood = face == null ? null : face.GetComponent<Image>();

            if (wood != null)
            {
                wood.color = tab.Key == slot ? SelectedTabTint : Color.white;
            }
        }
    }

    internal static void FillPresetVisibility(Func<int, bool> isVisible)
    {
        for (int index = 0; index < Cells.Count; index++)
        {
            Cells[index].Root.SetActive(isVisible(index));
        }
    }

    internal static void RetranslateTabs()
    {
        foreach (KeyValuePair<TabSlot, GameObject> tab in Tabs)
        {
            SetCaption(tab.Value, TabCaption(tab.Key));
        }
    }

    private static void Reveal(GameObject clone, Transform board)
    {
        var group = clone.GetComponent<CanvasGroup>();
        var canvas = clone.GetComponent<Canvas>();
        var boardRect = board.GetComponent<RectTransform>();

        Log.Debug($"Board parts: as cloned, alpha={(group == null ? -1f : group.alpha)}, "
                  + $"canvas={(canvas == null ? "none" : canvas.enabled + "/" + canvas.sortingOrder)}, "
                  + $"board active={board.gameObject.activeSelf}, pos={boardRect.anchoredPosition}.");

        if (group != null)
        {
            group.alpha = 1f;
            group.interactable = true;
            group.blocksRaycasts = true;
        }

        if (canvas != null)
        {
            canvas.enabled = true;
            canvas.overrideSorting = true;
            canvas.sortingOrder = BoardSortingOrder;
        }

        board.gameObject.SetActive(true);
        boardRect.anchorMin = boardRect.anchorMax = boardRect.pivot = new Vector2(0.5f, 0.5f);
        boardRect.anchoredPosition = Vector2.zero;
        boardRect.localScale = Vector3.one;

        foreach (CanvasGroup nested in clone.GetComponentsInChildren<CanvasGroup>(true))
        {
            nested.alpha = 1f;
        }
    }

    private static Panel ReadPanel(GameObject clone, Transform board)
    {
        Transform page = board.Find(BrushPageName);
        Transform columnOne = page == null ? null : page.Find(ColumnOneName);
        Transform columnTwo = page == null ? null : page.Find(ColumnTwoName);

        var panel = new Panel
        {
            Root = clone,
            Board = board,
            SettingsPage = page == null ? null : page.gameObject,
            GameColumnOne = columnOne,
            GameColumnTwo = columnTwo
        };

        if (columnOne != null)
        {
            panel.DropdownTemplate = Child(columnOne, DropdownRowName);
            panel.SliderTemplate = Child(columnOne, SliderRowName);
        }

        panel.Content = page == null ? board : page;

        if (columnTwo != null)
        {
            panel.GridTemplate = columnTwo.GetComponent<GridLayoutGroup>();

            if (columnTwo.childCount > 0)
            {
                panel.SwatchTemplate = Object.Instantiate(columnTwo.GetChild(0).gameObject, clone.transform);
                panel.SwatchTemplate.name = "SwatchTemplate";
                panel.SwatchTemplate.SetActive(false);
            }
        }

        return panel;
    }

    private static void ClearGameColumns(Panel panel)
    {
        if (panel.GameColumnTwo != null)
        {
            for (int index = panel.GameColumnTwo.childCount - 1; index >= 0; index--)
            {
                Object.DestroyImmediate(panel.GameColumnTwo.GetChild(index).gameObject);
            }

            panel.GameColumnTwo.gameObject.SetActive(false);
        }

        if (panel.GameColumnOne != null)
        {
            foreach (string rowName in new[] { DropdownRowName, SliderRowName, "BrushType", "Opacity" })
            {
                GameObject row = Child(panel.GameColumnOne, rowName);

                if (row != null)
                {
                    row.SetActive(false);
                }
            }
        }

        foreach (string strayName in new[] { "GUI_PaintingModeButtons", "HideButton", "GUI_StickerModePanel" })
        {
            GameObject stray = Child(panel.Board, strayName);

            if (stray != null)
            {
                stray.SetActive(false);
            }
        }
    }

    private static void StripForeignScripts(GameObject root)
    {
        int removedCount = 0;

        foreach (MonoBehaviour script in root.GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (script == null || script is TMP_Dropdown)
                continue;

            if (KeptScripts.Contains(script.GetType().Name))
                continue;

            Object.DestroyImmediate(script);
            removedCount++;
        }

        Log.Debug($"Board parts: stripped {removedCount} script(s) from the clone.");
    }

    private static TMP_Dropdown ConvertDropdown(GameObject row)
    {
        TMP_Dropdown source = row.GetComponentInChildren<TMP_Dropdown>(true);

        if (source == null)
        {
            Log.Warning("Board parts: a dropdown row has no dropdown; skipping it.");
            return null;
        }

        if (source.GetType() == typeof(TMP_Dropdown))
            return source;

        GameObject owner = source.gameObject;

        RectTransform template = source.template;
        TMP_Text captionText = source.captionText;
        TMP_Text itemText = source.itemText;
        Image captionImage = source.captionImage;
        Image itemImage = source.itemImage;
        Graphic targetGraphic = source.targetGraphic;
        ColorBlock colors = source.colors;

        Object.DestroyImmediate(source);

        TMP_Dropdown plain = owner.AddComponent<TMP_Dropdown>();
        plain.template = template;
        plain.captionText = captionText;
        plain.itemText = itemText;
        plain.captionImage = captionImage;
        plain.itemImage = itemImage;
        plain.targetGraphic = targetGraphic;
        plain.colors = colors;

        RewireTemplate(plain);

        return plain;
    }

    private static void RewireTemplate(TMP_Dropdown dropdown)
    {
        if (dropdown.template == null)
            return;

        Transform swatch = Descendant(dropdown.template, ItemSwatchName);
        Transform label = Descendant(dropdown.template, ItemLabelName);

        if (swatch != null)
        {
            swatch.gameObject.SetActive(false);
        }

        if (label is RectTransform labelRect)
        {
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(ItemLabelPadding, 0f);
            labelRect.offsetMax = new Vector2(-ItemLabelPadding, 0f);

            dropdown.itemText = labelRect.GetComponent<TMP_Text>();
        }

        dropdown.itemImage = null;
        dropdown.captionImage = null;
    }

    private static Transform Descendant(Transform root, string name)
    {
        foreach (Transform candidate in root.GetComponentsInChildren<Transform>(true))
        {
            if (candidate.name == name)
                return candidate;
        }

        return null;
    }

    private static void CollectTabs(Transform board)
    {
        Tabs.Clear();

        Transform strip = board.Find(CommandStripName);

        if (strip == null)
        {
            Log.Warning("Board parts: the command button strip is missing; tabs are unavailable.");
            return;
        }

        for (int slot = 0; slot < TabFaces.Length; slot++)
        {
            Register((TabSlot)slot, Child(strip, TabFaces[slot].SourceName));
        }

        GiveResetALabel();
        ArrangeTabs();
        LayOutTabs();
    }

    private static void LayOutTabs()
    {
        foreach (KeyValuePair<TabSlot, GameObject> tab in Tabs)
        {
            LayOutTab(tab.Value, TabFaces[(int)tab.Key]);
        }
    }

    private static void LayOutTab(GameObject button, TabFace tab)
    {
        Transform face = button.transform.Find(ButtonFaceName);

        if (face == null)
            return;

        if (face.Find(ButtonIconName) is RectTransform icon)
        {
            icon.anchorMin = icon.anchorMax = new Vector2(0.5f, 1f);
            icon.pivot = new Vector2(0.5f, 0.5f);
            icon.sizeDelta = new Vector2(tab.IconSize, tab.IconSize);
            icon.anchoredPosition = new Vector2(tab.IconX, -(tab.IconTop + tab.IconSize * 0.5f));
        }

        TMP_Text caption = FindCaption(button);

        if (caption == null)
            return;

        RectTransform captionRect = caption.rectTransform;

        captionRect.anchorMin = captionRect.anchorMax = new Vector2(0.5f, 0.5f);
        captionRect.pivot = new Vector2(0.5f, 0.5f);
        captionRect.sizeDelta = new Vector2(TabVisibleWidth, TabTextHeight);
        captionRect.anchoredPosition = new Vector2(tab.TextX, TabHeight * 0.5f - tab.TextCentre);

        caption.enableAutoSizing = false;
        caption.fontSize = tab.FontSize;
        caption.alignment = TextAlignmentOptions.Center;
        caption.textWrappingMode = TextWrappingModes.NoWrap;
    }

    private static void ArrangeTabs()
    {
        var heights = new List<float>();

        foreach (GameObject button in Tabs.Values)
        {
            heights.Add(((RectTransform)button.transform).anchoredPosition.y);
        }

        heights.Sort();
        heights.Reverse();

        for (int slot = 0; slot < TabFaces.Length; slot++)
        {
            MoveTab((TabSlot)slot, heights, TabFaces[slot].Order);
        }
    }

    private static void MoveTab(TabSlot slot, List<float> heights, int heightIndex)
    {
        if (heightIndex >= heights.Count || !Tabs.TryGetValue(slot, out GameObject button))
            return;

        var rect = (RectTransform)button.transform;

        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, heights[heightIndex]);
    }

    private static void GiveResetALabel()
    {
        bool hasResetTab = Tabs.TryGetValue(TabSlot.Reset, out GameObject reset);
        bool hasDonorTab = Tabs.TryGetValue(TabSlot.Settings, out GameObject donor);
        bool isAlreadyLabelled = hasResetTab && FindCaption(reset) != null;

        if (!hasResetTab || !hasDonorTab || isAlreadyLabelled)
        {
            return;
        }

        TMP_Text source = FindCaption(donor);
        Transform face = reset.transform.Find(ButtonFaceName);

        if (source == null || face == null)
        {
            Log.Warning("Board parts: no caption to copy onto the reset button; it stays unlabelled.");
            return;
        }

        GameObject copy = Object.Instantiate(source.gameObject, face, false);
        copy.name = CaptionName;
    }

    private static void Register(TabSlot slot, GameObject button)
    {
        if (button != null)
        {
            Tabs[slot] = button;
        }
    }

    private static string TabCaption(TabSlot slot)
    {
        return TabFaces[(int)slot].Caption();
    }

    private static void SetPresetLabel(GameObject cell, string text)
    {
        TMP_Text caption = FindCaption(cell);

        if (caption == null)
            return;

        caption.rectTransform.sizeDelta = new Vector2(PresetLabelWidth, caption.rectTransform.sizeDelta.y);

        caption.textWrappingMode = TextWrappingModes.NoWrap;
        caption.text = text;
    }

    private static void SetCaption(GameObject owner, string text)
    {
        TMP_Text caption = FindCaption(owner);

        if (caption != null)
        {
            caption.text = text;
        }
    }

    private static TMP_Text FindCaption(GameObject owner)
    {
        foreach (TMP_Text text in owner.GetComponentsInChildren<TMP_Text>(true))
        {
            if (text.name.StartsWith(CaptionName) || text.name == "Text")
                return text;
        }

        return null;
    }

    private static GameObject Child(Transform parent, string name)
    {
        Transform found = parent.Find(name);

        return found == null ? null : found.gameObject;
    }

    private static T FindInScene<T>() where T : Component
    {
        foreach (T candidate in Resources.FindObjectsOfTypeAll<T>())
        {
            if (candidate.gameObject.scene.IsValid())
                return candidate;
        }

        return null;
    }
}

internal sealed class DropdownRow
{
    private readonly GameObject _row;
    private readonly RectTransform _rect;
    private readonly TMP_Dropdown _dropdown;
    private readonly TMP_Text _caption;

    internal Action<int> OnChanged;

    internal TMP_Text BarText => _dropdown.captionText;

    internal DropdownRow(GameObject row, TMP_Dropdown dropdown, TMP_Text caption)
    {
        _row = row;
        _rect = row.GetComponent<RectTransform>();
        _dropdown = dropdown;
        _caption = caption;

        _dropdown.onValueChanged.RemoveAllListeners();
        _dropdown.onValueChanged.AddListener(value => OnChanged?.Invoke(value));

        Restack();
    }

    internal void SetCaption(string text)
    {
        if (_caption != null)
        {
            _caption.text = text;
        }
    }

    internal void SetCaptionLook(TMP_Text source)
    {
        BoardParts.CopyTextLook(source, _caption);
        BoardParts.CopyTextFont(source, _dropdown.itemText);
    }

    internal void SetOptions(IEnumerable<string> labels)
    {
        var options = new List<TMP_Dropdown.OptionData>();

        foreach (string label in labels)
        {
            options.Add(new TMP_Dropdown.OptionData(label));
        }

        _dropdown.ClearOptions();
        _dropdown.AddOptions(options);
        _dropdown.RefreshShownValue();
    }

    internal void SetValue(int value)
    {
        _dropdown.SetValueWithoutNotify(value);
        _dropdown.RefreshShownValue();
    }

    internal void SetVisible(bool isVisible)
    {
        if (_row.activeSelf != isVisible)
        {
            _row.SetActive(isVisible);
        }
    }

    internal void SetSlot(int slot, int slots)
    {
        float room = BoardParts.SlotWidth(slots) - BoardParts.Layout.SlotMargin * 2f;
        float scale = Mathf.Min(BoardParts.Layout.RowScale, room / BoardParts.RowPaintedWidth);

        _rect.anchorMin = _rect.anchorMax = new Vector2(0.5f, 0.5f);
        _rect.pivot = new Vector2(0.5f, 1f);
        _rect.sizeDelta = new Vector2(BoardParts.RowWidth, BoardParts.DropdownRowHeight);
        _rect.anchoredPosition = new Vector2(BoardParts.SlotCentre(slot, slots), BoardParts.Layout.ContentTop);
        _rect.localScale = Vector3.one * scale;
    }

    private void Restack()
    {
        if (_caption != null)
        {
            RectTransform header = _caption.rectTransform;

            header.anchorMin = new Vector2(0f, 1f);
            header.anchorMax = new Vector2(1f, 1f);
            header.pivot = new Vector2(0.5f, 1f);
            header.sizeDelta = new Vector2(0f, BoardParts.Layout.HeaderHeight);
            header.anchoredPosition = Vector2.zero;
            header.localScale = Vector3.one;

            _caption.alignment = TextAlignmentOptions.Center;
        }

        var bar = _dropdown.GetComponent<RectTransform>();

        bar.anchorMin = bar.anchorMax = new Vector2(0.5f, 1f);
        bar.pivot = new Vector2(0.5f, 1f);
        bar.sizeDelta = new Vector2(BoardParts.RowWidth, BoardParts.DropdownHeight);
        bar.anchoredPosition = new Vector2(0f, -(BoardParts.Layout.HeaderHeight + BoardParts.Layout.HeaderGap));
        bar.localScale = Vector3.one;
    }
}

internal sealed class SliderRow
{
    private const string ValueName = "Count";

    private readonly GameObject _row;
    private readonly RectTransform _rect;
    private readonly Slider _slider;
    private readonly TMP_Text _caption;
    private readonly TMP_Text _value;

    internal Action<float> OnChanged;

    internal SliderRow(GameObject row, Slider slider, TMP_Text caption)
    {
        _row = row;
        _rect = row.GetComponent<RectTransform>();
        _slider = slider;
        _caption = caption;
        _value = FindValue(slider);

        _slider.onValueChanged.RemoveAllListeners();
        _slider.wholeNumbers = false;
        _slider.onValueChanged.AddListener(value =>
        {
            ShowValue(value);
            OnChanged?.Invoke(value);
        });

        Restack();
    }

    internal void Place(float centreX, float topY)
    {
        _rect.anchorMin = _rect.anchorMax = new Vector2(0.5f, 0.5f);
        _rect.pivot = new Vector2(0.5f, 1f);
        _rect.sizeDelta = new Vector2(BoardParts.RowWidth, BoardParts.SliderRowHeight);
        _rect.anchoredPosition = new Vector2(centreX, topY);
        _rect.localScale = Vector3.one * BoardParts.Layout.RowScale;
    }

    internal void SetLabel(string text)
    {
        if (_caption != null)
        {
            _caption.text = text;
        }
    }

    internal void SetLabelLook(TMP_Text source)
    {
        BoardParts.CopyTextLook(source, _caption);
    }

    internal void SetLabelColor(Color color)
    {
        BoardParts.SetTextColor(_caption, color);
    }

    internal void SetRange(float min, float max, bool wholeNumbers)
    {
        _slider.wholeNumbers = wholeNumbers;
        _slider.minValue = min;
        _slider.maxValue = max;
    }

    internal void SetValue(float value)
    {
        _slider.value = value;

        ShowValue(_slider.value);
    }

    internal void SetVisible(bool isVisible)
    {
        if (_row.activeSelf != isVisible)
        {
            _row.SetActive(isVisible);
        }
    }

    private static TMP_Text FindValue(Slider slider)
    {
        foreach (TMP_Text text in slider.GetComponentsInChildren<TMP_Text>(true))
        {
            if (text.name == ValueName)
                return text;
        }

        return null;
    }

    private void ShowValue(float value)
    {
        if (_value == null)
            return;

        _value.text = value.ToString(_slider.wholeNumbers ? "0" : "0.0", CultureInfo.InvariantCulture);
    }

    private void Restack()
    {
        if (_caption != null)
        {
            RectTransform header = _caption.rectTransform;

            header.anchorMin = new Vector2(0f, 1f);
            header.anchorMax = new Vector2(1f, 1f);
            header.pivot = new Vector2(0.5f, 1f);
            header.sizeDelta = new Vector2(0f, BoardParts.Layout.HeaderHeight);
            header.anchoredPosition = Vector2.zero;
            header.localScale = Vector3.one;
        }

        var bar = _slider.GetComponent<RectTransform>();

        bar.anchorMin = bar.anchorMax = new Vector2(0.5f, 1f);
        bar.pivot = new Vector2(0.5f, 1f);
        bar.sizeDelta = new Vector2(BoardParts.SliderWidth, BoardParts.SliderHeight);
        bar.anchoredPosition = new Vector2(0f, -BoardParts.Layout.HeaderHeight);
        bar.localScale = Vector3.one;
    }
}
