"""Exercise tool discovery and invocation with a simulated model."""

import json
import unittest
from pathlib import Path
from types import SimpleNamespace
from unittest.mock import AsyncMock, patch

from agent_framework import BaseChatClient, ChatResponse, Content, FunctionInvocationLayer, Message
from azure_functions_agents import runner
from azure_functions_agents.harness._harness_binding import AppHarness, HarnessKind
from azure_functions_agents.harness._provider_config import InferenceTarget
from azure_functions_agents.harness.agent_framework import _maf_execution

from test_tools import expense_check, verified_total


class ToolCallingModel(FunctionInvocationLayer, BaseChatClient):
    def __init__(self):
        super().__init__()
        self.calls = 0

    def _inner_get_response(self, *, messages, stream, options, **kwargs):
        async def respond():
            self.calls += 1
            if self.calls == 1:
                content = Content.from_function_call(
                    "total-1", "calculate_total", arguments={"amounts": [2400, 3800]}
                )
                return ChatResponse(messages=Message("assistant", [content]), finish_reason="tool_calls")
            return ChatResponse(messages=Message("assistant", [Content.from_text("done")]))

        return respond()


class AgentToolTests(unittest.IsolatedAsyncioTestCase):
    async def test_agent_invokes_discovered_tool(self):
        client = ToolCallingModel()
        root = Path(__file__).resolve().parents[1] / "src"
        harness = AppHarness(HarnessKind.MAF, root)
        with patch.object(_maf_execution, "build_chat_client", return_value=(client, InferenceTarget())), \
             patch.object(_maf_execution, "_build_history_provider", return_value=None):
            result = await runner.run_agent(
                "Calculate the total.", model="test-model", agent_name="expense_check",
                mcp_tools=[], _harness=harness,
            )
        self.assertEqual(client.calls, 2)
        self.assertEqual(verified_total(result.tool_calls, [2400, 3800]), 6200)

    async def test_hosted_binding_with_simulated_model(self):
        output = {
            "total_amount_jpy": 6200, "status": "needs_review", "summary": "申請前チェック",
            "findings": [
                {"expense_number": 1, "status": "no_issues_found", "reasons": []},
                {"expense_number": 2, "status": "needs_review",
                 "reasons": ["上限を800円超過", "領収書未添付"]},
            ],
        }

        class PolicyModel(ToolCallingModel):
            def _inner_get_response(self, *, messages, stream, options, **kwargs):
                async def respond():
                    self.calls += 1
                    if self.calls == 2:
                        results = [item.result for message in messages for item in message.contents
                                   if item.type == "function_result" and item.call_id == "skill-1"]
                        self.loaded_policy = " ".join(map(str, results))
                    if self.calls == 3:
                        self.total_results = [item.result for message in messages for item in message.contents
                                              if item.type == "function_result" and item.call_id == "total-1"]
                    if self.calls == 1:
                        content = Content.from_function_call(
                            "skill-1", "load_skill", arguments={"skill_name": "expense-policy"}
                        )
                    elif self.calls == 2:
                        content = Content.from_function_call(
                            "total-1", "calculate_total", arguments={"amounts": [2400, 3800]}
                        )
                    else:
                        return ChatResponse(messages=Message(
                            "assistant", [Content.from_text(json.dumps(output, ensure_ascii=False))]
                        ))
                    return ChatResponse(messages=Message("assistant", [content]), finish_reason="tool_calls")

                return respond()

        client = PolicyModel()
        expenses = [
            {"date": "2026-11-20", "category": "meal", "description": "meal",
             "amount": amount, "receipt_attached": receipt}
            for amount, receipt in [(2400, True), (3800, False)]
        ]
        req = SimpleNamespace(json=AsyncMock(return_value={"expenses": expenses}))
        with patch.object(_maf_execution, "build_chat_client", return_value=(client, InferenceTarget())), \
             patch.object(_maf_execution, "_build_history_provider", return_value=None):
            response = await expense_check(req)
        self.assertEqual(client.calls, 3)
        self.assertIn("3,000", client.loaded_policy)
        self.assertEqual([str(value) for value in client.total_results], ["6200"])
        self.assertEqual(response.status_code, 200)
        self.assertEqual(json.loads(response.body), output)


if __name__ == "__main__":
    unittest.main()
