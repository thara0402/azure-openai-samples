"""HTTP integration tests against a running expense-check-v2 app."""

import json
import os
import unittest
from urllib.request import Request, urlopen


URL = os.environ.get("EXPENSE_CHECK_URL")


def expense(category, amount, receipt_attached):
    return {
        "date": "2026-11-20",
        "category": category,
        "description": "経費",
        "amount": amount,
        "receipt_attached": receipt_attached,
    }


@unittest.skipUnless(URL, "Set EXPENSE_CHECK_URL to a running expense-check-v2 endpoint")
class ExpenseCheckIntegrationTests(unittest.TestCase):
    def check_expenses(self, expenses):
        headers = {"Content-Type": "application/json"}
        if key := os.environ.get("AZURE_FUNCTION_KEY"):
            headers["x-functions-key"] = key
        request = Request(
            URL,
            data=json.dumps({"expenses": expenses}, ensure_ascii=False).encode("utf-8"),
            headers=headers,
            method="POST",
        )
        with urlopen(request, timeout=180) as response:
            return json.load(response)

    def test_sample_meals(self):
        result = self.check_expenses([expense("meal", 2400, True), expense("meal", 3800, False)])
        self.assertEqual(result["status"], "needs_review")
        first, second = result["findings"]
        self.assertEqual((first["expense_number"], first["status"], first["reasons"]),
                         (1, "no_issues_found", []))
        self.assertEqual((second["expense_number"], second["status"]), (2, "needs_review"))
        reasons = " ".join(second["reasons"])
        self.assertRegex(reasons.replace(",", ""), r"800\s*円")
        self.assertRegex(reasons, r"超過|超え")
        self.assertIn("領収書", reasons)
        self.assertRegex(reasons, r"未添付|添付されていない|添付がない|添付なし")

    def test_limit_and_other_categories(self):
        result = self.check_expenses([
            expense("meal", 3000, True),
            *[expense(category, 1000, False) for category in
              ("transportation", "lodging", "supplies", "other")],
        ])
        self.assertEqual(result["status"], "needs_review")
        self.assertEqual(len(result["findings"]), 5)
        self.assertEqual((result["findings"][0]["status"], result["findings"][0]["reasons"]),
                         ("no_issues_found", []))
        for number, finding in enumerate(result["findings"][1:], 2):
            with self.subTest(expense_number=number):
                self.assertEqual((finding["expense_number"], finding["status"]), (number, "needs_review"))
                reasons = " ".join(finding["reasons"])
                self.assertIn("領収書", reasons)
                self.assertRegex(reasons, r"未添付|添付されていない|添付がない|添付なし")
                self.assertRegex(reasons, r"上限")
                self.assertRegex(reasons, r"未定|ない|なし|未設定|規定がありません|定められていません|定められていない")


if __name__ == "__main__":
    unittest.main()
