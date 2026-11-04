using Microsoft.AspNetCore.Http;

namespace SNEL.WalletPlatform.Presentation.Common;

public static class HttpResponseHeaderExtensions
{
    /// <summary>
    /// Sets <c>X-Message</c> using only ASCII. Non-ASCII text (e.g. French) is percent-encoded; clients should check <c>X-Message-Encoding</c> and call <see cref="Uri.UnescapeDataString"/> when it is <c>percent-encoded</c>.
    /// </summary>
    public static void AppendXMessageHeader(this HttpResponse response, string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        if (ContainsNonAsciiHeaderChars(message))
        {
            response.Headers.Append("X-Message-Encoding", "percent-encoded");
            response.Headers.Append("X-Message", Uri.EscapeDataString(message));
        }
        else
        {
            response.Headers.Append("X-Message", message);
        }
    }

    private static bool ContainsNonAsciiHeaderChars(string value)
    {
        foreach (var c in value)
        {
            if (c is '\t')
                continue;
            if (c < (char)0x20 || c > (char)0x7e)
                return true;
        }

        return false;
    }
}
