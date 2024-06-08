﻿using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MotorRental.Application;
using MotorRental.Entities;
using MotorRental.Infrastructure.Presentation.Helper;
using MotorRental.Infrastructure.Presentation.Models;
using MotorRental.Infrastructure.Presentation.Models.DTO;
using System.Net;

namespace MotorRental.Infrastructure.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MotorController : ControllerBase
    {
        private readonly IMotorService _motorService;
        private readonly IMapper _mapper;
        private ApiResponse _response;

        public MotorController(IMotorService motorService, IMapper mapper)
        {
            _motorService = motorService;
            _mapper = mapper;
            _response = new();
        }

        [HttpPost("AddMotor")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ApiResponse> AddMotorBike([FromForm] MotorCreateDTO request)
        {
            
            // convert DTO to Domain
            var model = _mapper.Map<Motorbike>(request);

            // call Service
            var resultDomain = await _motorService.Add(model);

            if(resultDomain == null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.Result = resultDomain;
                _response.ErrorMessages.Add("Địt mẹ mày, try agian");
            }
            else
            {
                // process file image
                    // save image to server
                var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";
                var stringUrl = request.Image.SaveImage(resultDomain.Id.ToString(), baseUrl);

                // update result with ImageURL
                resultDomain.MotorbikeAvatar = stringUrl;
                resultDomain = await _motorService.Update(resultDomain);

                // Convert Domain to DTO
                var response = _mapper.Map<MotorDTO>(resultDomain);
                _response.StatusCode = HttpStatusCode.Created;
                _response.Result = response;
            }
            
            // Return APi Response
            return _response;
        }
    }
}