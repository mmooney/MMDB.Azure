using Microsoft.WindowsAzure.Storage;
using MMDB.Azure.Management.AzureDto.AzureCloudService;
using MMDB.Azure.Management.AzureDto.AzureLocation;
using MMDB.Azure.Management.AzureDto.AzureStorage;
using MMDB.Shared;
using NUnit.Framework;
using Ploeh.AutoFixture;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;

namespace MMDB.Azure.Management.Tests
{
    [Category("Integration")]
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
                    managementCertificateNode = xml.SelectSingleNode("/PublishData/PublishProfile/Subscription/@ManagementCertificate");
                    if (managementCertificateNode == null || string.IsNullOrEmpty(managementCertificateNode.Value))
                    {
                        throw new Exception("Missing /PublishData/PublishProfile/@ManagementCertificate in " + settingsFilePath);
                    }
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

            public Location GetDefaultLocation()
            {
                var locationList = this.Sut.GetLocationList();
                Assert.IsNotNull(locationList);
                Assert.IsNotEmpty(locationList);
                var eligibleLocationList = locationList.Where(i => i.AvailableServiceList != null
                                                            && i.AvailableServiceList.Contains(Location.EnumAvailableService.Compute)
                                                            && i.AvailableServiceList.Contains(Location.EnumAvailableService.Storage));
                Assert.IsNotNull(eligibleLocationList);
                Assert.IsNotEmpty(eligibleLocationList);
                var defaultLocation = eligibleLocationList.FirstOrDefault(i => i.Name == "East US");
                if (defaultLocation == null)
                {
                    defaultLocation = eligibleLocationList.First();
                }
                Assert.IsNotNull(defaultLocation);
                return defaultLocation;
            }

            public string CreateStorageAccountName()
            {
                return this.Fixture.Create<string>("StorageAccountName").Replace("-","").Substring(0,24).ToLower();
            }

