// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.ModelLibrary.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Febris.SharedServices
{
    public static class ManagementAlgorithms
    {
        /// <summary>
        /// Orders a chart by a generic label
        /// </summary>
        /// <param name="output"></param>
        public static async Task<GenericMixedChart> OrderChartLists(GenericMixedChart output)
        {
            try
            {
                foreach (var j in output.GenericChartList)
                {
                    j.GenericChartEntryList.OrderBy(x => x.Label);
                }
               
            }
            catch (Exception)
            {
                //throw;
            } 
            return output;
        }



    }


}
