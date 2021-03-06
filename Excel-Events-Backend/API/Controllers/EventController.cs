using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.Data.Interfaces;
using API.Dtos.Event;
using API.Models;
using API.Models.Custom;
using API.Services.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace API.Controllers
{
    [SwaggerTag("The routes under this controller are for perfoming CRUD optrations on Events table.")]
    [Route("/events")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly IEventRepository _repo;
        private readonly IMapper _mapper;
        private readonly IEventService _service;

        public EventController(IEventRepository repo, IMapper mapper, IEventService service)
        {
            _repo = repo;
            _mapper = mapper;
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<List<EventForListViewDto>>> Get()
        {
            List<EventForListViewDto> events = await _repo.EventList();
            return Ok(events);
        }

        [SwaggerOperation(Description = "This route is for fetching event by applying filter")]
        [HttpGet("event_type={eventType}&category={category}")]
        public async Task<ActionResult> FilteredList(string eventType, string category)
        {
            int eventTypeId, categoryId;
            eventTypeId = Array.IndexOf(Constants.EventType, eventType);
            categoryId = Array.IndexOf(Constants.Category, category);
            List<EventForListViewDto> filteredEvents = await _repo.FilteredList(eventTypeId, categoryId);
            return Ok(filteredEvents);
        }

        [SwaggerOperation(Description = "This route is for fetching event by type")]
        [HttpGet("type/{event_type}")]
        public async Task<ActionResult> GetEventsOfType(string event_type)
        {
            int eventTypeId = Array.IndexOf(Constants.EventType, event_type);
            List<EventForListViewDto> filteredEvents = await _repo.EventListOfType(eventTypeId);
            return Ok(filteredEvents);
        }

        [SwaggerOperation(Description = "This route is for fetching event by category")]
        [HttpGet("category/{category}")]
        public async Task<ActionResult> GetEventsOfCategory(string category)
        {
            int categoryId = Array.IndexOf(Constants.EventType, category);
            List<EventForListViewDto> filteredEvents = await _repo.EventListOfCategory(categoryId);
            return Ok(filteredEvents);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Event>> GetEvent(int id)
        {
            Event eventFromRepo = await _repo.GetEvent(id);
            return Ok(eventFromRepo);
        }

        [SwaggerOperation(Description = "This route is for Adding new Events")]
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult> AddEvent([FromForm] DataForAddingEventDto eventDataFromClient)
        {
            bool success = await _repo.AddEvent(eventDataFromClient);
            if (success)
                return Ok(new OkResponse { Response = "Success" });
            throw new Exception("Something went wrong");
        }

        [SwaggerOperation(Description = "This route is for Updating Events")]
        [Authorize(Roles = "Admin")]
        [HttpPut]
        public async Task<ActionResult> UpdateEvent([FromForm] DataForUpdatingEventDto eventDataFromClient)
        {
            bool success = await _repo.UpdateEvent(eventDataFromClient);
            if (success)
                return Ok(new OkResponse { Response = "Success" });
            throw new Exception("Something went wrong");
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete]    
        public async Task<ActionResult<OkResponse>> DeleteEvent(DataForDeletingEventDto dataForDeletingEvent)
        {
            bool success = await _repo.DeleteEvent(dataForDeletingEvent);
            if (success)
                return Ok(new OkResponse { Response = "Success" });
            throw new Exception("Error Deleting Event");
        }

    }
}