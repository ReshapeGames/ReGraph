using System;
using System.Collections.Generic;
using System.Linq;
using Reshape.Unity;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEditor;

namespace Reshape.ReGraph
{
    public class GraphNodeView : UnityEditor.Experimental.GraphView.Node
    {
        private static Color portUnreachableColor = new Color(0.89f, 0.46f, 0.46f);
        private static Color portDefaultColor = new Color(0.9411765f, 0.9411765f, 0.9411765f);

        public Action<GraphNodeView> OnNodeSelected;
        public Action<GraphNodeView> OnNodeUnselected;
        public SerializedGraph serializer;
        public GraphNode node;
        public GraphViewer viewer;
        public Port input;
        public Port output;
        public Port export;

        private Label descriptionLabel;
        private Label connectLabel;

        public GraphNodeView (SerializedGraph tree, GraphNode node, GraphViewer viewer) : base(AssetDatabase.GetAssetPath(GraphSettings.GetSettings().graphNodeXml))
        {
            serializer = tree;
            this.node = node;
            this.viewer = viewer;
            if (node != null)
            {
                title = node.GetNodeViewTitle();
                viewDataKey = node.guid;
                style.left = node.position.x;
                style.top = node.position.y;

                CreateInputPorts();
                CreateOutputPorts();
                /*
                //~~ TODO extra output port as export node
                CreateExportPorts();
                */
                SetupClasses();
                SetupDataBinding();
            }
            else
            {
                GraphRunner runner = serializer.serializedObject.targetObject as GraphRunner;
                ReDebug.LogWarning("Graph Editor", "System found a null graph node inside " + runner.gameObject.name, false);
            }
        }

        private void SetupDataBinding ()
        {
            var nodeProp = serializer.FindNode(serializer.Nodes, node);

            descriptionLabel = this.Q<Label>("description");
            UpdateDescriptionLabel();

            Label categoryLabel = this.Q<Label>("category");
            if (node is TriggerNode)
                categoryLabel.text = "Trigger";
            else if (node is ConditionNode)
                categoryLabel.text = "Condition";
            else if (node is BehaviourNode)
                categoryLabel.text = "Behaviour";

            connectLabel = this.Q<Label>("connectTo");
            if (node is RootNode)
                connectLabel.text = "Trigger";
            else if (node is TriggerNode)
                connectLabel.text = "Behaviour";
            else if (node is BehaviourNode)
                connectLabel.text = "Behaviour";
            UpdateConnectLabel();

            this.node.onEnableChange -= OnEnableChange;
            this.node.onEnableChange += OnEnableChange;
        }

        private void OnEnableChange ()
        {
            if (this.node.enabled)
                RemoveFromClassList(viewer.GetDisableStyle());
            else
                RemoveFromClassList(viewer.GetStyle(node));
            SetupClasses();
        }

        private void SetupClasses ()
        {
            if (!this.node.enabled)
                AddToClassList(viewer.GetDisableStyle());
            else
                AddToClassList(viewer.GetStyle(node));
        }

        private void CreateInputPorts ()
        {
            if (node is RootNode == false)
                input = new GraphNodePort(Direction.Input, Port.Capacity.Single);
            if (input != null)
            {
                input.portName = "";
                input.style.flexDirection = FlexDirection.Column;
                inputContainer.Add(input);
            }
        }

        private void CreateOutputPorts ()
        {
            if (node.GetChildrenType() == GraphNode.ChildrenType.Single)
                output = new GraphNodePort(Direction.Output, Port.Capacity.Single);
            else if (node.GetChildrenType() == GraphNode.ChildrenType.Multiple)
                output = new GraphNodePort(Direction.Output, Port.Capacity.Multi);
            if (output != null)
            {
                output.portName = "";
                output.style.flexDirection = FlexDirection.ColumnReverse;
                outputContainer.Add(output);
            }
        }

        private void CreateExportPorts ()
        {
            export = new GraphNodePort(Direction.Output, Port.Capacity.Single);
            if (export != null)
            {
                export.portName = "";
                export.style.flexDirection = FlexDirection.RowReverse;
                export.style.position = new StyleEnum<Position>(Position.Absolute);
                export.style.top = new StyleLength(-3f);
                export.style.left = new StyleLength(115f);
                extensionContainer.Add(export);
            }
        }

        public override void SetPosition (Rect newPos)
        {
            base.SetPosition(newPos);

            Vector2 position = new Vector2(newPos.xMin, newPos.yMin);
            serializer.SetNodePosition(node, position);
        }

        public override void OnSelected ()
        {
            base.OnSelected();
            if (OnNodeSelected != null)
            {
                OnNodeSelected.Invoke(this);
            }

            if (serializer.graph.selectedViewNode.Count == 1)
                HighlightReference();
            else
                viewer.UnhighlightAllReferenceNode();
        }

        public override void OnUnselected ()
        {
            base.OnUnselected();
            if (OnNodeUnselected != null)
            {
                OnNodeUnselected.Invoke(this);
            }

            if (node != null && node is TriggerBehaviourNode)
            {
                var referenceNode = this.node as TriggerBehaviourNode;
                if (!string.IsNullOrEmpty(referenceNode.triggerNodeId))
                {
                    //~~ NOTE unhighlight referenceNode
                    viewer.UnhighlightReferenceNode(referenceNode.triggerNodeId);
                }
            }
        }

