using Darl.GraphQL.Models.Models;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class ModelDetailsType : ObjectGraphType<ModelDetails>
    {
        public ModelDetailsType()
        {
            Name = "modelDetails";
            Description = "The extra details attached to a model such as the license.";
            Field(c => c.author, true).Description("The author of this model");
            Field(c => c.copyright, true).Description("The copyright of this model");
            Field(c => c.description, true).Description("The description of this model");
            Field(c => c.license, true).Description("The license of this model");
            Field(c => c.version, true).Description("The version of this model");
            Field(c => c.price, true).Description("The price per use");
            Field(c => c.currency, true).Description("The currency to charge in");
        }
    }
}
