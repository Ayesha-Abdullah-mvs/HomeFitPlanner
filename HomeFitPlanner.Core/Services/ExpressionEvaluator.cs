using System;
using HomeFitPlanner.Core.DataStructures.Arrays;
using HomeFitPlanner.Core.DataStructures.Stacks;

namespace HomeFitPlanner.Core.Services;

/// <summary>
/// Hand-written arithmetic expression evaluator using two stacks (Operands and Operators).
/// Evaluates workload formulas such as "(12 x 3) + (10 x 4) + (15 x 3)" to calculate total session reps.
/// </summary>
public static class ExpressionEvaluator
{
    public static double Evaluate(string expression)
    {
        if (string.IsNullOrEmpty(expression)) return 0;

        string sanitized = StringSanitizer.ManualTrim(expression);
        var values = new CustomStack<double>();
        var ops = new CustomStack<char>();

        int i = 0;
        while (i < sanitized.Length)
        {
            char c = sanitized[i];

            if (char.IsWhiteSpace(c))
            {
                i++;
                continue;
            }

            // Parse numbers (including decimals)
            if (char.IsDigit(c) || (c == '.' && i + 1 < sanitized.Length && char.IsDigit(sanitized[i + 1])))
            {
                int start = i;
                while (i < sanitized.Length && (char.IsDigit(sanitized[i]) || sanitized[i] == '.'))
                {
                    i++;
                }
                string numStr = sanitized.Substring(start, i - start);
                if (double.TryParse(numStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double val))
                {
                    values.Push(val);
                }
                continue;
            }

            if (c == '(')
            {
                ops.Push(c);
            }
            else if (c == ')')
            {
                while (!ops.IsEmpty && ops.Peek() != '(')
                {
                    ApplyTopOp(values, ops);
                }
                if (!ops.IsEmpty && ops.Peek() == '(')
                {
                    ops.Pop(); // Discard '('
                }
            }
            else if (c == '+' || c == '-' || c == '*' || c == 'x' || c == 'X' || c == '/')
            {
                char actualOp = (c == 'x' || c == 'X') ? '*' : c;

                // Handle unary minus: if at start or preceded by '('
                if (actualOp == '-' && (i == 0 || sanitized[i - 1] == '('))
                {
                    values.Push(0); // push 0 so 0 - x is calculated
                }

                while (!ops.IsEmpty && Precedence(ops.Peek()) >= Precedence(actualOp))
                {
                    ApplyTopOp(values, ops);
                }
                ops.Push(actualOp);
            }

            i++;
        }

        while (!ops.IsEmpty)
        {
            ApplyTopOp(values, ops);
        }

        return values.IsEmpty ? 0 : values.Pop();
    }

    private static int Precedence(char op)
    {
        if (op == '+' || op == '-') return 1;
        if (op == '*' || op == '/') return 2;
        return 0; // for '('
    }

    private static void ApplyTopOp(CustomStack<double> values, CustomStack<char> ops)
    {
        if (values.Count < 2 || ops.IsEmpty) return;

        char op = ops.Pop();
        double b = values.Pop();
        double a = values.Pop();

        double res = op switch
        {
            '+' => a + b,
            '-' => a - b,
            '*' => a * b,
            '/' => b != 0 ? a / b : 0,
            _ => 0
        };

        values.Push(res);
    }
}

