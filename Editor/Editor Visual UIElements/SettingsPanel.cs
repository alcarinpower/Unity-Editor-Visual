using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEditor;

namespace CodeDestroyer.Editor.EditorVisual.UIElements
{
    /// <summary>
    /// Represents a settings panel similar to Unity's Project Settings or Preferences. This panel requires a list of TreeViewItemData (for the left pane)
    /// and a dictionary of VisualElements (for the right pane) to function.
    /// </summary>
    [UxmlElement]
    public partial class SettingsPanel : VisualElement
    {
        [UxmlAttribute]
        public Color splitterLineColor;
        [UxmlAttribute]
        public float splitterLineWidth = 1f;
        [UxmlAttribute]
        public VisualElement searchBarContainer { get; set; }
        [UxmlAttribute]
        public ToolbarSearchField searchField { get; set; }
        [UxmlAttribute]
        public VisualElement searchBarLine { get; set; }
        [UxmlAttribute]
        public TwoPaneSplitView twoPaneSplitView { get; set; }
        [UxmlAttribute]
        public TreeView leftPaneTreeView { get; set; }
        [UxmlAttribute]
        public ScrollView rightPaneScrollView { get; set; }

        // Unity's Default UIElements Names
        private static readonly string TwoPaneSplitViewDragLine = "unity-two-pane-split-view__dragline";
        private static readonly string TwoPaneSplitViewDragLineAnchor = "unity-two-pane-split-view__dragline-anchor";
        private Color splitterLineHighlightColor = new Color(0.736f, 0.736f, 0.736f, 1.000f);

        private List<TreeViewItemData<string>> leftPaneTreeViewList;
        private Dictionary<string, VisualElement> rightPaneSettingsDict;
        private List<string> searchList = new List<string>();

        /// <summary>
        /// Initializes an empty SettingsPanel with default settings. Sets up the toolbar search field, search bar line, and the two-pane splitter layout.
        /// </summary>
        public SettingsPanel()
        {
            CalculateHighlightedColorBasedOnProSkin();

            this.splitterLineColor = GlobalVariables.DefaultLineColor;

            ToolBarSearchField();
            SearchbarLineField();
            TwoPaneSplitField(this.splitterLineColor, splitterLineHighlightColor, splitterLineWidth);

            style.height = Length.Percent(100);
        }

        /// <summary>
        /// Initializes the SettingsPanel with a list of TreeViewItemData for the left pane
        /// and a dictionary of VisualElements for the right pane. The left pane contains
        /// hierarchical tree view items, while the right pane will have associated VisualElements
        /// that correspond to the tree view items in the left pane.
        /// </summary>
        /// <param name="leftPaneTreeViewItems">A list of TreeViewItemData for the left pane. These items may contain nested children, representing a tree structure of settings.</param>
        /// <param name="settingsVisualElement">A dictionary mapping setting names (from left pane) to VisualElements in the right pane. The order of items in the dictionary is not important, but the keys must match the strings in the leftPaneTreeViewItems.</param>

        public SettingsPanel(ref List<TreeViewItemData<string>> leftPaneTreeViewItems, ref Dictionary<string, VisualElement> settingsVisualElement)
        {
            CreateSettingsPanel(ref leftPaneTreeViewItems, ref settingsVisualElement, ref this.splitterLineColor, ref this.splitterLineHighlightColor, ref this.splitterLineWidth);
        }

        /// <summary>
        /// Initializes the SettingsPanel with a list of TreeViewItemData for the left pane and a dictionary of VisualElements for the right pane.
        /// The left pane contains hierarchical tree view items, while the right pane will have associated VisualElements that correspond to the tree view items in the left pane.
        /// This constructor allows setting a custom splitter line width, while using the default splitter line color and highlight color.
        /// </summary>
        /// <param name="leftPaneTreeViewItems">A list of TreeViewItemData for the left pane. These items may contain nested children, representing a tree structure of settings.</param>
        /// <param name="settingsVisualElement">A dictionary mapping setting names (from the left pane) to VisualElements in the right pane. The order of items in the dictionary is not important, but the keys must match the strings in the leftPaneTreeViewItems.</param>
        /// <param name="splitterLineWidth">The width of the splitter line separating the left and right panes.</param>
        public SettingsPanel(ref List<TreeViewItemData<string>> leftPaneTreeViewItems, ref Dictionary<string, VisualElement> settingsVisualElement,
            float splitterLineWidth)
        {
            CreateSettingsPanel(ref leftPaneTreeViewItems, ref settingsVisualElement, ref this.splitterLineColor, ref this.splitterLineHighlightColor,
                ref splitterLineWidth);
        }

