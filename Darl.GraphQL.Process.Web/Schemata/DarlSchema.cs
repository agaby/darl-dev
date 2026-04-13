/// <summary>
/// DarlSchema.cs - Core module for the Darl.dev project.
/// </summary>

﻿using Darl.GraphQL.Process.Middleware;
using GraphQL.Types;
using GraphQL.Utilities;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Darl.GraphQL.Web.Models.Schemata
{
    public class DarlSchema : Schema
    {
        public DarlSchema(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            Query = serviceProvider.GetRequiredService<DarlQuery>();
            Mutation = serviceProvider.GetRequiredService<DarlMutation>();
            Subscription = serviceProvider.GetRequiredService<DarlSubscription>();
            Filter = serviceProvider.GetRequiredService<AdminFilter>();
        }
    }
}
