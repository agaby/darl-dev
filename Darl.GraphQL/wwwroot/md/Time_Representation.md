Time representation in ThinkBase
====
There are several use cases for knowledge graphs that require large time ranges. For instance, reasoning about ancient history. 
Standard time representations are inadequate for this purpose, since they usually restrict themselves to AD dates.

ThinkBase uses 64 bit doubles to represent time. Zero in this scale represents 0000-01-01 in ISO format and each unit is a second. In this format dates in the present day are around the value 6.3 * 10^10.

Negative values are therefore BC dates.

Resolution will vary over the numeric range, but there is adequate resolution to be able to represent down to the second for all of recorded history. 

This may sound overkill, but there are some astronomical events in the ancient world calculable to this kind of accuracy. 

User Interface components used in various places may not be able to represent BC dates, however.
