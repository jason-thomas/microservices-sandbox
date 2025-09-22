using System;
using System.Collections.Generic;
using Azure.Messaging.EventGrid;
using Draft.Worldpay.Parser.Models;
using Draft.Worldpay.Parser.Tests.Consumers;
using Xunit;

namespace Draft.Worldpay.Parser.Tests;

public class ECheckReturnRecordConsumerTests
{
    [Fact]
    public void CreateSqlRecord_MapsPayloadFieldsToTypedRecord()
    {
        // Arrange
        var processedAt = new DateTimeOffset(2024, 9, 15, 12, 30, 0, TimeSpan.Zero);
        var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Record Type"] = "Detail",
            ["Report Date"] = "2024-09-15",
            ["Merchant ID"] = "123456789",
            ["Deposit Account Number"] = "****6789",
            ["Customer Name"] = "John Smith",
            ["Transaction ID"] = "TRX123456",
            ["Tracking Number"] = "TRK-98312",
            ["Return Type"] = "Administrative Return",
            ["Return Code"] = "R01",
            ["Return Description"] = "Insufficient Funds",
            ["Return Amount"] = "125.76",
            ["Original Settlement Date"] = "2024-09-12"
        };

        var payload = new ECheckReturnRecordEvent
        {
            FileName = "ECheckReturnReport_20240915.csv",
            RowNumber = 1,
            Fields = fields,
            ProcessedAtUtc = processedAt
        };

        var eventGridEvent = new EventGridEvent(
            subject: "/worldpay/echeck/returns/ECheckReturnReport_20240915.csv/1",
            eventType: "worldpay.echeck.returnrecord",
            dataVersion: "1.0",
            data: BinaryData.FromObjectAsJson(payload))
        {
            EventTime = processedAt
        };

        var consumer = new ECheckReturnRecordConsumer();

        // Act
        var record = consumer.CreateSqlRecord(eventGridEvent);

        // Assert
        var expected = new ECheckReturnSqlRecord(
            FileName: "ECheckReturnReport_20240915.csv",
            RowNumber: 1,
            MerchantId: "123456789",
            DepositAccountNumber: "****6789",
            CustomerName: "John Smith",
            TransactionId: "TRX123456",
            TrackingNumber: "TRK-98312",
            ReturnType: "Administrative Return",
            ReturnCode: "R01",
            ReturnDescription: "Insufficient Funds",
            ReturnAmount: 125.76m,
            OriginalSettlementDate: new DateOnly(2024, 9, 12),
            ProcessedAtUtc: processedAt);

        Assert.Equal(expected, record);
    }
}
