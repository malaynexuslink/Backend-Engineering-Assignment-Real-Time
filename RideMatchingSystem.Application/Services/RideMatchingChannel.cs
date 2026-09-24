using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace RideMatchingSystem.Application.Services
{
    public class RideMatchingChannel
    {
        private readonly Channel<Guid> _channel;

        public RideMatchingChannel()
        {
            var options = new BoundedChannelOptions(5000)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = false,
                SingleWriter = false
            };
            _channel = Channel.CreateBounded<Guid>(options);
        }

        public async ValueTask EnqueueAsync(Guid rideId, CancellationToken ct = default) => await _channel.Writer.WriteAsync(rideId, ct);

        public IAsyncEnumerable<Guid> ReadAllAsync(CancellationToken ct = default) => _channel.Reader.ReadAllAsync(ct);
    }
}
