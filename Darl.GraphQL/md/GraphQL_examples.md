GraphQL Examples
=====

The GraphQL interface permits you to create, maintain and edit your models programmatically. The following are pieces of example GraphQL that you can copy to help you perform various tasks.

# Queries

## Viewing a list of rulesets
```json
{
  rulesets
  {
    name
  }
}
```

## Viewing a list of Machine learning models
```json
{
  mlmodels
  {
    name
  }
}
```
## Finding your API key
```json
{
  getApiKey
}
```
## listing collateral
```json
{
 collateral
  {
    name
    value
  }
}
```

## Seeing the language elements set in a ruleset

```json
{
  rulesetByName(name: "military_service.rule")
  {
    ruleform
    {
      language
      {
        languageList
        {
          name
          text
        }
      }
    }
  }
}
```

## See a list of preloaded data items in a rule set
```json
{
  rulesetByName(name: "military_service.rule")
  {
    ruleform{
      preload{
        name
        dataType
        value
      }
    }
  }
}
```
## Start a questionnaire
```json
{
  beginQuestionnaire(ruleSetName: "military_service.rule")
  {
    ieToken
    questionHeader
    questions
    {
      text
      categories
      reference
      questionType
    }
  }
}
```

## Continue a questionnaire
```json
{
  continueQuestionnaire(responses:{
     ieToken: "40a8a8e9-c24b-4711-bf66-0cb038746216",
    questions:
    [
    {
      reference: "military",
      sResponse: "Yes"
    }
  ]
  })
  {
       ieToken
    questionHeader
    questions
    {
      text
      categories
      reference
      questionType
    }
  }
}
```

# Mutations

## Setting the text for a ruleset question

``` json
mutation{
  updateRuleSetLanguageText(ruleSetName: "military_service.rule", 
    languageName: "military", languageText: "Did you serve in the US military?")
  {
    name
    text
  }
}
```

## Setting a preloaded data value for a rule set
```json
mutation{
  updateRulesetPreload(rulesetName: "military_service.rule", preloadData: 
    { name: "comply_text", dataType:  textual, value: "Thanks, that looks great. Expect an email in the near future."
  })
  {
    name
    value
  }
}
```
## Run a machine learning model
```json
mutation
{
  machineLearnModel(mlmodelname: "iris.mlmodel")
  {
    results{
      trainPerformance
      code
    }
  }
}
```