using System.Data;
using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;

namespace SNEL.WalletPlatform.Infrastructure.Oracle;

internal static class SnelOracleCommandLogger
{
    public static void LogCommand(ILogger logger, bool enabled, string operation, IDbCommand command)
    {
        if (!enabled)
            return;

        logger.LogInformation(
            "SNEL Oracle {Operation}. SQL: {Sql}. Parameters: {Parameters}",
            operation,
            command.CommandText,
            FormatParameters(command.Parameters));
    }

    private static string FormatParameters(IDataParameterCollection parameters)
    {
        if (parameters.Count == 0)
            return "[]";

        var values = new List<string>(parameters.Count);
        foreach (var parameter in parameters.Cast<IDataParameter>())
        {
            values.Add($"{parameter.ParameterName}={FormatValue(parameter.Value)}");
        }

        return $"[{string.Join(", ", values)}]";
    }

    private static string FormatValue(object? value)
    {
        if (value is null or DBNull)
            return "NULL";

        return value switch
        {
            DateTime dateTime => dateTime.ToString("O", CultureInfo.InvariantCulture),
            DateTimeOffset dateTimeOffset => dateTimeOffset.ToString("O", CultureInfo.InvariantCulture),
            string text => $"\"{text}\"",
            bool boolean => boolean ? "true" : "false",
            byte[] bytes => $"<byte[{bytes.Length}]>",
            _ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? "<unprintable>"
        };
    }
}
