using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

internal static class Utils
{
    public static float PARRALAX_MULTIPLIER = 1f;
    public static float MIN_DIFF_FOR_PARRALAX = 0.1f;
    public static System.Random random = new System.Random();
    public static byte[] byteBuffer = new byte[1];
    public static readonly string LevelStateFilename = "LevelState.lst";

    public static char[] newlineDelim = new char[1]
    {
            '\n'
    };

    public static string[] robustNewlineDelim = new string[2]
    {
            "\r\n",
            "\n"
    };

    public static char[] spaceDelim = new char[1]
    {
            ' '
    };

    public static string[] commaDelim = new string[3]
    {
            " ,",
            ", ",
            ","
    };

    public static string[] directorySplitterDelim = new string[2]
    {
            "/",
            "\\"
    };
    
    public static Texture2D white;
    public static Texture2D gradient;
    public static Texture2D gradientLeftRight;
    public static Vector3 vec3;
    public static Color col;

    public static bool compareRects(Rect first, Rect second)
    {
        return first.Equals(second) && first.width == (double)second.width &&
               first.height == (double)second.height;
    }

    public static Vector2 getParallax(Vector2 objectPosition, Vector2 CameraPosition, float objectDepth,
        float focusDepth)
    {
        if (objectDepth >= (double)focusDepth)
        {
            var num = objectDepth - (double)focusDepth > 0.100000001490116
                ? (objectDepth - focusDepth) * PARRALAX_MULTIPLIER
                : 0.0f * PARRALAX_MULTIPLIER;
            return new Vector2((CameraPosition.x - objectPosition.x) * num, 0.0f);
        }
        var num1 = objectDepth - (double)focusDepth < -0.0500000007450581
            ? (objectDepth - focusDepth) * PARRALAX_MULTIPLIER
            : 0.0f;
        return new Vector2(
            (float)((CameraPosition.x - (double)objectPosition.x) * (num1 == 0.0 ? 0.0 : num1 - 1.0)), 0.0f);
    }

    public static float rand(float range)
    {
        return (float)(random.NextDouble() * range - random.NextDouble() * range);
    }

    public static float randm(float range)
    {
        return (float)random.NextDouble() * range;
    }

    public static float rand()
    {
        return (float)random.NextDouble();
    }

    public static byte getRandomByte()
    {
        random.NextBytes(byteBuffer);
        return byteBuffer[0];
    }

    public static bool rectEquals(Rect rec1, Rect rec2)
    {
        return Math.Abs(rec1.x - rec2.x) < 1.0 / 1000.0 && Math.Abs(rec1.x - rec2.x) < 1.0 / 1000.0 &&
               (Math.Abs(rec1.width - rec2.width) < 1.0 / 1000.0 && Math.Abs(rec1.height - rec2.height) < 1.0 / 1000.0);
    }

    public static bool flipCoin()
    {
        return random.NextDouble() > 0.5;
    }

    public static byte randomCompType()
    {
        return flipCoin() ? (byte)1 : (byte)2; //PC or Server
    }

    public static void writeToFile(string data, string filename)
    {
        using (var streamWriter = new StreamWriter(filename))
        {
            streamWriter.Write(data);
            streamWriter.Flush();
            streamWriter.Close();
        }
    }

    public static void SafeWriteToFile(string data, string filename)
    {
        var str = filename + ".tmp";
        if (!Directory.Exists(str))
            Directory.CreateDirectory(Path.GetDirectoryName(str));
        using (var streamWriter = new StreamWriter(str, false))
        {
            streamWriter.Write(data);
            streamWriter.Flush();
            streamWriter.Close();
        }
        if (File.Exists(filename))
            File.Delete(filename);
        File.Move(str, filename);
    }

    public static void SafeWriteToFile(byte[] data, string filename)
    {
        var str = filename + ".tmp";
        if (!Directory.Exists(str))
            Directory.CreateDirectory(Path.GetDirectoryName(str));
        using (var streamWriter = new StreamWriter(str, false))
        {
            streamWriter.Write(data);
            streamWriter.Flush();
            streamWriter.Close();
        }
        File.Delete(filename);
        File.Move(str, filename);
    }

