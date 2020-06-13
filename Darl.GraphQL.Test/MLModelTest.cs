using Darl.GraphQL.Models.Models;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Darl.GraphQL.Test
{
    [TestClass]
    public class MLModelTest
    {
        GraphQLHttpClient client = null;


        [TestInitialize()]
        public void Initialize()
        {
            client = new GraphQLHttpClient("https://darl.dev/graphql/", new NewtonsoftJsonSerializer());
        }


        [TestCleanup()]
        public void Cleanup()
        {

        }

        [TestMethod]
        public async Task TestGetMLModel()
        {
            var req = new GraphQLHttpRequest() { Query = @"{ mlmodels{name } }" };
//            var resp = await client.PostAsync(req);
//            Assert.IsTrue(resp.Errors == null || resp.Errors.Length == 0);
//            var models = resp.GetDataFieldAs<List<MLModel>>("mlmodels");
//            Assert.AreEqual(4, models.Count);
//            req = new GraphQLHttpRequest() { OperationName = "getModelByName", Variables = new { name = models[0].Name }, Query = @"query getModelByName($name: String!){  mlmodelByName(name: $name){ name mlmodel{ author}}}" };
//            resp = await client.PostAsync(req);
//            Assert.IsTrue(resp.Errors == null || resp.Errors.Length == 0);
//            var model = resp.GetDataFieldAs<MLModel>("mlmodelByName");
//            Assert.AreEqual(models[0].Name, model.Name);

        }

        [TestMethod]
        public async Task TestCreateDeleteModel()
        {
            var modelName = "newmodel.mlmodel";
            var req = new GraphQLHttpRequest() { OperationName = "createEmptyMLModel", Variables = new { name = modelName }, Query = @"mutation createEmptyMLModel($name: String!){  createEmptyMLModel(name: $name){ name mlmodel{ author}}}" };
 //           var resp = await client.PostAsync(req);
 //           Assert.IsTrue(resp.Errors == null || resp.Errors.Length == 0);
            req = new GraphQLHttpRequest() { OperationName = "getModelByName", Variables = new { name = modelName }, Query = @"query getModelByName($name: String!){  mlmodelByName(name: $name){ name mlmodel{ author}}}" };
//            resp = await client.PostAsync(req);
//            Assert.IsTrue(resp.Errors == null || resp.Errors.Length == 0);
//            var model = resp.GetDataFieldAs<MLModel>("mlmodelByName");
//            Assert.AreEqual(modelName, model.Name);
            req = new GraphQLHttpRequest() { OperationName = "deleteMLModel", Variables = new { name = modelName }, Query = @"mutation deleteMLModel($name: String!){  deleteMLModel(name: $name){ name }}" };
//            resp = await client.PostAsync(req);
//            Assert.IsTrue(GraphQLHttpRequest() { OperationName = "getModelByName", Variables = new { name = modelName }, Query = @"query getModelByName($name: String!){  mlmodelByName(name: $name){ name }}" };
//            resp = await client.PostAsync(req);
//            Assert.IsTrue(resp.Errors == null || resp.Errors.Length == 0);
//            model = resp.GetDataFieldAs<MLModel>("mlmodelByName");
//            Assert.IsNull(model);

        }

        [TestMethod]
        public async Task TestRunModel()
        {
            var req = new GraphQLHttpRequest() { OperationName = "", Variables = null, Query = "mutation{machineLearnModel(mlmodelname: \"yingyang.mlmodel\"){ mlmodel { darl } results { trainPerformance }}}" };
 //           var resp = await client.PostAsync(req);
//            Assert.IsTrue(resp.Errors == null || resp.Errors.Length == 0);
        }


    }
}
