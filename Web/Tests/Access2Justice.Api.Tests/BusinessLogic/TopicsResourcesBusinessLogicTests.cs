﻿using Xunit;
using NSubstitute;
using Access2Justice.Shared.Interfaces;
using Newtonsoft.Json.Linq;
using Access2Justice.CosmosDb.Interfaces;
using Access2Justice.Api.BusinessLogic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Microsoft.Azure.Documents;
using System.IO;
using System.Linq;
using Access2Justice.Shared.Models;

namespace Access2Justice.Api.Tests.BusinessLogic
{
    public class TopicsResourcesBusinessLogicTests
    {
        private readonly IBackendDatabaseService backendDatabaseService;
        private readonly IDynamicQueries dynamicQueries;
        private readonly ICosmosDbSettings cosmosDbSettings;
        private readonly TopicsResourcesBusinessLogic topicsResourcesBusinessLogic;
        private readonly ITopicsResourcesBusinessLogic topicsResourcesSettings;

        //Mocked input data.
        private readonly string keyword = "eviction";
        private readonly string query = "select * from t";
        private readonly string topicId = "addf41e9-1a27-4aeb-bcbb-7959f95094ba";
        private readonly JArray emptyData = JArray.Parse(@"[{}]");
        private readonly JArray topicsData =
                  JArray.Parse(@"[{'id':'addf41e9-1a27-4aeb-bcbb-7959f95094ba','name':'Family',
                   'ParentTopicId':'','keywords':'eviction','location':[{'state':'Hawaii','county':'Kalawao County','city':'Kalawao',
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

        private readonly JArray actionPlanData =
                    JArray.Parse(@"[{'conditions': [{'condition': {'title': 'Take to your partner to see if you can come to an agreement', 'description': 'Why you should do this dolor sit amet.'}}],
                    'id': '807f2e0d-c431-4f1c-b8c8-1223e6750bec','name': 'Action Plan','description': 'This action plan is for tenants who are facing eviction and have experienced the following:',
                    'resourceType': 'Action Plans','externalUrl': 'www.youtube.com','url': 'access2justice.com','referenceTags': [{'id': 'aaa085ef-96fb-4fd0-bcd0-0472ede66512'}],'location': [{'state': 'Hawaii','county': '','city': 'Haiku-Pauwela','zipCode': ''},{'state': 'Alaska','county': '','city': '','zipCode': ''}],
                    'icon': './assets/images/resources/resource.png','createdBy': 'API','createdTimeStamp':'','modifiedBy': 'API','modifiedTimeStamp':''}]");

        private readonly JArray referencesInputData =
                    JArray.Parse(@"[{'conditions': [{'condition': {'title': 'Take to your partner to see if you can come to an agreement', 'description': 'Why you should do this dolor sit amet.'}}],
                    'parentTopicId': [{'id': '349fa67b-164f-4a65-bb5d-a5b3dd2640a6'}],'id': '807f2e0d-c431-4f1c-b8c8-1223e6750bec','name': 'Action Plan','description': 'This action plan is for tenants who are facing eviction and have experienced the following:',
                    'resourceType': 'Action Plans','externalUrl': 'www.youtube.com','url': 'access2justice.com','referenceTags': [{'id': 'aaa085ef-96fb-4fd0-bcd0-0472ede66512'}],'location': [{'state': 'Hawaii','county': '','city': 'Haiku-Pauwela','zipCode': ''},{'state': 'Alaska','county': '','city': '','zipCode': ''}],
                    'icon': './assets/images/resources/resource.png','createdBy': 'API','createdTimeStamp':'','modifiedBy': 'API','modifiedTimeStamp':''}]");

        private readonly JArray articleData =
                    JArray.Parse(@"[{'overview': 'Overview','headline1': 'HL1','headline2': 'HL2','headline3': 'HL3','id': '807f2e0d-c431-4f1c-b8c8-1223e6750bec','name': 'Article1','description': 'Subhead lorem ipsum solor sit amet bibodem',
                    'resourceType': 'Articles','externalUrl': 'www.youtube.com','url': 'access2justice.com','referenceTags': [{'id': 'aaa085ef-96fb-4fd0-bcd0-0472ede66512'}],'location': [{'state': 'Hawaii','county': '','city': 'Haiku-Pauwela','zipCode': ''},{'state': 'Alaska','county': '','city': '','zipCode': ''}],
                    'icon': './assets/images/resources/resource.png','createdBy': 'API','createdTimeStamp':'','modifiedBy': 'API','modifiedTimeStamp':''}]");

