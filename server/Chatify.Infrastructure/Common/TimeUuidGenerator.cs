using Chatify.Application.Common.Contracts;
using shortid;
using shortid.Configuration;
using SkbKontur.Cassandra.TimeBasedUuid;

namespace Chatify.Infrastructure.Common;

internal sealed class TimeUuidGenerator : IGuidGenerator
{
    public Guid New() => TimeGuid.NowGuid().ToGuid();
    public string NewStringId()
        => ShortId.Generate(new GenerationOptions(useNumbers: true, useSpecialCharacters: false, 12));
}