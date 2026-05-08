# Temperature conversion script
# Converts between Celsius and Fahrenheit.
#
# Usage:
#   python scripts/convert_temperature.py --value 100 --direction c2f
#   python scripts/convert_temperature.py --value 212 --direction f2c

import argparse
import json


def main() -> None:
    parser = argparse.ArgumentParser(
        description="Convert a temperature between Celsius and Fahrenheit.",
        epilog="Examples:\n"
        "  python scripts/convert_temperature.py --value 100 --direction c2f\n"
        "  python scripts/convert_temperature.py --value 212 --direction f2c",
        formatter_class=argparse.RawDescriptionHelpFormatter,
    )
    parser.add_argument("--value", type=float, required=True, help="The temperature value to convert.")
    parser.add_argument("--direction", choices=["c2f", "f2c"], required=True, help="Conversion direction: c2f (Celsius to Fahrenheit) or f2c (Fahrenheit to Celsius).")
    args = parser.parse_args()

    if args.direction == "c2f":
        result = round(args.value * 9 / 5 + 32, 4)
        from_unit = "Celsius"
        to_unit = "Fahrenheit"
    else:
        result = round((args.value - 32) * 5 / 9, 4)
        from_unit = "Fahrenheit"
        to_unit = "Celsius"

    print(json.dumps({"value": args.value, "from": from_unit, "to": to_unit, "result": result}))


if __name__ == "__main__":
    main()
