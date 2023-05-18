using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEngine.SceneManagement;

namespace Reshape.ReGraph
{
    public class GraphEditorWindow : EditorWindow
    {
        SerializedGraph serializer;
        GraphSettings settings;
        GraphViewer treeView;

        /*WindowInspectorView inspectorView;
        WindowBlackboardView blackboardView;
        WindowOverlayView overlayView;*/
        ToolbarMenu toolbarMenu;
        Label titleLabel;

        private bool refreshAfterNewlySaved;

        [MenuItem("Tools/Reshape/Graph Editor", priority = 11000)]
        public static void OpenWindow ()
        {
            GraphEditorWindow wd = GetWindow<GraphEditorWindow>();
            wd.titleContent = new GUIContent("GraphEditor");
            wd.minSize = new Vector2(800, 600);
        }

        public static void OpenWindow (GraphRunner runner)
        {
            GraphEditorWindow wd = GetWindow<GraphEditorWindow>();
            wd.titleContent = new GUIContent("Graph Editor");
            wd.minSize = new Vector2(800, 600);
            wd.SelectGraph(new SerializedObject(runner));
        }

        public static bool HasFocus ()
        {
            EditorWindow[] ed = (EditorWindow[]) Resources.FindObjectsOfTypeAll<EditorWindow>();
            for (int i = 0; i < ed.Length; i++)
            {
                if (ed[i].GetType() == typeof(GraphEditorWindow))
                {
                    var wnd = (GraphEditorWindow) ed[i];
                    return wnd.hasFocus;
                }
            }

            return false;
        }

        public static void RefreshCurrentGraph ()
        {
            EditorWindow[] ed = (EditorWindow[]) Resources.FindObjectsOfTypeAll<EditorWindow>();
            for (int i = 0; i < ed.Length; i++)
            {
                if (ed[i].GetType() == typeof(GraphEditorWindow))
                {
                    var wnd = (GraphEditorWindow) ed[i];
                    if (wnd.hasFocus && wnd.serializer != null)
                    {
                        GraphRunner runner = Selection.activeGameObject.GetComponent<GraphRunner>();
                        if (runner != null)
                            wnd.SelectGraph(new SerializedObject(runner));
                    }

                    return;
                }
            }
        }

        public void CreateGUI ()
        {
            settings = GraphSettings.GetSettings();

            VisualElement root = rootVisualElement;
            var visualTree = settings.graphXml;
            visualTree.CloneTree(root);
            var styleSheet = settings.graphStyle;
            root.styleSheets.Add(styleSheet);

            treeView = root.Q<GraphViewer>();
            //inspectorView = root.Q<InspectorView>();
            //blackboardView = root.Q<BlackboardView>();
            //overlayView = root.Q<OverlayView>("OverlayView");
            toolbarMenu = root.Q<ToolbarMenu>();
            titleLabel = root.Q<Label>("TitleLabel");

            toolbarMenu.RegisterCallback<MouseEnterEvent>((evt) =>
            {
                toolbarMenu.menu.MenuItems().Clear();
                toolbarMenu.menu.AppendAction($"Open Graph Inspector", (a) => { GraphInspector.OpenWindow(); });
                toolbarMenu.menu.AppendAction($"Open Graph Finder", (a) => { GraphFinder.OpenWindow(); });
                toolbarMenu.menu.AppendSeparator();
            });

            // Overlay view
            treeView.OnNodeSelected = OnNodeSelectionChanged;
            treeView.OnNodeUnselected = OnNodeUnselectionChanged;
            //overlayView.OnTreeSelected += SelectTree;
            Undo.undoRedoPerformed += OnUndoRedo;

            if (serializer == null)
            {
                //overlayView.Show();
            }
        }

        void OnUndoRedo ()
        {
            if (serializer != null)
            {
                treeView.PopulateView(serializer);
            }
        }

        private void OnSelectionChange ()
        {
            if (Selection.objects.Length == 1)
            {
                if (Selection.activeGameObject)
                {
                    GraphRunner runner = Selection.activeGameObject.GetComponent<GraphRunner>();
                    if (runner != null)
                    {
                        SelectGraph(new SerializedObject(runner));
                        return;
                    }
                }
            }

            ClearSelection();
        }