        private readonly JArray videoData =
                    JArray.Parse(@"[{'overview': 'Overview','isRecommended': 'Yes','videoUrl': 'Url','id': '807f2e0d-c431-4f1c-b8c8-1223e6750bec','name': 'Video','description': 'Subhead lorem ipsum solor sit amet bibodem',
                    'resourceType': 'Videos','externalUrl': 'www.youtube.com','url': 'access2justice.com','referenceTags': [{'id': 'aaa085ef-96fb-4fd0-bcd0-0472ede66512'}],'location': [{'state': 'Hawaii','county': '','city': 'Haiku-Pauwela','zipCode': ''},{'state': 'Alaska','county': '','city': '','zipCode': ''}],
                    'icon': './assets/images/resources/resource.png','createdBy': 'API','createdTimeStamp':'','modifiedBy': 'API','modifiedTimeStamp':''}]");

        private readonly JArray organizationData =
                    JArray.Parse(@"[{'overview': 'Overview','subService': 'Law Professional','street': '123 Street','city': 'Honululu','state': 'Hawaii','zipCode': '70085','telephone': '67586758768','eligibilityInformation': 'eligibility','reviewedByCommunityMember': 'Community member',
                    'reviewerFullName': 'John Smith','reviewerTitle': 'Lawyer','reviewerImage': 'ReviewerImage.jpeg','id': '807f2e0d-c431-4f1c-b8c8-1223e6750bec','name': 'Organization','description': 'Subhead lorem ipsum solor sit amet bibodem',
                    'resourceType': 'Organizations','externalUrl': 'www.youtube.com','url': 'access2justice.com','referenceTags': [{'id': 'aaa085ef-96fb-4fd0-bcd0-0472ede66512'}],'location': [{'state': 'Hawaii','county': '','city': 'Haiku-Pauwela','zipCode': ''},{'state': 'Alaska','county': '','city': '','zipCode': ''}],
                    'icon': './assets/images/resources/resource.png','createdBy': 'API','createdTimeStamp':'','modifiedBy': 'API','modifiedTimeStamp':''}]");

        private readonly JArray essentialReadingData =
                    JArray.Parse(@"[{'id': '807f2e0d-c431-4f1c-b8c8-1223e6750bec','name': 'Essential Reading in Hawaii','description': 'Subhead lorem ipsum solor sit amet bibodem',
                    'resourceType': 'Essential Readings','externalUrl': 'www.youtube.com','url': 'access2justice.com','referenceTags': [{'id': 'aaa085ef-96fb-4fd0-bcd0-0472ede66512'}],'location': [{'state': 'Hawaii','county': '','city': 'Haiku-Pauwela','zipCode': ''},{'state': 'Alaska','county': '','city': '','zipCode': ''}],
                    'icon': './assets/images/resources/resource.png','createdBy': 'API','createdTimeStamp':'','modifiedBy': 'API','modifiedTimeStamp':''}]");

        private readonly JArray topicData =
                    JArray.Parse(@"[{'id': '807f2e0d-c431-4f1c-b8c8-1223e6750bec','name': 'Family','parentTopicId': [{'id': 'aaa085ef-96fb-4fd0-bcd0-0472ede66512'}],'keywords': 'HOUSING',
                    'location': [{'state': 'Hawaii','county': '','city': 'Haiku-Pauwela','zipCode': ''},{'state': 'Alaska','county': '','city': '','zipCode': ''}],
                    'jsonContent':'jsonContent','icon': './assets/images/resources/resource.png','createdBy': 'API','createdTimeStamp':'','modifiedBy': 'API','modifiedTimeStamp':''}]");

        private readonly JArray referenceTagData =
                     JArray.Parse(@"[{'id': 'aaa085ef-96fb-4fd0-bcd0-0472ede66512'}]");
        private readonly JArray parentTopicIdData =
                     JArray.Parse(@"[{'id': '349fa67b-164f-4a65-bb5d-a5b3dd2640a6'}]");
        private readonly JArray locationData =
                     JArray.Parse(@"[{'state':'Hawaii','county':'','city':'Haiku-Pauwela','zipCode':''},{'state':'Alaska','county':'','city':'','zipCode':''}]");

        private readonly JArray conditionData =
                     JArray.Parse(@"[{'condition': {'title': 'Take to your partner to see if you can come to an agreement','description': 'Why you should do this dolor sit amet'}}]");

        private readonly JArray emptyResourceData = JArray.Parse(@"[{'referenceTags':[{'id': ''}],'location': [{'state': '','county':'','': '','zipCode':''}],'conditions': [{'condition': {'title': '','description': ''}}],'parentTopicId':[{'id':''}] }]");
        
        //Mocked result data.
        private readonly string expectedEmptyArrayObject = "[{}]";
        private readonly string emptyReferenceTagData = "";
        private readonly JArray emptyLocationData =
                     JArray.Parse(@"[{'state':'','county':'','city':'','zipCode':''}]");
        private readonly JArray emptyConditionObject =
                     JArray.Parse(@"[{'condition':[]}]");
        private readonly JArray EmptyReferences =
                     JArray.Parse(@"[[{'id':''}],[{'state':'','county':'','city':'','zipCode':''}],[{'condition':[{'title':'','description':''}]}],[{'id':''}]]");

        private readonly string expectedTopicId = "addf41e9-1a27-4aeb-bcbb-7959f95094ba";
        private readonly string expectedResourceId = "77d301e7-6df2-612e-4704-c04edf271806";
        private readonly JArray expectedformData =
                     JArray.Parse(@"[{'overview': 'Form1','fullDescription': 'Below is the form you will need if you are looking to settle your child custody dispute in court. We have included helpful tips to guide you along the way.',
                    'id':'77d301e7-6df2-612e-4704-c04edf271806','name': 'Form1','description': 'Subhead lorem ipsum solor sit amet bibodem consecuter orem ipsum solor sit amet bibodem',
                    'resourceType': 'Forms','externalUrl': 'www.youtube.com','url': 'access2justice.com','referenceTags': [{'id': 'aaa085ef-96fb-4fd0-bcd0-0472ede66512'}],
                    'location': [{'state': 'Hawaii','county':'','city': 'Haiku-Pauwela','zipCode':''}],'icon': './assets/images/resources/resource.png','createdBy': 'API','createdTimeStamp': '','modifiedBy': 'API','modifiedTimeStamp': ''}]");
        private readonly JArray expectedActionPlanData =
                    JArray.Parse(@"[{'conditions': [{'condition': [{'title': 'Take to your partner to see if you can come to an agreement', 'description': 'Why you should do this dolor sit amet.'}]}],
                    'id': '807f2e0d-c431-4f1c-b8c8-1223e6750bec','name': 'Action Plan','description': 'This action plan is for tenants who are facing eviction and have experienced the following:',
                    'resourceType': 'Action Plans','externalUrl': 'www.youtube.com','url': 'access2justice.com','referenceTags': [{'id': 'aaa085ef-96fb-4fd0-bcd0-0472ede66512'}],'location': [{'state': 'Hawaii','county': '','city': 'Haiku-Pauwela','zipCode': ''},{'state': 'Alaska','county': '','city': '','zipCode': ''}],
                    'icon': './assets/images/resources/resource.png','createdBy': 'API','createdTimeStamp':'','modifiedBy': 'API','modifiedTimeStamp':''}]");

        private readonly JArray expectedArticleData =
                    JArray.Parse(@"[{'overview': 'Overview','headline1': 'HL1','headline2': 'HL2','headline3': 'HL3','id': '807f2e0d-c431-4f1c-b8c8-1223e6750bec','name': 'Article1','description': 'Subhead lorem ipsum solor sit amet bibodem',
                    'resourceType': 'Articles','externalUrl': 'www.youtube.com','url': 'access2justice.com','referenceTags': [{'id': 'aaa085ef-96fb-4fd0-bcd0-0472ede66512'}],'location': [{'state': 'Hawaii','county': '','city': 'Haiku-Pauwela','zipCode': ''},{'state': 'Alaska','county': '','city': '','zipCode': ''}],
                    'icon': './assets/images/resources/resource.png','createdBy': 'API','createdTimeStamp':'','modifiedBy': 'API','modifiedTimeStamp':''}]");

        private readonly JArray expectedVideoData =
                    JArray.Parse(@"[{'overview': 'Overview','isRecommended': 'Yes','videoUrl': 'Url','id': '807f2e0d-c431-4f1c-b8c8-1223e6750bec','name': 'Video','description': 'Subhead lorem ipsum solor sit amet bibodem',
                    'resourceType': 'Videos','externalUrl': 'www.youtube.com','url': 'access2justice.com','referenceTags': [{'id': 'aaa085ef-96fb-4fd0-bcd0-0472ede66512'}],'location': [{'state': 'Hawaii','county': '','city': 'Haiku-Pauwela','zipCode': ''},{'state': 'Alaska','county': '','city': '','zipCode': ''}],
                    'icon': './assets/images/resources/resource.png','createdBy': 'API','createdTimeStamp':'','modifiedBy': 'API','modifiedTimeStamp':''}]");

        private readonly JArray expectedOrganizationData =
                    JArray.Parse(@"[{'overview': 'Overview','subService': 'Law Professional','street': '123 Street','city': 'Honululu','state': 'Hawaii','zipCode': '70085','telephone': '67586758768','eligibilityInformation': 'eligibility','reviewedByCommunityMember': 'Community member',
                    'reviewerFullName': 'John Smith','reviewerTitle': 'Lawyer','reviewerImage': 'ReviewerImage.jpeg','id': '807f2e0d-c431-4f1c-b8c8-1223e6750bec','name': 'Organization','description': 'Subhead lorem ipsum solor sit amet bibodem',
                    'resourceType': 'Organizations','externalUrl': 'www.youtube.com','url': 'access2justice.com','referenceTags': [{'id': 'aaa085ef-96fb-4fd0-bcd0-0472ede66512'}],'location': [{'state': 'Hawaii','county': '','city': 'Haiku-Pauwela','zipCode': ''},{'state': 'Alaska','county': '','city': '','zipCode': ''}],
                    'icon': './assets/images/resources/resource.png','createdBy': 'API','createdTimeStamp':'','modifiedBy': 'API','modifiedTimeStamp':''}]");

        private readonly JArray expectedEssentialReadingData =
                    JArray.Parse(@"[{'id': '807f2e0d-c431-4f1c-b8c8-1223e6750bec','name': 'Essential Reading in Hawaii','description': 'Subhead lorem ipsum solor sit amet bibodem',
                    'resourceType': 'Essential Readings','externalUrl': 'www.youtube.com','url': 'access2justice.com','referenceTags': [{'id': 'aaa085ef-96fb-4fd0-bcd0-0472ede66512'}],'location': [{'state': 'Hawaii','county': '','city': 'Haiku-Pauwela','zipCode': ''},{'state': 'Alaska','county': '','city': '','zipCode': ''}],
                    'icon': './assets/images/resources/resource.png','createdBy': 'API','createdTimeStamp':'','modifiedBy': 'API','modifiedTimeStamp':''}]");

        private readonly JArray expectedTopicData=
                    JArray.Parse(@"[{'id':'807f2e0d-c431-4f1c-b8c8-1223e6750bec','name':'Family','parentTopicId':[{'id':'aaa085ef-96fb-4fd0-bcd0-0472ede66512'}],'keywords':'HOUSING','jsonContent':'jsonContent','location':[{'state':'Hawaii','county':'','city':'Haiku-Pauwela','zipCode':''},{'state':'Alaska','county':'','city':'','zipCode':''}],'icon':'./assets/images/resources/resource.png','createdBy':'API','createdTimeStamp':'','modifiedBy':'API','modifiedTimeStamp':''}]");

        private readonly JArray expectedTopicsData=
                    JArray.Parse(@"[{'id':'807f2e0d-c431-4f1c-b8c8-1223e6750bec','name':'Family','parentTopicId':[{'id':'aaa085ef-96fb-4fd0-bcd0-0472ede66512'}],'keywords':'HOUSING','location':[{'state':'Hawaii','county':'','city':'Haiku-Pauwela','zipCode':''},{'state':'Alaska','county':'','city':'','zipCode':''}],'jsonContent':'jsonContent','icon':'./assets/images/resources/resource.png','createdBy':'API','createdTimeStamp':'','modifiedBy':'API','modifiedTimeStamp':''}]");

        private readonly string expectedReferenceTagData = "aaa085ef-96fb-4fd0-bcd0-0472ede66512";
        private readonly string expectedParentTopicIdData ="349fa67b-164f-4a65-bb5d-a5b3dd2640a6";
        private readonly JArray expectedLocationData =
                     JArray.Parse(@"[{'state':'Hawaii','county':'','city':'Haiku-Pauwela','zipCode':''},{'state':'Alaska','county':'','city':'','zipCode':''}]");
        private readonly JArray expectedReferenceLocationData =
                     JArray.Parse(@"[{'state': 'Hawaii','county':'','city': 'Haiku-Pauwela','zipCode':''}]");
        private readonly JArray expectedConditionData =
                     JArray.Parse(@"[{'condition':[{'title':'Take to your partner to see if you can come to an agreement', 'description':'Why you should do this dolor sit amet'}]}]");
        private readonly JArray expectedResourceReferences =
                     JArray.Parse(@"[[{'id':'aaa085ef-96fb-4fd0-bcd0-0472ede66512'}],[{'state':'Hawaii','county':'','city':'Haiku-Pauwela','zipCode':''},{'state':'Alaska','county':'','city':'','zipCode':''}]]");
        private readonly JArray expectedActionPlanReferences =
                     JArray.Parse(@"[[{'id':'aaa085ef-96fb-4fd0-bcd0-0472ede66512'}],[{'state':'Hawaii','county':'','city':'Haiku-Pauwela','zipCode':''},{'state':'Alaska','county':'','city':'','zipCode':''}],[{'condition':[{'title':'Take to your partner to see if you can come to an agreement','description':'Why you should do this dolor sit amet.'}]}]]");
        private readonly JArray expectedReferencesData =
                     JArray.Parse(@"[[{'id':'aaa085ef-96fb-4fd0-bcd0-0472ede66512'}],[{'state':'Hawaii','county':'','city':'Haiku-Pauwela','zipCode':''},{'state':'Alaska','county':'','city':'','zipCode':''}],[{'condition':[{'title':'Take to your partner to see if you can come to an agreement','description':'Why you should do this dolor sit amet.'}]}],[{'id':'349fa67b-164f-4a65-bb5d-a5b3dd2640a6'}]]");


        public TopicsResourcesBusinessLogicTests()
        {
            backendDatabaseService = Substitute.For<IBackendDatabaseService>();
            dynamicQueries = Substitute.For<IDynamicQueries>();
            cosmosDbSettings = Substitute.For<ICosmosDbSettings>();
            topicsResourcesSettings = Substitute.For<ITopicsResourcesBusinessLogic>();

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
        public void GetReferncesWithProperData()
        {
            //arrange
            var referenceInput = this.referencesInputData;
            var referenceTag = this.referenceTagData;
            var location = this.locationData;
            var condition = this.conditionData;
            var parentTopic = this.parentTopicIdData;

            //act
            var dbResponseReferenceTag = topicsResourcesSettings.GetReferenceTags(referenceTag).ReturnsForAnyArgs<dynamic>(expectedReferenceTagData);
            var dbResponseLocation = topicsResourcesSettings.GetLocations(location).ReturnsForAnyArgs<dynamic>(expectedReferenceLocationData);
            var dbResponseConditions = topicsResourcesSettings.GetConditions(condition).ReturnsForAnyArgs<dynamic>(expectedConditionData);
            var dbResponseParentTopicId = topicsResourcesSettings.GetParentTopicIds(parentTopic).ReturnsForAnyArgs<dynamic>(expectedParentTopicIdData);
            var response = topicsResourcesBusinessLogic.GetReferences(referenceInput[0]);
            var expectedReferenceData = JsonConvert.SerializeObject(expectedReferencesData);
            var actualReferenceData = JsonConvert.SerializeObject(response);
            
            //assert
            Assert.Equal(expectedReferenceData, actualReferenceData);
        }

        [Fact]
        public void GetReferncesWithEmptyData()
        {
            //arrange
            var emptyResource = this.emptyResourceData;
            var referenceTag = this.emptyData;
            var location = this.emptyLocationData;
            var conditon = this.emptyData;
            var parentTopic = this.emptyData;

            //act
            var dbResponseReferenceTag = topicsResourcesSettings.GetReferenceTags(referenceTag).ReturnsForAnyArgs<dynamic>(emptyReferenceTagData);
            var dbResponseLocation = topicsResourcesSettings.GetLocations(location).ReturnsForAnyArgs<dynamic>(emptyLocationData);
            var dbResponseCondition = topicsResourcesSettings.GetConditions(conditon).ReturnsForAnyArgs<dynamic>(emptyData);
            var dbResponseParentTopic = topicsResourcesSettings.GetParentTopicIds(parentTopic).ReturnsForAnyArgs<dynamic>(emptyData);
            var response = topicsResourcesBusinessLogic.GetReferences(emptyResource[0]);
            var ActualReferenceData = JsonConvert.SerializeObject(response);
            var expectedReferencesData = JsonConvert.SerializeObject(EmptyReferences);

            //assert
            Assert.Equal(expectedReferencesData, ActualReferenceData);
        }

        [Fact]
        public void GetReferenceTagsWithProperData()
        {
            //arrange
            var referenceTag = this.referenceTagData;

            //act
            var response = topicsResourcesBusinessLogic.GetReferenceTags(referenceTag);
            
            //assert
            Assert.Equal(expectedReferenceTagData, response[0].ReferenceTags);
        }

        [Fact]
        public void GetReferenceTagsWithEmptyData()
        {
            //arrange
            var referenceTag = this.emptyData;            

            //act
            var response = topicsResourcesBusinessLogic.GetReferenceTags(referenceTag);
            
            //assert
            Assert.Equal(emptyReferenceTagData, response[0].ReferenceTags);
        }

        [Fact]
        public void GetLocationWithProperData()
        {
            //arrange
            var location = this.locationData;

            //act
            var response = topicsResourcesBusinessLogic.GetLocations(location);
            var actualLocation = JsonConvert.SerializeObject(response);
            var expectedLocation = JsonConvert.SerializeObject(expectedLocationData);

            //assert
            Assert.Equal(expectedLocation, actualLocation);
        }

        [Fact]
        public void GetLocationWithEmptyData()
        {
            //arrange
            var location = this.emptyData;

            //act
            var response = topicsResourcesBusinessLogic.GetLocations(location);
            var actualLocation = JsonConvert.SerializeObject(response);
            var expectedLocation = JsonConvert.SerializeObject(emptyLocationData);

            //assert
            Assert.Equal(expectedLocation, actualLocation);
        }

        [Fact]
        public void GetConditionWithProperData()
        {
            //arrange
            var condition = this.conditionData;

            //act
            var response = topicsResourcesBusinessLogic.GetConditions(condition);
            var actualCondition = JsonConvert.SerializeObject(response);
            var expectedCondition = JsonConvert.SerializeObject(expectedConditionData);

            //assert
            Assert.Equal(expectedCondition, actualCondition);
        }

        [Fact]
        public void GetConditionWithEmptyData()
        {
            //arrange
            var condition = this.emptyData;

            //act
            var response = topicsResourcesBusinessLogic.GetConditions(condition);
            var actualCondition = JsonConvert.SerializeObject(response);
            var expectedCondition = JsonConvert.SerializeObject(emptyConditionObject);

            //assert
            Assert.Equal(expectedCondition, actualCondition);
        }

        [Fact]
        public void CreateResourceUploadAsyncWithProperData()
        {
            //arrange
            var form = this.formData;
            var resource = JsonConvert.SerializeObject(form);
            Document document = new Document();
            JsonTextReader reader = new JsonTextReader(new StringReader(form[0].ToString()));
            document.LoadFrom(reader);
            dynamic actualResourceData = null;

            //act
            var dbResponse = backendDatabaseService.CreateItemAsync<dynamic>(form, cosmosDbSettings.ResourceCollectionId).ReturnsForAnyArgs(document);
            var dbResponseResource = topicsResourcesSettings.CreateResourceDocumentAsync(resource).ReturnsForAnyArgs(form[0]);
            var response = topicsResourcesBusinessLogic.CreateResourcesUploadAsync("C:\\Users\\v-sobhad\\Desktop\\CreateJSON\\ResourceData.json").Result;
            foreach (var result in response)
            {
                actualResourceData = result;
            }

            //assert
            Assert.Equal(expectedformData[0].ToString(), actualResourceData.ToString());
        }

        [Fact]
        public void CreateResourceAsyncWithProperData()
        {
            //arrange
            var form = this.formData;
            var resource = JsonConvert.SerializeObject(form);
            Document document = new Document();
            JsonTextReader reader = new JsonTextReader(new StringReader(form[0].ToString()));
            document.LoadFrom(reader);
            dynamic actualResourceData = null;

            //act
            var dbResponse = backendDatabaseService.CreateItemAsync<dynamic>(form, cosmosDbSettings.ResourceCollectionId).ReturnsForAnyArgs(document);
            var dbResponseReferenceTag = topicsResourcesSettings.CreateResourcesForms(form[0]).ReturnsForAnyArgs<dynamic>(expectedformData[0]);
            var response = topicsResourcesBusinessLogic.CreateResourceDocumentAsync(resource).Result;
            foreach (var result in response)
            {
                actualResourceData = result;
            }

            //assert
            Assert.Equal(expectedformData[0].ToString(), actualResourceData.ToString());
        }

        [Fact]
        public void CreateResourcesFormsWithProperData()  //To do - CreateFormsAsyncEmptyData after excpetion logging
        {
            //arrange
            var form = this.formData[0];

            //act
            var dbResponseReferenceTag = topicsResourcesSettings.GetReferenceTags(referenceTagData).ReturnsForAnyArgs<dynamic>(expectedReferenceTagData);
            var dbResponseLocation = topicsResourcesSettings.GetLocations(locationData).ReturnsForAnyArgs<dynamic>(expectedReferenceLocationData);
            var dbResponse = topicsResourcesSettings.GetReferences(form).ReturnsForAnyArgs<dynamic>(expectedResourceReferences);
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

        [Fact]
        public void CreateResourcesActionPlansWithProperData()  //To do - CreateActionPlansAsyncEmptyData after excpetion logging
        {
            //arrange
            var actionPlan = this.actionPlanData[0];

            //act
            var dbResponseReferenceTag = topicsResourcesSettings.GetReferenceTags(referenceTagData).ReturnsForAnyArgs<dynamic>(expectedReferenceTagData);
            var dbResponseLocation = topicsResourcesSettings.GetLocations(locationData).ReturnsForAnyArgs<dynamic>(expectedReferenceLocationData);
            var dbResponseCondition = topicsResourcesSettings.GetConditions(conditionData).ReturnsForAnyArgs<dynamic>(expectedConditionData);
            var dbResponse = topicsResourcesSettings.GetReferences(actionPlan).ReturnsForAnyArgs<dynamic>(expectedActionPlanReferences);
            var response = topicsResourcesBusinessLogic.CreateResourcesActionPlans(actionPlan);
            var result = JsonConvert.SerializeObject(response);
            var actionPlanResult = (JObject)JsonConvert.DeserializeObject(result);
            result = actionPlanResult;
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
            Assert.Equal(expectedActionPlanData[0].ToString(), result.ToString());
        }

        [Fact]
        public void CreateResourcesArticlesWithProperData()
        {
            //arrange
            var article = this.articleData[0];

            //act
            var dbResponseReferenceTag = topicsResourcesSettings.GetReferenceTags(referenceTagData).ReturnsForAnyArgs<dynamic>(expectedReferenceTagData);
            var dbResponseLocation = topicsResourcesSettings.GetLocations(locationData).ReturnsForAnyArgs<dynamic>(expectedReferenceLocationData);
            var dbResponse = topicsResourcesSettings.GetReferences(article).ReturnsForAnyArgs<dynamic>(expectedResourceReferences);
            var response = topicsResourcesBusinessLogic.CreateResourcesArticles(article);
            var result = JsonConvert.SerializeObject(response);
            var articleResult = (JObject)JsonConvert.DeserializeObject(result);
            result = articleResult;
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
            Assert.Equal(expectedArticleData[0].ToString(), result.ToString());
        }

        [Fact]
        public void CreateResourcesVideosWithProperData()
        {
            //arrange
            var video = this.videoData[0];

            //act
            var dbResponseReferenceTag = topicsResourcesSettings.GetReferenceTags(referenceTagData).ReturnsForAnyArgs<dynamic>(expectedReferenceTagData);
            var dbResponseLocation = topicsResourcesSettings.GetLocations(locationData).ReturnsForAnyArgs<dynamic>(expectedReferenceLocationData);
            var dbResponse = topicsResourcesSettings.GetReferences(video).ReturnsForAnyArgs<dynamic>(expectedResourceReferences);
            var response = topicsResourcesBusinessLogic.CreateResourcesVideos(video);
            var result = JsonConvert.SerializeObject(response);
            var videoResult = (JObject)JsonConvert.DeserializeObject(result);
            result = videoResult;
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
            Assert.Equal(expectedVideoData[0].ToString(), result.ToString());
        }

        [Fact]
        public void CreateResourcesOrganizationsWithProperData()
        {
            //arrange
            var organization = this.organizationData[0];

            //act
            var dbResponseReferenceTag = topicsResourcesSettings.GetReferenceTags(referenceTagData).ReturnsForAnyArgs<dynamic>(expectedReferenceTagData);
            var dbResponseLocation = topicsResourcesSettings.GetLocations(locationData).ReturnsForAnyArgs<dynamic>(expectedReferenceLocationData);
            var dbResponse = topicsResourcesSettings.GetReferences(organization).ReturnsForAnyArgs<dynamic>(expectedResourceReferences);
            var response = topicsResourcesBusinessLogic.CreateResourcesOrganizations(organization);
            var result = JsonConvert.SerializeObject(response);
            var organizationResult = (JObject)JsonConvert.DeserializeObject(result);
            result = organizationResult;
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
            Assert.Equal(expectedOrganizationData[0].ToString(), result.ToString());
        }

        [Fact]
        public void CreateResourcesEssentialReadingsWithProperData()
        {
            //arrange
            var essentialReading = this.essentialReadingData[0];

            //act
            var dbResponseReferenceTag = topicsResourcesSettings.GetReferenceTags(referenceTagData).ReturnsForAnyArgs<dynamic>(expectedReferenceTagData);
            var dbResponseLocation = topicsResourcesSettings.GetLocations(locationData).ReturnsForAnyArgs<dynamic>(expectedReferenceLocationData);
            var dbResponse = topicsResourcesSettings.GetReferences(essentialReading).ReturnsForAnyArgs<dynamic>(expectedResourceReferences);
            var response = topicsResourcesBusinessLogic.CreateResourcesEssentialReadings(essentialReading);
            var result = JsonConvert.SerializeObject(response);
            var essentialReadingResult = (JObject)JsonConvert.DeserializeObject(result);
            result = essentialReadingResult;
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
            Assert.Equal(essentialReading.ToString(), result.ToString());
        }

        [Fact]
        public void GetParentTopicIdsWithProperData()
        {
            //arrange
            var parentId = this.parentTopicIdData;

            //act
            var response = topicsResourcesBusinessLogic.GetParentTopicIds(parentId);

            //assert
            Assert.Equal(expectedParentTopicIdData, response[0].ParentTopicIds);
        }

        [Fact]
        public void GetParentTopicIdsWithEmptyData()
        {
            //arrange
            var parentTopicId = this.emptyData;

            //act
            var response = topicsResourcesBusinessLogic.GetParentTopicIds(parentTopicId);

            //assert
            Assert.Equal(emptyReferenceTagData, response[0].ParentTopicIds);
        }

        [Fact]
        public void CreateTopicUploadAsyncWithProperData()
        {
            //arrange
            var topic = this.topicData;
            var topics = JsonConvert.SerializeObject(topic);
            Document document = new Document();
            JsonTextReader reader = new JsonTextReader(new StringReader(topic[0].ToString()));
            document.LoadFrom(reader);
            dynamic actualTopicData = null;

            //act
            var dbResponse = backendDatabaseService.CreateItemAsync<dynamic>(topic, cosmosDbSettings.TopicCollectionId).ReturnsForAnyArgs(document);
            var dbResponseResource = topicsResourcesSettings.CreateTopicDocumentAsync(topics).ReturnsForAnyArgs(topic[0]);
            var response = topicsResourcesBusinessLogic.CreateTopicsUploadAsync("C:\\Users\\v-sobhad\\Desktop\\CreateJSON\\TopicData.json").Result;
            foreach (var result in response)
            {
                actualTopicData = result;
            }

            //assert
            Assert.Equal(expectedTopicsData[0].ToString(), actualTopicData.ToString());
        }

        [Fact]
        public void CreateTopicDocumentAsyncWithProperData()
        {
            //arrange
            var topic = this.topicData;
            var resource = JsonConvert.SerializeObject(topic);
            Document document = new Document();
            JsonTextReader reader = new JsonTextReader(new StringReader(topic[0].ToString()));
            document.LoadFrom(reader);
            dynamic actualTopicData = null;

            //act
            var dbResponse = backendDatabaseService.CreateItemAsync<dynamic>(topic, cosmosDbSettings.TopicCollectionId).ReturnsForAnyArgs(document);
            var dbResponseReferenceTag = topicsResourcesSettings.CreateTopics(topic[0]).ReturnsForAnyArgs<dynamic>(expectedTopicData[0]);
            var response = topicsResourcesBusinessLogic.CreateTopicDocumentAsync(resource).Result;
            foreach (var result in response)
            {
                actualTopicData = result;
            }

            //assert
            Assert.Equal(expectedTopicsData[0].ToString(), actualTopicData.ToString());
        }

        [Fact]
        public void CreateTopicsWithProperData()
        {
            //arrange
            var topic = this.topicData[0];
            var referenceInput = this.referencesInputData;
            var referenceTag = this.referenceTagData;
            var location = this.locationData;
            var condition = this.conditionData;
            var parentTopic = this.parentTopicIdData;

            //act
            var dbResponseReferenceTag = topicsResourcesSettings.GetReferenceTags(referenceTag).ReturnsForAnyArgs<dynamic>(expectedReferenceTagData);
            var dbResponseLocation = topicsResourcesSettings.GetLocations(location).ReturnsForAnyArgs<dynamic>(expectedReferenceLocationData);
            var dbResponseConditions = topicsResourcesSettings.GetConditions(condition).ReturnsForAnyArgs<dynamic>(expectedConditionData);
            var dbResponseParentTopicId = topicsResourcesSettings.GetParentTopicIds(parentTopic).ReturnsForAnyArgs<dynamic>(expectedParentTopicIdData);
            var response = topicsResourcesBusinessLogic.CreateTopics(topic);
            var result = JsonConvert.SerializeObject(response);
            var topicResult = (JObject)JsonConvert.DeserializeObject(result);
            result = topicResult;
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
            Assert.Equal(expectedTopicData[0].ToString(), result.ToString());
        }
    }
}