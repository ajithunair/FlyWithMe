using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FlyWithMe.Api.Data;
using Microsoft.AspNetCore.Mvc;

namespace FlyWithMe.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class FlighPlanController : ControllerBase   
    {
        private readonly IDatabaseAdapter _database;
        public FlighPlanController(IDatabaseAdapter database)
        {
            _database = database;   
        }

        [HttpGet]
        public async Task<IActionResult> GetFlightPlans()
        {
            var flightPlans = await _database.GetFlightPlansAsync();
            return Ok(flightPlans);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFlightPlanById(string id)
        {
            var flightPlan = await _database.GetFlightPlanByIdAsync(id);
            if (flightPlan == null)
            {
                return NotFound();
            }

            return Ok(flightPlan);
        }

        [HttpPost("file")]
        public async Task<IActionResult> FileFlightPlan(Models.FlightPlan flightPlan)
        {
            var result = await _database.FileFlightPlanAsync(flightPlan);
            switch (result)
            {
                case TransactionResult.Success:
                    return Ok();
                case TransactionResult.BadRequest:
                    return BadRequest("Invalid flight plan data");
                case TransactionResult.ServerError:
                    return StatusCode(500, "Failed to file flight plan");
                default:
                    return StatusCode(500, "An unexpected error occurred");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFlightPlan(string id, Models.FlightPlan flightPlan)
        {
            var result = await _database.UpdateFlightPlanAsync(flightPlan, id);
            switch (result)
            {
                case TransactionResult.Success:
                    return Ok();
                case TransactionResult.NotFound:
                    return NotFound("Flight plan not found");
                case TransactionResult.BadRequest:
                    return BadRequest("Invalid flight plan data");
                case TransactionResult.ServerError:
                    return StatusCode(500, "Failed to update flight plan");
                default:
                    return StatusCode(500, "An unexpected error occurred");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFlightPlan(string id)
        {
            var result = await _database.DeleteFlightPlanAsync(id);
            if (result)
            {
                return Ok();
            }
            return StatusCode(500, "Failed to delete flight plan");
        }

        [HttpGet("airport/departure/{flightPlanId}")]
        public async Task<IActionResult> GetFlightPlansDepartureAirport(string flightPlanId)
        {
            var flightPlan = await _database.GetFlightPlanByIdAsync(flightPlanId);
            if (flightPlan == null)
            {
                return NotFound();
            }
            return Ok(flightPlan.DepartureAirport);
        }

        [HttpGet("route/{flightPlanId}")]
        public async Task<IActionResult> GetFlightPlansRoute(string flightPlanId)
        {
            var flightPlan = await _database.GetFlightPlanByIdAsync(flightPlanId);
            if (flightPlan == null)
            {
                return NotFound();
            }
            return Ok(flightPlan.Route);
        }

        [HttpGet("time/en-route/{flightPlanId}")]
        public async Task<IActionResult> GetFlightPlansTimeEnRoute(string flightPlanId)
        {
            var flightPlan = await _database.GetFlightPlanByIdAsync(flightPlanId);
            if (flightPlan == null)
            {
                return NotFound();
            }
            var timeEnRoute = flightPlan.ArrivalTime - flightPlan.DepartureTime;
            return Ok(timeEnRoute);
        }
    }
}
