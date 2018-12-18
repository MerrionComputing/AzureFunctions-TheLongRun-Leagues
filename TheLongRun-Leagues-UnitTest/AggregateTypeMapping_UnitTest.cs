using NUnit.Framework;
using System;
using TheLongRun.Common;
using TheLongRun.Common.Attributes.Settings;
using static TheLongRun.Common.Attributes.Settings.AggregateTypeMapping;

namespace Tests
{
    public class AggregateTypeMapping_UnitTest
    {

        [Test]
        public void Constructor_Test()
        {
            AggregateTypeMapping testSetting = new AggregateTypeMapping();
            Assert.IsNotNull(testSetting);

        }

        [Test]
        public void Constructor_MappingFromApplicationSetting_Test()
        {

            string ApplicationSettingName = "TYPEMAP_12_BANKING_ACCOUNT";
            string ApplicationSettingValue = "BlobStream;BankAccountStorageConnectionString";

            AggregateTypeMapping testSetting = AggregateTypeMapping.MappingFromApplicationSetting(ApplicationSettingName,ApplicationSettingValue );
            Assert.IsNotNull(testSetting);

        }

        [Test]
        public void Constructor_MappingFromApplicationSetting_Precedence_Test()
        {

            int expected = 12;
            int actual = 7;

            string ApplicationSettingName = "TYPEMAP_12_BANKING_ACCOUNT";
            string ApplicationSettingValue = "BlobStream;BankAccountStorageConnectionString";

            AggregateTypeMapping testSetting = AggregateTypeMapping.MappingFromApplicationSetting(ApplicationSettingName, ApplicationSettingValue);
            actual = testSetting.Precedence;

            Assert.AreEqual(expected, actual);

        }

        [Test]
        public void Constructor_MappingFromApplicationSetting_Domain_Test()
        {

            string expected = "BANKING";
            string actual = "Not set";

            string ApplicationSettingName = "TYPEMAP_12_BANKING_ACCOUNT";
            string ApplicationSettingValue = "BlobStream;BankAccountStorageConnectionString";

            AggregateTypeMapping testSetting = AggregateTypeMapping.MappingFromApplicationSetting(ApplicationSettingName, ApplicationSettingValue);
            actual = testSetting.MappedDomainName ;

            Assert.AreEqual(expected, actual);

        }

        [Test]
        public void Constructor_MappingFromApplicationSetting_AggregateType_Test()
        {

            string expected = "ACCOUNT";
            string actual = "Not set";

            string ApplicationSettingName = "TYPEMAP_12_BANKING_ACCOUNT";
            string ApplicationSettingValue = "BlobStream;BankAccountStorageConnectionString";

            AggregateTypeMapping testSetting = AggregateTypeMapping.MappingFromApplicationSetting(ApplicationSettingName, ApplicationSettingValue);
            actual = testSetting.MappedAggregateTypeName ;

            Assert.AreEqual(expected, actual);

        }

        [Test]
        public void Constructor_MappingFromApplicationSetting_BackingStorageType_Test()
        {

            BackingStorageType expected = BackingStorageType.BlobStream ;
            BackingStorageType actual = BackingStorageType.FileStream ;

            string ApplicationSettingName = "TYPEMAP_12_BANKING_ACCOUNT";
            string ApplicationSettingValue = "BlobStream;BankAccountStorageConnectionString";

            AggregateTypeMapping testSetting = AggregateTypeMapping.MappingFromApplicationSetting(ApplicationSettingName, ApplicationSettingValue);
            actual = testSetting.StorageType;

            Assert.AreEqual(expected, actual);

        }


        [Test]
        public void Constructor_MappingFromApplicationSetting_ConnectionString_Test()
        {

            string expected = "BankAccountStorageConnectionString";
            string actual = "not set";

            string ApplicationSettingName = "TYPEMAP_12_BANKING_ACCOUNT";
            string ApplicationSettingValue = "BlobStream;BankAccountStorageConnectionString";

            AggregateTypeMapping testSetting = AggregateTypeMapping.MappingFromApplicationSetting(ApplicationSettingName, ApplicationSettingValue);
            actual = testSetting.BlobStreamSettings.ConnectionStringName ;

            Assert.AreEqual(expected, actual);

        }

        [Test]
        public void Constructor_MappingFromApplicationSetting_ReadConnectionString_Test()
        {

            string expected = "BankAccountStorageReadConnectionString";
            string actual = "not set";

            string ApplicationSettingName = "TYPEMAP_12_BANKING_ACCOUNT";
            string ApplicationSettingValue = "BlobStream;BankAccountStorageConnectionString;BankAccountStorageReadConnectionString";

            AggregateTypeMapping testSetting = AggregateTypeMapping.MappingFromApplicationSetting(ApplicationSettingName, ApplicationSettingValue);
            actual = testSetting.BlobStreamSettings.ReadSideConnectionStringName ;

            Assert.AreEqual(expected, actual);

        }
    }
}
