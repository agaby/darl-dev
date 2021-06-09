# Adding more inferences with sets

We show you how to elaborate on the 5 minute KG that calculates bmi by adding the ability to interpret what the bmi says about your health.

[![IMAGE ALT TEXT HERE](https://img.youtube.com/vi/nlKYS3uqZlk/0.jpg)](https://www.youtube.com/watch?v=nlKYS3uqZlk)

This illustrates the use of sets and rules to generate inferences.

[Fuzzy sets are explained here](fuzzy_sets.md). ThinkBase and DARL use a sequence of numbers in ascending order to define sets and fuzzy numbers. If you define two numbers you are specifying an interval.

BMI values are interpreted in ranges from underweight through healthy and overweight to obese.

This line

```darl
output numeric bmi {{underweight,0,18.5}, {healthy, 18.5,25}, {overweight, 25,30}, {obese, 30, 100}} appraisal;
```

specifies 4 sets with the names and ranges given.

These can be used in rules as shown below:

```darl
if bmi is underweight then annotation will be document("Your bmi is %% bmi %%. You are underweight.",{bmi});
if bmi is healthy then annotation will be document("Your bmi is %% bmi %%. You are healthy.",{bmi});
if bmi is overweight then annotation will be document("Your bmi is %% bmi %%. You are overweight.",{bmi});
if bmi is obese then annotation will be document("Your bmi is %% bmi %%. You are obese.",{bmi});
```



