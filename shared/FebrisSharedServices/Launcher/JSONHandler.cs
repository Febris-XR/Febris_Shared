// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
//using Serilog;

namespace Febris.SharedServices.Launcher
{
    public class JSONHandler
    {
        private ILogger _log;
        private readonly IConfiguration _config;


        public JSONHandler()
        {
        }

        public JSONHandler(ILogger log)
        {
            _log = log;
        }

        public JSONHandler(ILogger log, IConfiguration config)
        {
            _log = log;
            _config = config;
        }

        public string DeserializeToken(string stringJson)
        {
            try
            {
                var jObj = JsonConvert.DeserializeObject<JObject>(stringJson);                
                var token = jObj.Property("token").Value.ToString();
                return token.ToString();                
            }
            catch (Exception ex)
            {
                _log.LogError(ex.Message);
                return string.Empty;
            }
        }
    }
}
