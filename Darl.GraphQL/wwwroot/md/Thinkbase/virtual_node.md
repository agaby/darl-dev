Thinkbase Virtual Node
====

Virtual nodes are the metadata of our knowledge graph system. Rather than copy the entire ontology into each knowledge graph we add only those ontology nodes that are referenced, and all those above in the hierarchy.
As new nodes are added, their lineages are used to construct a subset of the ontology. Information as to what connections are permitted between pairs of nodes, and which attributes are associated with each node is also collected.
This information is used for common-sense reasoning.
Rules added as attributes to virtual nodes will be applied to all real nodes with lineages at or below this nodes lineage in the lineage trees, unless overridden by another attribute of the same type.

# Virtual node properties

## Name

This is a string that is displayed to the user identifying the node. (Read only)

## Lineage

The ontology address of this node. (Read only)


## Properties

This is a collection of Attributes that contain more data about the node. This can include, rules, descriptions, etc.