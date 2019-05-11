using DarlCommon;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    public class StringDarlVarPair
    {
        public string Name { get; set; }

        public DarlVar Value { get; set; }
    }
}