            public DeploymentItem CreateDeployment(string serviceName, string storageAccountName, string deploymentSlot, string blobFileName, string containerName)
            {
                var storageAccount = this.Sut.CreateStorageAccount(storageAccountName);
                Assert.IsNotNull(storageAccount);
                Assert.AreEqual(storageAccountName, storageAccount.ServiceName);
                this.Sut.WaitForStorageAccountStatus(storageAccountName, StorageServiceProperties.EnumStorageServiceStatus.Created);

                File.WriteAllBytes(blobFileName, TestDataResources.MMDB_AzureSample_Web_Azure);
                var keys = this.Sut.GetStorageAccountKeys(storageAccountName);
                var blobUrl = this.Sut.UploadBlobFile(storageAccountName, keys.Primary, blobFileName, containerName);

                var service = this.Sut.CreateCloudService(serviceName);

                var deployment = this.Sut.CreateCloudServiceDeployment(serviceName, blobUrl, TestDataResources.ServiceConfiguration_Cloud, deploymentSlot);
                Assert.IsNotNull(deployment);
                this.Sut.WaitForCloudServiceDeploymentStatus(serviceName, deploymentSlot, DeploymentItem.EnumDeploymentItemStatus.Running);
                this.Sut.WaitForAllCloudServiceInstanceStatus(serviceName, deploymentSlot, RoleInstance.EnumInstanceStatus.ReadyRole, TimeSpan.FromMinutes(10));
                return deployment;
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
                Assert.IsNotEmpty(result);
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

            [Test]
            public void ExistingCloudService_ThrowsException()
            {
                var testData = TestData.Create();
                var serviceName = testData.Fixture.Create<string>("ServiceName");
                try
                {
                    var result1 = testData.Sut.CreateCloudService(serviceName);
                    Assert.IsNotNull(result1);
                    Assert.AreEqual(serviceName, result1.ServiceName);

                    Assert.Throws<Exception>(()=> testData.Sut.CreateCloudService(serviceName));
                }
                finally
                {
                    try
                    {
                        testData.Sut.DeleteCloudService(serviceName);
                    }
                    catch { }
                }
            }

            [Test]
            public void ProvideLabel_UseLabel()
            {
                var testData = TestData.Create();
                var serviceName = testData.Fixture.Create<string>("ServiceName");
                var label = testData.Fixture.Create<string>("Label");
                try
                {
                    var result = testData.Sut.CreateCloudService(serviceName, label: label);
                    Assert.IsNotNull(result);
                    Assert.AreEqual(serviceName, result.ServiceName);
                    Assert.IsNotNull(result.Properties);
                    Assert.IsNotNullOrEmpty(result.Properties.Label);
                    Assert.AreEqual(label, Encoding.UTF8.GetString(Convert.FromBase64String(result.Properties.Label)));
                }
                finally
                {
                    try
                    {
                        testData.Sut.DeleteCloudService(serviceName);
                    }
                    catch { }
                }
            }

            [Test]
            public void NoLocationOrAffinityGroup_DefaultLocation()
            {
                var testData = TestData.Create();
                var defaultLocation = testData.GetDefaultLocation();
                var serviceName = testData.Fixture.Create<string>("ServiceName");
                try
                {
                    var result = testData.Sut.CreateCloudService(serviceName);
                    Assert.IsNotNull(result);
                    Assert.AreEqual(serviceName, result.ServiceName);
                    Assert.IsNotNull(result.Properties);
                    Assert.IsNotNullOrEmpty(result.Properties.Label);
                    Assert.AreEqual(defaultLocation.Name, result.Properties.Location);
                }
                finally
                {
                    try
                    {
                        testData.Sut.DeleteCloudService(serviceName);
                    }
                    catch { }
                }
            }

            [Test]
            public void ProvideLocation_UseLocation()
            {
                var testData = TestData.Create();
                var serviceName = testData.Fixture.Create<string>("ServiceName");
                var location = "Japan East";
                try
                {
                    var result = testData.Sut.CreateCloudService(serviceName, location:location);
                    Assert.IsNotNull(result);
                    Assert.AreEqual(serviceName, result.ServiceName);
                    Assert.IsNotNull(result.Properties);
                    Assert.IsNotNullOrEmpty(result.Properties.Label);
                    Assert.AreEqual(location, result.Properties.Location);
                }
                finally
                {
                    try
                    {
                        testData.Sut.DeleteCloudService(serviceName);
                    }
                    catch { }
                }
            }

            [Test]
            public void ProvideAffinityGroup_UseAffinityGroup()
            {
                var testData = TestData.Create();
                var serviceName = testData.Fixture.Create<string>("ServiceName");
                var affinityGroupName = testData.Fixture.Create<string>("AffinityGroup");
                try
                {
                    var affinityGroup = testData.Sut.CreateAffinityGroup(affinityGroupName);
                    var result = testData.Sut.CreateCloudService(serviceName, affinityGroup: affinityGroupName);
                    Assert.IsNotNull(result);
                    Assert.AreEqual(serviceName, result.ServiceName);
                    Assert.IsNotNull(result.Properties);
                    Assert.IsNotNullOrEmpty(result.Properties.Label);
                    Assert.AreEqual(affinityGroupName, result.Properties.AffinityGroup);
                }
                finally
                {
                    try
                    {
                        testData.Sut.DeleteCloudService(serviceName, true);
                    }
                    catch { }
                    try 
                    {
                        testData.Sut.DeleteAffinityGroup(affinityGroupName);
                    }
                    catch {}
                }
            }

            [Test]
            public void ProvideLocationAndAffinityGroup_ThrowsException()
            {
                var testData = TestData.Create();
                var serviceName = testData.Fixture.Create<string>("ServiceName");
                var affinityGroupName = testData.Fixture.Create<string>("AffinityGroup");
                var location = testData.GetDefaultLocation();
                try
                {
                    var affinityGroup = testData.Sut.CreateAffinityGroup(affinityGroupName);
                    Assert.Throws<ArgumentException>(()=>testData.Sut.CreateCloudService(serviceName, location:location.Name, affinityGroup: affinityGroupName));
                }
                finally
                {
                    try
                    {
                        testData.Sut.DeleteCloudService(serviceName, true);
                    }
                    catch { }
                    try
                    {
                        testData.Sut.DeleteAffinityGroup(affinityGroupName);
                    }
                    catch { }
                }
            }
        }

        public class GetLocationList
        {
            [Test]
            public void ReturnsLocationList()
            {
                var testData = TestData.Create();

                var result = testData.Sut.GetLocationList();

                Assert.IsNotNull(result);
                Assert.IsNotEmpty(result);
            }
        }

        public class CheckCloudServiceNameAvailability
        {
            [Test]
            public void NewName_ReturnsTrue()
            {
                var testData = TestData.Create();
                var serviceName = testData.Fixture.Create<string>("ServiceName");

                string message;
                var result = testData.Sut.CheckCloudServiceNameAvailability(serviceName, out message);

                Assert.IsTrue(result);
                Assert.IsNullOrEmpty(message);
            }

