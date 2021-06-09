# Graphical Editing

The Real, Virtual and Recognition tabs all allow graphical editing.

The kinds of edits allowed for each tab vary.

| Real | Virtual | Recognition |
|---|---|---|
|Add/delete nodes| |Ad/delete nodes|
|Add/delete connections| | Add/delete connections|
|Add/delete any Attributes| Add/delete any attributes| Add/delete rule and text attributes|

The nodes and connections in the virtual world are added as you add new nodes to the real world, if the meta data does not already exist.

# Adding nodes

## In the Real tab

Click on any space in the tab to create a new node.

![new node](images/newNode.png)

## In the Recognition tab

Click on any space in the tab to create a new node.

![Recognition node add](images/recognitionAdd.png)

# Deleting nodes

Right button click on the node and select the "trash" symbol.
You can't delete nodes in the real tab.

# Adding connections

## In the Real tab

Connections are drawn graphically by dragging a red connection line from the source to the destination, starting from the red "button at the top of the source node that becomes visible on hovering over the node.

![adding a connection](images/conectionAdd.png)

You will be asked to supply details on the connection you have just created.

![adding a connection](images/conectionAdd2.png)

## In the recognition node

As above, but the type of the connection is set in recognition nodes, so no dialog is shown.

# Deleting connections

Right button click on the connection and select the "trash" symbol.
You can't delete connections in the real tab.

# Editing attributes

## In the real tab

![Edit nodes](images/nodEdit.png)

Right button clicking on a highlighted node will bring up this control.

From 1.00 clockwise, clicking on a segment will:

- View lineage, 
- Edit attributes
- View name
- View External ID
- Edit lifetime
- View help on nodes
- Delete this node

Attributes are data items associated with the node. They have a data type, a lineage, a value and a certainty value.

![Add Attribute](images/attributeAdd.png)

Attribute types are:

![Add Attribute](images/attributeAdd2.png)

Attributes have lineages too.

## in the Recognition tab

Right button click on a node to bring up the selection graphic:

![Recognition node add](images/recognitionAdd2.png)

Recognition nodes can only have two attributes, a text attribute that defines the response when a node is triggered, and a rule attribute that determines the functionality that fires at that time.

## General

If you highlight a node in the real tab, the node in the virtual tab that is the direct parent of that node is highlighted.

## Real tab buttons

![Real tab buttons](images/real_tab_buttons.png)

From left to right these are:

### Fit to screen

Fits the graph to the screen.

### search

Searches for a node by external ID

### Information

Gives information on that tab.

### Time

Sets the processing time for any time dependent evaluations.

### Settings

Allows you to set the time display mode to recent or historic. The latter uses AD/BC dates.
Sets whether the name or the externalId are used to label the nodes in the graph.

### Description

Enables you to add a description shown whenever the graph is loaded. 

## Virtual tab buttons

![Virtual tab buttons](images/virtual_tab_buttons.png)

From left to right these are:

### Fit to screen

Fits the graph to the screen.

### Settings

Sets whether the name or the lineage are used to label the nodes in the graph.

## Recognition tab buttons

![Recognition tab buttons](images/recognition_tab_buttons.png)

From left to right these are:

### Fit to screen

Fits the graph to the screen.

### Add root

Basic recognition nodes are added by default. Should you delete one you can recreate it here.

### Settings

Sets whether the name or the lineage are used to label the nodes in the graph.


