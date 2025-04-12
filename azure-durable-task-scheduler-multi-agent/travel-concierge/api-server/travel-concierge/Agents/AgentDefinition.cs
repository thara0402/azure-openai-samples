using OpenAI.Chat;
using travel_concierge.Models;

namespace travel_concierge.Agents
{
    internal class AgentDefinition
    {
        public static readonly ChatTool GetDestinationSuggestAgent = ChatTool.CreateFunctionTool(
            functionName: nameof(GetDestinationSuggestAgent),
            functionDescription: "希望の行き先に求める条件を自然言語で与えると、おすすめの旅行先を提案します。",
            functionParameters: JsonSchemaGenerator.GenerateSchemaAsBinaryData(SourceGenerationContext.Default.GetDestinationSuggestParameter));

        public static readonly ChatTool GetClimateAgent = ChatTool.CreateFunctionTool(
            functionName: nameof(GetClimateAgent),
            functionDescription: "指定された場所の気候を取得します。",
            functionParameters: JsonSchemaGenerator.GenerateSchemaAsBinaryData(SourceGenerationContext.Default.GetClimateParameter));

        public static readonly ChatTool GetSightseeingSpotAgent = ChatTool.CreateFunctionTool(
            functionName: nameof(GetSightseeingSpotAgent),
            functionDescription: "指定された場所の観光名所を取得します。",
            functionParameters: JsonSchemaGenerator.GenerateSchemaAsBinaryData(SourceGenerationContext.Default.GetSightseeingSpotParameter));

        public static readonly ChatTool GetHotelAgent = ChatTool.CreateFunctionTool(
            functionName: nameof(GetHotelAgent),
            functionDescription: "指定された場所のホテルを取得します。",
            functionParameters: JsonSchemaGenerator.GenerateSchemaAsBinaryData(SourceGenerationContext.Default.GetHotelParameter));

        public static readonly ChatTool SubmitReservationAgent = ChatTool.CreateFunctionTool(
            functionName: nameof(SubmitReservationAgent),
            functionDescription: "宿泊先の予約を行います。",
            functionParameters: JsonSchemaGenerator.GenerateSchemaAsBinaryData(SourceGenerationContext.Default.SubmitReservationParameter));
    }
}