    public static void appendToFile(string data, string filename)
    {
        var streamWriter = new StreamWriter(filename, true);
        streamWriter.Write(data);
        streamWriter.Close();
    }

    public static string readEntireFile(string filename)
    {
        var streamReader = new StreamReader(filename);
        var str = streamReader.ReadToEnd();
        streamReader.Close();
        return str;
    }

    public static char getRandomLetter()
    {
        return Convert.ToChar(Convert.ToInt32(Math.Floor(26.0 * random.NextDouble() + 65.0)));
    }

    public static char getRandomChar()
    {
        if (random.NextDouble() > 0.7)
            return string.Concat(Math.Min((int)Math.Floor((double)random.Next(0, 10)), 9))[0];
        return getRandomLetter();
    }

    public static char getRandomNumberChar()
    {
        return string.Concat(Math.Min((int)Math.Floor((double)random.Next(0, 10)), 9))[0];
    }

    public static Color convertStringToColor(string input)
    {
        var color = Color.white;
        var separator = new char[3]
        {
                ',',
                ' ',
                '/'
        };
        var strArray = input.Split(separator, StringSplitOptions.RemoveEmptyEntries);
        if (strArray.Length < 3)
            return color;
        var num1 = byte.MaxValue;
        var num2 = byte.MaxValue;
        var num3 = byte.MaxValue;
        for (var index = 0; index < 3; ++index)
        {
            try
            {
                var num4 = Convert.ToByte(strArray[index]);
                switch (index)
                {
                    case 0:
                        num1 = num4;
                        continue;
                    case 1:
                        num2 = num4;
                        continue;
                    case 2:
                        num3 = num4;
                        continue;
                    default:
                        continue;
                }
            }
            catch (FormatException ex)
            {
            }
            catch (OverflowException ex)
            {
            }
        }
        color = new Color(num1, num2, num3);
        return color;
    }

    public static Rect getClipRectForSpritePos(Rect bounds, Texture2D tex, Vector2 pos, float scale)
    {
        var num1 = (int)(tex.width * (double)scale);
        var num2 = (int)(tex.height * (double)scale);
        int y;
        var x = y = 0;
        var width = tex.width;
        var height = tex.height;
        if (pos.x < (double)bounds.x)
            x += (int)(bounds.x - (double)pos.x);
        if (pos.y < (double)bounds.y)
            y += (int)(bounds.y - (double)pos.y);
        if (pos.x + (double)num1 > bounds.x + bounds.width)
            width -= (int)((pos.x + (double)num1 - (bounds.x + bounds.width)) * (1.0 / scale));
        if (pos.y + (double)num2 > bounds.y + bounds.height)
            height -= (int)((pos.y + (double)num2 - (bounds.y + bounds.height)) * (1.0 / scale));
        if (x > tex.width)
        {
            x = tex.width;
            width = 0;
        }
        if (y > tex.height)
        {
            y = tex.height;
            height = 0;
        }
        return new Rect(x, y, width, height);
    }

    public static float QuadraticOutCurve(float point)
    {
        return (float)((-100000000.0 * point * (point - 2.0) - 1.0) / 100000000.0);
    }

    public static float QuadraticInCurve(float point)
    {
        return 1E+08f * point * point * point * point;
    }

    public static Rect InsetRectangle(Rect rect, int inset)
    {
        return new Rect(rect.x + inset, rect.y + inset, rect.width - inset * 2, rect.height - inset * 2);
    }

    public static Vector2 GetNearestPointOnCircle(Vector2 point, Vector2 CircleCentre, float circleRadius)
    {
        var num1 = point.x - CircleCentre.x;
        var num2 = point.y - CircleCentre.y;
        var num3 = (float)Math.Sqrt(num1 * (double)num1 + num2 * (double)num2);
        return new Vector2(CircleCentre.x + num1 / num3 * circleRadius, CircleCentre.y + num2 / num3 * circleRadius);
    }

    public static float Clamp(float val, float min, float max)
    {
        if (val < (double)min)
            val = min;
        if (val > (double)max)
            val = max;
        return val;
    }

