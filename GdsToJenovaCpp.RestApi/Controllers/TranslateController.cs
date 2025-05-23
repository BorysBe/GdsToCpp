﻿using GdsToJenovaCpp.Main.Builders;
using Microsoft.AspNetCore.Mvc;

namespace GdsToJenovaCpp.RestApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TranslateController : ControllerBase
    {
        public TranslateController()
        {
        }

        // POST api/<TranslateController>
        [HttpPost]
        public IActionResult Post([FromBody] string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return BadRequest("Input GDScript code is empty.");
            }

            string cppCode = new GdsToJenovaCppBuilder(value)
                .ReplaceComments()
                .AddGodotFunctionsUtilitiesHeader()
                .ReplaceMethods()
                .Build();

            return Ok(cppCode);
        }
    }
}