        public List<GraphNode> SortChildren ()
        {
            if (node is RootNode or TriggerNode or BehaviourNode)
            {
                var gNode = (GraphNode) node;
                List<GraphNode> sorted = gNode.children.ToList();
                sorted.Sort(SortByHorizontalPosition);
                return sorted;
            }

            return null;
        }

        private int SortByHorizontalPosition (GraphNode left, GraphNode right)
        {
            var leftX = left == null ? float.MaxValue : left.position.x;
            var rightX = right == null ? float.MaxValue : right.position.x;
            return leftX < rightX ? -1 : 1;
        }

        public override void BuildContextualMenu (ContextualMenuPopulateEvent evt)
        {
            if (evt.target is GraphNodeView)
            {
                var nodeView = (GraphNodeView) evt.target;
                if (nodeView.node is RootNode == false)
                {
                    evt = viewer.GetDeleteAction(evt);
                    evt = viewer.GetDuplicateAction(evt);
                }
            }

            base.BuildContextualMenu(evt);
        }

        public void Update ()
        {
            if (node != null && node.dirty)
            {
                UpdateDescriptionLabel();
                node.dirty = false;
                serializer.SaveNode(node);

                viewer.UnhighlightAllReferenceNode();
                HighlightReference();

                if (node.forceRepaint)
                {
                    node.forceRepaint = false;
                    UpdateConnectLabel();
                    UpdateChildrenPortColor();
                }
            }

            UpdateState();
        }

        private void UpdateDescriptionLabel ()
        {
            descriptionLabel.text = node.GetNodeViewDescription();
            if (string.IsNullOrEmpty(descriptionLabel.text))
                AddToClassList(viewer.GetRedLabelStyle());
            else
                RemoveFromClassList(viewer.GetRedLabelStyle());
        }

        public void HighlightReference ()
        {
            if (node != null && node is TriggerBehaviourNode)
            {
                var referenceNode = this.node as TriggerBehaviourNode;
                if (!string.IsNullOrEmpty(referenceNode.triggerNodeId))
                {
                    //~~ NOTE highlight referenceNode
                    viewer.HighlightReferenceNode(referenceNode.triggerNodeId);
                }
            }
        }

        public void UnhighlightReference ()
        {
            if (node != null && node is TriggerBehaviourNode)
            {
                var referenceNode = this.node as TriggerBehaviourNode;
                if (!string.IsNullOrEmpty(referenceNode.triggerNodeId))
                {
                    //~~ NOTE highlight referenceNode
                    viewer.UnhighlightReferenceNode(referenceNode.triggerNodeId);
                }
            }
        }

        public void ApplyRunningHighlight ()
        {
            AddToClassList("running");
        }

        public void UnapplyRunningHighlight ()
        {
            RemoveFromClassList("running");
        }

        public void ResetInputPortColor ()
        {
            if (input == null)
                return;
            if (input.portColor != portDefaultColor)
            {
                input.portColor = portDefaultColor;
                input.MarkDirtyRepaint();
                input.highlight = false;
            }
        }

        public void UpdateInputPortColor (GraphNodeView parentView)
        {
            if (input == null || parentView == null)
                return;
            if (!parentView.node.IsPortReachable(node))
            {
                if (input.portColor != portUnreachableColor)
                {
                    input.portColor = portUnreachableColor;
                    input.MarkDirtyRepaint();
                    input.highlight = false;
                }
            }
        }

        public void UpdateInputPortColor ()
        {
            if (input == null)
                return;
            if (input.node != null && input.connected)
            {
                var connection = input.connections.First();
                if (connection.output != null)
                {
                    var viewNode = (GraphNodeView) connection.output.node;
                    UpdateInputPortColor(viewNode);
                }
            }
        }

        public void UpdateChildrenPortColor ()
        {
            if (output != null && output.node != null && output.connected)
            {
                foreach (var connection in output.connections)
                {
                    if (connection.input != null)
                    {
                        var viewNode = (GraphNodeView) connection.input.node;
                        viewNode.ResetInputPortColor();
                        viewNode.UpdateInputPortColor(this);
                        connection.MarkDirtyRepaint();
                    }
                }

                viewer.RefreshEdge(this);
            }
        }

        private void UpdateConnectLabel ()
        {
            if (node is VariableBehaviourNode)
            {
                var behave = (VariableBehaviourNode) node;
                connectLabel.text = "Behaviour";
                if (behave.AcceptConditionNode())
                    connectLabel.text += " / Condition";
            }
            else if (node is DialogBehaviourNode)
            {
                var behave = (DialogBehaviourNode) node;
                connectLabel.text = "Behaviour";
                if (behave.AcceptConditionNode())
                    connectLabel.text += " / Condition";
            }
        }

        public void UpdateState ()
        {
            /*RemoveFromClassList("running");
            RemoveFromClassList("failure");
            RemoveFromClassList("success");

            if (Application.isPlaying)
            {
                switch (node.state)
                {
                    case GraphNode.State.Running:
                        if (node.started)
                        {
                            AddToClassList("running");
                        }

                        break;
                    case GraphNode.State.Failure:
                        AddToClassList("failure");
                        break;
                    case GraphNode.State.Success:
                        AddToClassList("success");
                        break;
                }
            }*/
        }
    }
}