    public static Vector2 Clamp(Vector2 val, float min, float max)
    {
        return new Vector2(Mathf.Clamp(val.x, min, max), Mathf.Clamp(val.y, min, max));
    }

    public static string RandomFromArray(string[] array)
    {
        return array[random.Next(array.Length)];
    }

    public static string GetNonRepeatingFilename(string filename, string extension, Folder f)
    {
        var str = filename;
        var num = 0;
        bool flag;
        do
        {
            flag = true;
            for (var index = 0; index < f.files.Count; ++index)
            {
                if (f.files[index].fname == str + extension)
                {
                    ++num;
                    str = string.Concat(filename, "(", num, ")");
                    flag = false;
                    break;
                }
            }
        } while (!flag);
        return str + extension;
    }

    public static string FlipRandomChars(string original, double chancePerChar)
    {
        var stringBuilder = new StringBuilder();
        for (var index = 0; index < original.Length; ++index)
        {
            if (random.NextDouble() < chancePerChar)
                stringBuilder.Append(getRandomChar());
            else
                stringBuilder.Append(original[index]);
        }
        return stringBuilder.ToString();
    }

    public static Vector2 RotatePoint(Vector2 point, float angle)
    {
        return PolarToCartesian(angle + GetPolarAngle(point), point.magnitude);
    }

    public static string GenerateReportFromException(Exception ex)
    {
        var data1 = ex.GetType() + "\r\n\r\n" + ex.Message + "\r\n\r\nSource : " + ex.Source + "\r\n\r\n" +
                    ex.StackTrace + ex + "\r\n\r\n";
        if (ex.InnerException != null)
            data1 = data1 + "Inner : ---------------\r\n\r\n" +
                    GenerateReportFromException(ex.InnerException)
                        .Replace("\t", "\0")
                        .Replace("\r\n", "\r\n\t")
                        .Replace("\0", "\t") + "\r\n\r\n";
        return data1;
    }

    public static bool FloatEquals(float a, float b)
    {
        return Math.Abs(a - b) < 9.99999974737875E-05;
    }

    public static Vector2 PolarToCartesian(float angle, float magnitude)
    {
        return new Vector2(magnitude * (float)Math.Cos(angle), magnitude * (float)Math.Sin(angle));
    }

    public static float GetPolarAngle(Vector2 point)
    {
        return (float)Math.Atan2(point.y, point.x);
    }

    public static Vector3 NormalizeRotationVector(Vector3 rot)
    {
        return new Vector3(rot.x % 6.283185f, rot.y % 6.283185f, rot.z % 6.283185f);
    }

    public static bool CheckStringIsRenderable(string input)
    {
        var str =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890,./!@#$%^&*()<>\\\":;{}_-+= \r\n?[]`~'|";
        for (var index = 0; index < input.Length; ++index)
        {
            if (!str.Contains(input[index]))
            {
                Console.WriteLine("\r\n------------------\r\nInvalid Char : {" + input[index] +
                                  "}\r\n----------------------\r\n");
                return false;
            }
        }
        return true;
    }

    public static void AppendToErrorFile(string text)
    {
        var path = "RuntimeErrors.txt";
        if (!File.Exists(path))
            File.WriteAllText(path, "fnet Runtime ErrorLog\r\n\r\n");
        using (var streamWriter = File.AppendText(path))
            streamWriter.WriteLine(text);
    }

    public static string[] SplitToTokens(string input)
    {
        return Regex.Matches(input, "[\\\"].+?[\\\"]|[^ ]+").Cast<Match>().Select(m => m.Value).ToList().ToArray();
    }

    public static string[] SplitToTokens(string[] input)
    {
        var stringBuilder = new StringBuilder();
        for (var index = 0; index < input.Length; ++index)
        {
            stringBuilder.Append(input[index]);
            stringBuilder.Append(" ");
        }
        return SplitToTokens(stringBuilder.ToString());
    }

    public static string ReadEntireContentsOfStream(Stream input)
    {
        var str = new StreamReader(input).ReadToEnd();
        input.Flush();
        input.Close();
        input.Dispose();
        return str;
    }
}