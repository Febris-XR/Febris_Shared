// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Febris.SharedServices.Launcher
{
    public class ConfigSettings
    {
        private ILogger _log;
        private IConfiguration _config;
        private readonly PCFileManager _fileManager;

        public ConfigSettings(ILogger log, IConfiguration config)
        {            
            _log = log;
            _config = config;
            _fileManager = new PCFileManager(_log, _config);
        }

        public ConfigSettings()
        {
            _fileManager = new PCFileManager(_log, _config);
        }

        public async Task<JObject> Get()
        {
            JObject configSettings = new JObject(); 
            try
            {
                string fileData= _fileManager.GetFileContent(PCFileSystem.ConfigLocation);
                configSettings = PCFileManager.ChangeToObject(fileData);
            }
            catch (System.Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex, "ConfigSettings.Get: suppressed exception");
            }
            return configSettings;
        }

        public async Task<bool> SetDomainPrefix(string input)
        {
            bool isSet = false;
            try
            {
                input = "{'domainprefix':" + "'"+input + "'}";
                JObject processedInput = PCFileManager.ChangeToObject(input);
                isSet = _fileManager.Set(processedInput, PCFileSystem.ConfigLocation);
            }
            catch (System.Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex, "ConfigSettings.SetDomainPrefix: suppressed exception");

            }
            return isSet;
        }

    }
}