        /// <summary>
        /// Initializes the SettingsPanel with a list of TreeViewItemData for the left pane and a dictionary of VisualElements for the right pane.
        /// The left pane contains hierarchical tree view items, while the right pane will have associated VisualElements that correspond to the tree view items in the left pane.
        /// This constructor allows setting a custom splitter line width and color, along with the highlight color for the splitter line.
        /// </summary>
        /// <param name="leftPaneTreeViewItems">A list of TreeViewItemData for the left pane. These items may contain nested children, representing a tree structure of settings.</param>
        /// <param name="settingsVisualElement">A dictionary mapping setting names (from the left pane) to VisualElements in the right pane. The order of items in the dictionary is not important, but the keys must match the strings in the leftPaneTreeViewItems.</param>
        /// <param name="splitterLineWidth">The width of the splitter line separating the left and right panes.</param>
        /// <param name="splitterLineColor">The color of the splitter line. If set to Color.clear, the default color will be used.</param>
        public SettingsPanel(ref List<TreeViewItemData<string>> leftPaneTreeViewItems, ref Dictionary<string, VisualElement> settingsVisualElement,
            float splitterLineWidth, Color splitterLineColor)
        {
            if (splitterLineColor != Color.clear)
            {
                this.splitterLineColor = splitterLineColor;
            }
            else
            {
                this.splitterLineColor = GlobalVariables.DefaultLineColor;
            }

            this.splitterLineHighlightColor = GetLighterColor(this.splitterLineColor, 0.7f);

            CreateSettingsPanel(ref leftPaneTreeViewItems, ref settingsVisualElement, ref this.splitterLineColor, ref this.splitterLineHighlightColor,
                ref splitterLineWidth);
        }



        private void CreateSettingsPanel(ref List<TreeViewItemData<string>> leftPaneTreeViewItems, ref Dictionary<string, VisualElement> rightPaneSettingsDict,
            ref Color splitterLineColor, ref Color splitterLineHighlightColor, ref float splitterLineWidth)
        {
            this.rightPaneSettingsDict = rightPaneSettingsDict;
            this.leftPaneTreeViewList = leftPaneTreeViewItems;
            this.splitterLineWidth = Mathf.Clamp(this.splitterLineWidth, 1f, 1000f);
            this.splitterLineColor = GlobalVariables.DefaultLineColor;


            for (int i = 0; i < leftPaneTreeViewItems.Count; i++)
            {
                CollectTreeViewDataRecursive(leftPaneTreeViewItems[i], ref searchList);
            }


            ToolBarSearchField();
            SearchbarLineField();
            TwoPaneSplitField(splitterLineColor, splitterLineHighlightColor, splitterLineWidth);

            style.height = Length.Percent(100);
        }

        private void ToolBarSearchField()
        {
            searchBarContainer = new VisualElement();
            searchBarContainer.style.flexDirection = FlexDirection.Row;
            searchBarContainer.style.justifyContent = Justify.FlexEnd;
            searchBarContainer.style.backgroundColor = GlobalVariables.DefaultBackgroundColor;

            float halfStandardVerticalSpacing = EditorGUIUtility.standardVerticalSpacing / 2f;
            searchBarContainer.style.paddingTop = halfStandardVerticalSpacing;
            searchBarContainer.style.paddingBottom = halfStandardVerticalSpacing;
            searchBarContainer.style.paddingRight = halfStandardVerticalSpacing;


            searchField = new ToolbarSearchField();
            searchField.RegisterValueChangedCallback(evt =>
            {
                if (string.IsNullOrEmpty(searchField.value))
                {
                    leftPaneTreeView.SetRootItems<string>(leftPaneTreeViewList);
                    leftPaneTreeView.RefreshItems();
                }
                else
                {
                    FilterLeftPaneTreeViewForSearchField(searchField.value);
                }
            });
            searchBarContainer.Add(searchField);
            Add(searchBarContainer);
        }
        private void SearchbarLineField()
        {
            searchBarLine = new Line();
            Add(searchBarLine);
        }

