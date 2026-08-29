// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Febris.SharedServices
{
    public interface IJsonStringDictionaryBuilder
    {
        string UpdateJsonDictionaryStringBuilder(string inputDictionaryString, string language, string input);
        string NewJsonDictionaryStringBuilder(string language, string input);
        string ConvertStringToJsonStringArrayString(string input);
        string GetLanguageFromJsonDictionary(string input);

    }
    public class JsonStringDictionaryBuilder : IJsonStringDictionaryBuilder
    {
        public string UpdateJsonDictionaryStringBuilder(string inputDictionaryString, string language, string input)
        {
            try
            {
                string outputDictionaryString = string.Empty;
                Dictionary<string, string> dictionary = new Dictionary<string, string>();

                dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(inputDictionaryString);
                dictionary.Add(language, input);
                outputDictionaryString = JsonConvert.SerializeObject(dictionary);

                return outputDictionaryString;
            }
            catch (Exception)
            {

                throw;
            }

        }
        public string NewJsonDictionaryStringBuilder(string language, string input)
        {
            try
            {
                string outputDictionaryString = string.Empty;
                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                dictionary.Add(language, input);
                outputDictionaryString = JsonConvert.SerializeObject(dictionary);
                return outputDictionaryString;
            }
            catch (Exception)
            {

                throw;
            }

        }
        public string ConvertStringToJsonStringArrayString(string input)
        {
            try
            {
                //variables
                string output = string.Empty;
                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                //remove enter from string if present
                string removedEnterInput = input.Replace("\r\n", string.Empty);
                //break up string into a dicitonary
                dictionary = removedEnterInput.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(i => i.Split(':'))
                    .ToDictionary(i => i[0], i => i[1]);
                //turn dictionary into json string                
                output = JsonConvert.SerializeObject(dictionary);
                return output;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public string GetLanguageFromJsonDictionary(string input)
        {
            try
            {
                string output = string.Empty;
                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(input);
                output = dictionary.Keys.First();

                return output;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public string GetValueFromDictionaryWithKey(string input, string key)
        {
            try
            {
                string output = string.Empty;
                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(input);
                dictionary.TryGetValue(key, out output);

                return output;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
