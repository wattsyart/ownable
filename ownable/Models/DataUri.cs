using System.Text;
using System.Text.RegularExpressions;

namespace ownable.Models;

public static class DataUri
{
    public static bool TryParseImage(string uri, out DataUriFormat? format)
    {
        format = new DataUriFormat();
        var match = Regex.Match(uri, @"data:image/(?<type>.+?),(?<data>.+)");
        if (!match.Success)
            return false;

        var imageType = match.Groups["type"].Value.ToLowerInvariant();
        var data = match.Groups["data"].Value;

        switch (imageType)
        {
            case "svg+xml;base64":
                format.Extension = ".svg";
                format.IsBase64 = true;
                break;
            case "png":
                format.Extension = ".png";
                break;
            case "png;base64":
                format.Extension = ".png";
                format.IsBase64 = true;
                break;
            case "bmp":
                format.Extension = ".bmp";
                break;
            case "bmp;base64":
                format.Extension = ".bmp";
                format.IsBase64 = true;
                break;
            case "gif":
                format.Extension = ".gif";
                break;
            case "gif;base64":
                format.Extension = ".gif";
                format.IsBase64 = true;
                break;
            case "svg+xml":
                format.Extension = ".svg";
                break;
            default:
                format.Extension = imageType;
                break;
        }

        format.ContentType = $"data:image/{imageType}";
        format.Data = format.IsBase64 ? Convert.FromBase64String(data) : Encoding.UTF8.GetBytes(data);
        return true;
    }

    public static bool TryParseApplication(string uri, out DataUriFormat? format)
    {
        format = new DataUriFormat();
        var match = Regex.Match(uri, @"data:application/(?<type>.+?),(?<data>.+)");
        if (!match.Success)
            return false;

        var applicationType = match.Groups["type"].Value.ToLowerInvariant();
        var data = match.Groups["data"].Value;

        switch (applicationType)
        {
            case "json;base64":
                format.Extension = ".json";
                format.IsBase64 = true;
                break;
            case "json":
                format.Extension = ".json";
                break;
            default:
                format.Extension = applicationType;
                break;
        }

        format.ContentType = $"data:application/{applicationType}";
        format.Data = format.IsBase64 ? Convert.FromBase64String(data) : Encoding.UTF8.GetBytes(data);
        return true;
    }
}