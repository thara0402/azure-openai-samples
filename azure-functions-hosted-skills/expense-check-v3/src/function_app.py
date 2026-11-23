import json
import logging
from pathlib import Path

import azure.functions as func
import frontmatter
from azurefunctions.extensions.http.fastapi import JSONResponse, Request
from jsonschema import Draft202012Validator, FormatChecker

from azure_functions_agents import HostedSkill, HostedSkillFunctionApp

app = HostedSkillFunctionApp()
_agent = frontmatter.load(Path(__file__).with_name("expense-check.agent.md"))
_input_validator = Draft202012Validator(_agent["input_schema"], format_checker=FormatChecker())


def verified_total(tool_calls, amounts):
    """Accept only a completed tool call for the exact submitted amounts."""
    calls = [call for call in tool_calls if call.get("tool_name") == "calculate_total"]
    if len(calls) != 1 or calls[0].get("success") is not True:
        raise ValueError("A successful calculate_total call is required")
    call = calls[0]
    arguments = call.get("arguments")
    result = call.get("result")
    try:
        if isinstance(arguments, str):
            arguments = json.loads(arguments)
        if isinstance(result, str):
            result = json.loads(result)
    except json.JSONDecodeError as exc:
        raise ValueError("Invalid calculate_total evidence") from exc
    if (
        not isinstance(arguments, dict)
        or arguments != {"amounts": amounts}
        or not isinstance(arguments.get("amounts"), list)
        or any(type(amount) is not int for amount in arguments["amounts"])
        or type(result) is not int
        or result < 1
    ):
        raise ValueError("Invalid calculate_total evidence")
    return result


@app.function_name(name="expense_check")
@app.route(route="expense-check", methods=["POST"], auth_level=func.AuthLevel.FUNCTION)
@app.hosted_skill(arg_name="skill", agent_name="expense_check")
async def expense_check(req: Request, skill: HostedSkill) -> JSONResponse:
    try:
        body = await req.json()
    except (ValueError, UnicodeDecodeError):
        return JSONResponse({"error": "Invalid JSON request"}, status_code=400)

    if not _input_validator.is_valid(body):
        return JSONResponse({"error": "Request does not match input_schema"}, status_code=400)

    result = await skill.run(json.dumps(body, ensure_ascii=False))
    try:
        total = verified_total(result.tool_calls, [item["amount"] for item in body["expenses"]])
        response = json.loads(result.content)
        if type(response["total_amount_jpy"]) is not int or response["total_amount_jpy"] != total:
            raise ValueError("Response total differs from tool result")
    except (ValueError, KeyError, TypeError):
        logging.exception("Expense check did not produce a verified tool result")
        return JSONResponse({"error": "Unable to verify expense total"}, status_code=502)
    return JSONResponse(response)
