using System;
using System.Collections.Generic;
using System.Text;
using GraphQL;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class DarlSchema : Schema
    {
        public DarlSchema(DarlQuery query, DarlMutation mutation, IDependencyResolver resolver)
        {
            Query = query;
            Mutation = mutation;
//            Subscription = subscription;
            DependencyResolver = resolver;
        }
    }
}