            [Test]
            public void ExistingName_ReturnsFalseWithMessage()
            {
                var testData = TestData.Create();
                var serviceName = testData.Fixture.Create<string>("ServiceName");

                try
                {
                    var service = testData.Sut.CreateCloudService(serviceName);
                    Assert.IsNotNull(service);
                    Assert.AreEqual(serviceName, service.ServiceName);

                    string message;
                    var result = testData.Sut.CheckCloudServiceNameAvailability(serviceName, out message);

                    Assert.IsFalse(result);
                    Assert.IsNotNullOrEmpty(message);
                }
                finally
                {
                    try
                    {
                        testData.Sut.DeleteCloudService(serviceName);
                    }
                    catch { }
                }
            }
        }

        public class GetCloudService
        {
            [Test]
            public void ValidServiceName_ReturnsCloudService()
            {
                var testData = TestData.Create();
                var serviceName = testData.Fixture.Create<string>("ServiceName");

                try
                {
                    var service = testData.Sut.CreateCloudService(serviceName);
                    Assert.IsNotNull(service);
                    Assert.AreEqual(serviceName, service.ServiceName);

                    var result = testData.Sut.GetCloudService(serviceName);

                    Assert.IsNotNull(result);
                    Assert.AreEqual(serviceName, result.ServiceName);
                }
                finally
                {
                    try
                    {
                        testData.Sut.DeleteCloudService(serviceName);
                    }
                    catch { }
                }
            }

            [Test]
            public void InalidServiceName_ReturnsNull()
            {
                var testData = TestData.Create();
                var serviceName = testData.Fixture.Create<string>("ServiceName");

                try
                {
                    var result = testData.Sut.GetCloudService(serviceName);

                    Assert.IsNull(result);
                }
                finally
                {
                    try
                    {
                        testData.Sut.DeleteCloudService(serviceName);
                    }
                    catch { }
                }
            }
        }

        public class GetStorageAccountList
        {
            [Test]
            public void ReturnsStorageAccountList()
            {
                var testData = TestData.Create();

                var result = testData.Sut.GetStorageAccountList();

                Assert.IsNotNull(result);
                Assert.IsNotEmpty(result);
            }
        }

        public class GetStorageAccount
        {
            [Test]
            public void srirachdemo()
            {
                var testData = TestData.Create();

                var result = testData.Sut.GetStorageAccount("srirachdemo");

                Assert.IsNotNull(result);
                
            }
        }

        public class CreateStorageAccount
        {
            [Test]
            public void CreatesNewStorageAccount()
            {
                var testData = TestData.Create();
                var storageAccountName = testData.CreateStorageAccountName();
                try
                {
                    var result = testData.Sut.CreateStorageAccount(storageAccountName);
                    Assert.IsNotNull(result);
                    Assert.AreEqual(storageAccountName, result.ServiceName);
                    testData.Sut.WaitForStorageAccountStatus(storageAccountName, StorageServiceProperties.EnumStorageServiceStatus.Created);
                }
                finally
                {
                    try
                    {
                        testData.Sut.DeleteStorageAccount(storageAccountName);
                    }
                    catch { }
                }
            }

            [Test]
            public void ExistingStorageAccount_ThrowsException()
            {
                var testData = TestData.Create();
                var storageAccountName = testData.CreateStorageAccountName();
                try
                {
                    var result1 = testData.Sut.CreateStorageAccount(storageAccountName);
                    Assert.IsNotNull(result1);
                    Assert.AreEqual(storageAccountName, result1.ServiceName);
                    testData.Sut.WaitForStorageAccountStatus(storageAccountName, StorageServiceProperties.EnumStorageServiceStatus.Created);

                    Assert.Throws<Exception>(() => testData.Sut.CreateStorageAccount(storageAccountName));
                }
                finally
                {
                    try
                    {
                        testData.Sut.DeleteStorageAccount(storageAccountName);
                    }
                    catch { }
                }
            }

            [Test]
            public void ProvideLabel_UseLabel()
            {
                var testData = TestData.Create();
                var storageAccountName = testData.CreateStorageAccountName();
                var label = testData.Fixture.Create<string>("Label");
                try
                {
                    var result = testData.Sut.CreateStorageAccount(storageAccountName, label: label);
                    Assert.IsNotNull(result);
                    Assert.AreEqual(storageAccountName, result.ServiceName);
                    Assert.IsNotNull(result.Properties);
                    Assert.IsNotNullOrEmpty(result.Properties.Label);
                    Assert.AreEqual(label, Encoding.UTF8.GetString(Convert.FromBase64String(result.Properties.Label)));
                    testData.Sut.WaitForStorageAccountStatus(storageAccountName, StorageServiceProperties.EnumStorageServiceStatus.Created);
                }
                finally
                {
                    try
                    {
                        testData.Sut.DeleteStorageAccount(storageAccountName);
                    }
                    catch { }
                }
            }

