using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.EventGrid;
using CsvHelper;
using CsvHelper.Configuration;
using Draft.Worldpay.Parser.Models;
using Draft.Worldpay.Parser.Options;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Draft.Worldpay.Functions;

public class ECheckReturnReportParser
{
    private readonly ILogger<ECheckReturnReportParser> _logger;
    private readonly EventGridPublisherClient _eventGridClient;
    private readonly EventGridPublisherOptions _publisherOptions;

    public ECheckReturnReportParser(
        ILogger<ECheckReturnReportParser> logger,
        EventGridPublisherClient eventGridClient,
        IOptions<EventGridPublisherOptions> publisherOptions)
    {
        _logger = logger;
        _eventGridClient = eventGridClient;
        _publisherOptions = publisherOptions.Value;
    }

    [Function(nameof(ECheckReturnReportParser))]
    public async Task Run(
        [BlobTrigger("worldpay-import-files/{name}", Connection = "2b7353_STORAGE")] Stream stream,
        string name,
        CancellationToken cancellationToken)
    {
        try
        {
            using var reader = new StreamReader(stream);
            var configuration = CreateCsvConfiguration(name);

            using var csv = new CsvReader(reader, configuration);

            if (!csv.Read())
            {
                _logger.LogWarning("Blob {BlobName} did not contain any records.", name);
                return;
            }

            csv.ReadHeader();
            var headers = csv.HeaderRecord;

            if (headers is null || headers.Length == 0)
            {
                _logger.LogWarning("Blob {BlobName} is missing a header row.", name);
                return;
            }

            var batch = new List<EventGridEvent>(_publisherOptions.BatchSize);
            var recordCount = 0;

            while (csv.Read())
            {
                cancellationToken.ThrowIfCancellationRequested();

                recordCount++;

                var fields = ExtractFields(headers, csv);

                _logger.LogInformation(
                    "Parsed eCheck return record {RowNumber} from {BlobName}: {@Fields}",
                    recordCount,
                    name,
                    fields);

                var processedAt = DateTimeOffset.UtcNow;
                var eventData = new ECheckReturnRecordEvent
                {
                    FileName = name,
                    RowNumber = recordCount,
                    Fields = fields,
                    ProcessedAtUtc = processedAt
                };

                var subject = BuildSubject(name, recordCount);

                var eventGridEvent = new EventGridEvent(
                    subject,
                    _publisherOptions.EventType,
                    _publisherOptions.DataVersion,
                    BinaryData.FromObjectAsJson(eventData))
                {
                    EventTime = processedAt
                };

                batch.Add(eventGridEvent);

                if (batch.Count >= _publisherOptions.BatchSize)
                {
                    await FlushAsync(batch, cancellationToken);
                }
            }

            if (recordCount == 0)
            {
                _logger.LogWarning("Blob {BlobName} did not contain any data rows.", name);
            }

            if (batch.Count > 0)
            {
                await FlushAsync(batch, cancellationToken);
            }

            _logger.LogInformation(
                "Published {Count} eCheck return records from {BlobName} to Event Grid.",
                recordCount,
                name);
        }
        catch (CsvHelperException csvException)
        {
            _logger.LogError(csvException, "Failed to parse eCheck return report {BlobName}.", name);
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error while processing eCheck return report {BlobName}.", name);
            throw;
        }
    }

    private CsvConfiguration CreateCsvConfiguration(string blobName)
    {
        var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            IgnoreBlankLines = true,
            TrimOptions = TrimOptions.Trim,
            MissingFieldFound = null,
            DetectColumnCountChanges = true
        };

        configuration.BadDataFound = args =>
        {
            _logger.LogWarning(
                "Bad data encountered while parsing eCheck return report {BlobName}: {RawRecord}",
                blobName,
                args.RawRecord);
        };

        configuration.PrepareHeaderForMatch = args => args.Header?.Trim();

        return configuration;
    }

    private static Dictionary<string, string> ExtractFields(string[] headers, CsvReader csv)
    {
        var fields = new Dictionary<string, string>(headers.Length, StringComparer.OrdinalIgnoreCase);

        foreach (var header in headers)
        {
            var value = csv.GetField(header) ?? string.Empty;
            fields[header] = value;
        }

        return fields;
    }

    private string BuildSubject(string blobName, int rowNumber)
    {
        var prefix = (_publisherOptions.SubjectPrefix ?? string.Empty).Trim().TrimEnd('/');
        var sanitizedName = blobName.Replace('\\', '/');
        var escapedName = Uri.EscapeDataString(sanitizedName);

        return string.IsNullOrEmpty(prefix)
            ? $"{escapedName}/{rowNumber}"
            : $"{prefix}/{escapedName}/{rowNumber}";
    }

    private async Task FlushAsync(List<EventGridEvent> events, CancellationToken cancellationToken)
    {
        if (events.Count == 0)
        {
            return;
        }

        await _eventGridClient.SendEventsAsync(events, cancellationToken);
        events.Clear();
    }
}
