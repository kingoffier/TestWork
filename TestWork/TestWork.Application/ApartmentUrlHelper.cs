namespace TestWork.Application;

public static class ApartmentUrlHelper
{
    public static string NormalizeAndValidate(string apartmentUrl)
    {
        if (!Uri.TryCreate(apartmentUrl, UriKind.Absolute, out Uri? uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            || (uri.Host != "prinzip.su" && uri.Host != "www.prinzip.su"))
        {
            throw new ArgumentException("Нужна корректная ссылка на квартиру с сайта prinzip.su.");
        }

        if (!uri.AbsolutePath.StartsWith("/flats/", StringComparison.OrdinalIgnoreCase)
            && !uri.AbsolutePath.StartsWith("/apartments/", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Ссылка должна вести на страницу квартиры на prinzip.su.");
        }

        UriBuilder builder = new(uri)
        {
            Scheme = Uri.UriSchemeHttps,
            Host = "prinzip.su",
            Port = -1,
            Query = string.Empty,
            Fragment = string.Empty,
            Path = uri.AbsolutePath.TrimEnd('/')
        };

        return builder.Uri.AbsoluteUri.TrimEnd('/');
    }
}
