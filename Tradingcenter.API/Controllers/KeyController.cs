﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Trainingcenter.Domain.DTOs.ExchangeKeyDTOs;
using Trainingcenter.Domain.Services.ExchangeKeyServices;

namespace Tradingcenter.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class KeyController : ControllerBase
    {
        private readonly IExchangeKeyServices _keyService;
        private HtmlEncoder _htmlEncoder;
        private JavaScriptEncoder _javaScriptEncoder;
        private UrlEncoder _urlEncoder;

        public KeyController(       IExchangeKeyServices keyService,
                                    HtmlEncoder htmlEncoder,
                                    JavaScriptEncoder javascriptEncoder,
                                    UrlEncoder urlEncoder)
        {
            _keyService = keyService;
            _htmlEncoder = htmlEncoder;
            _javaScriptEncoder = javascriptEncoder;
            _urlEncoder = urlEncoder;
        }

        // GET: api/Key
        [HttpGet]
        public async Task<IActionResult> Get(string name)
        {
            try {
                int userId = Int32.Parse(this.User.FindFirstValue(ClaimTypes.NameIdentifier));
                var keyList = await _keyService.GetExchangeKeys(name, userId);

                return StatusCode(200, keyList);
            }
            catch
            {
                return StatusCode(500, "Something went wrong while attempting to get keys");
            }
        }

        // POST: api/Key
        [HttpPost]
        public async Task<IActionResult> Post(ExchangeKeyToCreateDTO key)
        {
            try
            {
                key.Key = _javaScriptEncoder.Encode(key.Key);
                key.Secret = _javaScriptEncoder.Encode(key.Secret);

                if (key.Name != "BitMEX" && key.Name != "Binance")
                {
                    return StatusCode(400, "Name must have one of the following values: BitMEX, Binance");
                }
                int userId = Int32.Parse(this.User.FindFirstValue(ClaimTypes.NameIdentifier));

                if(await _keyService.KeyExists(key.Name, key.Key, userId))
                {
                    return StatusCode(400, "Duplicate key");
                }

                bool created = await _keyService.CreateExchangeKey(key, userId);
                if (created)
                {
                    return StatusCode(200);
                }
                return StatusCode(500, "Falied to save key");
            }
            catch
            {
                return StatusCode(500, "Something went wrong while attempting to create a key");
            }
        }
    }
}
