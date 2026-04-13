/// <summary>
/// QuestionInput.cs - Core module for the Darl.dev project.
/// </summary>

﻿using DarlCommon;

namespace Darl.GraphQL.Models.Models
{
    public class QuestionInput
    {
        public string reference { get; set; }
        public string sResponse { get; set; }
        public double dResponse { get; set; }
        public QuestionProxy.QType qType { get; set; }
    }
}
