using System.Collections.Generic;
using System.Threading.Tasks;
using API.Dtos.Event;
using API.Models;

namespace API.Data.Interfaces
{
    public interface IRegistrationRepository
    {
        Task<Registration> Register(int excelId, int eventId);
        Task<List<Registration>> ClearUserData(int excelId);
        Task<List<EventForListViewDto>> EventList(int excelId);
        Task<List<int>> UserList(int eventId);
        Task<bool> HasRegistered(int excelId, int eventId);
    }
}