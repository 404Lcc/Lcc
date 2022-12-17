using LccModel;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace LccEditor
{
    public class FrameworkTreeViewItem : TreeViewItem
    {
        private AObjectBase aObjectBase;
        public FrameworkTreeViewItem(AObjectBase aObjectBase, int id)
        {
            this.aObjectBase = aObjectBase;
            base.id = id;
        }
        public override string displayName
        {
            get
            {
                if (aObjectBase != null)
                {
                    string name = aObjectBase.GetType().Name;
                    return name;
                }
                else
                {
                    return "未运行";
                }
            }
        }
    }
    public class FrameworkTreeView : TreeView
    {
        private FrameworkTreeViewItem root;
        private int id;

        public Dictionary<int, AObjectBase> dict = new Dictionary<int, AObjectBase>();
        private Dictionary<AObjectBase, int> historyID = new();

        public FrameworkTreeView(TreeViewState state) : base(state)
        {
            Reload();
            useScrollView = true;
        }
        protected override TreeViewItem BuildRoot()
        {
            if (Application.isPlaying && Game.Scene != null)
            {
                id = 0;
                root = PreOrder(Game.Scene);
                root.depth = -1;
                SetupDepthsFromParentsAndChildren(root);
                return root;
            }
            else
            {
                root = new FrameworkTreeViewItem(null, 0);
                root.AddChild(new FrameworkTreeViewItem(null, 1));
                root.depth = -1;
                SetupDepthsFromParentsAndChildren(root);
                return root;
            }
        }


        private FrameworkTreeViewItem PreOrder(AObjectBase root)
        {
            if (root is null)
            {
                return null;
            }

            if (!historyID.TryGetValue(root, out int itemID))
            {
                id++;
                itemID = id;

                historyID[root] = itemID;
            }

            FrameworkTreeViewItem item = new FrameworkTreeViewItem(root, itemID);

            dict[itemID] = root;

            if (root.Components.Count > 0)
            {
                foreach (var component in root.Components.Values)
                {
                    item.AddChild(PreOrder(component));
                }
            }

            if (root.Children.Count > 0)
            {
                foreach (var child in root.Children.Values)
                {
                    item.AddChild(PreOrder(child));
                }
            }

            return item;
        }



    }

    public class FrameworkTreeWindow : EditorWindow
    {
        private FrameworkTreeView treeView;
        private SearchField searchField;


        private static FrameworkTreeWindow window;

        [MenuItem("Lcc框架/Framework Debugger")]
        private static void OpenWindow()
        {
            window = GetWindow<FrameworkTreeWindow>("Framework Debugger");
            window.Show();
        }
        private void OnEnable()
        {
            treeView = new FrameworkTreeView(new TreeViewState());
            searchField = new SearchField();
            searchField.downOrUpArrowKeyPressed += treeView.SetFocusAndEnsureSelectedItem;
        }

        private void OnGUI()
        {
            treeView.OnGUI(new Rect(0, 0, position.width, position.height));
            treeView.Reload();
        }
    }
}