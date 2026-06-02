using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FlyWithMe.Api.Data
{
    public class MongodbDatabase : IDatabaseAdapter
    {
        private IMongoCollection<BsonDocument> GetCollection(string databaseName, string collectionName)
        {
            var client = new MongoClient();
            var database = client.GetDatabase(databaseName);
            var collection = database.GetCollection<BsonDocument>(collectionName);
            return collection;
        }

        private FlightPlan? ConvertBsonToFlightPlan(BsonDocument document)
        {
            if (document == null)
            {
                return null;
            }
            return new FlightPlan
            {
                FlightPlanId = document["flight_plan_id"].AsString,
                AircraftIdentification = document["aircraft_identification"].AsString,
                AircraftType = document["aircraft_type"].AsString,
                Airspeed = document["airspeed"].AsInt32,
                Altitude = document["altitude"].AsInt32,
                FlightType = document["flight_type"].AsString,
                FuelHours = document["fuel_hours"].AsInt32,
                FuelMinutes = document["fuel_minutes"].AsInt32,
                DepartureTime = document["departure_time"].ToUniversalTime(),
                ArrivalTime = document["estimated_arrival_time"].ToUniversalTime(),
                DepartureAirport = document["departing_airport"].AsString,
                ArrivalAirport = document["arrival_airport"].AsString,
                Route = document["route"].AsString,
                Remarks = document["remarks"].AsString,
                NumberOnBoard = document["number_onboard"].AsInt32
            };
        }

        public async Task<List<FlightPlan>> GetFlightPlansAsync()
        {
            var collection = GetCollection("flywithme", "flightplans");
            var documents = await collection.Find(new BsonDocument()).ToListAsync();
            return documents.Select(ConvertBsonToFlightPlan).ToList();
        }

        public async Task<FlightPlan> GetFlightPlanByIdAsync(string flightPlanId)
        {
            var collection = GetCollection("flywithme", "flightplans");
            var document = await collection.Find(new BsonDocument { ["flight_plan_id"] = flightPlanId }).FirstOrDefaultAsync();
            var flightPlan = ConvertBsonToFlightPlan(document);
            if (flightPlan == null)
            {
                return new FlightPlan();
            }
            return flightPlan;
        }

        public async Task<bool> FileFlightPlanAsync(FlightPlan flightPlan)
        {
            var collection = GetCollection("flywithme", "flightplans");
            var document = new BsonDocument
            {
                ["flight_plan_id"] = Guid.NewGuid().ToString("N"),
                ["aircraft_identification"] = flightPlan.AircraftIdentification,
                ["aircraft_type"] = flightPlan.AircraftType,
                ["airspeed"] = flightPlan.Airspeed,
                ["altitude"] = flightPlan.Altitude,
                ["flight_type"] = flightPlan.FlightType,
                ["fuel_hours"] = flightPlan.FuelHours,
                ["fuel_minutes"] = flightPlan.FuelMinutes,
                ["departure_time"] = flightPlan.DepartureTime,
                ["estimated_arrival_time"] = flightPlan.ArrivalTime,
                ["departing_airport"] = flightPlan.DepartureAirport,
                ["arrival_airport"] = flightPlan.ArrivalAirport,
                ["route"] = flightPlan.Route,
                ["remarks"] = flightPlan.Remarks,
                ["number_onboard"] = flightPlan.NumberOnBoard
            };

            try
            {
                await collection.InsertOneAsync(document);
            }
            catch (System.Exception)
            {
                return false;
            }
            return true;
        }

        public async Task<bool> UpdateFlightPlanAsync(FlightPlan flightPlan, string flightPlanId)
        {
            var collection = GetCollection("flywithme", "flightplans");
            var filter = new BsonDocument { ["flight_plan_id"] = flightPlanId };
            var update = new BsonDocument
            {
                ["$set"] = new BsonDocument
                {
                    ["aircraft_identification"] = flightPlan.AircraftIdentification,
                    ["aircraft_type"] = flightPlan.AircraftType,
                    ["airspeed"] = flightPlan.Airspeed,
                    ["altitude"] = flightPlan.Altitude,
                    ["flight_type"] = flightPlan.FlightType,
                    ["fuel_hours"] = flightPlan.FuelHours,
                    ["fuel_minutes"] = flightPlan.FuelMinutes,
                    ["departure_time"] = flightPlan.DepartureTime,
                    ["estimated_arrival_time"] = flightPlan.ArrivalTime,
                    ["departing_airport"] = flightPlan.DepartureAirport,
                    ["arrival_airport"] = flightPlan.ArrivalAirport,
                    ["route"] = flightPlan.Route,
                    ["remarks"] = flightPlan.Remarks,
                    ["number_onboard"] = flightPlan.NumberOnBoard
                }
            };

            try
            {
                var result = await collection.UpdateOneAsync(filter, update);
                return result.ModifiedCount > 0;
            }
            catch (System.Exception)
            {
                return false;
            }   
        }

        public async Task<bool> DeleteFlightPlanAsync(string flightPlanId)
        {
            var collection = GetCollection("flywithme", "flightplans");
            try
            {
                var result = await collection.DeleteOneAsync(new BsonDocument { ["flight_plan_id"] = flightPlanId });
                return result.DeletedCount > 0;
            }
            catch (System.Exception)
            {
                return false;
            }
        }
    }
}