using UnityEditor;
using UnityEngine.UIElements;

namespace Reshape.ReGraph
{
    public class GraphDoubleClickSelection : MouseManipulator
    {
        double time;
        double doubleClickDuration = 0.3;

        public GraphDoubleClickSelection ()
        {
            time = EditorApplication.timeSinceStartup;
        }

        protected override void RegisterCallbacksOnTarget ()
        {
            target.RegisterCallback<MouseDownEvent>(OnMouseDown);
        }

        protected override void UnregisterCallbacksFromTarget ()
        {
            target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
        }

        private void OnMouseDown (MouseDownEvent evt)
        {
            var graphView = target as GraphViewer;
            if (graphView == null)
                return;

            double duration = EditorApplication.timeSinceStartup - time;
            if (duration < doubleClickDuration)
            {
                SelectChildren(evt);
            }

            time = EditorApplication.timeSinceStartup;
        }

        void SelectChildren (MouseDownEvent evt)
        {
            var graphView = target as GraphViewer;
            if (graphView == null)
                return;

            if (!CanStopManipulation(evt))
                return;

            GraphNodeView clickedElement = evt.target as GraphNodeView;
            if (clickedElement == null)
            {
                var ve = evt.target as VisualElement;
                clickedElement = ve.GetFirstAncestorOfType<GraphNodeView>();
                if (clickedElement == null)
                    return;
            }

            // Add children to selection so the root element can be moved
            Graph.Traverse(clickedElement.node, node =>
            {
                var view = graphView.FindNodeView(node);
                if (view != null)
                    graphView.AddToSelection(view);
            });
        }
    }
}