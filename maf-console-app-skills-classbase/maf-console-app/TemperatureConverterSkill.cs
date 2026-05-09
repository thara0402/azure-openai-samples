using Microsoft.Agents.AI;
using System.ComponentModel;
using System.Text.Json;

namespace maf_console_app;

/// <summary>
/// A temperature-converter skill defined as a C# class using attributes for discovery.
/// </summary>
/// <remarks>
/// Properties annotated with <see cref="AgentSkillResourceAttribute"/> are automatically
/// discovered as skill resources, and methods annotated with <see cref="AgentSkillScriptAttribute"/>
/// are automatically discovered as skill scripts. Alternatively,
/// <see cref="AgentSkill.Resources"/> and <see cref="AgentSkill.Scripts"/> can be overridden.
/// </remarks>
internal sealed class TemperatureConverterSkill : AgentClassSkill<TemperatureConverterSkill>
{
    /// <inheritdoc/>
    public override AgentSkillFrontmatter Frontmatter { get; } = new(
        "temperature-converter",
        "Convert between Celsius and Fahrenheit. Use when asked to convert degrees Celsius (°C) or Fahrenheit (°F).");

    /// <inheritdoc/>
    protected override string Instructions => """
    Use this skill when the user asks to convert temperatures between Celsius and Fahrenheit.

    1. Review the conversion-formula resource to find the preOffset, factor, and postOffset for the requested direction.
    2. Use the convert-temperature script, passing the value and the three parameters from the table.
    3. Present the result clearly with both units.
    """;

    /// <inheritdoc/>
    protected override JsonSerializerOptions? SerializerOptions => null;

    /// <summary>
    /// A resource describing the temperature conversion parameters.
    /// </summary>
    [AgentSkillResource("conversion-formula")]
    [Description("Lookup table of parameters for temperature conversions. Formula: result = (value + preOffset) × factor + postOffset")]
    public string ConversionFormula => """
    # Temperature Conversion Parameters

    Formula: **result = (value + preOffset) × factor + postOffset**

    | Direction              | preOffset | factor | postOffset |
    |------------------------|-----------|--------|------------|
    | Celsius → Fahrenheit   | 0         | 1.8    | 32         |
    | Fahrenheit → Celsius   | -32       | 0.5556 | 0          |
    """;

    /// <summary>
    /// Converts a temperature using the given parameters from the conversion-formula table.
    /// </summary>
    [AgentSkillScript("convert-temperature")]
    [Description("Converts a temperature value using formula: result = (value + preOffset) × factor + postOffset. Returns the result as JSON.")]
    private static string ConvertTemperature(double value, double preOffset, double factor, double postOffset)
    {
        double result = Math.Round((value + preOffset) * factor + postOffset, 4);
        return JsonSerializer.Serialize(new { value, preOffset, factor, postOffset, result });
    }
}
