using System;
using System.Text;

namespace HomeFitPlanner.Core.Algorithms;

/// <summary>
/// Hand-written implementations of recursion and divide-and-conquer arithmetic algorithms:
/// 1) Recursive workload progression modeling weekly adaptation.
/// 2) Grade-school O(N^2) large number multiplication.
/// 3) Karatsuba O(N^1.585) divide-and-conquer large number multiplication.
/// </summary>
public static class MathAlgorithms
{
    /// <summary>
    /// Recursively computes the progressive target workload for week N.
    /// Base case: week 1 = baseWorkload.
    /// Recursive step: Workload(N) = Workload(N-1) * (1 + rate) + bump.
    /// </summary>
    public static double CalculateRecursiveWorkload(int week, double baseWorkload, double weeklyGrowthRate = 0.05, double baseBump = 2.0)
    {
        if (week <= 1) return baseWorkload;
        double prev = CalculateRecursiveWorkload(week - 1, baseWorkload, weeklyGrowthRate, baseBump);
        return Math.Round(prev * (1.0 + weeklyGrowthRate) + baseBump, 1);
    }

    /// <summary>
    /// Grade-school classic digit-by-digit multiplication in O(N*M) ~ O(N^2).
    /// </summary>
    public static string MultiplyGradeSchool(string num1, string num2)
    {
        if (num1 == "0" || num2 == "0" || string.IsNullOrEmpty(num1) || string.IsNullOrEmpty(num2))
            return "0";

        int n1 = num1.Length;
        int n2 = num2.Length;
        int[] result = new int[n1 + n2];

        for (int i = n1 - 1; i >= 0; i--)
        {
            int d1 = num1[i] - '0';
            for (int j = n2 - 1; j >= 0; j--)
            {
                int d2 = num2[j] - '0';
                int sum = d1 * d2 + result[i + j + 1];

                result[i + j + 1] = sum % 10;
                result[i + j] += sum / 10;
            }
        }

        var sb = new StringBuilder();
        int start = 0;
        while (start < result.Length && result[start] == 0)
        {
            start++;
        }

        if (start == result.Length) return "0";

        for (int i = start; i < result.Length; i++)
        {
            sb.Append(result[i]);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Karatsuba divide-and-conquer large integer multiplication algorithm.
    /// Runs in O(N^1.585) time by reducing 4 subproblems to 3.
    /// </summary>
    public static string MultiplyKaratsuba(string num1, string num2)
    {
        if (num1 == "0" || num2 == "0" || string.IsNullOrEmpty(num1) || string.IsNullOrEmpty(num2))
            return "0";

        // Base case: for small lengths, grade-school multiplication is faster and avoids overhead
        if (num1.Length <= 4 || num2.Length <= 4)
        {
            return MultiplyGradeSchool(num1, num2);
        }

        // Equalize string lengths with leading zeros to power of 2 or max length
        int n = Math.Max(num1.Length, num2.Length);
        num1 = num1.PadLeft(n, '0');
        num2 = num2.PadLeft(n, '0');

        int m = n / 2;
        int m2 = n - m;

        string high1 = num1.Substring(0, m);
        string low1 = num1.Substring(m);
        string high2 = num2.Substring(0, m);
        string low2 = num2.Substring(m);

        // 3 recursive multiplications:
        // z2 = high1 * high2
        // z0 = low1 * low2
        // z1 = (high1 + low1) * (high2 + low2)
        string z2 = MultiplyKaratsuba(StripLeadingZeros(high1), StripLeadingZeros(high2));
        string z0 = MultiplyKaratsuba(StripLeadingZeros(low1), StripLeadingZeros(low2));

        string sum1 = AddLargeNumbers(high1, low1);
        string sum2 = AddLargeNumbers(high2, low2);
        string z1 = MultiplyKaratsuba(sum1, sum2);

        // Middle term = z1 - z2 - z0
        string middle = SubtractLargeNumbers(SubtractLargeNumbers(z1, z2), z0);

        // Result = z2 * 10^(2*m2) + middle * 10^(m2) + z0
        string term1 = ShiftLeft(z2, 2 * m2);
        string term2 = ShiftLeft(middle, m2);

        string finalSum = AddLargeNumbers(AddLargeNumbers(term1, term2), z0);
        return StripLeadingZeros(finalSum);
    }

    public static string AddLargeNumbers(string a, string b)
    {
        var sb = new StringBuilder();
        int i = a.Length - 1;
        int j = b.Length - 1;
        int carry = 0;

        while (i >= 0 || j >= 0 || carry > 0)
        {
            int d1 = i >= 0 ? a[i] - '0' : 0;
            int d2 = j >= 0 ? b[j] - '0' : 0;
            int sum = d1 + d2 + carry;
            carry = sum / 10;
            sb.Insert(0, (char)('0' + (sum % 10)));
            i--;
            j--;
        }

        return StripLeadingZeros(sb.ToString());
    }

    public static string SubtractLargeNumbers(string a, string b)
    {
        // Assumes a >= b
        a = StripLeadingZeros(a);
        b = StripLeadingZeros(b);
        if (a == b) return "0";

        var sb = new StringBuilder();
        int i = a.Length - 1;
        int j = b.Length - 1;
        int borrow = 0;

        while (i >= 0)
        {
            int d1 = a[i] - '0' - borrow;
            int d2 = j >= 0 ? b[j] - '0' : 0;

            if (d1 < d2)
            {
                d1 += 10;
                borrow = 1;
            }
            else
            {
                borrow = 0;
            }

            sb.Insert(0, (char)('0' + (d1 - d2)));
            i--;
            j--;
        }

        return StripLeadingZeros(sb.ToString());
    }

    private static string ShiftLeft(string num, int zeros)
    {
        if (num == "0" || string.IsNullOrEmpty(num) || zeros <= 0) return num;
        return num + new string('0', zeros);
    }

    private static string StripLeadingZeros(string s)
    {
        if (string.IsNullOrEmpty(s)) return "0";
        int start = 0;
        while (start < s.Length - 1 && s[start] == '0')
        {
            start++;
        }
        return s.Substring(start);
    }
}

