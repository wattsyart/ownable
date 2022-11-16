namespace ownable.Models
{
    internal static class JsonTokenMetadataExtensions
    {
        public static bool HasUriImageWithSchemes(this JsonTokenMetadata metadata, params string[] schemes)
        {
            return IsValidUriWithSchemes(metadata?.Image, schemes) || IsValidUriWithSchemes(metadata?.ImageData, schemes);
        }

        public static bool HasHttpImage(this JsonTokenMetadata metadata)
        {
            return IsValidHttpUri(metadata?.Image) || IsValidHttpUri(metadata?.ImageData);
        }

        public static bool HasHttpImageWithHost(this JsonTokenMetadata metadata, string host)
        {
            return IsValidHttpUriWithHost(metadata?.Image, host) || IsValidHttpUriWithHost(metadata?.ImageData, host);
        }

        public static bool IsValidHttpUriWithHost(this string? tokenUri, string host)
        {
            if (!IsValidHttpUri(tokenUri, out var uri))
                return false;

            return uri != null && !string.IsNullOrWhiteSpace(uri.Host) &&
                   uri.Host.Equals(host, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsValidHttpUri(this string? tokenUri) => IsValidUriWithSchemes(tokenUri, "https", "http");

        public static bool IsValidHttpUri(this string? tokenUri, out Uri? uri) => IsValidUriWithSchemes(tokenUri, out uri, "https", "http");

        public static bool IsValidUriWithSchemes(this string? tokenUri, params string[] schemes)
        {
            return IsValidUriWithSchemes(tokenUri, out _, schemes);
        }

        public static bool IsValidUriWithSchemes(this string? tokenUri, out Uri? uri, params string[] schemes)
        {
            if (!Uri.TryCreate(tokenUri, UriKind.Absolute, out uri))
                return false;

            foreach (var scheme in schemes)
                if (uri.Scheme.Equals(scheme, StringComparison.OrdinalIgnoreCase))
                    return true;

            return false;
        }
    }
}
