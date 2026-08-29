// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.EnumLibrary;
using Febris.ModelLibrary.Models.XApiModels;
using Febris.ModelLibrary.Models.XApiModels.ExtraModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Febris.SharedServices.XApi
{
    public class StatementFactory
    {
        public static XApiResultExtras FactorResultExtensionExtras(Result result)
        {
            try
            {
                XApiResultExtras extra = new XApiResultExtras();
                string[] extensionMapArray = result.Extensions?.ExtensionMap?.Split(',') ?? new string[] { };
                string[] notes = { };
                List<string> noteList = new List<string>();
                for (var i = 0; i < extensionMapArray.Length; i++)
                {
                    string[] extensionSingle = extensionMapArray[i].Split(':');
                    string key = extensionSingle[0] + ":" + extensionSingle[1];
                    ExtensionIRIOptions iri = ExtensionIRIResolver.GetVerbEnum(key);
                    switch (iri)
                    {
                        case ExtensionIRIOptions.RestartCounterIRI:
                            extra.RestartCount = Int32.Parse(extensionSingle[2]);
                            break;
                        case ExtensionIRIOptions.NotesIRI:
                            notes = extensionSingle[2].Split('|');
                            for (var j = 0; j < notes.Length; j++)
                            {
                                if (notes[j] != "|")
                                {
                                    string tempNote = notes[j];
                                    noteList.Add(tempNote);

                                }
                            }
                            extra.NotesList = noteList;
                            break;
                    }
                }
                extra.Result = result;
                extra.ResultUUID = result.UUID;

                return extra;
            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex);
            }
            return null;
        }        
    }
}
