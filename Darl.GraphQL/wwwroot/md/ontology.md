# Ontologies and Lineages

It's universally accepted that knowledge graphs need ontologies.

They serve two vital purposes:

- You can use them to merge data from many different sources.
- You can use them to do common-sense reasoning.

An example of the first is that one data source might have user records, another might have contact records, another customer records. All these are examples of people, so a knowledge graph would need to merge them into a single type.

An example of the second is controlling the structure of the knowledge graph. For instance you might have an attribute for the number of wheels an object has. Adding this attribute to a vehicle object makes sense, but not to a person.

An ontology is a collection of all, or most of, the kinds objects, attributes and relationships in the world.

Normally ontologies are arranged hierarchically, with "is a kind of", otherwise known as hypernymy, relationships.

ThinkBase uses an annotated, expanded and expandable version of [Princeton's WordNet](https://wordnet.princeton.edu/) as our base ontology.

Collectively we will call all the nodes in these hierarchies _concepts_ although WordNet calls them "cognitive synonyms".

Our ontology also contains NLP information that enables us to map words to concepts, and goves a part of speech for each concept.


## Lineages

Our ontology is so arranged using the "is a kind of" relationship that it forms a series of trees or Directed Acyclic Graphs.
This means that we can use something like a directory structure to address any point in the ontology.
A lineage is the address of a concept in the hierarchy. It uniquely locates a concept.

Examples:

```
noun:01,5,03,3,018
verb:363,1,0
adjective:5769
```

Each concept has a type-word associated with it. Type-words are not unique, but, in the average knowledge graph, clashes are unlikely, so in our user interfaces we tend to replace lineages with type-words to make the function more legible.

## Determining inheritance

For the purposes of common sense reasoning, it is generally true that rules that apply to a concept also apply to all concepts that are a kind of that concept.

For instance, a horse can be ridden, so, since a pony is a kind of horse, a pony can be ridden to.

It is therefore frequently useful to walk up  the lineage hierarchy to find any rules that apply to an object at a higher level.

Something that makes that much simpler is the fact that, if we have two lineages, A and B, and the text of A starts with the text of B, then A is a child of B.



## Composite lineages

Despite there being some 40,000 concepts in our ontology, there are often circumstances where the right lineage can't be found.

Objects in ThinkBase can have two lineages, a major and a minor. A composite lineage is constructed like this:

```
<major lineage>_<minor lineage>
```

The major lineage is used to determine inheritance.





