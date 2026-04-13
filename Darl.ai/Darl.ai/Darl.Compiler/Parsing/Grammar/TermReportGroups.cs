/// </summary>

﻿// ***********************************************************************
// Assembly         : DarlCompiler
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="TermReportGroups.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using System.Collections.Generic;

namespace DarlCompiler.Parsing
{

    //Terminal report group is a facility for improving syntax error messages. 
    // Darl parser/scanner reports an error like "Syntax error, invalid character. Expected: <expected list>."
    // The <expected list> is a list of all terminals (symbols) that are expected in current position.
    // This list might quite long and quite difficult to look through. The solution is to provide Group names for 
    // groups of terminals - these are groups of type Normal. 
    // Some terminals might be excluded from showing in expected list by including them into group of type DoNotReport. 
    // Finally, Operator group allows you to specify group name for all operator symbols without listing operators -
    // Darl will collect all operator symbols registered with RegisterOperator method automatically. 

    /// Enum TermReportGroupType
    /// </summary>
    public enum TermReportGroupType
    {
        /// The normal
        /// </summary>
        Normal,
        /// The do not report
        /// </summary>
        DoNotReport,
        /// The operator
        /// </summary>
        Operator
    }
    /// Class TermReportGroup.
    /// </summary>
    public class TermReportGroup
    {
        /// The alias
        /// </summary>
        public string Alias;
        /// The group type
        /// </summary>
        public TermReportGroupType GroupType;
        /// The terminals
        /// </summary>
        public TerminalSet Terminals = new TerminalSet();

        /// Initializes a new instance of the <see cref="TermReportGroup"/> class.
        /// </summary>
        /// <param name="alias">The alias.</param>
        /// <param name="groupType">Type of the group.</param>
        /// <param name="terminals">The terminals.</param>
        public TermReportGroup(string alias, TermReportGroupType groupType, IEnumerable<Terminal> terminals)
        {
            Alias = alias;
            GroupType = groupType;
            if (terminals != null)
                Terminals.UnionWith(terminals);
        }

    }

    /// Class TermReportGroupList.
    /// </summary>
    public class TermReportGroupList : List<TermReportGroup> { }

}