        VisualElement leftPaneContainer;
        private void TwoPaneSplitField(Color lineColor, Color lineHighlightColor, float lineWidth)
        {
            twoPaneSplitView = new TwoPaneSplitView(0, 200, TwoPaneSplitViewOrientation.Horizontal);
            
            // If you want to save twopanesplit view splitter, you can uncomment this below line
            //twoPaneSplitView.viewDataKey = "Main TwoPaneSplitView";

            leftPaneTreeView = new TreeView(20, MakeItemForLeftPaneTreeView, BindItemForLeftPaneTreeView);
            leftPaneTreeView.viewDataKey = "Left Pane TreeView";
            leftPaneTreeView.SetRootItems<string>(leftPaneTreeViewList);
            leftPaneTreeView.selectionType = SelectionType.Single;
            leftPaneTreeView.RefreshItems();
            leftPaneTreeView.style.minWidth = 100f;
            leftPaneTreeView.style.unityTextAlign = TextAnchor.MiddleCenter;

            // We need to create empty VisualElement to not make TreeView and TwoPaneSplitView conflict and be buggy
            leftPaneContainer = new VisualElement();
            leftPaneContainer.style.minWidth = 100f;


            leftPaneContainer.Add(leftPaneTreeView);


            leftPaneTreeView.selectedIndicesChanged += OnProjectSettingsChange;

            rightPaneScrollView = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
            rightPaneScrollView.style.minWidth = 150f;


            VisualElement dragLine = twoPaneSplitView.Q<VisualElement>(className: TwoPaneSplitViewDragLine);
            dragLine.style.backgroundColor = Color.clear;
            dragLine.style.width = lineWidth + 5f;
            dragLine.style.marginLeft = 5f;

            VisualElement anchor = twoPaneSplitView.Q<VisualElement>(className: TwoPaneSplitViewDragLineAnchor);
            anchor.style.backgroundColor = lineColor;
            anchor.style.width = lineWidth;


            bool isUserPressed = false;
            bool isMouseOverDragLine = false;


            dragLine.RegisterCallback<MouseEnterEvent>(evt =>
            {
                anchor.style.backgroundColor = lineHighlightColor;
                isMouseOverDragLine = true;
            });

            dragLine.RegisterCallback<MouseLeaveEvent>(evt =>
            {
                if (!isUserPressed)
                {
                    anchor.style.backgroundColor = lineColor;
                }
                isMouseOverDragLine = false;
            });


            dragLine.RegisterCallback<MouseDownEvent>(evt =>
            {
                anchor.style.backgroundColor = lineHighlightColor;
                isUserPressed = true;
            });
            anchor.RegisterCallback<MouseUpEvent>(evt =>
            {
                if (!isMouseOverDragLine)
                {
                    anchor.style.backgroundColor = lineColor;
                }
                isUserPressed = false;
            });

            twoPaneSplitView.Add(leftPaneContainer);
            twoPaneSplitView.Add(rightPaneScrollView);
            Add(twoPaneSplitView);
        }



        private VisualElement MakeItemForLeftPaneTreeView()
        {
            return new Label();
        }
        private void BindItemForLeftPaneTreeView(VisualElement item, int index)
        {
            Label labelItem = item as Label;
            labelItem.style.unityFontStyleAndWeight = FontStyle.Bold;
            labelItem.style.fontSize = 12;
            labelItem.style.marginTop = 3f;
            labelItem.style.unityTextAlign = TextAnchor.MiddleLeft;
            labelItem.style.alignSelf = Align.FlexStart;

            if (labelItem != null)
            {
                labelItem.text = leftPaneTreeView.GetItemDataForIndex<string>(index);
            }
        }


        private void OnProjectSettingsChange(IEnumerable<int> selectedItems)
        {
            IEnumerable<TreeViewItemData<string>> selectedTreeViewItems = leftPaneTreeView.GetSelectedItems<string>();

            foreach (TreeViewItemData<string> setting in selectedTreeViewItems)
            {
                rightPaneScrollView.Clear();

                if (setting.data != null)
                {
                    rightPaneScrollView.Add(rightPaneSettingsDict[setting.data]);
                }
            }
        }

        private void CalculateHighlightedColorBasedOnProSkin()
        {
            if (EditorGUIUtility.isProSkin)
            {
                this.splitterLineHighlightColor = GetLighterColor(this.splitterLineColor, 0.7f);
            }
            else
            {
                this.splitterLineHighlightColor = new Color(
                this.splitterLineHighlightColor.r * 0.1f,
                this.splitterLineHighlightColor.g * 0.1f,
                this.splitterLineHighlightColor.b * 0.1f,
                this.splitterLineHighlightColor.a);
            }
        }
        private Color GetLighterColor(Color originalColor, float blendAmount = 0.1f)
        {
            blendAmount = Mathf.Clamp01(blendAmount);

            Color lighterColor = Color.Lerp(originalColor, Color.white, blendAmount);

            return lighterColor;
        }
        private void FilterLeftPaneTreeViewForSearchField(string searchQuery)
        {
            searchQuery = searchQuery.Trim();

            List<TreeViewItemData<string>> filteredItems = new List<TreeViewItemData<string>>();

            for (int i = 0; i < searchList.Count; i++)
            {
                string searchString = searchList[i];

                if (searchString.ToLower().Contains(searchQuery.ToLower()))
                {
                    filteredItems.Add(new TreeViewItemData<string>(i, searchString));
                }
            }

            leftPaneTreeView.SetRootItems(filteredItems);
            leftPaneTreeView.RefreshItems();
            leftPaneTreeView.SetSelection(0);
        }
        private void CollectTreeViewDataRecursive(TreeViewItemData<string> item, ref List<string> searchList)
        {
            searchList.Add(item.data);

            if (item.children != null)
            {
                IEnumerator<TreeViewItemData<string>> enumerator = item.children.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    CollectTreeViewDataRecursive(enumerator.Current, ref searchList);
                }

            }
        }
    }

}
