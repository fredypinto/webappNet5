using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using Evertec.AppMovil.CoreWallet.Helpers.Globalization;
using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using Evertec.Automation.Net.CoreWalletClient.Models;
using Evertec.Automation.Net.CoreWalletClient.Services;
using System.IO;
using Evertec.Automation.Net.CoreWalletClient.Helpers;

namespace Evertec.Automation.Net.CoreWalletClient.Controllers
{
    [ApiController]
    [Route("api/CoreWallet")]
    public class CoreWalletController : ControllerBase
    {
        private IUserService _userService;

        public CoreWalletController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("authenticate")]
        public IActionResult Authenticate(AuthenticateRequest model)
        {
            var response = _userService.Authenticate(model);

            if (response == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            return Ok(response);
        }

        /// <summary>
        /// Metodo encargado de devolver el JWT cifrado para la autenticación de Flamingo
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        [Route("EncriptingAuth")]
        public string EncriptingAuth()
        {
            var request = this.Request;
            var header = request.Headers;

            var authData = new Dictionary<string, string>();
            authData.Add("UserName", header["UserName"]);
            authData.Add("Password", header["Password"]);
            authData.Add("AppKey", header["AppKey"]);
            authData.Add("InitialVector", header["InitialVector"]);
            authData.Add("AppSecret", header["AppSecret"]);

            string currentGUID = Guid.NewGuid().ToString();

            var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            double epoch = Math.Round((DateTime.UtcNow - unixEpoch).TotalSeconds);

            var jsonDevice = new Dictionary<string, object>()
            {
            { "deviceId", "B004FDF9-407B-4BC8-B37E-51035F7D6B11" },
            { "device", "device" },
            { "model", "iPhone" },
            { "manufacturer", "Apple" },
            { "name", "iPhone 11 Pro Max" },
            { "osVersion", "14.5" },
            { "platform", "iOS" },
            { "deviceType", "Virtual" },
            { "lastDateUpdated", "2021-09-10T10:29:01.202443-05:00" },
            { "applicationKey", "04026a1b-7430-4e77-b731-82546f4c4362" },
            { "TermsDate", "0001-01-01T00:00:00" }
            };
            var jsonUser = new Dictionary<string, object>()
            {
            { "userName", authData["UserName"] },
            { "password", authData["Password"] },
            { "device", jsonDevice }
            };
            var jsonString = new Dictionary<string, object>()
            {
            { "nonce", currentGUID },
            { "epoch", epoch },
            { "user", jsonUser }
            };

            string jsonPayLoad = TaskNewtonsoft.SerializarObjetoAJSON<Dictionary<string, object>>(jsonString);

            string payLoadEncryp = TasksDynamicKeys.EncryptDataToMemory(
                jsonPayLoad,
                authData["AppKey"],
                authData["InitialVector"]);

            jsonString = new Dictionary<string, object>() { { "Message", payLoadEncryp } };

            IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
            IJsonSerializer serializer = new JsonNetSerializer();
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);

            string jWT = encoder.Encode(jsonString, authData["AppSecret"]);

            return jWT;
        }

        /// <summary>
        /// Metodo encargado de devolver un valor para el campo PinBlockData, NewPinBlockData
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        [Route("GetWorkingPin")]
        public string GetWorkingPin()
        {
            var request = this.Request;
            var header = request.Headers;

            string workingPINBLOCK = string.Empty;
            byte[] decrypted_kwp;

            var keyExchange = new Dictionary<string, string>();
            keyExchange.Add("KeyKekClear", header["KeyKekClear"]);
            keyExchange.Add("KeyKwpClear", header["KeyKwpClear"]);
            keyExchange.Add("CardNumber", header["CardNumber"]);
            keyExchange.Add("Pin", header["Pin"]);

            Dictionary<string, object> dictionaryResponseKek = TasksDynamicKeys.GetKeyKEK(keyExchange["KeyKekClear"]);

            Dictionary<string, object> dictionaryResponseKwp = TasksDynamicKeys.GetJobKey(keyExchange["KeyKwpClear"], (byte[])dictionaryResponseKek["KEK"]);
            decrypted_kwp = (byte[])dictionaryResponseKwp["KWP"];

            workingPINBLOCK = TasksDynamicKeys.GetWorkingPINBLOCK(keyExchange["Pin"], keyExchange["CardNumber"], decrypted_kwp);

            return workingPINBLOCK;
        }
    }
}