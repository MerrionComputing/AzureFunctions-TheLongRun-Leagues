using System;
using System.Runtime.Serialization;
using CQRSAzure.EventSourcing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using TheLongRun.Common.Orchestration;
using TheLogRun.UnitTestProject.Mocking;

namespace TheLogRun.UnitTestProject.Projection
{
    [TestClass]
    public class ProjectionResponse_UnitTest
    {
        [TestMethod]
        public void Constructor_TestMethod()
        {
            ProjectionResponse testObj = ProjectionResponse.Create(Mocking.Mocking.GetProjectionUntyped());
            Assert.IsNotNull(testObj);
        }


        [TestMethod]
        public void GetValuesAsArray_NotNull_TestMethod()
        {
            ProjectionResponse projResp = ProjectionResponse.Create(Mocking.Mocking.GetProjectionUntyped());
            JArray testArray = projResp.Values;

            Assert.IsNotNull(testArray);

        }

        [TestMethod]
        public void GetValuesAsArray_ValueMatch_TestMethod()
        {
            decimal expected = 70.0M;
            decimal actual = -1.9M;

            ProjectionResponse projResp = ProjectionResponse.Create(Mocking.Mocking.GetProjectionUntyped());
            JArray testArray = projResp.Values;

            Mocking.Mocking.MockBalanceResponse testItem = testArray.First.ToObject<Mocking.Mocking.MockBalanceResponse>();
            if (null != testItem)
            {
                actual = testItem.Balance;
            }

            Assert.AreEqual(expected, actual);
        }


        [TestMethod]
        public void GetValuesAsArray_AccountMatch_TestMethod()
        {
            string expected = "Acct.1";
            string actual = "Not set";

            ProjectionResponse projResp = ProjectionResponse.Create(Mocking.Mocking.GetProjectionUntyped());
            JArray testArray = projResp.Values;

            Mocking.Mocking.MockBalanceResponse testItem = testArray.First.ToObject<Mocking.Mocking.MockBalanceResponse>();
            if (null != testItem)
            {
                actual = testItem.AccountNumber;
            }

            Assert.AreEqual(expected, actual);
        }
    }
}
namespace TheLogRun.UnitTestProject.Mocking
{

    public static partial class Mocking
    {

        public static IProjectionUntyped GetProjectionUntyped()
        {
            MockProjectionUntyped test = new MockProjectionUntyped();
            // add some values
            test.HandleEvent<MockDepositEvent>(new MockDepositEvent("Acct.1", 50.0M));
            test.HandleEvent<MockDepositEvent>(new MockDepositEvent("Acct.1", 20.0M));
            test.HandleEvent<MockDepositEvent>(new MockDepositEvent("Acct.2", 12.20M));
            return test;
        }

        /// <summary>
        /// A mock projection for unit test
        /// </summary>
        public class MockProjectionUntyped :
        CQRSAzure.EventSourcing.ProjectionBaseUntyped
        {


            public override bool SupportsSnapshots => true;

            /// <summary>
            /// Handle a deposit
            /// </summary>
            public void HandleEvent(MockDepositEvent eventHandled)
            {
                if (null != eventHandled)
                {
                    int rowNumber = GetAccountRow(eventHandled.AccountNumber);
                    base.IncrementValue<decimal>(nameof(Balance), eventHandled.Amount, rowNumber);
                }
            }


            private int GetAccountRow(string accountNumber)
            {
                int ret = 0;
                while (PropertyExists(nameof(AccountNumber), ret))
                {
                    string acctNum = base.GetPropertyValue<string>(nameof(AccountNumber), ret);
                    if (accountNumber.Equals(acctNum, StringComparison.OrdinalIgnoreCase))
                    {
                        return ret;
                    }
                    ret++;
                }
                // Not found - add it
                base.AddOrUpdateValue<string>(nameof(AccountNumber), ret, accountNumber);
                return ret;
            }

            private bool PropertyExists(string propertyName, int rowNumber)
            {
                return base.m_currentValues.Exists(f => f.Name.Equals(propertyName) && f.RowNumber == rowNumber);
            }

            public override void HandleEvent<TEvent>(TEvent eventToHandle)
            {
                if (eventToHandle.GetType() == typeof(MockDepositEvent))
                {
                    HandleEvent(eventToHandle as MockDepositEvent);
                }
            }

            public override void HandleEventJSon(string eventFullName, JObject eventToHandle)
            {
                if (eventFullName == typeof(MockDepositEvent).FullName)
                {
                    HandleEvent<MockDepositEvent>(eventToHandle.ToObject<MockDepositEvent>());
                }
            }

            public override bool HandlesEventType(Type eventType)
            {
                return HandlesEventTypeByName(eventType.FullName);
            }

            public override bool HandlesEventTypeByName(string eventTypeFullName)
            {
                if (eventTypeFullName == typeof(MockDepositEvent).FullName)
                {
                    return true;
                }

                return false;
            }

            #region Public properties

            /// <summary>
            /// The name of a query being executed
            /// </summary>
            public string QueryName
            {
                get
                {
                    return base.GetPropertyValue<string>(nameof(QueryName));
                }
            }

            /// <summary>
            /// An account number - to test multi-row projections
            /// </summary>
            public string AccountNumber(int row)
            {
                return base.GetPropertyValue<string>(nameof(AccountNumber), row);
            }

            public decimal Balance(int row)
            {
                return base.GetPropertyValue<decimal>(nameof(Balance), row);
            }

            #endregion
        }

        /// <summary>
        /// A mock event representing a deposit into an account
        /// </summary>
        public class MockDepositEvent
            : IEvent
        {

            public string AccountNumber { get; set; }

            public decimal Amount { get; set; }

            public MockDepositEvent(SerializationInfo info, StreamingContext context)
            {
                AccountNumber = info.GetString(nameof(AccountNumber));
                Amount = info.GetDecimal(nameof(Amount));
            }

            public MockDepositEvent(string accountNumber, decimal depositAmount)
            {
                AccountNumber = accountNumber;
                Amount = depositAmount;
            }

            public void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                info.AddValue(nameof(AccountNumber), AccountNumber);
                info.AddValue(nameof(Amount), Amount);
            }
        }

        public class MockBalanceResponse
        {

            public string AccountNumber { get; set; }

            public decimal Balance { get; set; }
        }
    }
}
