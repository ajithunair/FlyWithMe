using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Models;

namespace FlyWithMe.Api.Data
{
    public interface IDatabaseAdapter
    {
        Task<List<FlightPlan>> GetFlightPlansAsync();
        Task<FlightPlan> GetFlightPlanByIdAsync(string flightPlanId);
        Task<bool> FileFlightPlanAsync(FlightPlan flightPlan);
        Task<bool> UpdateFlightPlanAsync(FlightPlan flightPlan, string flightPlanId);
        Task<bool> DeleteFlightPlanAsync(string flightPlanId);  
    }
}