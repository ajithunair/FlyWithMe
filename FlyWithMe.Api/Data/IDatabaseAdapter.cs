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
        Task<FlightPlan?> GetFlightPlanByIdAsync(string flightPlanId);
        Task<TransactionResult> FileFlightPlanAsync(FlightPlan flightPlan);
        Task<TransactionResult> UpdateFlightPlanAsync(FlightPlan flightPlan, string flightPlanId);
        Task<bool> DeleteFlightPlanAsync(string flightPlanId);  
    }
}
