/// <summary>
/// QuestionSetInput.cs - Core module for the Darl.dev project.
/// </summary>

﻿using System.Collections.Generic;

namespace Darl.GraphQL.Models.Models
{
    public class QuestionSetInput
    {
        public List<QuestionInput> questions { get; set; }

        public string ieToken { get; set; }

    }
}
