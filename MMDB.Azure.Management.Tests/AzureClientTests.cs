using MMDB.Azure.Management.AzureDto.AzureCloudService;
using NUnit.Framework;
using Ploeh.AutoFixture;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace MMDB.Azure.Management.Tests
{
    public class AzureClientTests
    {
        public class TestData
        {
            public Fixture Fixture{ get; set; }
            public string AzureSubscriptionIdentifier { get; set; }
            public string AzureManagementCertificate { get; set; }
            public IAzureClient Sut { get; set; }

            public static TestData Create()
            {
                var fixture = new Fixture();
                var testData = new TestData
                {
                    Fixture = fixture
                };

                string settingsFilePath = Path.GetFullPath(".\\Azure.publishsettings.private");
                if(!File.Exists(settingsFilePath))
                {
                    throw new Exception("No azure publish settings file found at " + settingsFilePath);
                }
                var xml = new XmlDocument();
                xml.Load(settingsFilePath);
                var managementCertificateNode = xml.SelectSingleNode("/PublishData/PublishProfile/@ManagementCertificate");
                if(managementCertificateNode == null || string.IsNullOrEmpty(managementCertificateNode.Value))
                {
                    throw new Exception("Missing /PublishData/PublishProfile/@ManagementCertificate in " + settingsFilePath);
                }
                testData.AzureManagementCertificate = managementCertificateNode.Value;

                var subscriptionNode = xml.SelectSingleNode("/PublishData/PublishProfile/Subscription/@Id");
                if(subscriptionNode == null || string.IsNullOrEmpty(subscriptionNode.Value))
                {
                    throw new Exception("Missing /PublishData/PublishProfile/Subscription/@Id node in " + settingsFilePath);
                }
                testData.AzureSubscriptionIdentifier = subscriptionNode.Value;

                testData.Sut = new AzureClient(testData.AzureSubscriptionIdentifier, testData.AzureManagementCertificate);

                return testData;
            }
        }

        public class GetCloudServiceList
        {
            [Test]
            public void ReturnsCloudServiceList()
            {
                var testData = TestData.Create();

                var result = testData.Sut.GetCloudServiceList();

                Assert.IsNotNull(result);
                Assert.Less(0, result.Count);
            }
        }

        public class CreateCloudService
        {
            [Test]
            public void CreatesNewCloudService()
            {
                var testData = TestData.Create();
                var serviceName = testData.Fixture.Create<string>("ServiceName");
                try 
                {
                    var result = testData.Sut.CreateCloudService(serviceName);
                    Assert.IsNotNull(result);
                    Assert.AreEqual(serviceName, result.ServiceName);
                }
                finally
                {
                    try 
                    {
                        testData.Sut.DeleteCloudService(serviceName);
                    }
                    catch {}
                }
            }
        }
    }
}
