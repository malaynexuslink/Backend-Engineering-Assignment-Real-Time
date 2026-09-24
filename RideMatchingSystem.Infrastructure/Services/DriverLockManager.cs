using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideMatchingSystem.Infrastructure.Services
{
    public class DriverLockManager
    {
        private static readonly ConcurrentDictionary<Guid, SemaphoreSlim> _locks = new();
        public SemaphoreSlim GetLock(Guid driverId) => _locks.GetOrAdd(driverId, _ => new SemaphoreSlim(1, 1));
    }
}
