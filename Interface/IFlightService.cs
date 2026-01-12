using System.Collections.Generic;
using System.Linq;
using SemanticKernelTraining.Models;

namespace SemanticKernelTraining.Interface
{
    public interface IFlightService
    {
        Task<List<Flight>> GetFlightDetails(string destination, DateTime date);
        Task<bool> BookFlight(string flightId);
    }

    public class FlightService : IFlightService
    {
        private readonly List<Flight> _flights;

        public FlightService()
        {
            _flights = new List<Flight>
            {
                new Flight { Id = 1, Name = "Flight 1", Date = DateTime.Now.AddDays(1), Price = 100, Destination = "Tokyo", Airline = "Airline 1", IsBooked = false },
                new Flight { Id = 2, Name = "Flight 2", Date = DateTime.Now.AddDays(-2), Price = 200, Destination = "London", Airline = "Airline 2", IsBooked = false },
                new Flight { Id = 3, Name = "Flight 3", Date = DateTime.Now.AddDays(5), Price = 300, Destination = "Paris", Airline = "Airline 3", IsBooked = false },
                new Flight { Id = 4, Name = "Flight 4", Date = DateTime.Now.AddDays(10), Price = 400, Destination = "Tokyo", Airline = "Airline 4", IsBooked = false },
                new Flight { Id = 5, Name = "Flight 5", Date = DateTime.Now.AddDays(15), Price = 500, Destination = "Sydney", Airline = "Airline 5", IsBooked = false },
            };
        }

        public Task<List<Flight>> GetFlightDetails(string destination, DateTime date)
        {
            // Simple date match (ignoring time)
            return Task.FromResult(_flights
                .Where(f => f.Destination.Equals(destination, StringComparison.OrdinalIgnoreCase) 
                            && f.Date.Date == date.Date)
                .ToList());
        }

        public Task<bool> BookFlight(string flightId)
        {
            // Parse flightId to int since Flight.Id is int
            if (!int.TryParse(flightId, out int id))
            {
                return Task.FromResult(false);
            }

            var flight = _flights.FirstOrDefault(f => f.Id == id);
            if (flight == null || flight.IsBooked)
            {
                return Task.FromResult(false);
            }

            flight.IsBooked = true;
            return Task.FromResult(true);
        }
    }
}