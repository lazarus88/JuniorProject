using MainMikitan.Application.Features.Restaurant.Registration.Commands;
using MainMikitan.Domain.Requests.Admin;
using MainMikitan.Domain.Requests.RestaurantRequests;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace MainMikitan.API.Controllers.Admin;

public class AdminRestaurantController(IMediator mediator) : MainController(mediator)
{
    public static string Message { get; set; }
    [HttpGet("set-print-input")]
    public void SetRequest(string message)
    {
        Message = message;
    }
    [HttpGet("get-print-input")]
    public string GetRequest()
    {
        return Message;
    }

}
