﻿using Xunit;
using NSubstitute;
using Access2Justice.Shared.Interfaces;
using Newtonsoft.Json.Linq;
using Access2Justice.CosmosDb.Interfaces;
using Access2Justice.Api.BusinessLogic;
using Newtonsoft.Json;
using System;

namespace Access2Justice.Api.Tests.BusinessLogic
{
    public class TopicsResourcesBusinessLogicTests
    {
        private readonly IBackendDatabaseService backendDatabaseService;
        private readonly IDynamicQueries dynamicQueries;
        private readonly ICosmosDbSettings cosmosDbSettings;
        private readonly TopicsResourcesBusinessLogic topicsResourcesBusinessLogic;

        //Mocked input data.
        private readonly string keyword = "eviction";
        private readonly string query = "select * from t";
        private readonly string topicId = "addf41e9-1a27-4aeb-bcbb-7959f95094ba";
        private readonly JArray emptyData = JArray.Parse(@"[{}]");
        private readonly JArray topicsData =
                  JArray.Parse(@"[{'id':'addf41e9-1a27-4aeb-bcbb-7959f95094ba','name':'Family',
                   'parentTopicID':'','keywords':'eviction','location':[{'state':'Hawaii','county':'Kalawao County','city':'Kalawao',
                    'zipCode':'96742'},{'zipCode':'96741'},{'state':'Hawaii','county':'Honolulu County','city':'Honolulu'},{'state':
                   'Hawaii','city':'Hawaiian Beaches'},{'state':'Hawaii','city':'Haiku-Pauwela'},{'state':'Alaska'}],'jsonContent':'',
                   'icon':'./assets/images/topics/topic14.png','createdBy':'','createdTimeStamp':'','modifiedBy':'','modifiedTimeStamp':'
                   ','_rid':'mwoSALHtpAEBAAAAAAAAAA==','_self':'dbs/mwoSAA==/colls/mwoSALHtpAE=/docs/mwoSALHtpAEBAAAAAAAAAA==/',
                    '_etag':'\'05008e57-0000-0000-0000-5b0797c10000\'','_attachments':'attachments/','_ts':1527224257}]");
        private readonly JArray resourcesData =
                    JArray.Parse(@"[{'id':'77d301e7-6df2-612e-4704-c04edf271806','name':'Tenant Action Plan 
                    for Eviction','description':'This action plan is for tenants who are facing eviction and have experienced the following:',
                    'resourceType':'Action','externalUrl':'','url':'','topicTags':[{'id':'f102bfae-362d-4659-aaef-956c391f79de'},
                    {'id':'2c0cc7b8-62b1-4efb-8568-b1f767f879bc'},{'id':'3aa3a1be-8291-42b1-85c2-252f756febbc'}],'location':[{'state':'Hawaii',
                    'city':'Kalawao','zipCode':'96742'},{'zipCode':'96741'},{'state':'Alaska'}],'icon':'./assets/images/resources/resource.png',
                    'createdBy':'','createdTimeStamp':'','modifiedBy':'','modifiedTimeStamp':'','_rid':'mwoSAJdNlwIBAAAAAAAAAA==',
                    '_self':'dbs/mwoSAA==/colls/mwoSAJdNlwI=/docs/mwoSAJdNlwIBAAAAAAAAAA==/','_etag':'\'040007b5-0000-0000-0000-5b0792260000\'',
                    '_attachments':'attachments/','_ts':1527222822},{'id':'19a02209-ca38-4b74-bd67-6ea941d41518','name':'Legal Help Organization',
                    'description':'This action plan is for tenants who are facing eviction and have experienced the following:','resourceType':'Organization'
                    ,'externalUrl':'','url':'','topicTags':[{'id':'f102bfae-362d-4659-aaef-956c391f79de'},{'id':'3aa3a1be-8291-42b1-85c2-252f756febbc'}],
                    'location':[{'state':'Hawaii','city':'Kalawao','zipCode':'96742'}],'icon':'./assets/images/resources/resource.png','createdBy':'',
                    'createdTimeStamp':'','modifiedBy':'','modifiedTimeStamp':'','_rid':'mwoSAJdNlwIBAAAAAAAAAA==','_self':
                    'dbs/mwoSAA==/colls/mwoSAJdNlwI=/docs/mwoSAJdNlwIBAAAAAAAAAA==/','_etag':'\'040007b5-0000-0000-0000-5b0792260000\'',
                    '_attachments':'attachments/','_ts':1527222822}]");

        private readonly JArray formData =
                    JArray.Parse(@"[{'overview': 'Form1','fullDescription': 'Below is the form you will need if you are looking to settle your child custody dispute in court. We have included helpful tips to guide you along the way.',
                    'id':'77d301e7-6df2-612e-4704-c04edf271806','name': 'Form1','description': 'Subhead lorem ipsum solor sit amet bibodem consecuter orem ipsum solor sit amet bibodem',
                    'resourceType': 'Forms','externalUrl': 'www.youtube.com','url': 'access2justice.com','referenceTags': [{'id': 'aaa085ef-96fb-4fd0-bcd0-0472ede66512'}],
                    'location': [{'state': 'Hawaii','county':'','city': 'Haiku-Pauwela','zipCode':''}],'icon': './assets/images/resources/resource.png','createdBy': 'API','createdTimeStamp': '','modifiedBy': 'API','modifiedTimeStamp': ''}]");
        
        //Mocked result data.
        private readonly string expectedEmptyArrayObject = "[{}]";
        private readonly string expectedTopicId = "addf41e9-1a27-4aeb-bcbb-7959f95094ba";
        private readonly string expectedResourceId = "77d301e7-6df2-612e-4704-c04edf271806";
        private readonly JArray expectedformData =
                     JArray.Parse(@"[{'overview': 'Form1','fullDescription': 'Below is the form you will need if you are looking to settle your child custody dispute in court. We have included helpful tips to guide you along the way.',
                    'id':'77d301e7-6df2-612e-4704-c04edf271806','name': 'Form1','description': 'Subhead lorem ipsum solor sit amet bibodem consecuter orem ipsum solor sit amet bibodem',
                    'resourceType': 'Forms','externalUrl': 'www.youtube.com','url': 'access2justice.com','referenceTags': [{'id': 'aaa085ef-96fb-4fd0-bcd0-0472ede66512'}],
                    'location': [{'state': 'Hawaii','county':'','city': 'Haiku-Pauwela','zipCode':''}],'icon': './assets/images/resources/resource.png','createdBy': 'API','createdTimeStamp': '','modifiedBy': 'API','modifiedTimeStamp': ''}]");
        
        public TopicsResourcesBusinessLogicTests()
        {
            backendDatabaseService = Substitute.For<IBackendDatabaseService>();
            dynamicQueries = Substitute.For<IDynamicQueries>();
            cosmosDbSettings = Substitute.For<ICosmosDbSettings>();

            topicsResourcesBusinessLogic = new TopicsResourcesBusinessLogic(dynamicQueries, cosmosDbSettings, backendDatabaseService);
            cosmosDbSettings.AuthKey.Returns("dummyAuth");
            cosmosDbSettings.Endpoint.Returns(new System.Uri("https://bing.com"));
            cosmosDbSettings.DatabaseId.Returns("dummyDb");
            cosmosDbSettings.TopicCollectionId.Returns("TopicCollection");
            cosmosDbSettings.ResourceCollectionId.Returns("ResourceCollection");            
        }

        [Fact]
        public void GetTopicAsyncWithProperData()
        {
            //arrange
            var dbResponse = backendDatabaseService.QueryItemsAsync(cosmosDbSettings.ResourceCollectionId, query);
            dbResponse.ReturnsForAnyArgs<dynamic>(topicsData);

            //act
            var response = topicsResourcesBusinessLogic.GetTopicsAsync(keyword);
            string result = JsonConvert.SerializeObject(response);

            //assert
            Assert.Contains(expectedTopicId, result, StringComparison.InvariantCultureIgnoreCase);
        }

        [Fact]
        public void GetTopicAsyncWithEmptyData()
        {
            //arrange
            var dbResponse = backendDatabaseService.QueryItemsAsync(cosmosDbSettings.ResourceCollectionId, query);
            dbResponse.ReturnsForAnyArgs<dynamic>(emptyData);

            //act
            var response = topicsResourcesBusinessLogic.GetTopicsAsync(keyword);
            string result = JsonConvert.SerializeObject(response);

            //assert
            Assert.Contains(expectedEmptyArrayObject, result, StringComparison.InvariantCultureIgnoreCase);
        }

        [Fact]
        public void GetResourcesAsyncWithProperData()
        {
            //arrange
            var dbResponse = backendDatabaseService.QueryItemsAsync(cosmosDbSettings.ResourceCollectionId, query);
            dbResponse.ReturnsForAnyArgs<dynamic>(resourcesData);

            //act
            var response = topicsResourcesBusinessLogic.GetResourcesAsync(keyword);
            string result = JsonConvert.SerializeObject(response);

            //assert
            Assert.Contains(expectedResourceId, result, StringComparison.InvariantCultureIgnoreCase);
        }

        [Fact]
        public void GetResourcesAsyncWithEmptyData()
        {
            //arrange

            var dbResponse = backendDatabaseService.QueryItemsAsync(cosmosDbSettings.ResourceCollectionId, query);
            dbResponse.ReturnsForAnyArgs<dynamic>(emptyData);

            //act
            var response = topicsResourcesBusinessLogic.GetResourcesAsync(keyword);
            string result = JsonConvert.SerializeObject(response);

            //assert
            Assert.Contains(expectedEmptyArrayObject, result, StringComparison.InvariantCultureIgnoreCase);
        }

        [Fact]
        public void GetTopicsAsyncWithProperData()
        {
            //arrange
            var dbResponse = backendDatabaseService.QueryItemsAsync(cosmosDbSettings.TopicCollectionId, query);
            dbResponse.ReturnsForAnyArgs<dynamic>(topicsData);
            //act
            var response = topicsResourcesBusinessLogic.GetTopLevelTopicsAsync().Result;
            string result = JsonConvert.SerializeObject(response);
            //assert
            Assert.Contains(expectedTopicId, result, StringComparison.InvariantCulture);
        }

        [Fact]
        public void GetTopicsAsyncWithEmptyData()
        {
            //arrange
            var dbResponse = backendDatabaseService.QueryItemsAsync(cosmosDbSettings.TopicCollectionId, query);
            dbResponse.ReturnsForAnyArgs<dynamic>(emptyData);

            //act
            var response = topicsResourcesBusinessLogic.GetTopLevelTopicsAsync();
            string result = JsonConvert.SerializeObject(response);

            //assert
            Assert.Contains("[{}]", result, StringComparison.InvariantCultureIgnoreCase);
        }

        [Fact]
        public void GetSubTopicsAsyncWithProperData()
        {
            //arrange
            var dbResponse = backendDatabaseService.QueryItemsAsync(cosmosDbSettings.TopicCollectionId, query);
            dbResponse.ReturnsForAnyArgs<dynamic>(resourcesData);
            //act
            var response = topicsResourcesBusinessLogic.GetSubTopicsAsync(topicId).Result;
            string result = JsonConvert.SerializeObject(response);
            //assert
            Assert.Contains(expectedResourceId, result, StringComparison.InvariantCulture);
        }

        [Fact]
        public void GetSubTopicsAsyncWithEmptyData()
        {
            //arrange
            var dbResponse = backendDatabaseService.QueryItemsAsync(cosmosDbSettings.TopicCollectionId, query);
            dbResponse.ReturnsForAnyArgs<dynamic>(emptyData);

            //act
            var response = topicsResourcesBusinessLogic.GetSubTopicsAsync(topicId);
            string result = JsonConvert.SerializeObject(response);

            //assert
            Assert.Contains("[{}]", result, StringComparison.InvariantCultureIgnoreCase);
        }
                
        [Fact]
        public void GetSubTopicDetailsAsyncWithProperData()
        {
            //arrange
            var dbResponse = backendDatabaseService.QueryItemsAsync(cosmosDbSettings.ResourceCollectionId, query);
            dbResponse.ReturnsForAnyArgs<dynamic>(topicsData);
            //act
            var response = topicsResourcesBusinessLogic.GetResourceAsync(topicId).Result;
            string result = JsonConvert.SerializeObject(response);
            //assert
            Assert.Contains(topicId, result, StringComparison.InvariantCulture);
        }

        [Fact]
        public void GetSubTopicDetailsAsyncEmptyData()
        {
            //arrange
            var dbResponse = backendDatabaseService.QueryItemsAsync(cosmosDbSettings.ResourceCollectionId, query);
            dbResponse.ReturnsForAnyArgs<dynamic>(emptyData);

            //act
            var response = topicsResourcesBusinessLogic.GetResourceAsync(topicId);
            string result = JsonConvert.SerializeObject(response);

            //assert
            Assert.Contains("[{}]", result, StringComparison.InvariantCultureIgnoreCase);
        }
        
        [Fact]
        public void CreateFormsAsyncWithProperData()
        {
            //arrange
            var form = this.formData[0];

            //act
            var response = topicsResourcesBusinessLogic.CreateResourcesForms(form);
            var result = JsonConvert.SerializeObject(response);
            var formResult = (JObject)JsonConvert.DeserializeObject(result);
            result = formResult;
            foreach (JProperty field in result)
            {
                if (field.Name == "createdTimeStamp")
                {
                    field.Value = "";
                }

                else if (field.Name == "modifiedTimeStamp")
                {
                    field.Value = "";
                }
            }         
            
            //assert
            Assert.Equal(expectedformData[0].ToString(), result.ToString());
        }

        //[Fact]
        //public void CreateFormsAsyncWithEmptyData()
        //{
        //    //arrange
        //    var form = this.emptyData[0];

        //    //act
        //    var response = topicsResourcesBusinessLogic.CreateResourcesForms(form);
        //    var result = JsonConvert.SerializeObject(response);
        //    var formResult = (JObject)JsonConvert.DeserializeObject(result);
        //    result = formResult;
        //    foreach (JProperty field in result)
        //    {
        //        if (field.Name == "createdTimeStamp")
        //        {
        //            field.Value = "";
        //        }

        //        else if (field.Name == "modifiedTimeStamp")
        //        {
        //            field.Value = "";
        //        }
        //    }

        //    //assert
        //    Assert.Equal(expectedformData[0].ToString(), result.ToString());
        //}

        [Fact]
        public void CreateResourceAsyncWithProperData()
        {
            //arrange
            var form = this.formData[0];

            //act
            var response = topicsResourcesBusinessLogic.CreateResourcesForms(form);
            var result = JsonConvert.SerializeObject(response);
            var formResult = (JObject)JsonConvert.DeserializeObject(result);
            result = formResult;
            foreach (JProperty field in result)
            {
                if (field.Name == "createdTimeStamp")
                {
                    field.Value = "";
                }

                else if (field.Name == "modifiedTimeStamp")
                {
                    field.Value = "";
                }
            }

            //assert
            Assert.Equal(expectedformData[0].ToString(), result.ToString());
        }
    }    
}