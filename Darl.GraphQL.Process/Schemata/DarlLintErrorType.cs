/// </summary>

﻿using DarlCommon;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class DarlLintErrorType : ObjectGraphType<DarlLintView>
    {
        public DarlLintErrorType()
        {
            Name = "DarlLintError";
            Description = "Represents a syntax error found in DARL code.";
            Field(c => c.column_no_start);
            Field(c => c.column_no_stop);
            Field(c => c.line_no);
            Field(c => c.message);
            Field(c => c.severity);
        }
    }
}
