using NUnit.Framework;
using System;
using TheLongRun.Common;
using TheLongRun.Common.Attributes;
using TheLongRun.Common.Attributes.Settings;
using static TheLongRun.Common.Attributes.Settings.AggregateTypeMapping;

namespace Tests
{
    public class AggregateTypeMapping_UnitTest
        : IDisposable 
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

        [Test]
        public void DomainNameAttribute_EnvironmentVariables_Test()
        {

            string expected= ConnectionStringNameAttribute.DefaultConnectionStringName("Test", "test type");

            Assert.IsNotEmpty(expected, "No default connection");

        }

        [Test]
        public void DomainNameAttribute_EnvironmentVariables_Match_Test()
        {
            string expected = "CommandStorageConnectionString";
            string actual = "Not set";


            actual = ConnectionStringNameAttribute.DefaultConnectionStringName("COMMAND", "test type");

 
            Assert.AreEqual(expected, actual);

        }

        [Test]
        public void DomainNameAttribute_EnvironmentVariables_QueryMatch_Test()
        {
            string expected = "QueryStorageConnectionString";
            string actual = "Not set";

            actual = ConnectionStringNameAttribute.DefaultConnectionStringName("Query", "tested type");


            Assert.AreEqual(expected, actual);

        }

        [Test]
        public void DomainNameAttribute_EnvironmentVariables_QueryExactMatch_Test()
        {
            string expected = "QueryTestStorageConnectionString";
            string actual = "Not set";


            actual = ConnectionStringNameAttribute.DefaultConnectionStringName("Query", "test type");

            Assert.AreEqual(expected, actual);

        }

        [Test]
        public void ConfiguredAggregateTypeMappings_Test()
        {


            int expected = 4;
            int actual = 0;

            actual = AggregateTypeMapping.ConfiguredAggregateTypeMappings.Count;


            Assert.AreEqual(expected, actual);

        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects).
                    Environment.SetEnvironmentVariable("TYPEMAP_12_BANKING_ACCOUNT", "");
                    Environment.SetEnvironmentVariable("TYPEMAP_1_COMMAND_*", "");
                    Environment.SetEnvironmentVariable("TYPEMAP_3_QUERY_*", "");
                    Environment.SetEnvironmentVariable("TYPEMAP_2_QUERY_test type", "");
                }

                disposedValue = true;
            }
        }


        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion

         static AggregateTypeMapping_UnitTest()
        {
            Environment.SetEnvironmentVariable("TYPEMAP_1_COMMAND_*", "BlobStream;CommandStorageConnectionString;CommandStorageConnectionStringReadConnectionString");
            Environment.SetEnvironmentVariable("TYPEMAP_12_BANKING_ACCOUNT", "BlobStream;BankAccountStorageConnectionString;BankAccountStorageReadConnectionString");
            Environment.SetEnvironmentVariable("TYPEMAP_3_Query_*", "BlobStream;QueryStorageConnectionString;CommandStorageConnectionStringReadConnectionString");
            Environment.SetEnvironmentVariable("TYPEMAP_2_Query_test type", "BlobStream;QueryTestStorageConnectionString;CommandStorageConnectionStringReadConnectionString");

        }
    }
}
