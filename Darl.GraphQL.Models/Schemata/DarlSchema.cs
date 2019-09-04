using System;
using System.Collections.Generic;
using System.Text;
using GraphQL;
using GraphQL.Types;
using GraphQL.Utilities;

namespace Darl.GraphQL.Models.Schemata
{
    public class DarlSchema : Schema
    {
        public DarlSchema(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            Query = serviceProvider.GetRequiredService<DarlQuery>();
            Mutation = serviceProvider.GetRequiredService<DarlMutation>();
        }
    }
}
