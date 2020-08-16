using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public static class Extensions
{
    public static Vector2Int RandomVec2(Vector2Int _min, Vector2Int _max) => RandomVec2(_min.x, _min.y, _max.x, _max.y);
    public static Vector2Int RandomVec2(int _minX, int _minY, Vector2Int _max) => RandomVec2(_minX, _minY, _max.x, _max.y);
    public static Vector2Int RandomVec2(Vector2Int _min, int _maxX, int _maxY) => RandomVec2(_min.x, _min.y, _maxX, _maxY);
    public static Vector2Int RandomVec2(int _minX, int _minY, int _maxX, int _maxY) => new Vector2Int(Random.Range(_minX, _maxX), Random.Range(_minY, _maxY));
    public static Vector3 ToTopDownVec3(this Vector2 _value) => new Vector3(_value.x, 0, _value.y);
    public static Vector2 ToTopDownVec2(this Vector3 _value) => new Vector2(_value.x, _value.z);
    public static Vector2 Round(this Vector2 _value) => new Vector2(_value.x.RoundToInt(), _value.y.RoundToInt());
    public static Vector2 Scale(this Vector2 _a, Vector2 _b) => Vector2.Scale(_a, _b);
    public static Vector3 Scale(this Vector3 _value, float _a, float _b, float _c) => Vector3.Scale(_value, new Vector3(_a,_b,_c));
    public static Vector3 Scale(this Vector3 _a, Vector3 _b) => Vector3.Scale(_a, _b);
    public static Vector4 Scale(this Vector4 _a, Vector4 _b) => Vector4.Scale(_a, _b);
    public static Vector3 RotateTowards(this Vector3 _value, Vector3 _target, float _degrees, float _mag) => Vector3.RotateTowards(_value, _target, _degrees * Mathf.Rad2Deg, _mag);
    public static Quaternion RotateTowards(this Quaternion _value, Quaternion _target, float _degrees) => Quaternion.RotateTowards(_value, _target, _degrees);
    public static Quaternion ToQuaternion(this Vector3 _value) => Quaternion.Euler(_value);
    public static Vector2 Rotate(this Vector2 _value, float _degrees) => new Vector2(
            _value.x * Mathf.Cos(_degrees * Mathf.Deg2Rad)
            - _value.y * Mathf.Sin(_degrees * Mathf.Deg2Rad),
            _value.x * Mathf.Sin(_degrees * Mathf.Deg2Rad)
            + _value.y * Mathf.Cos(_degrees * Mathf.Deg2Rad)
            );
    public static Vector2Int RoundToVec2Int(this Vector2 _value) => new Vector2Int(_value.x.RoundToInt(), _value.y.RoundToInt());
    public static bool IsInRange(this int _value, int _min, int _max) => Mathf.Clamp(_value, _min, _max) == _value;
    public static bool IsInRange(this Vector2Int _value, Vector2Int _min, Vector2Int _max) => (_value.x.IsInRange(_min.x, _max.x) && _value.y.IsInRange(_min.y, _max.y));
    public static int Abs(this int _value) => Mathf.Abs(_value);
    public static int ToInt(this bool _value) => _value ? 1 : 0;
    public static int ToInt(this string _value) => int.TryParse(_value, out int result) ? result : 0;
    public static float ToFloat(this string _value) => float.TryParse(_value, out float result) ? result : 0;
    public static float ToFloat(this bool _value) => _value ? 1.0f : 0.0f;
    public static bool Invert(this bool _value) => !_value;
    public static float Round(this float _value, int _places) => (Mathf.Round(_value * (10 * _places)) / (10 * _places));
    public static int Clamp(this int _value, int _min, int _max) => Mathf.Clamp(_value, _min, _max);
    public static int Loop(this int _value, int _min, int _max)
    {
        if (_value < _min) return ((_max + 1) - (_min - _value)).Loop(_min, _max);
        else if (_value > _max) return((_min - 1) + (_value - _max)).Loop(_min, _max);
        return _value;
    }
    public static bool IsEven(this int _value) => _value % 2 == 0 ? true : false;
    public static bool IsOdd(this int _value) => _value % 2 == 0 ? false : true;
    public static bool ToBool(this int _value) => _value != 0;
    public static float FloorToFloat(this float _value) => Mathf.Floor(_value);
    public static int FloorToInt(this float _value) => Mathf.FloorToInt(_value);
    public static float CeilToFloat(this float _value) => Mathf.Ceil(_value);
    public static int CeilToInt(this float _value) => Mathf.CeilToInt(_value);
    public static float Clamp01(this int _value) => Mathf.Clamp01(_value);
    public static float Clamp(this float _value, float _min, float _max) => Mathf.Clamp(_value, _min, _max);
    public static float InverseLerp(this float _value, float _a, float _b) => Mathf.InverseLerp(_a, _b, _value);
    public static float Loop(this float _value, float _min, float _max)
    {
        if (_value < _min) return ((_max + 1.0f) - (_min - _value)).Loop(_min, _max);
        else if (_value > _max) return ((_min - 1.0f) + (_value - _max)).Loop(_min, _max);
        return _value;
    }
    public static float Abs(this float _value) => Mathf.Abs(_value);
    public static int RoundToInt(this float _value) => Mathf.RoundToInt(_value);
    public static bool ToBool(this float _value) => _value != 0.0f;
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
    public static void CopyToClipboard(string _copy)
    {
        TextEditor te = new TextEditor();
        te.text = _copy;
        te.SelectAll();
        te.Copy();
    }
    public static string PasteFromClipboard()
    {
        TextEditor te = new TextEditor();
        te.Paste();
        return te.text;
    }
    public static bool ToBool(this string str) => str == "true" || str == "1";
    public static string Sanitize(this string _string)
    {
        StringBuilder builder = new StringBuilder(_string);
        for(int i= 0; i < _string.Length; i++)
        {
            if(!AlphaNumeric.Contains(_string[i].ToString().ToLower()))
            {
                builder.Remove(i, 1);
                builder.Insert(i, '_');
            }
        }
        return builder.ToString();
    }
    public static string RandomString(int _length, bool _alphanumeric)
    {
        string result = "";
        if (_length <= 0) return result;
        for (int i = 0; i < _length; i++)
        {
            if(_alphanumeric) result += AlphaNumeric[Random.Range(0, AlphaNumeric.Length)];
            else result += Alphabet[Random.Range(0, Alphabet.Length)];
        }
        return result;
    }
    public static string RandomString() => RandomString(false);
    public static string RandomString(int _length) => RandomString(_length, false);
    public static string RandomString(bool _alphanumeric) => RandomString(16, _alphanumeric);

    public static string Alphabet = "abcdefghijklmnopqrstuvwxyz";
    public static string AlphaNumeric = "abcdefghijklmnopqrstuvwxyz0123456789";


    public static List<T> ToList<T>(this T[] _array) => new List<T>(_array);
    public static T RandomItem<T>(this T[] _array) => _array.ToList().RandomItem();
    public static T RandomItem<T>(this List<T> _list)
    {
        int count = _list.Count;
        return _list[Random.Range(0, count - 1)];
    }
}
