using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;

//  Fletcher's Useful Functions

public static class Extensions
{
    public static Vector2Int RandomVec2(Vector2Int _min, Vector2Int _max) => RandomVec2(_min.x, _min.y, _max.x, _max.y);
    /// <summary>
    /// Return a random Vector2Int with specified axis limits
    /// </summary>
    public static Vector2Int RandomVec2(int _minX, int _minY, Vector2Int _max) => RandomVec2(_minX, _minY, _max.x, _max.y);
    /// <summary>
    /// Return a random Vector2Int with specified axis limits
    /// </summary>
    public static Vector2Int RandomVec2(Vector2Int _min, int _maxX, int _maxY) => RandomVec2(_min.x, _min.y, _maxX, _maxY);
    /// <summary>
    /// Return a random Vector2Int with specified axis limits
    /// </summary>
    public static Vector2Int RandomVec2(int _minX, int _minY, int _maxX, int _maxY) => new Vector2Int(Random.Range(_minX, _maxX), Random.Range(_minY, _maxY));
    /// <summary>
    /// Return a random Vector3 with specified axis limits
    /// </summary>
    public static Vector3 RandomVec3(float _min, float _max) => new Vector3(Random.Range(_min, _max), Random.Range(_min, _max), Random.Range(_min, _max));
    /// <summary>
    /// Convert this Vector2 to a Vector3 (X,_,Y)
    /// </summary>
    public static Vector3 ToTopDownVec3(this Vector2 _value) => new Vector3(_value.x, 0, _value.y);
    /// <summary>
    /// Convert this Vector3 to a Vector2 (X,Z)
    /// </summary>
    public static Vector2 ToTopDownVec2(this Vector3 _value) => new Vector2(_value.x, _value.z);
    /// <summary>
    /// Round this Vector2 to the closest axis integers
    /// </summary>
    public static Vector2 Round(this Vector2 _value) => new Vector2(_value.x.RoundToInt(), _value.y.RoundToInt());
    /// <summary>
    /// Scale this Vector2 by a specified value
    /// </summary>
    public static Vector2 Scale(this Vector2 _a, Vector2 _b) => Vector2.Scale(_a, _b);
    /// <summary>
    /// Scale this Vector3 by a specified value
    /// </summary>
    public static Vector3 Scale(this Vector3 _a, Vector3 _b) => Vector3.Scale(_a, _b);
    /// <summary>
    /// Scale this Vector4 by a specified value
    /// </summary>
    public static Vector4 Scale(this Vector4 _a, Vector4 _b) => Vector4.Scale(_a, _b);
    /// <summary>
    /// Return true if these two Vector2 values approximately equal.
    /// </summary>
    public static bool Approximately(this Vector2 _a, Vector2 _b)
    {
        bool x = Mathf.Approximately(_a.x, _b.x);
        bool y = Mathf.Approximately(_a.y, _b.y);
        return x && y;
    }
    /// <summary>
    /// Return true if these two Vector3 values approximately equal.
    /// </summary>
    public static bool Approximately(this Vector3 _a, Vector3 _b)
    {
        bool x = Mathf.Approximately(_a.x, _b.x);
        bool y = Mathf.Approximately(_a.y, _b.y);
        bool z = Mathf.Approximately(_a.z, _b.z);
        return x && y && z;
    }
    /// <summary>
    /// Rorate this Vector3 towards a target value by specified degrees and max magnitude
    /// </summary>
    public static Vector3 RotateTowards(this Vector3 _value, Vector3 _target, float _degrees, float _mag) => Vector3.RotateTowards(_value, _target, _degrees * Mathf.Rad2Deg, _mag);
    /// <summary>
    /// Rotate this Quaternion towards a target value by specified degrees
    /// </summary>
    public static Quaternion RotateTowards(this Quaternion _value, Quaternion _target, float _degrees) => Quaternion.RotateTowards(_value, _target, _degrees);
    /// <summary>
    /// Convert this Vector3 to a Quaternion
    /// </summary>
    public static Quaternion ToQuaternion(this Vector3 _value) => Quaternion.Euler(_value);
    /// <summary>
    /// Rotate this Vector2 by a specified number of degrees
    /// </summary>
    public static Vector2 Rotate(this Vector2 _value, float _degrees) => new Vector2(
        _value.x * Mathf.Cos(_degrees * Mathf.Deg2Rad) -
        _value.y * Mathf.Sin(_degrees * Mathf.Deg2Rad),
        _value.x * Mathf.Sin(_degrees * Mathf.Deg2Rad) +
        _value.y * Mathf.Cos(_degrees * Mathf.Deg2Rad)
    );
    /// <summary>
    /// Round this Vector2 to a Vector2Int
    /// </summary>
    public static Vector2Int RoundToVec2Int(this Vector2 _value) => new Vector2Int(_value.x.RoundToInt(), _value.y.RoundToInt());
    /// <summary>
    /// Return the angle of this Vector2
    /// </summary>
    public static float ToAngle(this Vector2 _value)
    {
        if (_value == Vector2.zero) return 0.0f;
        return Mathf.Atan2(_value.x, _value.y) * Mathf.Rad2Deg;
    }
    /// <summary>
    /// Return true if this integer is within the bounds of two other values
    /// </summary>
    public static bool IsInRange(this int _value, int _min, int _max) => Mathf.Clamp(_value, _min, _max) == _value;
    /// <summary>
    /// Return true if this Vector2 is within the bounds of two other values
    /// </summary>
    public static bool IsInRange(this Vector2Int _value, Vector2Int _min, Vector2Int _max) => (_value.x.IsInRange(_min.x, _max.x) && _value.y.IsInRange(_min.y, _max.y));
    /// <summary>
    /// Return the Absolute value of this integer
    /// </summary>
    public static int Abs(this int _value) => Mathf.Abs(_value);
    /// <summary>
    /// Clamp this Vector2 to within the bounds of two specified values
    /// </summary>
    public static Vector2 Clamp(this Vector2 _value, Vector2 _min, Vector2 _max)
    {
        _value.x = _value.x.Clamp(_min.x, _max.x);
        _value.y = _value.y.Clamp(_min.y, _max.y);
        return _value;
    }
    /// <summary>
    /// Create a Vector2 from this float
    /// </summary>
    public static Vector2 ToVector2(this float _value) => new Vector2(_value, _value);
    /// <summary>
    /// Clamp this Vector3 to within the bounds of two specified values
    /// </summary>
    public static Vector3 Clamp(this Vector3 _value, Vector3 _min, Vector3 _max)
    {
        _value.x = _value.x.Clamp(_min.x, _max.x);
        _value.y = _value.y.Clamp(_min.y, _max.y);
        _value.z = _value.z.Clamp(_min.z, _max.z);
        return _value;
    }
    /// <summary>
    /// Creat a Vector3 from this float
    /// </summary>
    public static Vector3 ToVector3(this float _value) => new Vector3(_value, _value, _value);
    /// <summary>
    /// Convert this boolean to an integer
    /// </summary>
    public static int ToInt(this bool _value) => _value ? 1 : 0;
    /// <summary>
    /// Convert this boolean to an integer
    /// </summary>
    public static int ToInt(this string _value) => int.TryParse(_value, out int result) ? result : 0;
    /// <summary>
    /// Convert this string to an integer
    /// </summary>
    public static float ToFloat(this string _value) => float.TryParse(_value, out float result) ? result : 0;
    /// <summary>
    /// Convert this boolean to a float
    /// </summary>
    public static float ToFloat(this bool _value) => _value ? 1.0f : 0.0f;
    /// <summary>
    /// Invert this boolean
    /// </summary>
    public static bool Inv(this bool _value) => !_value;
    /// <summary>
    /// Round this value
    /// </summary>
    public static float Round(this float _value, int _places) => Mathf.Round(_value * (10 * _places)) / (10 * _places);
    /// <summary>
    /// Return this value clamped between a min and max
    /// </summary>
    public static int Clamp(this int _value, int _min, int _max) => Mathf.Clamp(_value, _min, _max);
    /// <summary>
    /// Return the remainder after looping a value between a min and max
    /// </summary>
    public static int Loop(this int _value, int _min, int _max)
    {
        if (_value < _min) return ((_max + 1) - (_min - _value)).Loop(_min, _max);
        else if (_value > _max) return ((_min - 1) + (_value - _max)).Loop(_min, _max);
        return _value;
    }
    /// <summary>
    /// Return true if this integer is even
    /// </summary>
    public static bool IsEven(this int _value) => _value % 2 == 0 ? true : false;
    /// <summary>
    /// Return true if this integer is odd
    /// </summary>
    public static bool IsOdd(this int _value) => _value % 2 == 0 ? false : true;
    /// <summary>
    /// Convert this integer to a boolean
    /// </summary>
    public static bool ToBool(this int _value) => _value != 0;
    /// <summary>
    /// Floor this float value
    /// </summary>
    public static float FloorToFloat(this float _value) => Mathf.Floor(_value);
    /// <summary>
    /// Floor this float to an integer
    /// </summary>
    public static int FloorToInt(this float _value) => Mathf.FloorToInt(_value);
    /// <summary>
    /// Return this value clamped between zero and one
    /// </summary>
    public static float Clamp01(this int _value) => Mathf.Clamp01(_value);
    /// <summary>
    /// Return this value clamped between a min and max
    /// </summary>
    public static float Clamp(this float _value, float _min, float _max) => Mathf.Clamp(_value, _min, _max);
    /// <summary>
    /// Inverse linear interpolate this value between a min and max
    /// </summary>
    public static float InverseLerp(this float _value, float _min, float _max) => Mathf.InverseLerp(_min, _max, _value);
    /// <summary>
    /// Return the remainder after looping a value between a min and max
    /// </summary>
    public static float Loop(this float _value, float _min, float _max)
    {
        if (_value < _min) return ((_max + 1.0f) - (_min - _value)).Loop(_min, _max);
        else if (_value > _max) return ((_min - 1.0f) + (_value - _max)).Loop(_min, _max);
        return _value;
    }
    /// <summary>
    /// Return the Absolute value of this float
    /// </summary>
    public static float Abs(this float _value) => Mathf.Abs(_value);
    /// <summary>
    /// Return the distance between this Transform and another
    /// </summary>
    public static float Distance(this Transform _a, Transform _b) => Vector3.Distance(_a.position, _b.position);
    /// <summary>
    /// Return the distance between this Transform and a Vector3
    /// </summary>
    public static float Distance(this Transform _a, Vector3 _b) => Vector3.Distance(_a.position, _b);
    /// <summary>
    /// Return the distance between this Vector3 and a Transform
    /// </summary>
    public static float Distance(this Vector3 _a, Transform _b) => Vector3.Distance(_a, _b.position);
    /// <summary>
    /// Return the index of a List<Vector3> value which is closest to this Vector3
    /// </summary>
    public static int ClosestPoint(this Vector3 _value, List<Vector3> _list)
    {
        int result = 0;
        float closestDist = Mathf.Infinity;
        for (int i = 0; i < _list.Count; i++)
        {
            float dist = Vector3.Distance(_value, _list[i]);
            if (dist < closestDist)
            {
                closestDist = dist;
                result = i;
            }
        }
        return result;
    }
    /// <summary>
    /// Return the index of this List<Vector3> which is closest to a specified target
    /// </summary>
    public static int ClosestPoint(this List<Vector3> _list, Vector3 _value) => ClosestPoint(_value, _list);
    /// <summary>
    /// Round this float value to an integer
    /// </summary>
    public static int RoundToInt(this float _value) => Mathf.RoundToInt(_value);
    /// <summary>
    /// Ceil this float value to an integer
    /// </summary>
    public static int CeilToInt(this float _value) => Mathf.CeilToInt(_value);
    /// <summary>
    /// Convert this float to a boolean value
    /// </summary>
    public static bool ToBool(this float _value) => _value != 0.0f;
    /// <summary>
    /// Clamp this angle to a given center point, and range
    /// </summary>
    public static float ClampAngle(this float _value, float _center, float _range)
    {
        float low = _center + _range;
        float high = _center + 360.0f - _range;
        float mid = (low + high) / 2.0f;
        float v = _value;
        if (v < 0.0f) v += 360.0f;
        if (v > low && v < mid) return low;
        if (v < high && v > mid) return high;
        return v;
    }
    /// <summary>
    /// Copy this string value to the clipboard
    /// </summary>
    public static void CopyToClipboard(string _copy)
    {
        TextEditor te = new TextEditor();
        te.text = _copy;
        te.SelectAll();
        te.Copy();
    }
    /// <summary>
    /// Return a string value fromthe clipboard
    /// </summary>
    public static string PasteFromClipboard()
    {
        TextEditor te = new TextEditor();
        te.Paste();
        return te.text;
    }
    /// <summary>
    /// Convert this string to a boolean
    /// </summary>
    public static bool ToBool(this string str) => str.ToLower() == "true" || str == "1" || str.ToLower() == "t";
    /// <summary>
    /// Return a boolean with equal chance of true or false
    /// </summary>
    public static bool CoinFlip() => (UnityEngine.Random.Range(0.0f, 1.0f) > 0.5f);
    /// <summary>
    /// Remove non-letters from this string, swap them for '_'
    /// </summary>
    public static string Sanitize(this string _string)
    {
        if (string.IsNullOrEmpty(_string))
        {
            return string.Empty;
        }
        StringBuilder builder = new StringBuilder(_string);
        for (int i = 0; i < _string.Length; i++)
        {
            if (!AlphaNumeric.Contains(_string[i].ToString().ToLower()))
            {
                builder.Remove(i, 1);
                builder.Insert(i, '_');
            }
        }
        return builder.ToString();
    }
    /// <summary>
    /// Remove non-letters from this string, swap them for '_'
    /// </summary>
    public static string LettersOnly(this string _string)
    {
        if (string.IsNullOrEmpty(_string))
        {
            return string.Empty;
        }
        StringBuilder builder = new StringBuilder(_string);
        for (int i = 0; i < _string.Length; i++)
        {
            if (!Alphabet.Contains(_string[i].ToString().ToLower()))
            {
                builder.Remove(i, 1);
                builder.Insert(i, '_');
            }
        }
        return builder.ToString();
    }
    /// <summary>
    /// Remove non-letters/periods from this string, swap them for '_'
    /// </summary>
    public static string LettersAndPeriodsOnly(this string _string)
    {
        if (string.IsNullOrEmpty(_string))
        {
            return string.Empty;
        }
        StringBuilder builder = new StringBuilder(_string);
        for (int i = 0; i < _string.Length; i++)
        {
            if (!AlphabetAndPeriod.Contains(_string[i].ToString().ToLower()))
            {
                builder.Remove(i, 1);
                builder.Insert(i, '_');

            }
        }
        return builder.ToString();
    }
    /// <summary>
    /// Convert this string to title case (This is title case)
    /// </summary>
    public static string ToTitleCase(this string _string) => string.IsNullOrEmpty(_string) ? string.Empty : char.ToUpper(_string[0]) + (_string.Substring(1).ToLower());
    /// <summary>
    /// Return a string of random letters and numbers of specified length
    /// </summary>
    public static string RandomString(int _length, bool _alphanumeric)
    {
        string result = "";
        if (_length <= 0) return result;
        for (int i = 0; i < _length; i++)
        {
            if (_alphanumeric) result += AlphaNumeric[Random.Range(0, AlphaNumeric.Length)];
            else result += Alphabet[Random.Range(0, Alphabet.Length)];
        }
        return result;
    }
    /// <summary>
    /// Return a string of 16 random letters
    /// </summary>
    public static string RandomString() => RandomString(false);
    /// <summary>
    /// Return a string of random letters of specified length
    /// </summary>
    public static string RandomString(int _length) => RandomString(_length, false);
    /// <summary>
    /// Return a string of 16 random letters and numbers
    /// </summary>
    public static string RandomString(bool _alphanumeric) => RandomString(16, _alphanumeric);
    /// <summary>
    /// Lower-case alphabet
    /// </summary>
    public static string Alphabet = "abcdefghijklmnopqrstuvwxyz";
    /// <summary>
    /// Lower-case alphabet and period
    /// </summary>
    public static string AlphabetAndPeriod = "abcdefghijklmnopqrstuvwxyz.";
    /// <summary>
    /// Lower-case alphabet and numbers 0 - 9
    /// </summary>
    public static string AlphaNumeric = "abcdefghijklmnopqrstuvwxyz0123456789";
    /// <summary>
    /// Convert an array into a List
    /// </summary>
    public static List<T> ToList<T>(this T[] _array) => new List<T>(_array);
    /// <summary>
    /// Return a random item from this array
    /// </summary>
    public static T RandomItem<T>(this T[] _array) => _array.ToList().RandomItem();
    /// <summary>
    /// Return a random item from this List
    /// </summary>
    public static T RandomItem<T>(this List<T> _list)
    {
        int count = _list.Count;
        return _list[Random.Range(0, count - 1)];
    }
    /// <summary>
    /// Return the last item from this List
    /// </summary>
    public static T LastItem<T>(this List<T> _list)
    {
        if (_list.Count == 0) return default;
        return _list[_list.Count - 1];
    }
    /// <summary>
    /// Return this List with it's order reversed
    /// </summary>
    public static List<T> Reversed<T>(this List<T> _list)
    {
        List<T> reversedList = new List<T>(_list);
        reversedList.Reverse();
        return reversedList;
    }
    /// <summary>
    /// Sort this List of strings into alphabetical order
    /// </summary>
    public static void Aphabetise(this List<string> _list) => _list.Sort((a, b) => string.Compare(a, b));
    /// <summary>
    /// Convert this Color to a hexadecimal string
    /// </summary>
    public static string ToHex(this Color col) => ColorUtility.ToHtmlStringRGB(col);
    /// <summary>
    /// Return a saturated Color with this hue
    /// </summary>
    public static Color NumberToColor(this float _value, float _max) => Color.HSVToRGB(Mathf.InverseLerp(0.0f, _max, _value), 1.0f, 1.0f);
    /// <summary>
    /// Return a saturated Color with this hue
    /// </summary>
    public static Color NumberToColor(this int _value, int _max) => Color.HSVToRGB(Mathf.InverseLerp(0.0f, _max, _value), 1.0f, 1.0f);
    /// <summary>
    /// Return the value of Phi (Mathf.PI * (3.0f - Mathf.Sqrt(5.0f)));
    /// </summary>
    public static float Phi = Mathf.PI * (3.0f - Mathf.Sqrt(5.0f));
    /// <summary>
    /// Return a list of 3D points using the Fibonacci algorithm, with a given number of samples
    /// </summary>
    public static List<Vector3> FibonacciPoints(int _samples)
    {
        List<Vector3> points = new List<Vector3>();
        for (int i = 0; i < _samples; i++)
        {
            float y = 1.0f - (i / (float)(_samples - 1) * 2.0f);
            float radius = Mathf.Sqrt(1.0f - (y * y));
            float theta = Phi * i;
            float x = Mathf.Cos(theta) * radius;
            float z = Mathf.Sin(theta) * radius;
            points.Add(new Vector3(x, y, z));
        }
        return points;
    }
    /// <summary>
    /// Transform the bounds of this MonoBehaviour
    /// </summary>
    public static Bounds TransformBounds(this MonoBehaviour _mb, Bounds _localBounds)
    {
        var center = _mb.transform.TransformPoint(_localBounds.center);
        var extents = _localBounds.extents;
        var axisX = _mb.transform.TransformVector(extents.x, 0, 0);
        var axisY = _mb.transform.TransformVector(0, extents.y, 0);
        var axisZ = _mb.transform.TransformVector(0, 0, extents.z);
        extents.x = Mathf.Abs(axisX.x) + Mathf.Abs(axisY.x) + Mathf.Abs(axisZ.x);
        extents.y = Mathf.Abs(axisX.y) + Mathf.Abs(axisY.y) + Mathf.Abs(axisZ.y);
        extents.z = Mathf.Abs(axisX.z) + Mathf.Abs(axisY.z) + Mathf.Abs(axisZ.z);
        return new Bounds { center = center, extents = extents };
    }
    /// <summary>
    /// Log a debug message with an auto-generated color
    /// </summary>
    public static void Log(string _log = " ", Object _obj = null)
    {
        string caller = new StackTrace().GetFrame(1).GetMethod().Name;
        UnityEngine.Debug.Log($"<b>F{Time.frameCount} {(_obj != null ? _obj.name + ":" : "")} <color={Color.HSVToRGB(caller.GetHashCode() / (float)int.MaxValue * 0.5f + 0.5f, 0.6f, 0.8f, false).ToHex()}>{caller}()</color></b> {_log}", _obj);
    }
    /// <summary>
    /// Log a debug warning with an auto-generated color
    /// </summary>
    public static void LogWarning(string _log = " ", Object _obj = null)
    {
        string caller = new StackTrace().GetFrame(1).GetMethod().Name;
        UnityEngine.Debug.LogWarning($"<b>F{Time.frameCount} {(_obj != null ? _obj.name + ":" : "")} <color={Color.HSVToRGB(caller.GetHashCode() / (float)int.MaxValue * 0.5f + 0.5f, 0.6f, 0.8f, false).ToHex()}>{caller}()</color></b> {_log}", _obj);
    }
    /// <summary>
    /// Check if a GUID is NULL or empty
    /// </summary>
    public static bool IsNullOrEmpty(this System.Guid guid) => guid == null || guid == System.Guid.Empty;
}