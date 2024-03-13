using Chatify.Application.Common.Contracts;
using Chatify.Shared.Abstractions.Common;
using shortid;
using shortid.Configuration;
using SkbKontur.Cassandra.TimeBasedUuid;

namespace Chatify.Infrastructure.Common;

public sealed class TimeUuidGenerator : IGuidGenerator
{
    public Guid New() => TimeGuid.NowGuid().ToGuid();
    public string NewStringId()
        => ShortId.Generate(new GenerationOptions(useNumbers: true, useSpecialCharacters: false, 12));
}