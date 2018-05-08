using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Features.Communication
{
    [Route("api/[controller]")]
    public class CommunicationsController : Controller
    {
        private readonly IMediator mediator;

        public CommunicationsController(IMediator mediator)
        {
            this.mediator = mediator;
        }
        
        [HttpPost]
        public async Task<Create.Response> PostAsync([FromBody] Create.Command command)
        {
            return await mediator.Send(command);
        }

      
    }
}
