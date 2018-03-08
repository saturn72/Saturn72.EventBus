using System.Net;
using System.Threading.Tasks;
using EventBus.Common;
using Microsoft.AspNetCore.Mvc;

namespace EventPublisherApp.Controllers
{
    [Route("api/[controller]")]
    public class EventController : Controller
    {
        #region Fields

        private readonly IEventBus _eventBus;
        #endregion
        
        #region ctor
        public EventController(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        #endregion

        [HttpPost("publish")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> Post([FromBody] string eventData)
        {
            await Task.Run(() => _eventBus.Publish(new SimpleIntegrationEvent { Content = eventData }));

            return Ok();
        }
    }
}
