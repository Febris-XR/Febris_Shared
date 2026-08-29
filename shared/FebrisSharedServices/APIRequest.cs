// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.EnumLibrary;
using Febris.ModelLibrary.Models.TicketModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Febris.SharedServices
{
    #region Enum options
    //public enum httpVerb
    //{
    //    GET,
    //    POST,
    //    PUT
    //}
    //public enum Authenticationtype
    //{
    //    Basic,
    //    License,
    //    BearerToken,
    //    Cookie
    //}
    //public enum AuthenticaitonTechnique
    //{
    //    Token,
    //    Cookie
    //}
    #endregion

    public class APIRequest
    {
        #region variables
        private readonly ILogger _log;
        private readonly IConfiguration _config;

        //private readonly TokenHandler _tokenHandler;
        //private readonly Utilites.UniqueIdentifier _uniqueIdentifier;
        //private readonly DataProtection _dataProtection;
        //private readonly FebrisLocalLibrary.Service.ConfigSettings _configSettings;
        //private string _token;
        #endregion

        #region internal model
        public string endPoint { get; set; }
        public httpVerb httpMethod { get; set; }
        public Authenticationtype authType { get; set; }
        public AuthenticaitonTechnique authTech { get; set; }
        public string contentType { get; set; }
        public string token { get; set; }
        public string license { get; set; }
        public string postJSON { get; set; }
        public string snickerDoodle { get; set; }

        public LicenseAuthenticationRequest LicenseAuthenticationRequest { get; set; }
        #endregion


        #region constructors
        public APIRequest()
        {

        }
        public APIRequest(ILogger log)
        {
            _log = log;
            //_tokenHandler = new TokenHandler(_log);
            //_uniqueIdentifier = new Utilites.UniqueIdentifier(_log);
            //_dataProtection = new DataProtection(_log);
            //_configSettings = new Service.ConfigSettings(_log, _config);
        }
        public APIRequest(ILogger log, IConfiguration config)
        {
            _log = log;
            _config = config;
            //_tokenHandler = new TokenHandler(_log, _config);
            //_uniqueIdentifier = new Utilites.UniqueIdentifier(_log, _config);
            //_dataProtection = new DataProtection(_log, _config);
            //_configSettings = new Service.ConfigSettings(_log, _config);
        }

        //public APIRequest(ILogger log)
        //{
        //    _log = log;
        //    _tokenHandler = new TokenHandler(_log);
        //    _uniqueIdentifier = new Utilites.UniqueIdentifier(_log);
        //    _dataProtection = new DataProtection(_log);
        //    _configSettings = new Service.ConfigSettings(_log, _config);
        //}

        //public APIRequest(ILogger log, IConfiguration config)
        //{
        //    _log = log;
        //    _config = config;
        //    _tokenHandler = new TokenHandler(_log, _config);
        //    _uniqueIdentifier = new Utilites.UniqueIdentifier(_log, _config);
        //    _dataProtection = new DataProtection(_log, _config);
        //    _configSettings = new Service.ConfigSettings(_log, _config);
        //}
        #endregion


        #region generic request
        public async Task<string> MakeRequest()
        {
            //await AlterEndpoint();
            //###############################################################################################################################
            //Utilites.UniqueIdentifier.SetHardwareLicense();
            //set token


            #region autentication type setting
            if (authType == Authenticationtype.BearerToken)
            {
                if (token == string.Empty)
                {
                    return "No Token";
                }
            }
            else if (authType == Authenticationtype.License)
            {
                //get token                
                //token = _tokenHandler.GetStoredToken();
                if (license == string.Empty)
                {
                    return "No license present";
                }
            }
            else if (authType == Authenticationtype.Basic)
            {
                //get token                
                //token = _tokenHandler.GetStoredToken();
                //if (token == string.Empty)
                //{
                //    return "No Token";
                //}
            }
            else if (authType == Authenticationtype.Cookie)
            {
                //get token                
                //token = _tokenHandler.GetStoredToken();
                //if (token == string.Empty)
                //{
                //    return "No Token";
                //}
            }
            else
            {
                return "No authentication type set";
            }
            #endregion

            string strResponseValue = string.Empty;


            try
            {
                //HttpWebRequest request = (HttpWebRequest)WebRequest.Create(endPoint);//HttpWebRequest is being retired
                WebRequest request = WebRequest.Create(endPoint);
                request.Method = httpMethod.ToString();


                #region Authenticaiton and headers
                //############################################################################################################################
                //add headers
                //  1) used basic for token request
                //  2) use token for everything else
                //        Add authentication here. https://www.youtube.com/watch?v=XX5pn4pJ4b0&list=PLpSmZmoBaROZNRmR3BHPHY6cqNOLqLkKA&index=2
                // ############################################################################################################################
                string userName = string.Empty;
                string secret = string.Empty;
                bool exists = false;
                string authHeader = string.Empty;
                if (authType == Authenticationtype.BearerToken)
                {
                    request.Headers.Add("Authorization", "Bearer " + token);
                }
                else if (authType == Authenticationtype.License)
                {
                    authHeader = System.Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(license));
                    request.Headers.Add("Authorization", authType.ToString() + " " + authHeader);
                }
                else if (authType == Authenticationtype.Basic)
                {
                    authHeader = System.Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(userName + ":" + secret));
                    request.Headers.Add("Authorization", authType.ToString() + " " + authHeader);
                }
                else if (authType == Authenticationtype.Cookie)
                {
                    //CookieContainer cookieJar = new CookieContainer();
                    Cookie thinMint = new Cookie("Febris.AuthCookie", snickerDoodle);
                    //Febris.SharedServices.FebrisLog.Info("*****Gathered cookie to be attached" + thinMint + "*****");
                    request = await TryAddCookie(request, thinMint);
                    //cookieJar.SetCookies(thinMint);
                    //request = await TryAddCookie(request, cookieJar);
                    if (request == null)
                    {
                        return ("The cookies crumbled and could not be delivered");
                    }
                }
                else
                {
                    return "Header Building Failed";
                }


                #region not used right now but is  normaly used for hardware filter
                ///can use this for filtering out who is making the requests. 
                ///the id or uuid of the license

                //if (hardwareLicense != string.Empty && hardwareLicense != null && authType != Authenticationtype.Basic)
                //if (authType != Authenticationtype.Basic)
                //{
                //    //hardwareLicense = Utilites.UniqueIdentifier.GetStoredUniqueIdentifier();
                //    //hardwareLicense = _uniqueIdentifier.GetStoredUniqueIdentifier();
                //    ////request.Headers.Add("hardwareLicense", hardwareLicense);
                //    //request.Headers.Add("hardwareLicense", hardwareLicense);
                //}
                //############################################################################################################################
                #endregion
                #endregion

                #region Post Request
                if (request.Method == httpVerb.POST.ToString() && postJSON != string.Empty)
                {
                    request.ContentType = "application/json";
                    using (StreamWriter swJSONPayload = new StreamWriter(request.GetRequestStream()))
                    {
                        swJSONPayload.Write(postJSON);

                        swJSONPayload.Close();
                    }
                }
                #endregion

                #region Put Request
                if (request.Method == httpVerb.PUT.ToString() && postJSON != string.Empty)
                {
                    request.ContentType = "application/json";
                    using (StreamWriter swJSONPayload = new StreamWriter(request.GetRequestStream()))
                    {
                        swJSONPayload.Write(postJSON);

                        swJSONPayload.Close();
                    }
                }
                #endregion

                #region Get Request                
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        //Try to get new token if it fails initally. 
                        try
                        {
                            return response.StatusCode.ToString();
                        }
                        catch (Exception ex)
                        {
                            _log.LogError(ex.Message);
                            return "Could Not Find Anything Here";
                            throw;

                            //new ApplicationException("error code: " + response.StatusCode.ToString());
                        }
                    }

                    //stream response data
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        if (responseStream != null)
                        {
                            using (StreamReader reader = new StreamReader(responseStream))
                            {
                                strResponseValue = reader.ReadToEnd();
                            }
                        }
                    }
                }
                #endregion

            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex);
                //_log.LogWarning(ex.Message);
                if (ex.Message == "The remote server returned an error: (401) Unauthorized.")
                {
                    //token = _tokenHandler.GetToken();
                }
            }

            return strResponseValue;
        }


        //        public async Task<string> MakeRequest()
        //        {
        //            //await AlterEndpoint();
        //            //###############################################################################################################################
        //            //Utilites.UniqueIdentifier.SetHardwareLicense();
        //            //set token


        //            #region autentication type setting
        //            if (authType == Authenticationtype.BearerToken)
        //            {
        //                if (token == string.Empty)
        //                {
        //                    return "No Token";
        //                }
        //            }
        //            else if (authType == Authenticationtype.License)
        //            {
        //                //get token                
        //                //token = _tokenHandler.GetStoredToken();
        //                if (license == string.Empty)
        //                {
        //                    return "No license present";
        //                }
        //            }
        //            else if (authType == Authenticationtype.Basic)
        //            {
        //                //get token                
        //                //token = _tokenHandler.GetStoredToken();
        //                //if (token == string.Empty)
        //                //{
        //                //    return "No Token";
        //                //}
        //            }
        //            else if (authType == Authenticationtype.Cookie)
        //            {
        //                //get token                
        //                //token = _tokenHandler.GetStoredToken();
        //                //if (token == string.Empty)
        //                //{
        //                //    return "No Token";
        //                //}
        //            }
        //            else
        //            {
        //                return "No authentication type set";
        //            }
        //            #endregion

        //            string strResponseValue = string.Empty;


        //            try
        //            {
        //                //HttpWebRequest request = (HttpWebRequest)WebRequest.Create(endPoint);//HttpWebRequest is being retired
        //                WebRequest request = WebRequest.Create(endPoint); 
        //                request.Method = httpMethod.ToString();

        //#if (DEBUG)
        //                HttpClientHandler httpClientHandler = new HttpClientHandler();
        //                #endregion
        //#endif

        //                #region Authenticaiton and headers
        //                //############################################################################################################################
        //                //add headers
        //                //  1) used basic for token request
        //                //  2) use token for everything else
        //                //        Add authentication here. https://www.youtube.com/watch?v=XX5pn4pJ4b0&list=PLpSmZmoBaROZNRmR3BHPHY6cqNOLqLkKA&index=2
        //                // ############################################################################################################################
        //                string userName = string.Empty;
        //                string secret = string.Empty;
        //                bool exists = false;
        //                string authHeader = string.Empty;
        //                if (authType == Authenticationtype.BearerToken)
        //                {
        //                    request.Headers.Add("Authorization", "Bearer " + token);
        //                }
        //                else if (authType == Authenticationtype.License)
        //                {
        //                    authHeader = System.Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(license));
        //                    request.Headers.Add("Authorization", authType.ToString() + " " + authHeader);
        //                }
        //                else if (authType == Authenticationtype.Basic)
        //                {
        //                    authHeader = System.Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(userName + ":" + secret));
        //                    request.Headers.Add("Authorization", authType.ToString() + " " + authHeader);
        //                }
        //                else if (authType == Authenticationtype.Cookie)
        //                {
        //                    //CookieContainer cookieJar = new CookieContainer();
        //                    Cookie thinMint = new Cookie("Febris.AuthCookie", snickerDoodle);
        //                    request = await TryAddCookie(request, thinMint);
        //                    //cookieJar.SetCookies(thinMint);
        //                    //request = await TryAddCookie(request, cookieJar);
        //                    if (request==null)
        //                    {
        //                        return ("The cookies crumbled and could not be delivered");
        //                    }
        //                }
        //                else
        //                {
        //                    return "Header Building Failed";
        //                }


        //                #region not used right now but is  normaly used for hardware filter
        //                ///can use this for filtering out who is making the requests. 
        //                ///the id or uuid of the license

        //                //if (hardwareLicense != string.Empty && hardwareLicense != null && authType != Authenticationtype.Basic)
        //                //if (authType != Authenticationtype.Basic)
        //                //{
        //                //    //hardwareLicense = Utilites.UniqueIdentifier.GetStoredUniqueIdentifier();
        //                //    //hardwareLicense = _uniqueIdentifier.GetStoredUniqueIdentifier();
        //                //    ////request.Headers.Add("hardwareLicense", hardwareLicense);
        //                //    //request.Headers.Add("hardwareLicense", hardwareLicense);
        //                //}
        //                //############################################################################################################################
        //                #endregion
        //                #endregion

        //                #region Post Request
        //                if (request.Method == httpVerb.POST.ToString() && postJSON != string.Empty)
        //                {
        //                    request.ContentType = "application/json";
        //                    using (StreamWriter swJSONPayload = new StreamWriter(request.GetRequestStream()))
        //                    {
        //                        swJSONPayload.Write(postJSON);

        //                        swJSONPayload.Close();
        //                    }
        //                }
        //                #endregion

        //                #region Get Request                
        //                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
        //                {
        //                    if (response.StatusCode != HttpStatusCode.OK)
        //                    {
        //                        //Try to get new token if it fails initally. 
        //                        try
        //                        {
        //                            return response.StatusCode.ToString();
        //                        }
        //                        catch (Exception ex)
        //                        {
        //                            _log.LogError(ex.Message);
        //                            return "Could Not Find Anything Here";
        //                            throw;

        //                            //new ApplicationException("error code: " + response.StatusCode.ToString());
        //                        }
        //                    }

        //                    //stream response data
        //                    using (Stream responseStream = response.GetResponseStream())
        //                    {
        //                        if (responseStream != null)
        //                        {
        //                            using (StreamReader reader = new StreamReader(responseStream))
        //                            {
        //                                strResponseValue = reader.ReadToEnd();
        //                            }
        //                        }
        //                    }
        //                }
        //                #endregion

        //            }
        //            catch (Exception ex)
        //            {
        //                Febris.SharedServices.FebrisLog.Error(ex);
        //                //_log.LogWarning(ex.Message);
        //                if (ex.Message == "The remote server returned an error: (401) Unauthorized.")
        //                {
        //                    //token = _tokenHandler.GetToken();
        //                }
        //            }

        //            return strResponseValue;
        //        }
        public async Task<byte[]> MakeRequest(string input)
        {
            //await AlterEndpoint();
            //###############################################################################################################################
            //Utilites.UniqueIdentifier.SetHardwareLicense();
            //set token
            byte[] output = new byte[1];
            #region autentication type setting
            if (authType == Authenticationtype.BearerToken)
            {
                if (token == string.Empty)
                {
                    return output;


                }
            }
            else if (authType == Authenticationtype.License)
            {
                //get token                
                //token = _tokenHandler.GetStoredToken();
                if (license == string.Empty)
                {
                    return output;
                }
            }
            else if (authType == Authenticationtype.Basic)
            {
                //get token                
                //token = _tokenHandler.GetStoredToken();
                //if (token == string.Empty)
                //{
                //    return "No Token";
                //}
            }
            else if (authType == Authenticationtype.Cookie)
            {
                //get token                
                //token = _tokenHandler.GetStoredToken();
                //if (token == string.Empty)
                //{
                //    return "No Token";
                //}
            }
            else
            {
                return output;
            }
            #endregion

            try
            {
                //HttpWebRequest request = (HttpWebRequest)WebRequest.Create(endPoint);//HttpWebRequest is being retired
                WebRequest request = WebRequest.Create(endPoint);
                request.Method = httpMethod.ToString();


                #region Authenticaiton and headers
                //############################################################################################################################
                //add headers
                //  1) used basic for token request
                //  2) use token for everything else
                //        Add authentication here. https://www.youtube.com/watch?v=XX5pn4pJ4b0&list=PLpSmZmoBaROZNRmR3BHPHY6cqNOLqLkKA&index=2
                // ############################################################################################################################
                string userName = string.Empty;
                string secret = string.Empty;
                bool exists = false;
                string authHeader = string.Empty;
                if (authType == Authenticationtype.BearerToken)
                {
                    request.Headers.Add("Authorization", "Bearer " + token);
                }
                else if (authType == Authenticationtype.License)
                {
                    authHeader = System.Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(license));
                    request.Headers.Add("Authorization", authType.ToString() + " " + authHeader);
                }
                else if (authType == Authenticationtype.Basic)
                {
                    authHeader = System.Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(userName + ":" + secret));
                    request.Headers.Add("Authorization", authType.ToString() + " " + authHeader);
                }
                else if (authType == Authenticationtype.Cookie)
                {
                    //CookieContainer cookieJar = new CookieContainer();
                    Febris.SharedServices.FebrisLog.Info("*****Gathered cookie " + snickerDoodle + "*****");
                    Cookie thinMint = new Cookie("Febris.AuthCookie", snickerDoodle);
                    request = await TryAddCookie(request, thinMint);
                    //cookieJar.SetCookies(thinMint);
                    //request = await TryAddCookie(request, cookieJar);
                    if (request == null)
                    {
                        Febris.SharedServices.FebrisLog.Info("*****The cookies crumbled and could not be delivered*****");
                        return output;
                    }
                }
                else
                {
                    Febris.SharedServices.FebrisLog.Info("*****Header building failed*****");
                    return output;
                }


                #region not used right now but is  normaly used for hardware filter
                ///can use this for filtering out who is making the requests. 
                ///the id or uuid of the license

                //if (hardwareLicense != string.Empty && hardwareLicense != null && authType != Authenticationtype.Basic)
                //if (authType != Authenticationtype.Basic)
                //{
                //    //hardwareLicense = Utilites.UniqueIdentifier.GetStoredUniqueIdentifier();
                //    //hardwareLicense = _uniqueIdentifier.GetStoredUniqueIdentifier();
                //    ////request.Headers.Add("hardwareLicense", hardwareLicense);
                //    //request.Headers.Add("hardwareLicense", hardwareLicense);
                //}
                //############################################################################################################################
                #endregion
                #endregion

                #region Post Request
                if (request.Method == httpVerb.POST.ToString() && postJSON != string.Empty)
                {
                    request.ContentType = "application/json";
                    using (StreamWriter swJSONPayload = new StreamWriter(request.GetRequestStream()))
                    {
                        swJSONPayload.Write(postJSON);

                        swJSONPayload.Close();
                    }
                }
                #endregion

                #region Get Request                
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        //Try to get new token if it fails initally. 
                        try
                        {
                            return output;
                            //return response.StatusCode.ToString();
                        }
                        catch (Exception ex)
                        {
                            _log.LogError(ex.Message);
                            return output;
                            //return "Could Not Find Anything Here";
                            throw;

                            //new ApplicationException("error code: " + response.StatusCode.ToString());
                        }
                    }

                    //stream response data
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        if (responseStream != null)
                        {
                            using (MemoryStream streamToWriteTo = new MemoryStream())
                            {
                                await responseStream.CopyToAsync(streamToWriteTo);
                                output = streamToWriteTo.ToArray();
                            }
                        }
                    }
                }
                #endregion

            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex);
                //_log.LogWarning(ex.Message);
                if (ex.Message == "The remote server returned an error: (401) Unauthorized.")
                {
                    //token = _tokenHandler.GetToken();
                }
            }

            return output;
            //return (T)strResponseValue;
        }
        #endregion

        #region Cookie helpers
        public async Task<WebRequest> TryAddCookie(WebRequest webRequest, Cookie cookie)
        {
            try
            {
                HttpWebRequest httpRequest = webRequest as HttpWebRequest;
                if (httpRequest == null)
                {
                    return null;
                }

                if (httpRequest.CookieContainer == null)
                {
                    httpRequest.CookieContainer = new CookieContainer();
                }
                Uri site = new Uri(endPoint);
                httpRequest.CookieContainer.Add(site, cookie);
                return httpRequest;

            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex);
                throw;
            }
        }

        public async Task<WebRequest> TryAddCookie(WebRequest webRequest, CookieContainer cookieJar)
        {
            try
            {

                HttpWebRequest httpRequest = webRequest as HttpWebRequest;
                if (httpRequest == null)
                {
                    return null;
                }
                if (httpRequest.CookieContainer == null)
                {
                    httpRequest.CookieContainer = new CookieContainer();
                }

                httpRequest.CookieContainer = cookieJar;
                return httpRequest;

            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex);
                throw;
            }
        }
        #endregion

        #region Image request
        /// <summary>
        /// This really doesn't seem to be used anywhere.
        /// </summary>
        /// <param name="PhotoName"></param>
        /// <returns></returns>
        public async Task<string> MakeImageRequest(string PhotoName)
        {
            //await AlterEndpoint();
            string strResponseValue = string.Empty;
            HttpWebRequest jpgRequest = (HttpWebRequest)WebRequest.Create(endPoint + ".jpg");
            HttpWebRequest pngRequest = (HttpWebRequest)WebRequest.Create(endPoint + ".png");
            jpgRequest.Method = httpMethod.ToString();
            pngRequest.Method = httpMethod.ToString();
            //token = TokenHandler.GetStoredToken();
            //token = _tokenHandler.GetStoredToken();
            try
            {
                using (HttpWebResponse jpgResponse = (HttpWebResponse)jpgRequest.GetResponse())
                {
                    if (jpgResponse.StatusCode == HttpStatusCode.OK)
                    {
                        PhotoName = PhotoName + ".jpg";
                    }
                }
            }
            catch
            {
                using (HttpWebResponse pngResponse = (HttpWebResponse)pngRequest.GetResponse())
                {
                    if (pngResponse.StatusCode == HttpStatusCode.OK)
                    {
                        PhotoName = PhotoName + ".png";
                    }
                }
            }
            return strResponseValue;
        }
        #endregion

        #region video upload
        //        public bool VideoUpload(string FileName)
        //        {
        //            bool rslt = false;
        //            try
        //            {
        //                //#region await async
        //                ////make awaiter to essentually make a que so the http requests are not overwhelmed 
        //                //await Service.AsyncAwaiter.AwaitAsync(nameof(FebrisRestClient), async () =>
        //                //{
        //                //hardwareLicense = Utilites.UniqueIdentifier.GetHardwareLicense();
        //                //token = TokenHandler.GetStoredToken();
        //                hardwareLicense = _uniqueIdentifier.GetHardwareLicense();
        //                token = _tokenHandler.GetStoredToken();
        //                if (token == string.Empty)
        //                {
        //                    return false;
        //                }
        //                HttpClientHandler httpClientHandler = new HttpClientHandler();
        //#if (DEBUG)
        //                #endregion
        //#endif
        //                using (var client = new HttpClient(httpClientHandler))
        //                {
        //                    using (MultipartFormDataContent content = new MultipartFormDataContent())
        //                    {
        //                        var fileContent = new ByteArrayContent(System.IO.File.ReadAllBytes(FileName));
        //                        fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")//have to make sure this is form data. 
        //                        {
        //                            FileName = Path.GetFileName(FileName)
        //                        };
        //                        //content.Add(fileContent);
        //                        #region Authenticaiton and headers
        //                        //############################################################################################################################
        //                        //add headers  
        //                        //-----------Says I am miss using headers here. 
        //                        //############################################################################################################################
        //                        //string authHeader = string.Empty;
        //                        //fileContent.Headers.Add("Authorization", "Bearer " + StaticDetails.token);//authType.ToString() + " " + authHeader);
        //                        //fileContent.Headers.Add("hardwareLicense", StaticDetails.hardwareId);                    
        //                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        //                        client.DefaultRequestHeaders.Add("hardwareLicense", hardwareLicense);

        //                        //############################################################################################################################
        //                        #endregion


        //                        content.Add(fileContent);

        //                        var requestUri = SharedDetails.SharedDetails.VideoUploaderUrl;// + StaticDetails.UniqueId;
        //                        requestUri = AlterEndpoint(requestUri).Result;

        //                        try
        //                        {
        //                            using (HttpResponseMessage result = client.PostAsync(requestUri, content).Result)
        //                            {
        //                                if (result.StatusCode != HttpStatusCode.OK)
        //                                {
        //                                    //Try to get new token if it fails initally. 
        //                                    try
        //                                    {
        //                                        _tokenHandler.GetToken();
        //                                    }
        //                                    catch (Exception)
        //                                    {
        //                                        throw new ApplicationException("error code: " + result.StatusCode.ToString());
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    rslt = true;
        //                                }
        //                            }
        //                        }
        //                        catch (Exception ex)
        //                        {
        //                            _log.LogError(ex.Message);
        //                        }
        //                    }
        //                }
        //                //});
        //                //#endregion

        //            }
        //            catch (Exception ex)
        //            {
        //                _log.LogWarning(ex.Message);
        //                if (ex.Message == "The remote server returned an error: (401) Unauthorized.")
        //                {
        //                    token = _tokenHandler.GetToken();
        //                }
        //            }
        //            return rslt;
        //        }
        #endregion

        #region module downloader
        //        public async Task<bool> ModuleDownloader(string FileName)
        //        {

        //            bool success = false;
        //            //#region usinging async awaiter to throttle number of calls possible
        //            ////make awaiter to essentually make a que so the http requests are not overwhelmed 
        //            //await Service.AsyncAwaiter.AwaitAsync(nameof(FebrisRestClient), async () =>
        //            //{
        //            //token = TokenHandler.GetStoredToken();
        //            //hardwareLicense = Utilites.UniqueIdentifier.GetHardwareLicense();
        //            //token = _tokenHandler.GetStoredToken();
        //            //hardwareLicense = _uniqueIdentifier.GetHardwareLicense();
        //            try
        //            {
        //                //string APIurl = StaticDetails.APIDownloaderPath + FileName;
        //                //string APIurl = FebrisLocalLibrary.SharedDetails.SharedDetails.ModuleDownloaderUrl + FileName;
        //                //APIurl = AlterEndpoint(APIurl).Result;
        //                HttpClientHandler httpClientHandler = new HttpClientHandler();
        //#if (DEBUG)
        //                #endregion
        //#endif
        //                using (HttpClient client = new HttpClient(httpClientHandler))
        //                {
        //                    try
        //                    {
        //                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        //                        client.DefaultRequestHeaders.Add("hardwareLicense", hardwareLicense);
        //                        using (HttpResponseMessage response = await client.GetAsync(APIurl, HttpCompletionOption.ResponseHeadersRead))
        //                        {
        //                            if (response.StatusCode != HttpStatusCode.OK)
        //                            {
        //                                try
        //                                {
        //                                    success = false;
        //                                    _tokenHandler.GetToken();
        //                                    //Communication.TokenHandler.GetToken();
        //                                }
        //                                catch (Exception ex)
        //                                {
        //                                    _log.LogInformation(ex.Message);
        //                                    throw;// new ApplicationException("error code: " + response.StatusCode.ToString());
        //                                }
        //                            }

        //                            using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync())
        //                            {
        //                                string fileToWriteTo = Path.Combine(FileSystem.FileSystem.ZippedModulePath, FileName);

        //                                using (Stream streamToWriteTo = File.Open(fileToWriteTo, FileMode.Create))
        //                                {
        //                                    #region test 3
        //                                    Process process = Service.ProgressBarService.StartProgressBar(FileName, StatusType.Downloading);
        //                                    await streamToReadFrom.CopyToAsync(streamToWriteTo);
        //                                    Service.ProgressBarService.StopProgressBar(process);
        //                                    #endregion
        //                                    success = true;
        //                                }
        //                            }
        //                        }

        //                    }
        //                    catch (Exception e)
        //                    {
        //                        _log.LogError(e.Message);
        //                    }
        //                }
        //                //await FileManager.FileUnzipper(newFileNameandPath);                
        //            }
        //            catch (Exception e)
        //            {
        //                _log.LogWarning(e.Message);
        //            }

        //            //});
        //            //#endregion
        //            return success;
        //        }

        #endregion

        #region not used
        #region Image request
        /// <summary>
        /// This really doesn't seem to be used anywhere.
        /// </summary>
        /// <param name="PhotoName"></param>
        /// <returns></returns>
        //        public async Task<string> MakeImageRequest(string PhotoName)
        //        {
        //            await AlterEndpoint();
        //            string strResponseValue = string.Empty;
        //            HttpWebRequest jpgRequest = (HttpWebRequest)WebRequest.Create(endPoint + ".jpg");
        //            HttpWebRequest pngRequest = (HttpWebRequest)WebRequest.Create(endPoint + ".png");
        //            jpgRequest.Method = httpMethod.ToString();
        //            pngRequest.Method = httpMethod.ToString();
        //            //token = TokenHandler.GetStoredToken();
        //            token = _tokenHandler.GetStoredToken();
        //#if (DEBUG)
        //            HttpClientHandler httpClientHandler = new HttpClientHandler();
        //            #endregion
        //#endif
        //            try
        //            {
        //                using (HttpWebResponse jpgResponse = (HttpWebResponse)jpgRequest.GetResponse())
        //                {
        //                    if (jpgResponse.StatusCode == HttpStatusCode.OK)
        //                    {
        //                        PhotoName = PhotoName + ".jpg";
        //                    }
        //                }
        //            }
        //            catch
        //            {
        //                using (HttpWebResponse pngResponse = (HttpWebResponse)pngRequest.GetResponse())
        //                {
        //                    if (pngResponse.StatusCode == HttpStatusCode.OK)
        //                    {
        //                        PhotoName = PhotoName + ".png";
        //                    }
        //                }
        //            }
        //            return strResponseValue;
        //        }
        #endregion

        #region video upload
        //        public bool VideoUpload(string FileName)
        //        {
        //            bool rslt = false;
        //            try
        //            {
        //                //#region await async
        //                ////make awaiter to essentually make a que so the http requests are not overwhelmed 
        //                //await Service.AsyncAwaiter.AwaitAsync(nameof(FebrisRestClient), async () =>
        //                //{
        //                //hardwareLicense = Utilites.UniqueIdentifier.GetHardwareLicense();
        //                //token = TokenHandler.GetStoredToken();
        //                hardwareLicense = _uniqueIdentifier.GetHardwareLicense();
        //                token = _tokenHandler.GetStoredToken();
        //                if (token == string.Empty)
        //                {
        //                    return false;
        //                }
        //                HttpClientHandler httpClientHandler = new HttpClientHandler();
        //#if (DEBUG)
        //                #endregion
        //#endif
        //                using (var client = new HttpClient(httpClientHandler))
        //                {
        //                    using (MultipartFormDataContent content = new MultipartFormDataContent())
        //                    {
        //                        var fileContent = new ByteArrayContent(System.IO.File.ReadAllBytes(FileName));
        //                        fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")//have to make sure this is form data. 
        //                        {
        //                            FileName = Path.GetFileName(FileName)
        //                        };
        //                        //content.Add(fileContent);
        //                        #region Authenticaiton and headers
        //                        //############################################################################################################################
        //                        //add headers  
        //                        //-----------Says I am miss using headers here. 
        //                        //############################################################################################################################
        //                        //string authHeader = string.Empty;
        //                        //fileContent.Headers.Add("Authorization", "Bearer " + StaticDetails.token);//authType.ToString() + " " + authHeader);
        //                        //fileContent.Headers.Add("hardwareLicense", StaticDetails.hardwareId);                    
        //                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        //                        client.DefaultRequestHeaders.Add("hardwareLicense", hardwareLicense);

        //                        //############################################################################################################################
        //                        #endregion


        //                        content.Add(fileContent);

        //                        var requestUri = SharedDetails.SharedDetails.VideoUploaderUrl;// + StaticDetails.UniqueId;
        //                        requestUri = AlterEndpoint(requestUri).Result;

        //                        try
        //                        {
        //                            using (HttpResponseMessage result = client.PostAsync(requestUri, content).Result)
        //                            {
        //                                if (result.StatusCode != HttpStatusCode.OK)
        //                                {
        //                                    //Try to get new token if it fails initally. 
        //                                    try
        //                                    {
        //                                        _tokenHandler.GetToken();
        //                                    }
        //                                    catch (Exception)
        //                                    {
        //                                        throw new ApplicationException("error code: " + result.StatusCode.ToString());
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    rslt = true;
        //                                }
        //                            }
        //                        }
        //                        catch (Exception ex)
        //                        {
        //                            _log.LogError(ex.Message);
        //                        }
        //                    }
        //                }
        //                //});
        //                //#endregion

        //            }
        //            catch (Exception ex)
        //            {
        //                _log.LogWarning(ex.Message);
        //                if (ex.Message == "The remote server returned an error: (401) Unauthorized.")
        //                {
        //                    token = _tokenHandler.GetToken();
        //                }
        //            }
        //            return rslt;
        //        }
        #endregion

        #region module downloader
        //        public async Task<bool> ModuleDownloader(string FileName)
        //        {

        //            bool success = false;
        //            //#region usinging async awaiter to throttle number of calls possible
        //            ////make awaiter to essentually make a que so the http requests are not overwhelmed 
        //            //await Service.AsyncAwaiter.AwaitAsync(nameof(FebrisRestClient), async () =>
        //            //{
        //            //token = TokenHandler.GetStoredToken();
        //            //hardwareLicense = Utilites.UniqueIdentifier.GetHardwareLicense();
        //            token = _tokenHandler.GetStoredToken();
        //            hardwareLicense = _uniqueIdentifier.GetHardwareLicense();
        //            try
        //            {
        //                //string APIurl = StaticDetails.APIDownloaderPath + FileName;
        //                string APIurl = FebrisLocalLibrary.SharedDetails.SharedDetails.ModuleDownloaderUrl + FileName;
        //                APIurl = AlterEndpoint(APIurl).Result;
        //                HttpClientHandler httpClientHandler = new HttpClientHandler();
        //#if (DEBUG)
        //                #endregion
        //#endif
        //                using (HttpClient client = new HttpClient(httpClientHandler))
        //                {
        //                    try
        //                    {
        //                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        //                        client.DefaultRequestHeaders.Add("hardwareLicense", hardwareLicense);
        //                        using (HttpResponseMessage response = await client.GetAsync(APIurl, HttpCompletionOption.ResponseHeadersRead))
        //                        {
        //                            if (response.StatusCode != HttpStatusCode.OK)
        //                            {
        //                                try
        //                                {
        //                                    success = false;
        //                                    _tokenHandler.GetToken();
        //                                    //Communication.TokenHandler.GetToken();
        //                                }
        //                                catch (Exception ex)
        //                                {
        //                                    _log.LogInformation(ex.Message);
        //                                    throw;// new ApplicationException("error code: " + response.StatusCode.ToString());
        //                                }
        //                            }

        //                            using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync())
        //                            {
        //                                string fileToWriteTo = Path.Combine(FileSystem.FileSystem.ZippedModulePath, FileName);

        //                                using (Stream streamToWriteTo = File.Open(fileToWriteTo, FileMode.Create))
        //                                {
        //                                    #region test 3                                    
        //                                    await streamToReadFrom.CopyToAsync(streamToWriteTo);
        //                                    #endregion
        //                                    success = true;
        //                                }
        //                            }
        //                        }

        //                    }
        //                    catch (Exception e)
        //                    {
        //                        _log.LogError(e.Message);
        //                    }
        //                }
        //                //await FileManager.FileUnzipper(newFileNameandPath);                
        //            }
        //            catch (Exception e)
        //            {
        //                _log.LogWarning(e.Message);
        //            }

        //            //});
        //            //#endregion
        //            return success;
        //        }

        #endregion

        #region check internet connection
        //public static bool IsConnnectedToInternet()
        //{
        //    try
        //    {
        //        Ping myPing = new Ping();
        //        string host = "google.com";
        //        byte[] buffer = new byte[32];
        //        int timeout = 1000;
        //        PingOptions pingOptions = new PingOptions();
        //        PingReply reply = myPing.Send(host, timeout, buffer, pingOptions);
        //        return (reply.Status == IPStatus.Success);
        //    }
        //    catch
        //    {
        //        return false;
        //    }

        //}

        #endregion

        #region endpoint modification
        //        private async Task AlterEndpoint()
        //        {
        //            JObject prefix = await _configSettings.Get();
        //#if (DEBUG)
        //            endPoint = SharedDetails.SharedDetails.UrlStart + endPoint;
        //#elif(STAGING)
        //            endPoint = SharedDetails.SharedDetails.UrlStart + endPoint;
        //#else
        //            endPoint = SharedDetails.SharedDetails.UrlStart + prefix["domainprefix"].ToString() + "." + endPoint;        
        //#endif
        //        }


        //        private async Task<string> AlterEndpoint(string input)
        //        {
        //            string output = string.Empty;
        //            JObject prefix = await _configSettings.Get();
        //#if (DEBUG)
        //            output = SharedDetails.SharedDetails.UrlStart + input;
        //#elif (STAGING)
        //            output = SharedDetails.SharedDetails.UrlStart + input;
        //#else
        //            output = SharedDetails.SharedDetails.UrlStart + prefix["domainprefix"].ToString() + "." + input;
        //#endif
        //            return output;
        //        }
        #endregion
        #endregion

    }
}
