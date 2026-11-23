"""Unit tests for calculation and verification of agent tool evidence."""

import json
import os
import sys
import unittest
from pathlib import Path
from types import SimpleNamespace
from unittest.mock import AsyncMock

src = Path(__file__).resolve().parents[1] / "src"
sys.path.insert(0, str(src))
os.environ.setdefault("AZURE_FUNCTIONS_AGENTS_APP_ROOT", str(src))
os.environ.setdefault("FOUNDRY_MODEL", "test-model")
os.environ.setdefault("AZURE_OPENAI_ENDPOINT", "https://example.openai.azure.com")
os.environ.setdefault("AZURE_OPENAI_DEPLOYMENT", "test-model")

from tools.calculate_total import calculate_total
from function_app import app, expense_check, verified_total


class CalculateTotalTests(unittest.TestCase):
    def test_sums_all_amounts(self):
        self.assertEqual(calculate_total([2400, 3800]), 6200)
        self.assertEqual(calculate_total([1]), 1)
        self.assertEqual(calculate_total([10**20, 1]), 10**20 + 1)

    def test_rejects_invalid_amounts(self):
        for amounts in ([], [0], [-1], [True], [1.5], ["100"]):
            with self.subTest(amounts=amounts), self.assertRaises(ValueError):
                calculate_total(amounts)


class ToolEvidenceTests(unittest.TestCase):
    def setUp(self):
        self.call = {
            "tool_name": "calculate_total",
            "arguments": json.dumps({"amounts": [2400, 3800]}),
            "result": "6200",
            "success": True,
        }

    def test_accepts_completed_call_for_all_amounts(self):
        self.assertEqual(verified_total([self.call], [2400, 3800]), 6200)
        self.call.update(arguments={"amounts": [2400, 3800]}, result=6200)
        self.assertEqual(verified_total([self.call], [2400, 3800]), 6200)

    def test_rejects_missing_failed_or_invalid_calls(self):
        cases = [
            [],
            [{**self.call, "success": False}],
            [{**self.call, "success": None}],
            [{**self.call, "result": "error"}],
            [{**self.call, "result": None}],
            [{**self.call, "arguments": {"amounts": [2400]}}],
            [{**self.call, "arguments": "not json"}],
            [self.call, self.call],
        ]
        for calls in cases:
            with self.subTest(calls=calls), self.assertRaises(ValueError):
                verified_total(calls, [2400, 3800])


class HandlerTests(unittest.IsolatedAsyncioTestCase):
    def test_function_is_indexed_with_expected_route_and_auth(self):
        functions = app.get_functions()
        binding = json.loads(functions[0].get_function_json())["bindings"][0]
        self.assertEqual(len(functions), 1)
        self.assertEqual((binding["route"], binding["authLevel"]), ("expense-check", "FUNCTION"))

    async def asyncSetUp(self):
        self.expenses = [
            {"date": "2026-11-20", "category": "meal", "description": "meal",
             "amount": amount, "receipt_attached": receipt}
            for amount, receipt in [(2400, True), (3800, False)]
        ]
        self.body = {"expenses": self.expenses}
        self.output = {
            "total_amount_jpy": 6200, "status": "needs_review", "summary": "申請前チェック",
            "findings": [],
        }
        self.call = {
            "tool_name": "calculate_total", "arguments": {"amounts": [2400, 3800]},
            "result": 6200, "success": True,
        }
        self.skill = SimpleNamespace(run=AsyncMock(return_value=SimpleNamespace(
            content=json.dumps(self.output), tool_calls=[self.call],
        )))

    async def invoke(self):
        request = SimpleNamespace(json=AsyncMock(return_value=self.body))
        handler = expense_check._function.get_user_function().__wrapped__
        return await handler(request, self.skill)

    async def test_uses_verified_tool_result(self):
        response = await self.invoke()
        self.assertEqual(response.status_code, 200)
        self.assertEqual(json.loads(response.body)["total_amount_jpy"], 6200)
        self.assertEqual(json.loads(self.skill.run.await_args.args[0]), self.body)

    async def test_rejects_invalid_request_without_running_agent(self):
        self.body["expenses"][0]["amount"] = True
        response = await self.invoke()
        self.assertEqual(response.status_code, 400)
        self.skill.run.assert_not_awaited()

    async def test_failed_or_missing_tool_never_returns_check(self):
        for calls in ([], [{**self.call, "success": False}],
                      [{**self.call, "arguments": {"amounts": [2400]}}]):
            self.skill.run.return_value.tool_calls = calls
            with self.subTest(calls=calls):
                response = await self.invoke()
                self.assertEqual(response.status_code, 502)
                self.assertNotIn("status", json.loads(response.body))

    async def test_mismatched_response_total_is_rejected(self):
        self.skill.run.return_value.content = json.dumps({**self.output, "total_amount_jpy": 1})
        response = await self.invoke()
        self.assertEqual(response.status_code, 502)


if __name__ == "__main__":
    unittest.main()
