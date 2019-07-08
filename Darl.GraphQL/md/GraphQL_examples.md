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

## Interact with a Bot
```json
query interact($model: String!, $convId: String!, $data: darlVarUpdate!)
{
  interact(botModelName: $model, conversationId: $convId, conversationData: $data)
  {
    response
    {
      value
      dataType
      categories
    }
  }
}
```

## Get lineages for a word
```json
{
  getLineagesForWord(word: "immigration")
  {
    description
    lineage
    typeWord
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

## Factory reset - Dangerous!
```json
mutation
{
  factoryReset
}
```

## Update the DARL code in a rulesset
```json
mutation
{
  updateRuleSetDarl(name: "military_service.rule", darl: "ruleset military_service {} ")
  {
    name
  }
}
```
## Update SendGrid credentials
```json
mutation
{
  updateSendgridCredentials(botModelName: "thousandquestions.model", 
    sendGridAPIKey: "SG.xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx")
  {
    sendGridAPIKey
  }
}
```

## Add a phrase to botmodel
```json
mutation
{
  createPhrase(botModelName: "thousandquestions.model",
    path: "verb:067,4,0|noun:01,0,2,00,00,15,20,19,1",
    attribute: {
      call: "military_service.rule"
    }
  )
  {
    darl
  }
}
```

## Update a ruleset trigger
```json
mutation
{
  updateRuleSetTrigger(ruleSetName: "military_service.rule", trigger: {
  	sendEmailSource: RESULTS,
    sendEmail: "trigger.true",
    addressSource: FIXED,
    addressText: "andy@darl.ai",
    emailFrom: "support@darl.ai",
    subjectSource: FIXED,
    subjectText: "Application on website" ,
    bodySource: RESULTS,
    bodyText: "emailText";
  })
  {
    sendEmail
  }
}
```