// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.EnumLibrary
{
    public enum XApiObjectType
    {
        Activity,
        Agent,
        Group,
        SubStatement,
        StatementReference
    }
    public class ObjectTypeResolver
    {
        public static string XApiObjectTypeResolver(XApiObjectType input)
        {
            string output = string.Empty;
            switch (input)
            {
                case XApiObjectType.Activity:
                    output = "Activity";
                    break;
                case XApiObjectType.Agent:
                    output = "Agent";
                    break;
                case XApiObjectType.Group:
                    output = "Group";
                    break;
                case XApiObjectType.SubStatement:
                    output = "SubStatement";
                    break;
                case XApiObjectType.StatementReference:
                    output = "StatementRef";
                    break;                
            }
            return output;
        }
    }
}
