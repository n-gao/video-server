using System.Security.Cryptography;
using System.Text;

public static class StringExtension {
    public static string ToSHA256(this string str) {
        var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(str));
        var builder = new StringBuilder();
        foreach (var bytes in hash)
            builder.Append(bytes.ToString("x2"));
        return builder.ToString();
    }
}
