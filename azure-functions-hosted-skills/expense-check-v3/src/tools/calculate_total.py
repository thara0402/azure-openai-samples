def calculate_total(amounts: list[int]) -> int:
    """Calculate the total in JPY from every expense amount."""
    if not amounts or any(type(amount) is not int or amount < 1 for amount in amounts):
        raise ValueError("amounts must contain positive integer JPY amounts")
    return sum(amounts)
