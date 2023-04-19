# Getting Started

1. [Setup Graph into GameObject](#setup-graph-into-gameobject)
2. [Editing Graph](#editing-graph)
3. [Graph Node](#graph-node)
4. [Demo Sample](#demo-sample)
 
## Setup Graph into GameObject 

Add Graph Runner component into any gameObject. <br/>
<img src="Images/StartAddGraphRunner.png" alt="Add Graph Runner Component"> <br/>

Select Type "Behaviour Graph" then click on Create Graph button. <br/>
<img src="Images/StartCreateGraph.png" alt="Create Graph Runner"> <br/>

If the scene not yet have Graph Manager, below warning message will prompted. <br/>
<img src="Images/StartCreateGraphWarning.png" alt="Create Graph Warning"> <br/>

Close the warning then right mouse click on Project windows to create Graph Manager. <br/>
<img src="Images/StartCreateGraphManager.png" alt="Create Graph Manager"> <br/>

Once added Graph Manager into scene, cilck on Create Graph button again at the Graph Runner component. <br/>
<img src="Images/StartCreateGraph.png" alt="Create Graph Runner"> <br/>

You will see "Edit Graph" button after success create graph. Click on "Edit Graph" to open Graph Editor.
<img src="Images/StartEditGraph.png" alt="Edit Graph"> <br/>

## Editing Graph 

Graph Editor is a window for editing the graph, everything about the graph is configure at here. <br/>
Graph always have a Start node and this node cannot be remove. <br/>
You could hold middle mouse button to move graph viewing position, scroll middle mouse wheel to zoom in/out graph viewing size. <br/>
<img src="Images/StartGraphEditor.png" alt="Graph Editor Intro"> <br/>

Right mouse click in the Graph Editor to add more nodes into the graph. <br/>
Trigger node and Behaviour node are basic type of nodes you always use. <br/>
<img src="Images/StartTriggerNode.png" alt="Graph Trigger Node"> <img src="Images/StartBehaviourNode.png" alt="Start Behaviour Node"> <br/>

Graph is design to execute node base on trigger driven waterfall method. <br/>
Start node can connect to multiple trigger nodes, Trigger node can connect to multiple behaviour nodes, Behaviour node can connect to multiple behaviour nodes. <br/>
If Trigger node get executed then all behaviour nodes under it get executed base on left to right, top to bottom order.
<img src="Images/StartGraphFlow.png" alt="Graph FLow Logic"> <br/>
Above graph will log message to Console window when runner get active at the first time. <br/>
Log message will appear base on this order : A, 1, 2, 4, B, 3
Log message 'C' will appear when a rigidbody enter into collider that exist at the same gameObject with graph runner.


## Graph Node

Graph Node is combine of 4 section : Input Port, Name, Description and Output Port <br/>
<img src="Images/StartNodeExplain.png" alt="Explain Graph Node"> <br/>
### Section 1 : Input Port
Serve as Input port, it only allow one node connecting to it. <br/>
It show what type of node that it allow connecting.
### Section 2 : Name
Display the node type name
### Section 3 : Description
Display the functionality of the node
### Section 4 : Output Port
Serve as Output port, it can connect to multiple nodes. <br/>
It show what type of node that it can connect to.

## Demo Sample

There are plenty of graph demo in the sample that you could import from ReGraph package. <br/>
Once imported samples, run LabMain scene to try out the demo, as well as check out how each graph have setup. <br/>
