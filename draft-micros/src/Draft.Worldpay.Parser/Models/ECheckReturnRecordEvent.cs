using System;
using System.Collections.Generic;

namespace Draft.Worldpay.Parser.Models;

public sealed class ECheckReturnRecordEvent
{
    public required string FileName { get; init; }

    public required int RowNumber { get; init; }

    public required IReadOnlyDictionary<string, string> Fields { get; init; }

    public required DateTimeOffset ProcessedAtUtc { get; init; }
}