            [Test]
            public void NoLocationOrAffinityGroup_DefaultLocation()
            {
                var testData = TestData.Create();
                var defaultLocation = testData.GetDefaultLocation();
                var storageAccountName = testData.CreateStorageAccountName();
                try
                {
                    var result = testData.Sut.CreateStorageAccount(storageAccountName);
                    Assert.IsNotNull(result);
                    Assert.AreEqual(storageAccountName, result.ServiceName);
                    Assert.IsNotNull(result.Properties);
                    Assert.IsNotNullOrEmpty(result.Properties.Label);
                    Assert.AreEqual(defaultLocation.Name, result.Properties.Location);
                    testData.Sut.WaitForStorageAccountStatus(storageAccountName, StorageServiceProperties.EnumStorageServiceStatus.Created);
                }
                finally
                {
                    try
                    {
                        testData.Sut.DeleteStorageAccount(storageAccountName);
                    }
                    catch { }
                }
            }

            [Test]
            public void ProvideLocation_UseLocation()
            {
                var testData = TestData.Create();
                var storageAccountName = testData.CreateStorageAccountName();
                var location = "Japan East";
                try
                {
                    var result = testData.Sut.CreateStorageAccount(storageAccountName, location: location);
                    Assert.IsNotNull(result);
                    Assert.AreEqual(storageAccountName, result.ServiceName);
                    Assert.IsNotNull(result.Properties);
                    Assert.IsNotNullOrEmpty(result.Properties.Label);
                    Assert.AreEqual(location, result.Properties.Location);
                    testData.Sut.WaitForStorageAccountStatus(storageAccountName, StorageServiceProperties.EnumStorageServiceStatus.Created);
                }
                finally
                {
                    try
                    {
                        testData.Sut.DeleteStorageAccount(storageAccountName);
                    }
                    catch { }
                }
            }

            [Test]
            public void ProvideAffinityGroup_UseAffinityGroup()
            {
                var testData = TestData.Create();
                var storageAccountName = testData.CreateStorageAccountName();
                var affinityGroupName = testData.Fixture.Create<string>("AffinityGroup");
                try
                {
                    var affinityGroup = testData.Sut.CreateAffinityGroup(affinityGroupName);
                    var result = testData.Sut.CreateStorageAccount(storageAccountName, affinityGroup: affinityGroupName);
                    Assert.IsNotNull(result);
                    Assert.AreEqual(storageAccountName, result.ServiceName);
                    Assert.IsNotNull(result.Properties);
                    Assert.IsNotNullOrEmpty(result.Properties.Label);
                    Assert.AreEqual(affinityGroupName, result.Properties.AffinityGroup);
                    testData.Sut.WaitForStorageAccountStatus(storageAccountName, StorageServiceProperties.EnumStorageServiceStatus.Created);
                }
                finally
                {
                    try
                    {
                        testData.Sut.DeleteStorageAccount(storageAccountName, true);
                    }
                    catch { }
                    try
                    {
                        testData.Sut.DeleteAffinityGroup(affinityGroupName);
                    }
                    catch { }
                }
            }

            [Test]
            public void ProvideLocationAndAffinityGroup_ThrowsException()
            {
                var testData = TestData.Create();
                var storageAccountName = testData.CreateStorageAccountName();
                var affinityGroupName = testData.Fixture.Create<string>("AffinityGroup");
                var location = testData.GetDefaultLocation();
                try
                {
                    var affinityGroup = testData.Sut.CreateAffinityGroup(affinityGroupName);
                    Assert.Throws<ArgumentException>(() => testData.Sut.CreateStorageAccount(storageAccountName, location: location.Name, affinityGroup: affinityGroupName));
                }
                finally
                {
                    try
                    {
                        testData.Sut.DeleteStorageAccount(storageAccountName, true);
                    }
                    catch { }
                    try
                    {
                        testData.Sut.DeleteAffinityGroup(affinityGroupName);
                    }
                    catch { }
                }
            }
        }

