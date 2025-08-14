using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DealersSendTexts
{
    public class Location
    {
        public SerialVector3 Position;
        public string Name;
        public string Region;
        public int    StartedCount;
        public int    SuccessCount;
        public int    FailureCount;

        public Dictionary<string, int> CustomerCount { get; } = new Dictionary<string, int>();
        public Dictionary<string, int> DealerCount   { get; } = new Dictionary<string, int>();

        public void RecordStarted() => StartedCount++;
        public void RecordSuccess() => SuccessCount++;
        public void RecordFailure() => FailureCount++;
        public void RecordCustomer(string name) => CustomerCount[name] = CustomerCount.TryGetValue(name, out int count) ? count + 1 : 1;
        public void RecordDealer(string name)   => DealerCount[name]   = DealerCount  .TryGetValue(name, out int count) ? count + 1 : 1;
        public float DistanceTo(Vector3 location) => Vector3.Distance(location, Position.ToVector3());
    }

    public static class LocationManager
    {
        private static readonly List<Location> Locations = new List<Location>();
        
        public static void Register(string name, Vector3 position)
        {
            if (!Locations.Any(l => l.Name == name))
                Locations.Add(new Location { Name = name, Position = new SerialVector3(position) });
        }

        public static void Add(Location location)
        {
            if (!Locations.Contains(location))
                Locations.Add(location);
        }

        public static Location GetNearest(Vector3 position) => Locations.OrderBy(l => l.DistanceTo(position)).FirstOrDefault();
        public static Location GetNearest(Vector3 position, out float distance)
        {
            Location nearest = GetNearest(position);
            distance = Vector3.Distance(nearest.Position.ToVector3(), position);
            return nearest;
        }

        public static string Describe(Vector3 position, string noDistPrefix = "to")
        {
            Location nearest = GetNearest(position, out float distance);
            return distance > 1.5f ? $"{distance:#.#} meters from {nearest.Name}" : $"{noDistPrefix} {nearest.Name}";
        }

        public static List<Location> All() => Locations;

        public static void ClearAll()
        {
            foreach (Location location in Locations)
            {
                location.CustomerCount.Clear();
                location.DealerCount.Clear();
            }

            Locations.Clear();
        }
    }
}