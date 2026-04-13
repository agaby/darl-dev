/// <summary>
/// AlexaTypeType.cs - Core module for the Darl.dev project.
/// </summary>

﻿using GraphQL.Types;
using Type = Darl.GraphQL.Process.Models.Alexa.Type;

namespace Darl.GraphQL.Models.Schemata
{
    public class AlexaTypeType : ObjectGraphType<Type>
    {
        public AlexaTypeType()
        {
            Name = "type";
            Description = "Alexa type for skill definition";
            Field(c => c.name);
        }
    }
}
