using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using TestWork.Core.Interfaces.Subscription;
using TestWork.Core.Models;

namespace TestWork.Application.Services;

public partial class ApartmentPriceParser : IApartmentPriceParser
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ApartmentPriceParser(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<ParsedApartmentModel> GetApartmentAsync(string apartmentUrl)
    {
        string normalizedUrl = ApartmentUrlHelper.NormalizeAndValidate(apartmentUrl);
        HttpClient httpClient = _httpClientFactory.CreateClient("Prinzip");

        using HttpResponseMessage response = await httpClient.GetAsync(normalizedUrl);
        response.EnsureSuccessStatusCode();
        string html = await response.Content.ReadAsStringAsync();
        string finalUrl = response.RequestMessage?.RequestUri?
            .GetLeftPart(UriPartial.Path)
            .TrimEnd('/') ?? normalizedUrl;

        foreach (Match match in JsonLdScriptRegex().Matches(html))
        {
            string json = WebUtility.HtmlDecode(match.Groups["json"].Value);

            try
            {
                using JsonDocument document = JsonDocument.Parse(json);
                ParsedApartmentModel? apartment = FindProduct(document.RootElement, finalUrl);
                if (apartment is not null)
                {
                    return apartment;
                }
            }
            catch (JsonException)
            {
                
            }
        }

        throw new InvalidOperationException("Не удалось найти цену квартиры на странице.");
    }

    private static ParsedApartmentModel? FindProduct(JsonElement element, string expectedUrl)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            if (IsProduct(element)
                && TryGetPrice(element, out decimal price)
                && ProductMatchesUrl(element, expectedUrl))
            {
                string? name = element.TryGetProperty("name", out JsonElement nameElement)
                    ? nameElement.GetString()
                    : null;

                return new ParsedApartmentModel
                {
                    Price = price,
                    Name = name
                };
            }

            foreach (JsonProperty property in element.EnumerateObject())
            {
                ParsedApartmentModel? result = FindProduct(property.Value, expectedUrl);
                if (result is not null)
                {
                    return result;
                }
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement item in element.EnumerateArray())
            {
                ParsedApartmentModel? result = FindProduct(item, expectedUrl);
                if (result is not null)
                {
                    return result;
                }
            }
        }

        return null;
    }

    private static bool IsProduct(JsonElement element)
    {
        return element.TryGetProperty("@type", out JsonElement type)
            && type.ValueKind == JsonValueKind.String
            && type.GetString() == "Product";
    }

    private static bool ProductMatchesUrl(JsonElement product, string expectedUrl)
    {
        if (!product.TryGetProperty("url", out JsonElement urlElement)
            || urlElement.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        string? productUrl = urlElement.GetString()?.TrimEnd('/');
        return string.Equals(productUrl, expectedUrl, StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryGetPrice(JsonElement product, out decimal price)
    {
        price = 0;

        if (!product.TryGetProperty("offers", out JsonElement offers))
        {
            return false;
        }

        JsonElement offer = offers.ValueKind == JsonValueKind.Array
            ? offers.EnumerateArray().FirstOrDefault()
            : offers;

        if (offer.ValueKind != JsonValueKind.Object
            || !offer.TryGetProperty("price", out JsonElement priceElement))
        {
            return false;
        }

        return priceElement.ValueKind switch
        {
            JsonValueKind.Number => priceElement.TryGetDecimal(out price),
            JsonValueKind.String => decimal.TryParse(
                priceElement.GetString(),
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out price),
            _ => false
        };
    }

    [GeneratedRegex(
        "<script[^>]*type=[\\\"']application/ld\\+json[\\\"'][^>]*>(?<json>.*?)</script>",
        RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex JsonLdScriptRegex();
}
