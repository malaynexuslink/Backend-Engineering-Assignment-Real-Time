using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideMatchingSystem.Domain.Enums
{
    public enum RideStatus
    {
        Requested = 0,
        Searching = 1,
        Matched = 2,
        InProgress = 3,
        Completed = 4,
        Failed = 5
    }
}
