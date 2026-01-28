using System.Globalization;
using System.Text.Json;
using PersonalFinance.Application.InvoiceImport.Responses;
using PersonalFinance.Shared.Results;

namespace PersonalFinance.Application.InvoiceImport.Services;

public static class InvoiceInterpretationParser
{
    public static Result<InvoiceInterpretationResponse> Parse(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Result<InvoiceInterpretationResponse>.Failure(Errors.ValidationError, "Empty JSON.");
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            var dataRoot = root;
            if (root.ValueKind == JsonValueKind.Array)
            {
                dataRoot = root.EnumerateArray().FirstOrDefault();
            }

            if (dataRoot.ValueKind != JsonValueKind.Object)
            {
                return Result<InvoiceInterpretationResponse>.Failure(Errors.ValidationError, "Invalid JSON root.");
            }

            var vendor = GetString(dataRoot, "vendorName");
            var dateText = GetString(dataRoot, "invoiceDate");
            var currency = GetString(dataRoot, "currency");
            var notes = GetString(dataRoot, "notes");
            var total = GetDecimal(dataRoot, "totalAmount");
            var confidence = GetDouble(dataRoot, "confidence");

            if (string.IsNullOrWhiteSpace(vendor))
            {
                return Result<InvoiceInterpretationResponse>.Failure(Errors.ValidationError, "Vendor is required.");
            }

            if (!DateTime.TryParse(dateText, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var date))
            {
                return Result<InvoiceInterpretationResponse>.Failure(Errors.ValidationError, "Invalid invoice date.");
            }

            if (total <= 0)
            {
                return Result<InvoiceInterpretationResponse>.Failure(Errors.ValidationError, "Total amount must be greater than zero.");
            }

            if (string.IsNullOrWhiteSpace(currency))
            {
                return Result<InvoiceInterpretationResponse>.Failure(Errors.ValidationError, "Currency is required.");
            }

            if (confidence < 0d || confidence > 1d)
            {
                return Result<InvoiceInterpretationResponse>.Failure(Errors.ValidationError, "Confidence must be between 0 and 1.");
            }

            var lineItems = GetLineItems(dataRoot);

            return Result<InvoiceInterpretationResponse>.Success(new InvoiceInterpretationResponse
            {
                VendorName = vendor,
                InvoiceDate = date,
                TotalAmount = total,
                Currency = currency,
                Notes = notes ?? string.Empty,
                Confidence = confidence,
                LineItems = lineItems
            });
        }
        catch (JsonException ex)
        {
            return Result<InvoiceInterpretationResponse>.Failure(Errors.ValidationError, ex.Message);
        }
    }

    private static string? GetString(JsonElement root, string property)
    {
        return root.TryGetProperty(property, out var element) && element.ValueKind == JsonValueKind.String
            ? element.GetString()
            : null;
    }

    private static decimal GetDecimal(JsonElement root, string property)
    {
        if (root.TryGetProperty(property, out var element))
        {
            if (element.ValueKind == JsonValueKind.Number && element.TryGetDecimal(out var result))
            {
                return result;
            }

            if (element.ValueKind == JsonValueKind.String && decimal.TryParse(element.GetString(), out var parsed))
            {
                return parsed;
            }
        }

        return 0m;
    }

    private static double GetDouble(JsonElement root, string property)
    {
        if (root.TryGetProperty(property, out var element))
        {
            if (element.ValueKind == JsonValueKind.Number && element.TryGetDouble(out var result))
            {
                return result;
            }

            if (element.ValueKind == JsonValueKind.String && double.TryParse(element.GetString(), out var parsed))
            {
                return parsed;
            }
        }

        return -1d;
    }

    private static IReadOnlyList<InvoiceLineItemResponse>? GetLineItems(JsonElement root)
    {
        if (!root.TryGetProperty("lineItems", out var element) || element.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        var list = new List<InvoiceLineItemResponse>();
        foreach (var item in element.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            var description = GetString(item, "description")
                ?? GetString(item, "name")
                ?? string.Empty;
            var quantity = GetNullableDecimal(item, "quantity");
            var unitPrice = GetNullableDecimal(item, "unitPrice");
            var totalPrice = GetNullableDecimal(item, "totalPrice")
                ?? GetNullableDecimal(item, "totalAmount");

            list.Add(new InvoiceLineItemResponse
            {
                Description = description,
                Quantity = quantity,
                UnitPrice = unitPrice,
                TotalPrice = totalPrice
            });
        }

        return list.Count == 0 ? null : list;
    }

    private static decimal? GetNullableDecimal(JsonElement root, string property)
    {
        if (root.TryGetProperty(property, out var element))
        {
            if (element.ValueKind == JsonValueKind.Null)
            {
                return null;
            }

            if (element.ValueKind == JsonValueKind.Number && element.TryGetDecimal(out var result))
            {
                return result;
            }

            if (element.ValueKind == JsonValueKind.String && decimal.TryParse(element.GetString(), out var parsed))
            {
                return parsed;
            }
        }

        return null;
    }
}
