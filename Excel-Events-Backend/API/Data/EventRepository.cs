using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data.Interfaces;
using API.Dtos.Event;
using API.Extensions.CustomExceptions;
using API.Models;
using API.Services.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class EventRepository : IEventRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IEventService _service;
        public EventRepository(DataContext context, IMapper mapper, IEventService service)
        {
            _context = context;
            _mapper = mapper;
            _service = service;
        }

        public async Task<List<EventForListViewDto>> EventList()
        {
            var events = await _context.Events.Select(e => _mapper.Map<EventForListViewDto>(e)).ToListAsync();
            return events;
        }

        public async Task<List<EventForListViewDto>> FilteredList(int eventTypeId, int categoryId)
        {
            var filteredEvents = await _context.Events.Where(e => e.EventTypeId == eventTypeId && e.CategoryId == categoryId).ToListAsync();
            return filteredEvents.Select(e => _mapper.Map<EventForListViewDto>(e)).ToList();
        }

        public async Task<List<EventForListViewDto>> EventListOfType(int eventTypeId)
        {
            var filteredEvents = await _context.Events.Where(e => e.EventTypeId == eventTypeId).ToListAsync();
            return filteredEvents.Select(e => _mapper.Map<EventForListViewDto>(e)).ToList();
        }
        public async Task<List<EventForListViewDto>> EventListOfCategory(int categoryId)
        {
            var filteredEvents = await _context.Events.Where(e => e.CategoryId == categoryId).ToListAsync();
            return filteredEvents.Select(e => _mapper.Map<EventForListViewDto>(e)).ToList();
        }

        public async Task<EventForDetailedViewDto> GetEvent(int id)
        {
            var eventFromdb = await _context.Events.Include(e => e.Rounds)
                .Include(e => e.EventHead1).Include(e => e.EventHead2)
                .FirstOrDefaultAsync(e => e.Id == id);
            return _mapper.Map<EventForDetailedViewDto>(eventFromdb);
        }

        public async Task<Event> AddEvent(DataForAddingEventDto eventDataFromClient)
        {
            if (eventDataFromClient.Icon is null) throw new DataInvalidException("Please provide an icon for the event");
            var newEvent = _mapper.Map<Event>(eventDataFromClient);
            await _context.Events.AddAsync(newEvent);
            if (await _context.SaveChangesAsync() <= 0) throw new Exception("Problem Saving Changes");
            var imageUrl = await _service.UploadEventIcon(newEvent.Id.ToString(), eventDataFromClient.Icon);
            newEvent.Icon = imageUrl;
            if (await _context.SaveChangesAsync() > 0) return newEvent;
            throw new Exception("Trouble saving Image Name");
        }
        
        public async Task<Event> DeleteEvent(DataForDeletingEventDto dataForDeletingEvent)
        {
            var eventToDelete = await _context.Events.FindAsync(dataForDeletingEvent.Id);
            if (eventToDelete.Name != dataForDeletingEvent.Name) 
                throw new DataInvalidException("Id and Name does not match");
            if (eventToDelete.Icon != null) 
                await _service.DeleteEventIcon(dataForDeletingEvent.Id, eventToDelete.Icon);
            _context.Events.Remove(await _context.Events.FindAsync(dataForDeletingEvent.Id));
            if (await _context.SaveChangesAsync() > 0) return eventToDelete;
            throw new Exception("Error Deleting Event");
        }

        public async Task<Event> UpdateEvent(DataForUpdatingEventDto eventDataFromClient)
        {
            var eventFromDb = await _context.Events.FindAsync(eventDataFromClient.Id);
            var eventForUpdate = _mapper.Map<Event>(eventDataFromClient);
            if (eventDataFromClient.Icon != null)
            {
                await _service.DeleteEventIcon(eventFromDb.Id, eventFromDb.Icon);
                var imageUrl = await _service.UploadEventIcon(eventDataFromClient.Id.ToString(), eventDataFromClient.Icon);
                eventForUpdate.Icon = !eventFromDb.Icon.Equals(imageUrl) ? imageUrl : eventFromDb.Icon;
            }
            else
                eventForUpdate.Icon = eventFromDb.Icon;
            CopyChanges(eventForUpdate, eventFromDb);
            if (await _context.SaveChangesAsync() > 0) return eventFromDb;
            throw new Exception("Problem in updating event");
        }
        private static void CopyChanges(Event src, Event dest)
        {
            dest.Icon = src.Icon;
            dest.CategoryId = src.CategoryId;
            dest.EventTypeId = src.EventTypeId;
            dest.About = src.About;
            dest.Format = src.Format;
            dest.Rules = src.Rules;
            dest.Venue = src.Venue;
            dest.Datetime = src.Datetime;
            dest.EntryFee = src.EntryFee;
            dest.PrizeMoney = src.PrizeMoney;
            dest.EventHead1Id = src.EventHead1Id;
            dest.EventHead2Id = src.EventHead2Id;
            dest.IsTeam = src.IsTeam;
            dest.TeamSize = src.TeamSize;
            dest.EventStatusId = src.EventStatusId;
            dest.NumberOfRounds = src.NumberOfRounds;
            dest.CurrentRound = src.CurrentRound;
            dest.NeedRegistration = src.NeedRegistration;
        }
    }
}