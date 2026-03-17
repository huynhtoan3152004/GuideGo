using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace GuideGo_Service.Helpers;

public static class VnPayHelper
{
    public static string BuildPaymentUrl(
        string baseUrl,
        string tmnCode,
        string hashSecret,
        string returnUrl,
        string txnRef,
        long amountVnd,
        string orderInfo,
        string ipAddress,
        DateTime createDate,
        string locale = "vn")
    {
        // StringComparer.Ordinal matches VNPay's VnPayCompare (CompareOptions.Ordinal)
        var vnpParams = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            { "vnp_Amount",     amountVnd.ToString() },
            { "vnp_Command",    "pay" },
            { "vnp_CreateDate", createDate.ToString("yyyyMMddHHmmss") },
            { "vnp_CurrCode",   "VND" },
            { "vnp_IpAddr",     ipAddress },
            { "vnp_Locale",     locale },
            { "vnp_OrderInfo",  orderInfo },
            { "vnp_OrderType",  "other" },
            { "vnp_ReturnUrl",  returnUrl },
            { "vnp_TmnCode",    tmnCode },
            { "vnp_TxnRef",     txnRef },
            { "vnp_Version",    "2.1.0" }
        };

        // Build query string exactly like VNPay official C# sample
        var data = new StringBuilder();
        foreach (var kv in vnpParams)
        {
            if (!string.IsNullOrEmpty(kv.Value))
            {
                data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
            }
        }

        string queryString = data.ToString();
        // signData = queryString without trailing '&'
        string signData = queryString.TrimEnd('&');
        string secureHash = HmacSha512(hashSecret, signData);

        // URL already ends with '&' before appending vnp_SecureHash
        return $"{baseUrl}?{queryString}vnp_SecureHash={secureHash}";
    }

    public static bool VerifySignature(IQueryCollection queryParams, string hashSecret)
    {
        var receivedHash = queryParams["vnp_SecureHash"].ToString();
        if (string.IsNullOrEmpty(receivedHash)) return false;

        var vnpParams = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var (key, value) in queryParams)
        {
            if (key.StartsWith("vnp_") &&
                key != "vnp_SecureHash" &&
                key != "vnp_SecureHashType")
            {
                vnpParams[key] = value.ToString();
            }
        }

        var data = new StringBuilder();
        foreach (var kv in vnpParams)
        {
            if (!string.IsNullOrEmpty(kv.Value))
            {
                data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
            }
        }

        string signData = data.ToString().TrimEnd('&');
        string computedHash = HmacSha512(hashSecret, signData);

        return computedHash.Equals(receivedHash, StringComparison.OrdinalIgnoreCase);
    }

    private static string HmacSha512(string key, string data)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var dataBytes = Encoding.UTF8.GetBytes(data);
        using var hmac = new HMACSHA512(keyBytes);
        var hash = hmac.ComputeHash(dataBytes);
        var sb = new StringBuilder();
        foreach (var b in hash) sb.Append(b.ToString("x2"));
        return sb.ToString();
    }
}
