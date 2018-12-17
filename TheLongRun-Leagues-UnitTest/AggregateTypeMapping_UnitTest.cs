using NUnit.Framework;
using System;
using TheLongRun.Common;
using TheLongRun.Common.Attributes.Settings;

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

    }
}
