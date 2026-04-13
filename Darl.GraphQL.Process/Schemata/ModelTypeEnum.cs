/// <summary>
/// ModelTypeEnum.cs - Core module for the Darl.dev project.
/// </summary>

﻿using Darl.GraphQL.Models.Models;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class ModelTypeEnum : EnumerationGraphType<ModelType>
    {
        public ModelTypeEnum()
        {
            Name = "modelType";
            Description = "The type of model being edited";
        }
    }
}
