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
    values
    {
      name
      value
    }
    questions
    {
      text
      categories
      reference
      qType
    }
    responses
    {
      rType
      mainText
      annotation
    }
  }
}
```

## Continue a questionnaire
```json
{
  continueQuestionnaire(responses:{
     ieToken: "<ieToken value returned by begin questionnaire>",
    questions:
    [
    {
      reference: "name",
      sResponse: "Yes",
      qType: textual
    }
  ]
  })
  {
    complete
    ieToken
    questionHeader
    values
    {
      name
      value
    }
    questions
    {
      text
      categories
      reference
      qType
    }
    responses
    {
      mainText
      annotation
      rType
    }
  }
}
```

## Go Back a step with a questionnaire
```json
query back($ieToken: String!)
{
  backtrackQuestionnaire(ieToken: $ieToken)
  {
    complete 
    ieToken 
    questionHeader 
    percentComplete 
    canUnwind
    values {
      name 
      value 
    } 
    questions 
    { 
      text 
      categories  
      reference 
      qType 
      sResponse  
      dResponse
    } 
    responses 
    { 
      mainText
      annotation 
      rType 
      preamble
    }
  }
}
```

## Interact with a Bot
```json
query interact($model: String!, $convId: String!, $data: darlVarInput!)
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
## Investigate the bot text recognition tree
```json
{
  getChildrenLineageNodes(botModelName: "thousandquestions.model", path: "", isRoot: true )
  {
    text
  }
}
```



## Use the DARL linter
```json
query lint($darl: String!)
{
  lintDarl(darl: $darl)
  {
    column_no_start
    column_no_stop
    line_no
    message
    severity
  }
}
```

## Getting an Alexa InteractionModel to build an Alexa skill
```json
alexaInteractionModel(name: "is_it_using_AI.rule" invocationName: "using AI")
  {
    languageModel
    {
      invocationName
      intents
      {
        name
        samples
        slots
        {
          name
          samples
          type
        }
      }
      types
      {
        name
      }
    }
  }
```
note: In the returned Json, rename _alexaInteractionModel_ to _interactionModel_ and remove the outer braces and _data:_ . You can then drop the Json into the Alexa Developer Console Json editor.
_

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
    bodyText: "emailText"
  })
  {
    sendEmail
  }
}
```

## Exclude a Ruleset output from the visible results
```json
mutation
{
 updateRuleSetOutputFormat(ruleSetName: "military_service.rule", outputName: "emailText", outputUpdate:
  {
    hide: true
  })
  {
    hide
  }
}
```

## Infer a ruleset using DARL code
```json
mutation ifd($code: String!, $inputs: [darlVarInput]!)
{
  inferFromDarl(code: $code, inputs: $inputs)
  {
    name
    value
    dataType
    unknown
    weight
  }
}

```

## Upgrade your account
You can upgrade your account to _corporate_ or _embedded_ in order to use more facilities.
Charges for these levels can be found [here](https://darl.ai/#pricing).
__Warning__, upgrading will incur charges, considerable charges in the case of _embedded_. 
```
mutation
{
  updateSubscriptionType(type: corporate)
}
```

## Close your account
This process will generate a bill for unpaid charges to date and close your account immediately.
Models will not be immediately destroyed, but will only be accessible through agreement with ourselves, so please ensure you download all models before closing your account.

```
mutation
{
  closeAccount
}
```