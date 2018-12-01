using System;
using System.Collections.Generic;
using System.Text;

namespace TheLongRun.Common.Attributes.Settings
{
    public class TableStreamSettings
        : CQRSAzure.EventSourcing.Azure.Table.ITableSettings
    {

        /// <summary>
        /// The name of the domain in which this entity/attribute exists
        /// </summary>
        public string DomainName { get; set; }

        public string SequenceNumberFormat { get ; set ; }

        public bool IncludeDomainInTableName { get ; set ; }

        public string RowNumberFormat { get ; set ; }

        public string ConnectionStringName { get ; set ; }

        public string ReadSideConnectionStringName { get ; set ; }

    }
}
