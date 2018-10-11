using System;
using CQRSAzure.EventSourcing;
using CQRSAzure.IdentifierGroup;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using TheLongRun.Common.Orchestration;

namespace TheLogRun.UnitTestProject.Classifier
{
    [TestClass]
    public class ClassifierRespons_UnitTest
    {
        [TestMethod]
        public void Constructor_TestMethod()
        {
            ClassifierResponse testObj = null;
            //TODO ClassifierResponse testObj = ClassifierResponse.Create(Mocking.GetProjectionUntyped());
            Assert.IsNotNull(testObj);
        }
    }
}

namespace TheLogRun.UnitTestProject.Mocking
{
    public static partial class Mocking
    {

        public static IClassifierUntyped GetClassifierUntyped()
        {
            MockClassifierUntyped classifier = new MockClassifierUntyped();
            return classifier;
        }

        public class MockClassifierProcessor :
            IClassifierProcessorUntyped
        {
            public IClassifierDataSourceHandler.EvaluationResult Classify(IClassifierUntyped classifierToProcess = null, 
                DateTime? effectiveDateTime = null, 
                bool forceExclude = false, 
                IProjectionUntyped projection = null)
            {
                throw new NotImplementedException();
            }
        }


        /// <summary>
        /// A test classifier that returns true (in) if the balance is even 
        /// </summary>
        public class MockClassifierUntyped :
           CQRSAzure.IdentifierGroup.IClassifierUntyped
        {

            private bool _isOdd = false;

            public bool SupportsSnapshots
            {
                get
                {
                    return false;
                }
            }

            public IClassifier.ClassifierDataSourceType ClassifierDataSource
            {
                get
                {
                    return IClassifier.ClassifierDataSourceType.EventHandler;
                }
            }

            public IClassifierDataSourceHandler.EvaluationResult EvaluateEvent(string eventClassName, JObject eventToHandle)
            {
                if (eventClassName == typeof(Mocking.MockDepositEvent ).FullName)
                {
                    return EvaluateMockDepositEvent(eventToHandle.ToObject<MockDepositEvent>());
                }
                else
                {
                    return IClassifierDataSourceHandler.EvaluationResult.Unchanged;
                }
            }

            public IClassifierDataSourceHandler.EvaluationResult EvaluateMockDepositEvent(Mocking.MockDepositEvent depositEvent)
            {
                if (null !=depositEvent )
                {
                    _isOdd &= ((depositEvent.Amount % 2 != 0));
                }
                if (_isOdd )
                {
                    // It is an odd number
                    return IClassifierDataSourceHandler.EvaluationResult.Exclude;
                }
                else
                {
                    // It is an even number
                    return IClassifierDataSourceHandler.EvaluationResult.Include;
                }
            }

            public IClassifierDataSourceHandler.EvaluationResult EvaluateProjection<TProjection>(TProjection projection) where TProjection : IProjectionUntyped
            {
                throw new NotImplementedException();
            }

            public bool HandlesEventType(string eventTypeName)
            {
                if (eventTypeName == typeof(MockDepositEvent).FullName)
                {
                    return true;
                }

                return false;
            }

            public void LoadFromSnapshot(IClassifierSnapshotUntyped latestSnapshot)
            {
                throw new NotImplementedException();
            }

            public IClassifierSnapshotUntyped ToSnapshot()
            {
                throw new NotImplementedException();
            }
        }
    }
}
