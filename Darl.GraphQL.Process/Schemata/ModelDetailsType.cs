using Darl.GraphQL.Models.Models;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class ModelDetailsType : ObjectGraphType<ModelDetails>
    {
        public ModelDetailsType()
        {
            Field(c => c.author, true).Description("The author of this model");
            Field(c => c.copyright, true).Description("The copyright of this model");
            Field(c => c.description, true).Description("The description of this model");
            Field(c => c.license, true).Description("The license of this model");
            Field(c => c.version, true).Description("The version of this model");
        }
    }
}
