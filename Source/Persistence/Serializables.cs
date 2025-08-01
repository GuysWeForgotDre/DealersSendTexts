using System;
using UnityEngine;

namespace DealersSendTexts
{
    [Serializable]
    public struct SerialVector3
    {
        public float x, y, z;

        public SerialVector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
        public SerialVector3(Vector3 v) { x = v.x; y = v.y; z = v.z; }

        public readonly Vector3 ToVector3() => new Vector3(x, y, z);
    }

    [Serializable]
    public struct FailureKey
    {
        public string Customer;
        public string Location;

        public FailureKey(string customer, string location)
        {
            Customer = customer;
            Location = location;
        }

        public override readonly bool Equals(object obj) => obj is FailureKey other && Customer == other.Customer && Location == other.Location;
        public override readonly int GetHashCode() => Customer.GetHashCode() ^ Location.GetHashCode();
    }
}