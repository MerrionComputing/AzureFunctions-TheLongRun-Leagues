//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using CQRSAzure.EventSourcing;

namespace Leagues.Season
{
    
    /// <summary>
    /// A defined season
    /// </summary>
    /// <remarks>
    /// This is usally identified by a year or year range, e.g. 2017 or 2017-18
    /// </remarks>
    public partial interface ISeason :
        IAggregationIdentifier<string>
    {

    }
}