        public class CheckStorageAccountNameAvailability
        {
            [Test]
            public void NewName_ReturnsTrue()
            {
                var testData = TestData.Create();
                var storageAccountName = testData.CreateStorageAccountName();

                string message;
                var result = testData.Sut.CheckStorageAccountNameAvailability(storageAccountName, out message);

                Assert.IsTrue(result);
                Assert.IsNullOrEmpty(message);
            }

            [Test]
            public void ExistingName_ReturnsFalseWithMessage()
            {
                var testData = TestData.Create();
                var storageAccountName = testData.CreateStorageAccountName();

                try
                {
                    var service = testData.Sut.CreateStorageAccount(storageAccountName);
                    Assert.IsNotNull(service);
                    Assert.AreEqual(storageAccountName, service.ServiceName);
                    
                    testData.Sut.WaitForStorageAccountStatus(storageAccountName, StorageServiceProperties.EnumStorageServiceStatus.Created);

                    string message;
                    var result = testData.Sut.CheckStorageAccountNameAvailability(storageAccountName, out message);

                    Assert.IsFalse(result);
                    Assert.IsNotNullOrEmpty(message);
                }
                finally
                {
                    try
                    {
                        testData.Sut.DeleteStorageAccount(storageAccountName);
                    }
                    catch { }
                }
            }
        }

        public class GetStorageAccountKeys
        {
            [Test]
            public void ReturnsStorageAccountKeys()
            {
                var testData = TestData.Create();
                var storageAccountName = testData.CreateStorageAccountName();
                try
                {
                    var storageAccount = testData.Sut.CreateStorageAccount(storageAccountName);
                    Assert.IsNotNull(storageAccount);
                    Assert.AreEqual(storageAccountName, storageAccount.ServiceName);
                    testData.Sut.WaitForStorageAccountStatus(storageAccountName, StorageServiceProperties.EnumStorageServiceStatus.Created);
                    
                    var result = testData.Sut.GetStorageAccountKeys(storageAccountName);
                    Assert.IsNotNull(result);
                    Assert.IsNotNullOrEmpty(result.Primary);
                    Assert.IsNotNullOrEmpty(result.Secondary);
                }
                finally
                {
                    try
                    {
                        testData.Sut.DeleteStorageAccount(storageAccountName);
                    }
                    catch { }
                }
            }
        }

        public class UploadBlobFile
        {
            [Test]
            public void UploadsBlobFile()
            {
                var testData = TestData.Create();
                var storageAccountName = testData.CreateStorageAccountName();
                string blobFileName = Path.GetTempFileName();
                string containerName = testData.Fixture.Create<string>("Container").ToLower();
                try
                {
                    var storageAccount = testData.Sut.CreateStorageAccount(storageAccountName);
                    Assert.IsNotNull(storageAccount);
                    Assert.AreEqual(storageAccountName, storageAccount.ServiceName);
                    testData.Sut.WaitForStorageAccountStatus(storageAccountName, StorageServiceProperties.EnumStorageServiceStatus.Created);

                    File.WriteAllText(blobFileName, testData.Fixture.Create<string>("TestData"));

                    var keys = testData.Sut.GetStorageAccountKeys(storageAccountName);

                    var result = testData.Sut.UploadBlobFile(storageAccountName, keys.Primary, blobFileName, containerName);

                    Assert.IsNotNullOrEmpty(result);

                    var connectionString = string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", storageAccountName, keys.Primary);
                    var account = CloudStorageAccount.Parse(connectionString);
                    var blobClient = account.CreateCloudBlobClient();
                    var container = blobClient.GetContainerReference(containerName);
                    var blobName = new Uri(result).Segments.Last();
                    var blobReference = container.GetBlockBlobReference(blobName);
                    Assert.IsNotNull(blobReference);
                }
                finally
                {
                    try 
                    {
                        if(File.Exists(blobFileName))
                        {
                            File.Delete(blobFileName);
                        }
                    }
                    catch {}
                    try
                    {
                        testData.Sut.DeleteStorageAccount(storageAccountName);
                    }
                    catch { }
                }
            }

            private void AssertUrlExists(string url)
            {
                var request = (HttpWebRequest)HttpWebRequest.CreateDefault(new Uri(url));
                request.Method = "HEAD";
                using(var response = request.GetResponse())
                {
                    //yay
                }
            }
        }

