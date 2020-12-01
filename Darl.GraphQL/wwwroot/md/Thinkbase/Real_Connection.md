Thinkbase Real Connection
====

Real connections represent real relationships between nodes. A connection connects two nodes and is directed. This means there is an implied order to the nodes. 

# Real connection properties

## Name

This is a string that is displayed to the user identifying the connection.

## Lineage

This defines the 'kind' of the connection, and the various common sense reasoning processes depend on this being accurate.
The lineage is the concatenated address from the type root in the DARL ontology. Connections often correspond  to verbs.


## Existence

This is from 0 to 4 dateTime values that define the period of existence of this relationship. This value is used in temporal reasoning about the relationship.
A single value implies that the Connection was momentary, two an  interval, meaning the relationship has a defined existence between two times, three a fuzzy momentary connection, and four a fuzzy interval.
Use the minimum and maximum times to represent an undefined start or end, especially for people who are currently alive. 
A connection  with existence from the minimum time to the maximum time the system supports is considered permanent.

## Properties

This is a collection of Attributes that contain more data about the connection. This can include, rules, descriptions, etc.