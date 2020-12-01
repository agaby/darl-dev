Knowledge graphs (Beta)
========

Dr Andy created one of the first integrated knowledge graph software products, called Thinkbase, in 1998. Unfortunately it was way before it's time...

Now the rest of technology has caught up we can start again.

A knowledge graph is a combination of a graph database and our expert system software that weaponizes it into a full scale reasoning system.

A conventional graph database permits objects, attributes and relationships to be of different types, with a name to differentiate them. Our new ThinkBase knowledge graph imposes an ontology so that we can reason about those objects, and create common sense constraints on the relationships and attributes appropriate to them.

The ontology is based on our version of Princeton's WordNet, which is used in our ChatBot framework and other places.

This ontology is extensible. 

If you locate objects in a graph database against  a general ontology like WordNet you can reason about the objects in the database, and constrain new entries. 

This relies on not only the ontology, but a set of known permitted associations between elements of the ontology.

We have mined large scale text corpora to generate such associations, and the process is ongoing.

In our implementation, each user has partitioned space in the ThinkBase knowledge graph, but can take advantage of the generic knowledge in the graph, which is continually updated through our on-line research tool, _the dreamer_.

For now, the following GraphQL queries and mutations are defined to access your knowledge graph and the generic knowledge.

|Name | parameters | function|
|-----|-----|-----|
|getGraphObjects|name,lineage|get graph objects based on name and lineage|
|getGraphObjectsFuzzy|name,lineage,similarity|get graph objects based on  a fuzzy match with name and lineage|
|getGraphObjectByid|id|get a graph object based on id|
|createGraphObject|graphObject,definitive|Add a new graph object|
|createGraphConnection|graphConnection,definitive|Add a new graph connection|
|deleteGraphObject|id|delete a graphObject|
|deleteGraphConnection|id|delete a graph connection|
|updateGraphObject|graphObject,definitive|Update a graph object|
|updateGraphConnection|graphConnection,definitive|Update a graph connection|

More information about each parameter can be found in the schema for the GraphQL front end via GraphiQL, etc.

ids are guids used to uniquely identify each object. To add a connection you must first obtain the ids of the objects at each end.

The definitive flag enables you to load an object with attributes that are not considered compliant based on the knowledge already built into Thinkbase of what attributes go with what objects, or, in the case of connections, enables you to add a connection that is not associated by Thinkbase with the objects you have linked it to.

Only use the definitive flag if you are confident the relationships make sense. In order for Thinkbase to remain coherent each object must be marked with the appropriate lineage. 

Lineage lookups can be done using _getLineageForWord_.  Although this is an overhead for data entry, most tasks will contain a limited set of different objects, so assigning lineages need not be onerous.

See [Knowledge graphs and Chatbots — An analytical approach](https://chatbotslife.com/knowledge-graphs-and-chatbots-an-analytical-approach-a8dcc8649100) for an overview of the process.

To make use of the knowledge graph data you have loaded in a ruleset or chatbot, use the __Value__ and __Graph__ stores. See [Stores](stores.md).

## Corporate users only
Knowledge graphs require far more resources than other DARL offerings, so we are restricting use of ThinkBase to users who sign up to corporate usage.

To upgrade your account to _corporate_ use the following mutation:

```
mutation
{
  updateSubscriptionType(type: corporate)
}
```

Pricing for corporate usage can be found [here](//https://darl.ai/#pricing).
