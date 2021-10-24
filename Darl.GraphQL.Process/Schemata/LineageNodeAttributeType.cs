using Darl.GraphQL.Models.Models;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class LineageNodeAttributeType : ObjectGraphType<LineageNodeAttributes>
    {
        public LineageNodeAttributeType()
        {
            Name = "lineageNodeAttributes";
            Description = "The attributes associated with a phrase.";
            Field(c => c.darl, true).Description("A DARL fragment executed when this phrase occurs.").DefaultValue("");
            Field(c => c.call, true).Description("A darl ruleset to call as a result of this phrase").DefaultValue("");
            Field(c => c.path, true).Description("The path to access this node");
            Field(c => c.present).DefaultValue(false).Description("true if this attribute and location in the tree exist");
            Field(c => c.randomResponse).Description("If true one of the random responses is chosen at random").DefaultValue(false);
            Field(c => c.response, true).Description("A single response returned when randomResponse is false").DefaultValue("");
            Field(c => c.definition, true).Description("A textual description of the attached node").DefaultValue("");
            Field<ListGraphType<StringGraphType>>("accessRoles", "predefined roles that can access this attribute.", resolve: context => context.Source.accessRoles);
            Field<ListGraphType<StringGraphType>>("implications", "Hints as to other relationships", resolve: context => context.Source.implications);
            Field<ListGraphType<StringGraphType>>("randomResponses", "A set of responses that will be selected at random if randomResponse is true", resolve: context => context.Source.randomResponses);
        }
    }
}
