using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using System;
using System.Linq;

namespace Reshape.ReGraph
{
    public class GraphViewer : GraphView
    {
        public new class UxmlFactory : UxmlFactory<GraphViewer, GraphView.UxmlTraits> { }

        public Action<GraphNodeView> OnNodeSelected;
        public Action<GraphNodeView> OnNodeUnselected;

        SerializedGraph serializer;
        GraphSettings settings;

        public GraphViewer ()
        {
            settings = GraphSettings.GetSettings();

            Insert(0, new GridBackground());

            var zoomer = new ContentZoomer();
            zoomer.maxScale = 1.3f;
            this.AddManipulator(zoomer);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new GraphDoubleClickSelection());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            var styleSheet = settings.graphStyle;
            styleSheets.Add(styleSheet);

            viewTransformChanged += OnViewTransformChanged;
        }

        void OnViewTransformChanged (GraphView graphView)
        {
            Vector3 pos = contentViewContainer.transform.position;
            Vector3 size = contentViewContainer.transform.scale;
            serializer?.SetViewTransform(pos, size);
        }

        public GraphNodeView FindNodeView (GraphNode node)
        {
            if (node == null) return null;
            return GetNodeByGuid(node.guid) as GraphNodeView;
        }

        public void CleanView ()
        {
            CleanElements(graphElements.ToList());
        }

        public void ClearView ()
        {
            CleanView();
            serializer = null;
        }

        public void PopulateView (SerializedGraph tree)
        {
            serializer = tree;
            CleanView();
            Debug.Assert(serializer.graph.Created);

            serializer.graph.nodes.ForEach(n => CreateNodeView(n));

            serializer.graph.nodes.ForEach(n =>
            {
                if (n != null)
                {
                    AddElements(n);
                    n.runner = serializer.runner;
                }
            });

            contentViewContainer.transform.position = serializer.graph.viewPosition;
            contentViewContainer.transform.scale = serializer.graph.viewScale;
        }

        public void RefreshEdge (GraphNodeView nodeView)
        {
            if (nodeView == null) return;
            CleanElements(nodeView.output.connections);
            AddElements(nodeView.node);
        }

        public override List<Port> GetCompatiblePorts (Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> returnList = new List<Port>();
            List<Port> portList = ports.ToList();
            GraphNodeView startNodeView = (GraphNodeView) startPort.node;
            for (int i = 0; i < portList.Count; i++)
            {
                Port endPort = portList[i];
                if (serializer.graph.Type == Graph.GraphType.BehaviourGraph)
                {
                    GraphNodeView endNodeView = (GraphNodeView) endPort.node;
                    if (startPort.direction == Direction.Output)
                    {
                        if (startNodeView.node is RootNode && endNodeView.node is TriggerNode == false)
                            continue;
                        if (startNodeView.node is TriggerNode && endNodeView.node is BehaviourNode == false)
                            continue;
                        if (startNodeView.node is BehaviourNode && endNodeView.node is BehaviourNode == false)
                            continue;
                        if (startNodeView.node is VariableBehaviourNode or DialogBehaviourNode == false && endNodeView.node is ConditionNode)
                            continue;
                    }
                    else
                    {
                        if (startNodeView.node is TriggerNode && endNodeView.node is RootNode == false)
                            continue;
                        if (startNodeView.node is ConditionNode && endNodeView.node is VariableBehaviourNode or DialogBehaviourNode == false)
                            continue;
                        if (startNodeView.node is BehaviourNode && endNodeView.node is BehaviourNode or TriggerNode == false)
                            continue;
                    }
                }

                if (endPort.direction != startPort.direction && endPort.node != startPort.node)
                    returnList.Add(endPort);
            }

            return returnList;
        }

