using Azure;
using Azure.AI.OpenAI;
using maf_console_app;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

var settings = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build()
    .GetSection(nameof(AzureOpenAISettings)).Get<AzureOpenAISettings>() ?? throw new NullReferenceException();

var instructions = """
                You are a helpful assistant.
            """;

var userMessages = new String[] {
    "摂氏20度は華氏何度ですか？また、華氏50度は摂氏何度ですか？",
    "マラソン（42.195キロメートル）は何マイルですか？また、75キログラムは何ポンドですか？"
};

Console.WriteLine("エージェントのセットアップ中・・・");

// --- Build the code-defined skill ---
// --- Unit converter skill ---
var unitConverterSkill = new AgentInlineSkill(
    name: "unit-converter",
    description: "Convert between common units using a multiplication factor. Use when asked to convert miles, kilometers, pounds, or kilograms.",
    instructions: """
        Use this skill when the user asks to convert between units.

        1. Review the conversion-table resource to find the factor for the requested conversion.
        2. Check the conversion-policy resource for rounding and formatting rules.
        3. Use the convert script, passing the value and factor from the table.
        """)
    // 1. Static Resource: conversion tables
    .AddResource(
        "conversion-table",
        """
        # Conversion Tables

        Formula: **result = value × factor**

        | From        | To          | Factor   |
        |-------------|-------------|----------|
        | miles       | kilometers  | 1.60934  |
        | kilometers  | miles       | 0.621371 |
        | pounds      | kilograms   | 0.453592 |
        | kilograms   | pounds      | 2.20462  |
        """)
    // 2. Dynamic Resource: conversion policy (computed at runtime)
    .AddResource("conversion-policy", () =>
    {
        const int Precision = 4;
        return $"""
            # Conversion Policy

            **Decimal places:** {Precision}
            **Format:** Always show both the original and converted values with units
            **Generated at:** {DateTime.UtcNow:O}
            """;
    })
    // 3. Code Script: convert
    .AddScript("convert", (double value, double factor) =>
    {
        double result = Math.Round(value * factor, 4);
        return JsonSerializer.Serialize(new { value, factor, result });
    });

// --- Temperature converter skill ---
var temperatureConverterSkill = new AgentInlineSkill(
    name: "temperature-converter",
    description: "Convert temperatures between Celsius and Fahrenheit. Use when asked to convert °C to °F or °F to °C.",
    instructions: """
        Use this skill when the user asks to convert temperatures between Celsius and Fahrenheit.

        1. Review the conversion-formula resource to understand how to convert.
        2. Check the conversion-policy resource for rounding and formatting rules.
        3. Use the convert script, passing the value and the conversion direction.
        """)
    // 1. Static Resource: conversion formulas
    .AddResource(
        "conversion-formula",
        """
        # Temperature Conversion Formulas

        | From       | To         | Formula                        |
        |------------|------------|--------------------------------|
        | Celsius    | Fahrenheit | result = value × 9/5 + 32      |
        | Fahrenheit | Celsius    | result = (value - 32) × 5/9    |
        """)
    // 2. Dynamic Resource: conversion policy (computed at runtime)
    .AddResource("conversion-policy", () =>
    {
        const int Precision = 2;
        return $"""
            # Conversion Policy

            **Decimal places:** {Precision}
            **Format:** Always show both the original and converted values with units (°C or °F)
            **Generated at:** {DateTime.UtcNow:O}
            """;
    })
    // 3. Code Script: convert
    .AddScript("convert", (double value, string direction) =>
    {
        var dir = direction.ToLowerInvariant();
        bool toCelsius = dir.Contains("fahrenheit") && dir.Contains("celsius")
            ? dir.IndexOf("fahrenheit") < dir.IndexOf("celsius")
            : dir.Contains("f") && !dir.StartsWith("c");
        double result = toCelsius
            ? Math.Round((value - 32) * 5.0 / 9.0, 2)
            : Math.Round(value * 9.0 / 5.0 + 32, 2);
        return JsonSerializer.Serialize(new { value, direction, result });
    });

// --- Skills Provider ---
var skillsProvider = new AgentSkillsProvider(unitConverterSkill, temperatureConverterSkill);

// --- Agent Setup ---
var aoaiClient = new AzureOpenAIClient(
    new Uri(settings.Endpoint),
    new AzureKeyCredential(settings.ApiKey));

var chatClient = aoaiClient.GetChatClient(settings.DeploymentName)
    .AsIChatClient();

var agent = new ChatClientAgent(chatClient, new ChatClientAgentOptions
{
    ChatOptions = new ChatOptions
    {
        Instructions = instructions
    },
    AIContextProviders = [skillsProvider]
});

Console.WriteLine("チャットを開始する");

var session = await agent.CreateSessionAsync();

foreach (var userMessage in userMessages)
{
    Console.WriteLine($"{ChatRole.User}: {userMessage}");

    var result = await agent.RunAsync(userMessage, session);
    Console.WriteLine($"{ChatRole.Assistant}: {result.Text}");

    Console.WriteLine();
}
