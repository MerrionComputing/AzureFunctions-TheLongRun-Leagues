using CQRSAzure.EventSourcing;
using System;
using System.Collections.Generic;
using System.Text;

namespace TheLongRun.Common.Orchestration
{
    public class WriteContext
        : IWriteContext
    {

        private readonly string _who;
        public string Who
        {
            get
            {
                return _who;
            }
        }

        private readonly string _source;
        public string Source
        {
            get
            {
                return _source;
            }
        }

        private readonly string _commentary;
        public string Commentary
        {
            get
            {
                return _commentary;
            }
        }

        private readonly string _correlationidentifier;
        public string CorrelationIdentifier
        {
            get
            {
                return _correlationidentifier;
            }
        }

        public WriteContext(string who,
            string source,
            string commentary = "",
            string correlationidentifier = "")
        {

            _who = who;
            _source = source;
            _commentary = commentary;
            _correlationidentifier = correlationidentifier;

        }
    }
}
