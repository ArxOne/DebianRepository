using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ArxOne.Debian.Stanza;

public class StanzaMapper
{
    public IFormatProvider FormatProvider { get; set; } = CultureInfo.InvariantCulture;

    public Stanza Extract<T>(T o) => Extract(o, typeof(T));

    public Stanza Extract(object o, Type t)
    {
        var stanza = new Stanza();
        foreach (var (propertyInfo, name) in GetProperties(t))
            stanza[name] = ToStanzaValue(propertyInfo.GetValue(o));
        return stanza;
    }

    public void Map<T>(T o, Stanza stanza) => Map(o, typeof(T), stanza);
    public void Map(object o, Type t, Stanza stanza)
    {
        foreach (var (propertyInfo, name) in GetProperties(t))
            if (stanza.TryGetValue(name, out var stanzaValue))
                propertyInfo.SetValue(o, FromStanzaValue(stanzaValue, propertyInfo.PropertyType));
    }

    private static IEnumerable<(PropertyInfo PropertyInfo, string Name)> GetProperties(Type t)
    {
        return t.GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(p => (p, GetMappingName(p)));
    }

    private static string GetMappingName(MemberInfo propertyInfo)
    {
        var lowerToUpper = new Regex(@"(?<before>[a-z])(?<after>[A-Z])");
        return lowerToUpper.Replace(propertyInfo.Name, match => $"{match.Groups["before"].Value}-{match.Groups["after"].Value}");
    }

    private StanzaValue? ToStanzaValue(object o)
    {
        return o switch
        {
            null => null,
            DateTime dateTime => dateTime.ToString("R"),
            DateTimeOffset dateTimeOffset => dateTimeOffset.ToString("R"),
            IFormattable formattable => formattable.ToString(null, FormatProvider),
            IEnumerable<string> lines => new StanzaValue(lines),
            IEnumerable<byte> bytes => Convert.ToHexString(bytes.ToArray()).ToLower(),
            _ => o.ToString()
        };
    }

    private object FromStanzaValue(StanzaValue value, Type targetType)
    {
        if (targetType == typeof(string))
            return value.Text;
        if (targetType == typeof(IEnumerable<string>) || targetType == typeof(IReadOnlyCollection<string>) || targetType == typeof(IReadOnlyList<string>))
            return value.Lines;
        if (targetType == typeof(long))
            return long.Parse(value.Text);
        if (targetType == typeof(ulong))
            return ulong.Parse(value.Text);
        if (targetType == typeof(int))
            return int.Parse(value.Text);
        if (targetType == typeof(uint))
            return uint.Parse(value.Text);
        if (targetType == typeof(byte[]) || targetType == typeof(IEnumerable<byte>))
            return Convert.FromHexString(value.Text);
        if (targetType == typeof(DateTime))
            return DateTime.ParseExact(value.Text, "R", FormatProvider);
        if (targetType == typeof(DateTimeOffset))
            return DateTimeOffset.ParseExact(value.Text, "R", FormatProvider);
        throw new NotSupportedException($"Don’t know how to convert stanza value to {targetType}");
    }
}
