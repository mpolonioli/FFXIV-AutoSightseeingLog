using System.Text;

namespace AutoSightseeingLog.Windows;

// The catalogs carry printf formats ("%d s") while Glasswork's slider takes a .NET numeric format. Every character but
// the number is escaped, so a translation's dots, commas or zeros stay plain text, and an optional word for zero becomes
// the format's zero section. Keep it in a mutable field: it converts again only when the language hands back new
// strings, and a copy would cache into itself.
internal struct SliderFormat
{
    private const string PrintfInteger = "%d";
    private const string PrintfPercent = "%%";
    private const char NumberPlaceholder = '0';
    private const char Escape = '\\';
    private const string ZeroSection = ";;";

    private string? source;
    private string? zeroSource;
    private string? format;

    public string For(string printf, string? zero = null)
    {
        if (format is not null && ReferenceEquals(printf, source) && ReferenceEquals(zero, zeroSource))
        {
            return format;
        }

        source = printf;
        zeroSource = zero;
        format = Convert(printf, zero);
        return format;
    }

    private static string Convert(string printf, string? zero)
    {
        var builder = new StringBuilder(printf.Length * 2);
        for (var index = 0; index < printf.Length; index++)
        {
            if (string.CompareOrdinal(printf, index, PrintfInteger, 0, PrintfInteger.Length) == 0)
            {
                builder.Append(NumberPlaceholder);
                index += PrintfInteger.Length - 1;
                continue;
            }

            if (string.CompareOrdinal(printf, index, PrintfPercent, 0, PrintfPercent.Length) == 0)
            {
                index += PrintfPercent.Length - 1;
            }

            builder.Append(Escape).Append(printf[index]);
        }

        if (zero is null)
        {
            return builder.ToString();
        }

        builder.Append(ZeroSection);
        for (var index = 0; index < zero.Length; index++)
        {
            builder.Append(Escape).Append(zero[index]);
        }

        return builder.ToString();
    }
}
