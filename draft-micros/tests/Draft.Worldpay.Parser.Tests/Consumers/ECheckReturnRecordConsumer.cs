using System;
using System.Collections.Generic;
using System.Globalization;
using Azure.Messaging.EventGrid;
using Draft.Worldpay.Parser.Models;

namespace Draft.Worldpay.Parser.Tests.Consumers;

public sealed class ECheckReturnRecordConsumer
{
    public ECheckReturnSqlRecord CreateSqlRecord(EventGridEvent eventGridEvent)
    {
        var payload = eventGridEvent.Data.ToObjectFromJson<ECheckReturnRecordEvent>()
            ?? throw new InvalidOperationException("Event payload could not be deserialized to ECheckReturnRecordEvent.");

        return MapToSqlRecord(payload);
    }

    private static ECheckReturnSqlRecord MapToSqlRecord(ECheckReturnRecordEvent payload)
    {
        var fields = payload.Fields;

        return new ECheckReturnSqlRecord(
            FileName: payload.FileName,
            RowNumber: payload.RowNumber,
            MerchantId: Get(fields, "Merchant ID"),
            DepositAccountNumber: Get(fields, "Deposit Account Number"),
            CustomerName: Get(fields, "Customer Name"),
            TransactionId: Get(fields, "Transaction ID"),
            TrackingNumber: Get(fields, "Tracking Number"),
            ReturnType: Get(fields, "Return Type"),
            ReturnCode: Get(fields, "Return Code"),
            ReturnDescription: Get(fields, "Return Description"),
            ReturnAmount: ParseDecimal(Get(fields, "Return Amount")),
            OriginalSettlementDate: ParseDate(Get(fields, "Original Settlement Date")),
            ProcessedAtUtc: payload.ProcessedAtUtc);
    }

    private static string Get(IReadOnlyDictionary<string, string> fields, string key)
    {
        return fields.TryGetValue(key, out var value) ? value : string.Empty;
    }

    private static decimal? ParseDecimal(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount)
            ? amount
            : null;
    }

    private static DateOnly? ParseDate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return DateOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date
            : null;
    }
}

public sealed record ECheckReturnSqlRecord(
    string FileName,
    int RowNumber,
    string MerchantId,
    string DepositAccountNumber,
    string CustomerName,
    string TransactionId,
    string TrackingNumber,
    string ReturnType,
    string ReturnCode,
    string ReturnDescription,
    decimal? ReturnAmount,
    DateOnly? OriginalSettlementDate,
    DateTimeOffset ProcessedAtUtc);
