using Darl.GraphQL.Models.Models;
using GraphQL.Client;
using GraphQL.Common.Request;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Test
{
    [TestClass]
    public class MLModelTest
    {
        GraphQLClient client = null;


        [TestInitialize()]
        public void Initialize()
        {
            client = new GraphQLClient("https://darl.dev/graphql/");
            client.Options.JsonSerializerSettings.Converters.Add(new StringEnumConverter());
        }


        [TestCleanup()]
        public void Cleanup()
        {

        }

        [TestMethod]
        public async Task TestGetMLModel()
        {
            var req = new GraphQLRequest() { Query = @"{ mlmodels{name } }" };
            var resp = await client.PostAsync(req);
            Assert.IsTrue(resp.Errors == null || resp.Errors.Length == 0);
            var models = resp.GetDataFieldAs<List<MLModel>>("mlmodels");
            Assert.AreEqual(4, models.Count);
            req = new GraphQLRequest() { OperationName = "getModelByName", Variables = new { name = models[0].Name }, Query = @"query getModelByName($name: String!){  mlmodelByName(name: $name){ name mlmodel{ author}}}" };
            resp = await client.PostAsync(req);
            Assert.IsTrue(resp.Errors == null || resp.Errors.Length == 0);
            var model = resp.GetDataFieldAs<MLModel>("mlmodelByName");
            Assert.AreEqual(models[0].Name, model.Name);

        }

        [TestMethod]
        public async Task TestCreateDeleteModel()
        {
            var modelName = "newmodel.mlmodel";
            var req = new GraphQLRequest() { OperationName = "createEmptyMLModel", Variables = new { name = modelName }, Query = @"mutation createEmptyMLModel($name: String!){  createEmptyMLModel(name: $name){ name mlmodel{ author}}}" };
            var resp = await client.PostAsync(req);
            Assert.IsTrue(resp.Errors == null || resp.Errors.Length == 0);
            req = new GraphQLRequest() { OperationName = "getModelByName", Variables = new { name = modelName }, Query = @"query getModelByName($name: String!){  mlmodelByName(name: $name){ name mlmodel{ author}}}" };
            resp = await client.PostAsync(req);
            Assert.IsTrue(resp.Errors == null || resp.Errors.Length == 0);
            var model = resp.GetDataFieldAs<MLModel>("mlmodelByName");
            Assert.AreEqual(modelName, model.Name);
            req = new GraphQLRequest() { OperationName = "deleteMLModel", Variables = new { name = modelName }, Query = @"mutation deleteMLModel($name: String!){  deleteMLModel(name: $name){ name }}" };
            resp = await client.PostAsync(req);
            Assert.IsTrue(resp.Errors == null || resp.Errors.Length == 0);
            req = new GraphQLRequest() { OperationName = "getModelByName", Variables = new { name = modelName }, Query = @"query getModelByName($name: String!){  mlmodelByName(name: $name){ name }}" };
            resp = await client.PostAsync(req);
            Assert.IsTrue(resp.Errors == null || resp.Errors.Length == 0);
            model = resp.GetDataFieldAs<MLModel>("mlmodelByName");
            Assert.IsNull(model);

        }

        [TestMethod]
        public async Task TestRunModel()
        {
            var req = new GraphQLRequest() { OperationName = "", Variables = null, Query = "mutation{machineLearnModel(mlmodelname: \"yingyang.mlmodel\"){ mlmodel { darl } results { trainPerformance }}}" };
            var resp = await client.PostAsync(req);
            Assert.IsTrue(resp.Errors == null || resp.Errors.Length == 0);
        }


    }
}