        private GraphViewChange OnGraphViewChanged (GraphViewChange graphViewChange)
        {
            if (graphViewChange.elementsToRemove != null)
            {
                graphViewChange.elementsToRemove.ForEach(elem =>
                {
                    GraphNodeView nodeView = elem as GraphNodeView;
                    if (nodeView != null)
                    {
                        nodeView.UnhighlightReference();
                        serializer.DeleteNode(nodeView.node);
                        OnNodeSelected(null);
                    }

                    Edge edge = elem as Edge;
                    if (edge != null)
                    {
                        GraphNodeView parentView = edge.output.node as GraphNodeView;
                        GraphNodeView childView = edge.input.node as GraphNodeView;
                        serializer.RemoveChild(parentView.node, childView.node);
                        childView.ResetInputPortColor();
                    }
                });
            }

            if (graphViewChange.edgesToCreate != null)
            {
                graphViewChange.edgesToCreate.ForEach(edge =>
                {
                    GraphNodeView parentView = edge.output.node as GraphNodeView;
                    GraphNodeView childView = edge.input.node as GraphNodeView;
                    serializer.AddChild(parentView.node, childView.node);
                    childView.UpdateInputPortColor(parentView);
                });
            }

            nodes.ForEach((n) =>
            {
                GraphNodeView view = n as GraphNodeView;
                List<GraphNode> sorted = view.SortChildren();
                if (sorted != null)
                    serializer.SortChildren(view.node, sorted);
            });

            return graphViewChange;
        }

