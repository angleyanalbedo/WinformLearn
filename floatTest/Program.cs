namespace floatTest
{
    using System;
    using System.Collections.Generic;

    namespace FloatPrecisionExample
    {
        class Program
        {
            static void Main(string[] args)
            {
                Console.WriteLine("C# Float 精度丢失示例");
                Console.WriteLine("=====================");

                // 1. 十进制到二进制的转换问题
                Console.WriteLine("\n1. 十进制到二进制的转换问题:");
                float tenth = 0.1f;
                float third = 1.0f / 3.0f;
                Console.WriteLine($"0.1f 的实际值: {tenth}");
                Console.WriteLine($"1.0f/3.0f 的实际值: {third}");

                // 2. 累加误差
                Console.WriteLine("\n2. 累加误差:");
                float sum = 0.0f;
                for (int i = 0; i < 10; i++)
                {
                    sum += 0.1f;
                }
                Console.WriteLine($"0.1f 累加10次: {sum}");
                Console.WriteLine($"等于1.0f吗? {sum == 1.0f}");

                // 3. 大数加小数的问题
                Console.WriteLine("\n3. 大数加小数的问题:");
                float largeNumber = 100000000f;
                float smallNumber = 0.1f;
                float result = largeNumber + smallNumber;
                Console.WriteLine($"{largeNumber} + {smallNumber} = {result}");
                Console.WriteLine($"实际差异: {result - largeNumber}");

                // 4. 比较问题
                Console.WriteLine("\n4. 浮点数比较问题:");
                float a = 0.1f * 0.1f;
                float b = 0.01f;
                Console.WriteLine($"0.1f * 0.1f = {a}");
                Console.WriteLine($"0.01f = {b}");
                Console.WriteLine($"直接比较: {a == b}");
                Console.WriteLine($"使用容差比较: {ApproximatelyEqual(a, b, 0.0001f)}");

                // 5. 解决方案：使用decimal类型
                Console.WriteLine("\n5. 解决方案：使用decimal类型:");
                decimal decimalTenth = 0.1m;
                decimal decimalSum = 0.0m;
                for (int i = 0; i < 10; i++)
                {
                    decimalSum += decimalTenth;
                }
                Console.WriteLine($"0.1m 累加10次: {decimalSum}");
                Console.WriteLine($"等于1.0m吗? {decimalSum == 1.0m}");

                Console.WriteLine("\n点击任意键退出...");
                Console.ReadKey();
            }

            // 使用容差比较浮点数
            static bool ApproximatelyEqual(float a, float b, float tolerance)
            {
                return Math.Abs(a - b) < tolerance;
            }
        }
    }
}
