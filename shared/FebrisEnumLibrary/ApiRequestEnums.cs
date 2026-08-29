// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.EnumLibrary
{
    public enum httpVerb
    {
        GET,
        POST,
        PUT,
        DELETE
    }
    public enum Authenticationtype
    {
        Basic,
        License,
        BearerToken,
        Cookie
    }
    public enum AuthenticaitonTechnique
    {
        None,
        Token,
        //RefreshToken,
        Cookie
    }

    //public enum ContentType
    //{
    //    application/json,
    //    RefreshToken,
    //    Cookie
    //}
    public enum ReturnDataType
    {
        none,
        stringReturn,
        bytearray
    }
}
