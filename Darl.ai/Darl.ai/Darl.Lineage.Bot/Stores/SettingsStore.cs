/// </summary>

﻿using DarlCommon;
using DarlLanguage.Processing;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Darl.Lineage.Bot.Stores
{
    public class SettingsStore : ILocalStore
    {
        public SettingsStore(Dictionary<string, string> _settings)
        {
            settings = _settings;
        }

        public Dictionary<string, string> settings { get; set; } = new Dictionary<string, string>();

        public Task<DarlResult> ReadAsync(List<string> address)
        {
            //built ins
            if (address[0].ToLower() == "time")
            {
                return Task.FromResult<DarlResult>(new DarlResult("", DateTime.UtcNow.ToShortTimeString(), DarlResult.DataType.textual));
            }
            if (address[0].ToLower() == "date")
            {
                return Task.FromResult<DarlResult>(new DarlResult("", DateTime.UtcNow.ToShortDateString(), DarlResult.DataType.textual));
            }
            if (settings.ContainsKey(address[0]))
            {
                var dv = JsonConvert.DeserializeObject<DarlVar>(settings[address[0]]);
                return Task.FromResult<DarlResult>(DarlVarExtensions.Convert(dv));
            }
            return Task.FromResult<DarlResult>(new DarlResult(0.0, true));
        }

        public Task WriteAsync(List<string> address, DarlResult value)
        {
            //built ins
            if (address[0].ToLower() == "time")
            {
                return Task.CompletedTask;
            }
            if (address[0].ToLower() == "date")
            {
                return Task.CompletedTask;
            }
            value.name = address[0];
            var val = JsonConvert.SerializeObject(DarlVarExtensions.Convert(value));
            if (settings.ContainsKey(address[0]))
            {
                settings[address[0]] = val;
            }
            else
            {
                settings.Add(address[0], val);
            }
            return Task.CompletedTask;
        }

        public Dictionary<string, string> ConvertSettings()
        {
            var simplified = new Dictionary<string, string>();
            foreach (var s in settings.Keys)
            {
                simplified.Add(s, JsonConvert.DeserializeObject<DarlVar>(settings[s]).Value);
            }
            return simplified;
        }
    }
}
