using Microsoft.SemanticKernel;
using System.ComponentModel;
using SemanticKernelTraining.Interface;

namespace SemanticKernelTraining.FlightFilterPlugin
{
    /// <summary>
    /// Plugin for flight search and booking operations with filter support
    /// </summary>
    public class FlightFilterPlugin
    {
        private readonly IFlightService _flightService;

        /// <summary>
        /// Initializes a new instance of FlightFilterPlugin
        /// </summary>
        /// <param name="flightService">Flight service instance</param>
        public FlightFilterPlugin(IFlightService flightService)
        {
            _flightService = flightService;
        }

        /// <summary>
        /// Gets flight details for a specific destination and date
        /// </summary>
        /// <param name="kernel">Semantic Kernel instance</param>
        /// <param name="city">Destination city</param>
        /// <param name="date">Travel date in yyyy-MM-dd format</param>
        /// <returns>List of available flights</returns>
        [KernelFunction("get_flight_details")]
        [Description("Get flights for destination and date")]
        [return : Description("List of flights")]
        public async Task<string> GetFlightDetails(Kernel kernel, [Description("City (e.g. Tokyo)")] string city, [Description("Date (yyyy-MM-dd)")] string date)
        {
            Console.WriteLine($"[Plugin] Received city parameter: '{city}'");

            if (!DateTime.TryParseExact(date, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var parsedDate))
            {
                throw new ArgumentException($"Invalid date format. Expected yyyy-MM-dd, received: {date}");
            }

            var flights = await _flightService.GetFlightDetails(city, parsedDate);

            if (!flights.Any()) return $"No flights found to {city} on {date}.";

            return string.Join("\n", flights.Select(f => $"Flight {f.Id}: {f.Name} - {f.Airline} - {f.Date.ToString("yyyy-MM-dd")} - {f.Price}"));
        }

        /// <summary>
        /// Books a flight using the flight ID
        /// </summary>
        /// <param name="kernel">Semantic Kernel instance</param>
        /// <param name="flightId">The unique identifier of the flight to book</param>
        /// <returns>Booking confirmation message</returns>
        [KernelFunction("book_flight")]
        [Description("Book a flight by ID")]
        [return : Description("Confirmation")]
        public async Task<string> BookFlight(Kernel kernel, [Description("Flight ID")] string flightId)
        {
            Console.WriteLine($"[Plugin] Received flightId parameter: '{flightId}'");

            var result = await _flightService.BookFlight(flightId);

            Console.WriteLine($"[Plugin] Result received from AI");
            return result ? "Flight booked successfully" : "Flight booking failed, Seat may not be available";
        }
    }

}
