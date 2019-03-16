using Darl.GraphQL.Models.Services;
using DarlCommon;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class LanguageFormatType : ObjectGraphType<LanguageFormat>
    {

        public LanguageFormatType(ILanguageTextService languagetexts)
        {
            Field(c => c.DefaultLanguage);
            Field<ListGraphType<LanguageTextType>>("languageList", resolve: context => languagetexts.GetLanguageTextsAsync());
        }
    }
}
