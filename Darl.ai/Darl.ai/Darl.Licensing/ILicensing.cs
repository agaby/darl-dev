/// <summary>
/// </summary>

﻿using System;

namespace Darl.Licensing
{
    public interface ILicensing
    {
        string CreateKey(DateTime endDate, string company, string email);
        bool CheckKey(string key);

    }
}