        public override void BuildContextualMenu (ContextualMenuPopulateEvent evt)
        {
            if (serializer != null && serializer.graph != null)
            {
                Vector2 nodePosition = this.ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);
                Dictionary<string, System.Type> list = GetContextualList(serializer.graph);
                foreach (var menuItem in list)
                    evt.menu.AppendAction(menuItem.Key, (a) => CreateNode(menuItem.Value, nodePosition));
            }
        }

        /* START - Custom View start here */
        public Dictionary<string, System.Type> GetContextualList (Graph graph)
        {
            var list = new Dictionary<string, System.Type>();
            List<System.Type> existed = new List<Type>();
            if (graph.Type == Graph.GraphType.BehaviourGraph)
            {
                var types = TypeCache.GetTypesDerivedFrom<TriggerNode>();
                foreach (var type in types)
                {
                    if (!existed.Contains(type))
                    {
                        list.Add($"Trigger/{type.Name.Substring(0, type.Name.IndexOf("TriggerNode", StringComparison.Ordinal))}", type);
                        existed.Add(type);
                    }
                }

                types = TypeCache.GetTypesDerivedFrom<ConditionNode>();
                foreach (var type in types)
                {
                    if (!existed.Contains(type))
                    {
                        list.Add($"Condition/{type.Name.Substring(0, type.Name.IndexOf("ConditionNode", StringComparison.Ordinal))}", type);
                        existed.Add(type);
                    }
                }

                types = TypeCache.GetTypesDerivedFrom<BehaviourNode>();
                foreach (var type in types)
                {
                    if (!existed.Contains(type) && type != typeof(ConditionNode))
                    {
                        var behaviourNode = (BehaviourNode)Activator.CreateInstance(type);
                        list.Add($"Behaviour/{behaviourNode.GetNodeMenuDisplayName()}", type);
                        existed.Add(type);
                    }
                }
            }

            return list;
        }

        public string GetStyle (GraphNode node)
        {
            if (node is TriggerNode)
            {
                return "trigger";
            }
            else if (node is RootNode)
            {
                return "root";
            }
            else if (node is ConditionNode)
            {
                return "condition";
            }
            else if (node is BehaviourNode)
            {
                return "behaviour";
            }

            return string.Empty;
        }

        public string GetDisableStyle ()
        {
            return "disable";
        }

        public string GetRedLabelStyle ()
        {
            return "red-label";
        }
        /* END - Custom View end here */

        public ContextualMenuPopulateEvent GetDeleteAction (ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Delete", (Action<DropdownMenuAction>) (a => this.DeleteSelectionCallback(UnityEditor.Experimental.GraphView.GraphView.AskUser.DontAskUser)),
                (Func<DropdownMenuAction, DropdownMenuAction.Status>) (a => this.canDeleteSelection ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled));
            evt.menu.AppendSeparator();
            return evt;
        }

        public ContextualMenuPopulateEvent GetDuplicateAction (ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Duplicate", (Action<DropdownMenuAction>) (a =>
                {
                    var list = selection.OfType<GraphElement>();
                    foreach (var element in list)
                    {
                        if (element is GraphNodeView)
                        {
                            GraphNodeView nodeView = (GraphNodeView) element;
                            Rect pos = nodeView.GetPosition();
                            DuplicateNode(nodeView.node, new Vector2(pos.x + 10, pos.y + 10));
                        }
                    }
                }),
                (Func<DropdownMenuAction, DropdownMenuAction.Status>) (a => this.canDuplicateSelection ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled));
            evt.menu.AppendSeparator();
            return evt;
        }

        void SelectFolder (string path)
        {
            // https://forum.unity.com/threads/selecting-a-folder-in-the-project-via-button-in-editor-window.355357/
            // Check the path has no '/' at the end, if it does remove it,
            // Obviously in this example it doesn't but it might
            // if your getting the path some other way.

            if (path[path.Length - 1] == '/')
                path = path.Substring(0, path.Length - 1);

            // Load object
            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));

            // Select the object in the project folder
            Selection.activeObject = obj;

            // Also flash the folder yellow to highlight it
            EditorGUIUtility.PingObject(obj);
        }

        void CreateNode (System.Type type, Vector2 position)
        {
            GraphNode node = serializer.CreateNode(type, position);
            CreateNodeView(node);
        }
        
        void DuplicateNode (GraphNode selectedNode, Vector2 position)
        {
            GraphNode node = serializer.CloneNode(selectedNode, position);
            CreateNodeView(node);
        }

        void CreateNodeView (GraphNode node)
        {
            GraphNodeView nodeView = new GraphNodeView(serializer, node, this);
            nodeView.OnNodeSelected = OnNodeSelected;
            nodeView.OnNodeUnselected = OnNodeUnselected;
            AddElement(nodeView);
        }

        public void UpdateNodeStates ()
        {
            if (serializer != null)
                serializer.graph.selectedViewNode = selection;
            nodes.ForEach(n =>
            {
                GraphNodeView view = n as GraphNodeView;
                view.Update();
            });
        }

        public void HighlightReferenceNode (string nodeId)
        {
            nodes.ForEach(n =>
            {
                GraphNodeView view = n as GraphNodeView;
                if (view.node != null && view.node.guid == nodeId)
                    view.ApplyRunningHighlight();
            });
        }

        public void UnhighlightReferenceNode (string nodeId)
        {
            nodes.ForEach(n =>
            {
                GraphNodeView view = n as GraphNodeView;
                if (view.node != null && view.node.guid == nodeId)
                    view.UnapplyRunningHighlight();
            });
        }

        public void UnhighlightAllReferenceNode ()
        {
            nodes.ForEach(n =>
            {
                GraphNodeView view = n as GraphNodeView;
                view.UnapplyRunningHighlight();
            });
        }

        private void CleanElements (IEnumerable<GraphElement> elements)
        {
            graphViewChanged -= OnGraphViewChanged;
            DeleteElements(elements.ToList());
            graphViewChanged += OnGraphViewChanged;
        }

        private void AddElements (GraphNode parentNode)
        {
            var children = Graph.GetChildren(parentNode);
            children.ForEach(c =>
            {
                GraphNodeView parentView = FindNodeView(parentNode);
                GraphNodeView childView = FindNodeView(c);
                if (childView != null && parentView != null)
                {
                    Edge edge = parentView.output.ConnectTo(childView.input);
                    AddElement(edge);
                    childView.UpdateInputPortColor();
                }
            });
        }
    }
}