        void SelectGraph (SerializedObject runnerObj)
        {
            if (runnerObj == null)
            {
                ClearSelection();
                return;
            }
            
            GraphRunner runner = runnerObj.targetObject as GraphRunner;
            if (runner != null && !runner.graph.Created)
            {
                ClearSelection();
                return;
            }
            
            if (serializer != null && serializer.runner == runner && serializer.graph == runner.graph)
                return;
            
            serializer = new SerializedGraph(runnerObj);
            ResetViewPreviewNode();

            refreshAfterNewlySaved = false;
            if (string.IsNullOrEmpty(runner.gameObject.scene.path))
            {
                if (titleLabel != null)
                    titleLabel.text = $"Graph from GameObject {runner.gameObject.name} in a newly created scene";
                EditorSceneManager.sceneSaved -= OnSceneSaved;
                EditorSceneManager.sceneSaved += OnSceneSaved;
            }
            else
            {
                if (titleLabel != null)
                    titleLabel.text = $"Graph from GameObject {runner.gameObject.name} in {runner.gameObject.scene.path} [ID:{runner.graph.id}]";
            }

            //overlayView.Hide();
            if (treeView != null)
                treeView.PopulateView(serializer);
            //blackboardView.Bind(serializer);
        }

        void ClearSelection ()
        {
            if (serializer != null)
            {
                ResetViewPreviewNode();
                serializer = null;
            }

            //overlayView.Show();
            if (treeView != null)
                treeView.ClearView();
            //inspectorView.ClearSelection(serializer);

            if (titleLabel != null)
                titleLabel.text = "Graph View";
        }

        private void OnSceneSaved (Scene scene)
        {
            EditorSceneManager.sceneSaved -= OnSceneSaved;
            EditorApplication.delayCall += () => { refreshAfterNewlySaved = true; };
        }

        void OnNodeSelectionChanged (GraphNodeView node)
        {
            if (serializer != null && serializer.graph != null && node != null)
            {
                AssignViewPreviewNode(node.node);
                //RepaintInspector("UnityEditor.GameObjectInspector");
            }
            //inspectorView.UpdateSelection(serializer, node);
        }

        void OnNodeUnselectionChanged (GraphNodeView node)
        {
            if (serializer != null)
            {
                ResetViewPreviewNode();
                //RepaintInspector("UnityEditor.GameObjectInspector");
            }
        }

        private void ResetViewPreviewNode ()
        {
            serializer.SetViewPreviewNode(null);
            serializer.SetViewPreviewSelected(false);
        }

        private void AssignViewPreviewNode (GraphNode node)
        {
            serializer.SetViewPreviewNode(node);
            serializer.SetViewPreviewSelected(true);
        }

        private void OnInspectorUpdate ()
        {
            if (Selection.objects.Length == 1)
            {
                if (Selection.activeGameObject)
                {
                    GraphRunner runner = Selection.activeGameObject.GetComponent<GraphRunner>();
                    if (runner != null)
                    {
                        if (serializer == null || runner.graph != serializer.graph)
                        {
                            SelectGraph(new SerializedObject(runner));
                        }
                        else if (runner.graph.Created && treeView.graphElements.Count() == 0)
                        {
                            SelectGraph(new SerializedObject(runner));
                        }

                        if (runner.graph.selectedViewNode != null && runner.graph.selectedViewNode.Count == 1 && !runner.graph.HavePreviewNode())
                        {
                            GraphNodeView nodeView = runner.graph.selectedViewNode[0] as GraphNodeView;
                            if (nodeView != null)
                                AssignViewPreviewNode(nodeView.node);
                        }
                    }
                    else
                    {
                        if (serializer != null && serializer.graph != null)
                        {
                            ClearSelection();
                        }
                    }
                }
            }

            if (refreshAfterNewlySaved)
            {
                if (serializer.SetGraphId())
                {
                    if (titleLabel != null)
                        titleLabel.text = $"Graph from GameObject {serializer.runner.gameObject.name} in {serializer.runner.gameObject.scene.path} [ID:{serializer.runner.graph.id}]";
                    refreshAfterNewlySaved = false;
                    EditorSceneManager.MarkSceneDirty(serializer.runner.gameObject.scene);
                    EditorSceneManager.SaveScene(serializer.runner.gameObject.scene, serializer.runner.gameObject.scene.path);
                }
            }

            treeView?.UpdateNodeStates();
        }

        private void OnEnable ()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable ()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged (PlayModeStateChange obj)
        {
            switch (obj)
            {
                case PlayModeStateChange.EnteredEditMode:
                    EditorApplication.delayCall += OnSelectionChange;
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    EditorApplication.delayCall += OnSelectionChange;
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    break;
            }
        }

        private void RepaintInspector (string typeName)
        {
            Editor[] ed = (Editor[]) Resources.FindObjectsOfTypeAll<Editor>();
            for (int i = 0; i < ed.Length; i++)
            {
                if (ed[i].GetType().ToString() == typeName)
                {
                    ed[i].Repaint();
                    return;
                }
            }
        }
    }
}