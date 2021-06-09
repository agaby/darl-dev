# Rule Hierarchies

ThinkBase enables you to add rule sets to nodes in the real tab, or nodes in the virtual tab.

This strategy both enables common sense reasoning, and simplifies dramatically the amount of rule writing required.

When processing a node during a seek or discover operation, ThinkBase will try to find data items on that node, or rule sets that generate the sought data item.

It looks first for data in the Knowledge State, then for data in the real tab, then for a rule to generate that data in the real tab, then a rule to generate that data in the virtual tab.

The nodes in the real tab all have lineages with matching nodes in the virtual tab. ThinkBase will look across from the real node to the corresponding virtual node by lineage.

It doesn't stop there. If a rule set is not found, ThinkBase will search up the lineage tree.

So for instance, if you have a customer object in the real tab with no rule set for the required value, the customer lineage object in the virtual tab will be searched, and then up the tree.
If  a "person lineage" node further up the tree contains a rule set of the right type that will be used.

This permits multiple layers of reasoning to be used.
 

