---
name: temperature-converter
description: Convert temperatures between Celsius and Fahrenheit. Use when asked to convert Celsius to Fahrenheit or Fahrenheit to Celsius.
---

## Usage

When the user requests a temperature conversion:
1. First, review `references/conversion-formulas.md` to find the correct formula
2. Run the `scripts/convert_temperature.py` script with `--value <number> --direction <c2f|f2c>` (e.g. `--value 100 --direction c2f`)
3. Present the converted value clearly with both units