        public class GetCloudServiceDeploymentList
        {
            [Test]
            public void InvalidServiceName_ThrowsException()
            {
                var testData = TestData.Create();
                var serviceName = testData.Fixture.Create<string>("ServiceName");
                try
                {
                    Assert.Throws<FileNotFoundException>(()=>testData.Sut.GetCloudServiceDeploymentList(serviceName));
                }
                finally
                {
                    try
                    {
                        testData.Sut.DeleteCloudService(serviceName);
                    }
                    catch { }
                }
            }

            [Test]
            public void ValidServiceName_NoDeployments_ReturnsEmptyList()
            {
                var testData = TestData.Create();
                var serviceName = testData.Fixture.Create<string>("ServiceName");
                try
                {
                    testData.Sut.CreateCloudService(serviceName);

                    var result = testData.Sut.GetCloudServiceDeploymentList(serviceName);

                    Assert.IsNotNull(result);
                    Assert.IsEmpty(result);
                }
                finally
                {
                    try
                    {
                        testData.Sut.DeleteCloudService(serviceName);
                    }
                    catch { }
                }
            }

            [Test]
            public void ValidServiceName_WithDeployment_ReturnsItem()
            {
                var testData = TestData.Create();
                var serviceName = testData.Fixture.Create<string>("ServiceName");
                var storageAccountName = testData.CreateStorageAccountName();
                var deploymentSlot = "production";
                var blobFileName = Path.GetTempFileName();
                var containerName = testData.Fixture.Create<string>("Container").ToLower();
                try
                {
                    var deployment = testData.CreateDeployment(serviceName, storageAccountName, deploymentSlot, blobFileName, containerName);

                    var deploymentList = testData.Sut.GetCloudServiceDeploymentList(serviceName);
                    Assert.IsNotNull(deploymentList);
                    Assert.AreEqual(1, deploymentList.Count);
                    Assert.AreEqual(deployment.Name, deploymentList[0].Name);
                }
                finally
                {
                    try 
                    {
                        if(File.Exists(blobFileName))
                        {
                            File.Delete(blobFileName);
                        }
                    }
                    catch {}
                    try
                    {
                        testData.Sut.DeleteCloudService(serviceName);
                    }
                    catch {}
                    try 
                    {
                        testData.Sut.DeleteStorageAccount(storageAccountName);
                    }
                    catch {}
                }
            }

        }

        public class GetCloudServiceDeployment
        {
            [Test]
            public void InvalidServiceName_ReturnsNull()
            {
                var testData = TestData.Create();
                var serviceName = testData.Fixture.Create<string>("ServiceName");
                var deploymentSlot = "production";
                try
                {
                    var result = testData.Sut.GetCloudServiceDeployment(serviceName, deploymentSlot);

                    Assert.IsNull(result);
                }
                finally
                {
                    try
                    {
                        testData.Sut.DeleteCloudService(serviceName);
                    }
                    catch { }
                }
            }

            [Test]
            public void ValidServiceName_InvalidSlot_ReturnsNull()
            {
                var testData = TestData.Create();
                var serviceName = testData.Fixture.Create<string>("ServiceName");
                var deploymentSlot = "production";
                try
                {
                    var service = testData.Sut.CreateCloudService(serviceName);

                    var result = testData.Sut.GetCloudServiceDeployment(serviceName, deploymentSlot);

                    Assert.IsNull(result);
                }
                finally
                {
                    try
                    {
                        testData.Sut.DeleteCloudService(serviceName);
                    }
                    catch { }
                }
            }

            [Test]
            public void ValidServiceName_ValidSlot_ReturnItem()
            {
                var testData = TestData.Create();
                var serviceName = testData.Fixture.Create<string>("ServiceName");
                var storageAccountName = testData.CreateStorageAccountName();
                var deploymentSlot = "production";
                var blobFileName = Path.GetTempFileName();
                var containerName = testData.Fixture.Create<string>("Container").ToLower();
                try
                {
                    var deployment = testData.CreateDeployment(serviceName, storageAccountName, deploymentSlot, blobFileName, containerName);

                    var result = testData.Sut.GetCloudServiceDeployment(serviceName, deploymentSlot);

                    Assert.IsNotNull(result);
                }
                finally
                {
                    try
                    {
                        if (File.Exists(blobFileName))
                        {
                            File.Delete(blobFileName);
                        }
                    }
                    catch { }
                    try
                    {
                        testData.Sut.DeleteCloudService(serviceName);
                    }
                    catch { }
                    try
                    {
                        testData.Sut.DeleteStorageAccount(storageAccountName);
                    }
                    catch { }
                }
            }
        }
    }
}
