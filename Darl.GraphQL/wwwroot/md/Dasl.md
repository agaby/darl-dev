---
title: Doctor Andy's Simulation Language (DASL)
description: Simulating Systems over time with DARL.
output:
  html_document:
    toc: true
    toc_float: true
---


Doctor Andy's Simulation Language (DASL)
===

Darl can be used to simulate knowledge in the form of a transform between a set of inputs and a set of outputs.
These inputs and outputs represent some moment in time, some state pertaining in a real or imagined world within prescribed time limits.
Clearly Darl could be much more useful if time could be added to this processing, where a sequence of such states could be presented to Darl, and where Darl could be driven by time series and create simulated time series in turn.

Adding the dimension of time can complicate a system enormously. 
Happily, Darl can be extended to become a time based simulation engine very easily. These extensions constitute DASL.

We need only one addition to the Darl language to handle the dimension of time: a delay element.

The Delay element exists at the top level of a Darl program, defined along with the Wire elements that connect rule sets together and to the connectors of the code block.

```darl
delay map1 logisticMap.in1 {1};
```

The above defines a delay element taking it's input from a program level output(the first parameter), and passing it's delayed value to a ruleset input (the second parameter). The delay characteristics can be determined by the third parameter.
This third parameter is a fuzzy number, so can consist of 1,4 ascending values. Its units are those of the clock used to time the simulation, so only integer values make sense here.

The concept of a fuzzy delay is quite esoteric, but matches real world situations very well.

Suppose you wished to simulate the opening of a new company. The process of registering a company with the national registration body might take 1-2 days. Opening a bank account might take 2-3 weeks.
If we choose the simulation clock to be one day, then the first is
```darl
delay namechosen company.registered {1,2};
```

and the second might be

```darl
delay bankapplication company.bankaccount {7,14};
```

In other circumstances triangular or trapezoidal fuzzy numbers may be useful.

As well as the extensions to the Darl language we need to extend the functionality of the Darl interpreter to handle an extra dimension of data.

The following is a complete Darl program simulating the [Logistic Map](https://en.wikipedia.org/wiki/Logistic_map) equation, that creates a chaotic sequence of numbers.

```darl
ruleset logisticMap
{
    input numeric in1;
    output numeric out1;
    if anything then out1 will be 4 * in1 * ( 1 - in1);
}
mapoutput map1;
mapinput in1;
wire logisticMap.out1 map1;
wire in1 logisticMap.in1;
delay map1 logisticMap.in1 {0};
```

The form of the logistic map equation is simply X(n+1) = rX(n)(1-X(n))

Selecting r as 4 guarantees chaos.

The following diagram shows how the delay is wired:
![logistic map diagram](/images/Logisticmap_diagram.jpg)

## Handling temporal data
Temporal data comes in two forms, events associated with one or more data values, and regularly sampled time series.
Of the two, an event representation is the most general, since a sampled time series can be represented as a sequence of events.

We are interested here in multivariate data. In the real world data that might be used is sampled at a range of frequencies, or occurs as spontaneous events.
We would like, however, our simulation to have a regular sample period so that calculations using delays can be generated cleanly and graphed easily.

The solution is a data structure, [DaslState](./DaslState), and the addition of a Temporal database to Dasl.

DaslState is a data structure that contains a time stamp and a list of [DarlVar](./DarlVar) data structures, each of which contains a single named value.

A DaslState data structure represents an event, holding a time tag and containing the named data values that changed at the moment represented.

The temporal database is a way of collecting a set of such events, ordering them in time, and then constructing a regularly sampled series representing the state of each required value at the sample time.

## Initialising a simulation

The logistic map equation above defines how to create the next value from the previous one, but does not define the initial conditions.
I.e. what is the first value of X?

In the case above we need only a single value to start the sequence, but in the general case, where long delays may be present it may take many sample steps to initialize a model.

Furthermore a typical use case of a Dasl rule set  would be to fine tune it against existing data before using it to create predictive simulations.

For instance, given a model of factory processes and the progress of products through manufacturing, the user would be interested in first discovering whether the simulated model matched the real flow using real data, before using the simulation to work out what would happen if, say, a particular machine failed.

This leads us to define the following conventions that you should adhere to in creating data as a list of DaslState objects.

+ You should supply an array of DaslState objects for input to the simulation
+ You should also supply a sample time interval for the simulation
+ The array of DaslState objects can be in any order.
+ There should be at least two DaslState objects in the array.
+ DaslState objects may have an empty list of DarlVar data values.
+ DaslState objects need only contain DarlVar data values that have changed at the given time stamp point in time.
+ The earliest of the DaslState objects in the array, as defined by their time stamp, will be used as the start of the simulation.
+ The simulation proceeds at sample time intervals from this earliest time until simulation time is equal to or greater than the latest DaslState value as defined by its time stamp.
+ At each sample created by the temporal database the value of an input is set by the last event occurring at or before that time, but not occurring in the previous sample.
+ if an input is served by a delay, but there is no value in the DaslState array supplying that input value the delay value is used. If there is a value the delay is ignored.

The last feature is the most powerful. It means you can supply a sequence of data values for the simulation, and when they stop, and they don't all need to stop at the same time, then the simulation takes over.
You could thus use real time data with a simulation where the latest DaslState (with an empty DarlVar array) is set to some time in the future, thus forcing the simulation to generate true predictions.

The only drawback to using events rather than simple arrays of data is that you need to create dummy time tags for any series, like the Logistic map example above, where no real world time is required. This is a small price to pay for the power of this representation form.

As a note, it would be quite possible to arrange Dasl so that it was event driven - i.e. the simulation could be run for every event in the DaslState array, rather than at clocked intervals. This and other additions like trading times are in the pipeline.



