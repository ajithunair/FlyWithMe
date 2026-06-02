using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FlyWithMe.Api.Data
{
    public class MongodbDatabase : IDatabaseAdapter
    {
        private readonly IMongoCollection<BsonDocument> _collection;

        public MongodbDatabase(IOptions<MongoDbSettings> mongoDbOptions)
        {
            var settings = mongoDbOptions.Value;

            if (string.IsNullOrWhiteSpace(settings.ConnectionString))
            {
                throw new InvalidOperationException("MongoDB connection string is not configured.");
            }

            if (string.IsNullOrWhiteSpace(settings.DatabaseName))
            {
                throw new InvalidOperationException("MongoDB database name is not configured.");
            }

            if (string.IsNullOrWhiteSpace(settings.CollectionName))
            {
                throw new InvalidOperationException("MongoDB collection name is not configured.");
            }

            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _collection = database.GetCollection<BsonDocument>(settings.CollectionName);
        }

        private static DateTime ReadDateTime(BsonValue value)
        {
            if (value == null || value.IsBsonNull)
            {
                return default;
            }

            if (value.BsonType == BsonType.DateTime)
            {
                return value.ToUniversalTime();
            }

            if (value.BsonType == BsonType.String &&
                DateTime.TryParse(
                    value.AsString,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal,
                    out var parsedDate))
            {
                return parsedDate;
            }

            throw new FormatException($"Unsupported date value '{value}' for flight plan.");
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
                DepartureTime = ReadDateTime(document["departure_time"]),
                ArrivalTime = ReadDateTime(document["estimated_arrival_time"]),
                DepartureAirport = document["departing_airport"].AsString,
                ArrivalAirport = document["arrival_airport"].AsString,
                Route = document["route"].AsString,
                Remarks = document["remarks"].AsString,
                NumberOnBoard = document["number_onboard"].AsInt32
            };
        }

        public async Task<List<FlightPlan>> GetFlightPlansAsync()
        {
            var documents = await _collection.Find(new BsonDocument()).ToListAsync();
            return documents
                .Select(ConvertBsonToFlightPlan)
                .OfType<FlightPlan>()
                .ToList();
        }

        public async Task<FlightPlan?> GetFlightPlanByIdAsync(string flightPlanId)
        {
            var document = await _collection.Find(new BsonDocument { ["flight_plan_id"] = flightPlanId }).FirstOrDefaultAsync();
            return ConvertBsonToFlightPlan(document);
        }

        public async Task<TransactionResult> FileFlightPlanAsync(FlightPlan flightPlan)
        {
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
                await _collection.InsertOneAsync(document);
                if (document["_id"].IsObjectId)
                {
                    return TransactionResult.Success;
                }
                return TransactionResult.BadRequest;
            }
            catch (System.Exception)
            {
                return TransactionResult.ServerError;
            }
        }

        public async Task<TransactionResult> UpdateFlightPlanAsync(FlightPlan flightPlan, string flightPlanId)
        {
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

            var result = await _collection.UpdateOneAsync(filter, update);
            if (result.MatchedCount == 0)
            {
                return TransactionResult.NotFound;
            }
            if (result.ModifiedCount == 0)
            {
                return TransactionResult.BadRequest;
            }
            return TransactionResult.Success;
        }

        public async Task<bool> DeleteFlightPlanAsync(string flightPlanId)
        {
            try
            {
                var result = await _collection.DeleteOneAsync(new BsonDocument { ["flight_plan_id"] = flightPlanId });
                return result.DeletedCount > 0;
            }
            catch (System.Exception)
            {
                return false;
            }
        }
    }
}
