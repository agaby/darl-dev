The DARL GraphQL API
=====

Our GraphQL API is intended to give you full control over the objects in your account.
We chose GraphQL because it avoids a great deal of programming and is completely generic: if you know how to access FaceBook or GitHub, you know how to access the darl.dev API.

GraphQL is self-documenting and some excellent tooling exists to enable you to use it.

You can access our GraphQL API through our site [darl.dev](https://darl.dev).

The actual endpoint is 
```
https://darl.dev/graphql
```

The main page uses the Playground access tool. Voyager and GraphiQL are also supported on other pages.

Whatever language and platform you use will have support, normally in the form of a library, for GraphQL. 

The general form for accessing the GraphQL API from your own code will be the following.

+ Develop and test a query or mutation using GraphiQL or Playground, and parameterize it. 

+ Using a GraphQL client load the query or mutation and pass the parameters for your particular call. 

+ Call the API and catch the response

+ Convert the response to objects usable within your language or just parse the Json.

The GraphQL query language is very simple to learn, and is a kind of simplified json. Read more here [GraphQL.org](https://graphql.org/).

## Javascript example code

An example of this process for javascript using the [graphql.js](https://github.com/f/graphql.js)
client library is shown here.

```
    var graph = graphql("https://darl.dev/graphql");
    var allrulesets = graph(`{ rulesets { name }}`);
    const rs = await allrulesets();
```
in this example _rs_ contains a list of the ruleset names in your account.

Another example using parameters is:

```
    var graph = graphql("https://darl.dev/graphql");
    var darlContent = graph(`query getdarl($name: String!){  getDarlFromRuleSet(ruleSetName: $name)}`);
    var content = await darlContent({ name: rsname });
```
Where the GraphQL fetches the DARL content of a ruleset, the name of which is determined by the parameter _$name_
which is provided when _darlContent_ is called.

Finally an example of mutation writes the darl code back. Here there are two parameters: the filename and the darl content.

```
    var graph = graphql("https://darl.dev/graphql");
    var savecontent = graph('mutation setdarl($name: String!, $darl: String!){updateDarlInRuleset(ruleSetName: $name, darl: $darl)}');
    await savecontent({ name: rsname, darl: content });
```

## C# example code 

A similar example can be seen using the .net [graphql-client](https://github.com/graphql-dotnet/graphql-client) library.
With this library you specify the operation name, variables in an anonymous class, and the graphQL text. This code fetches the name and author of a particular machine learning model.

```
    GraphQLClient client = new GraphQLClient("https://darl.dev/graphql/");
    var req = new GraphQLRequest() { OperationName = "getModelByName", Variables = new { name = models[0].Name }, Query = @"query getModelByName($name: String!){  mlmodelByName(name: $name){ name mlmodel{ author}}}" };
    var resp = await client.PostAsync(req);
```






