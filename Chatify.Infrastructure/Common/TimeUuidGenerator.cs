using Chatify.Application.Common.Contracts;
using SkbKontur.Cassandra.TimeBasedUuid;

namespace Chatify.Infrastructure.Common;

internal sealed class TimeUuidGenerator : IGuidGenerator
{
    public Guid New() => TimeGuid.NowGuid().ToGuid();
}