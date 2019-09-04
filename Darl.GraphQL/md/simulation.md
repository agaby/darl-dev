Simulating a system using DASL
======
DASL is a variant of DARL that permits simulation of a complex system over time. The behaviour of the system is determined by a DARL ruleset that contains DASL extensions. Such a ruleset must be processed using the _simulate_ query.
# Delays
The only addition DARL needs to perform simulation in time is the addition of a _delay_ element.

# Simple example

The following DASL ruleset implements the simplest known generator of a chaotic time series, the _logistic map_.
```darl
ruleset logisticMap
{
	input numeric in1 ;
	output numeric out1;
	if anything then out1 will be 4 * in1 * ( 1 - in1);
}
mapoutput map1;
mapinput in1;
wire logisticMap.out1 map1;
wire in1 logisticMap.in1;
delay map1 logisticMap.in1 {0};
```

The ruleset _logisticMap_ creates a new value, _out1_, based on the value of _in1_. The wiring wires t6hese values to the edge of the ruleset, and also pipes the result back to _in1_ via the delay block.
The effect is like this:

![logistic map block diagram](/images/logisticmap_diagram.jpg)

To run a simulation through the Darl.dev site use the following GraphQL:
```json
query sim($sim: daslSetInput!)
{
  simulate(name: "logisticmap.rule" dataSet: $sim sampleType: sampled)
  {
    events
    {
      timeStamp
      values
      {
        name
        value
      }
    }
  }
}
```

The query variable $sim contains the data for the simulation. In order to seed the simulation the input _in1_ has to be set to an initial value, in this case 0.3, and thereafter the value is fed back via the delay.

Time values are arbitrary for this example, so we'll set the start of the simulation at 1/1/1990, set the clock for 10 minutes, and run the process for one day.

To do this we need to pass in the following in the variable _$sim_.

```json
{
  "sim": 
  {
    "sampleTime": 600,
    "events": 
    [
      {
        "timeStamp": "1991-01-01T00:00:00.0",
        "values": 
        [
          {
            "value": 0.3,
            "name": "in1",
            "dataType": "numeric"
          }
        ]
      },
      {
        "timeStamp": "1991-01-01T00:05:00.0",
        "values": []
      },
      {
        "timeStamp": "1991-01-02T00:00:00.0",
        "values": []
      }
    ]
  }
}
```

GraphQL interprets _sampleTime_ as a float value containing seconds. The events array contains 3 events. The first is the start of the simulation that sets the input _in1_ to 0.3, the seed value. The second event sets all input values to _unknown_ 5 minutes later. The last sets the end of the simulation a day after the start.

The temporal database inside the simulation engine loads events and sorts them and then creates samples from the earliest to the latest events in the simulation at _sampleTime_ intervals.

The simulation engine uses supplied data where available, and thereafter replaces it with simulated data if available. 

In this case a value of 0.3 is available for the first sample, but not for the next, so the calculated value coming out of the _delay_ element is substituted.

The result is a seemingly random sequence of numbers between 0 and 1.

```json
{
  "data": {
    "simulate": {
      "events": [
        {
          "timeStamp": "1991-01-01",
          "values": [
            {
              "name": "in1",
              "value": "0.3"
            },
            {
              "name": "logisticMap.in1",
              "value": "0.3"
            },
            {
              "name": "logisticMap.out1",
              "value": "0.84"
            },
            {
              "name": "map1",
              "value": "0.84"
            }
          ]
        },
        {
          "timeStamp": "1991-01-01",
          "values": [
            {
              "name": "logisticMap.out1",
              "value": "0.5376000000000002"
            },
            {
              "name": "map1",
              "value": "0.5376000000000002"
            },
            {
              "name": "logisticMap.in1",
              "value": "0.84"
            }
          ]
        },
        {
          "timeStamp": "1991-01-01",
          "values": [
            {
              "name": "logisticMap.out1",
              "value": "0.9943449599999998"
            },
            {
              "name": "map1",
              "value": "0.9943449599999998"
            },
            {
              "name": "logisticMap.in1",
              "value": "0.5376000000000001"
            }
          ]
        },
```


