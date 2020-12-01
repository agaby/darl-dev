Thinkbase Real Node
====

Real nodes represent objects in the real (or simulated) world.
They represent things and are tied into an ontology of things, as defined by the DARL ontology based on WordNet.

Nodes can be linked to other nodes via connections, and can have attributes. 
Typically it is the attributes that differentiate one node from another.

# Real node properties

## Name

This is a string that is displayed to the user identifying the node.]

## Lineage

This defines the 'kind' of the node, and the various common sense reasoning processes depend on this being accurate.
The lineage is the concatenated address from the type root in the DARL ontology. Nodes normally correspond  to nouns.

## ExternalId

This is a user defined string that is used to associate this node with an external data system. Typically a Guid, but sometimes a unique name.

## Existence

This is from 0 to 4 dateTime values that define the period of existence of this object. This value is used in temporal reasoning about the object.
A single value implies that the object is an event, two an  interval, meaning the object has a defined existence between two times, three a fuzzy event, and four a fuzzy interval.
Use the minimum and maximum times to represent an undefined start or end, especially for people who are currently alive. 
An object with existence from the minimum time to the maximum time the system supports is considered immortal.

## Properties

This is a collection of Attributes that contain more data about the node. This can include, rules, descriptions, etc.