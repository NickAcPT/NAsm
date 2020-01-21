using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace ObjectWeb
{
    public abstract class EnumBase : IComparable<EnumBase>, IComparable
    {
        private static readonly Dictionary<Type, EnumBase[]> ValuesMap = new Dictionary<Type, EnumBase[]>();
        private readonly string _name;

        private readonly int _ordinal;

        protected EnumBase(int ordinal, string name)
        {
            _ordinal = ordinal;
            _name = name;
        }

        public int CompareTo(object obj)
        {
            return CompareTo((EnumBase) obj);
        }

        public int CompareTo(EnumBase other)
        {
            return _ordinal - other._ordinal;
        }

        public int Ordinal()
        {
            return _ordinal;
        }

        public string Name()
        {
            return _name;
        }

        public override string ToString()
        {
            return _name;
        }

        public static bool IsEnum(Type t)
        {
            return ValuesMap.ContainsKey(t);
        }

        protected static void RegisterValues<T>(EnumBase[] values) where T : EnumBase
        {
            ValuesMap[typeof(T)] = values;
        }

        public static EnumBase[] GetEnumValues(Type enumType)
        {
            EnumBase[] result;
            if (ValuesMap.TryGetValue(enumType, out result)) return result;

            RuntimeHelpers.RunClassConstructor(enumType.TypeHandle);
            return ValuesMap[enumType];
        }

        public static T FindByName<T>(string name) where T : EnumBase
        {
            return name == null ? null : (T) GetEnumValues(typeof(T)).FirstOrDefault(val => val.Name() == name);
        }

        public static T GetByName<T>(string name) where T : EnumBase
        {
            if (name == null) throw new ArgumentNullException("name");

            return (T) GetEnumValues(typeof(T)).First(val => val.Name() == name);
        }
    }

    public class System
    {
        public static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static int Compare(int x, int y)
        {
            return x < y ? -1 : x == y ? 0 : 1;
        }

        public static int Compare(long x, long y)
        {
            return x < y ? -1 : x == y ? 0 : 1;
        }

        public static long CurrentTimeMillis()
        {
            return (long) (DateTime.UtcNow - Epoch).TotalMilliseconds;
        }

        public static int FloorDiv(int x, int y)
        {
            var r = x / y;
            if ((x ^ y) < 0 && r * y != x) r--;

            return r;
        }

        public static int Round(float v)
        {
            return (int) Math.Floor(v + 0.5f);
        }

        public static long Round(double v)
        {
            return (long) Math.Floor(v + 0.5d);
        }

        public static int HighestOneBit(int i)
        {
            var u = (uint) i;
            u |= u >> 1;
            u |= u >> 2;
            u |= u >> 4;
            u |= u >> 8;
            u |= u >> 16;
            return (int) (u - (u >> 1));
        }
    }

    public class Arrays
    {
        public static List<T> AsList<T>(params T[] a)
        {
            return a.ToList();
        }

        public static void Fill<T>(T[] a, T val)
        {
            Fill(a, 0, a.Length, val);
        }

        public static void Fill<T>(T[] a, int from, int to, T val)
        {
            for (var i = from; i < to; i++) a[i] = val;
        }

        public static T[] CopyOf<T>(T[] a, int newSize)
        {
            var result = new T[newSize];
            a.CopyTo(result, 0);
            return result;
        }

        public static int HashCode<T>(T[] a)
        {
            if (a == null) return 0;

            var result = 1;
            foreach (var element in a) result = 31 * result + element.GetHashCode();

            return result;
        }

        public static string ToString<T>(T[] a)
        {
            var sb = new StringBuilder();
            sb.Append("[");
            for (var i = 0; i < a.Length; i++)
            {
                if (i > 0) sb.Append(", ");

                sb.Append(a[i]);
            }

            sb.Append("]");
            return sb.ToString();
        }
    }

    public static class Collections
    {
        public static object Put(IDictionary map, object key, object value)
        {
            var result = map.Contains(key) ? map[key] : null;
            map[key] = value;
            return result;
        }

        public static TV Put<TK, TV>(IDictionary<TK, TV> map, TK key, TV value)
        {
            TV result;
            if (!map.TryGetValue(key, out result)) result = default;

            map[key] = value;
            return result;
        }

        public static TV Remove<TK, TV>(IDictionary<TK, TV> map, TK key)
        {
            TV result;
            if (map.TryGetValue(key, out result))
            {
                map.Remove(key);
                return result;
            }

            return default;
        }

        public static T RemoveFirst<T>(LinkedList<T> linkedList)
        {
            var result = linkedList.First.Value;
            linkedList.RemoveFirst();
            return result;
        }

        public static T RemoveLast<T>(LinkedList<T> linkedList)
        {
            var result = linkedList.Last.Value;
            linkedList.RemoveLast();
            return result;
        }

        public static void PutAll<TCk, TCv, TIk, TIv>(IDictionary<TCk, TCv> collection, IDictionary<TIk, TIv> items)
            where TIk : TCk where TIv : TCv
        {
            foreach (var e in items) collection[e.Key] = e.Value;
        }

        public static void AddAll<T>(ICollection<T> collection, IEnumerable<T> items)
        {
            foreach (var item in items) collection.Add(item);
        }

        public static T[] ToArray<T>(ICollection<T> collection)
        {
            return ToArray(collection, new T[collection.Count]);
        }

        public static T[] ToArray<T>(ICollection<T> collection, T[] array)
        {
            var i = 0;
            foreach (var item in collection) array[i++] = item;

            return array;
        }
    }

    public static class Runtime
    {
        public static char CodePointAt(this string source, int place)
        {
            return source[place];
        }

        public static char OffsetByCodePoints(this string source, int place, int offset)
        {
            return (char) (source[place] + offset);
        }

        public static string ReplaceAll(this string source, string pattern, string replacement)
        {
            return Regex.Replace(source, pattern, replacement);
        }

        public static int BitCount(long i)
        {
            // HD, Figure 5-14
            i = i - ((i >> 1) & 0x5555555555555555L);
            i = (i & 0x3333333333333333L) + ((i >> 2) & 0x3333333333333333L);
            i = (i + (i >> 4)) & 0x0f0f0f0f0f0f0f0fL;
            i = i + (i >> 8);
            i = i + (i >> 16);
            i = i + (i >> 32);
            return (int) i & 0x7f;
        }

        public static int NumberOfLeadingZeros(long i)
        {
            // HD, Figure 5-6
            if (i == 0)
                return 64;
            var n = 1;
            var x = (int) (i >> 32);
            if (x == 0)
            {
                n += 32;
                x = (int) i;
            }

            if (x >> 16 == 0)
            {
                n += 16;
                x <<= 16;
            }

            if (x >> 24 == 0)
            {
                n += 8;
                x <<= 8;
            }

            if (x >> 28 == 0)
            {
                n += 4;
                x <<= 4;
            }

            if (x >> 30 == 0)
            {
                n += 2;
                x <<= 2;
            }

            n -= x >> 31;
            return n;
        }

        public static int RotateLeft(this int value, int count)
        {
            return (value << count) | (value >> (32 - count));
        }

        public static int RotateRight(this int value, int count)
        {
            return (value >> count) | (value << (32 - count));
        }

        public static bool IsAssignableFrom(Type baseType, Type type)
        {
            if (baseType.IsAssignableFrom(type)) return true;

            var baseTypeGeneric = baseType.IsGenericType ? baseType.GetGenericTypeDefinition() : baseType;
            var typeGeneric = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
            return baseTypeGeneric.IsAssignableFrom(typeGeneric);
        }

        public static bool InstanceOf(object o, Type type)
        {
            if (o == null) return false;

            if (type.IsInstanceOfType(o)) return true;

            return o.GetType().IsGenericType && o.GetType().GetGenericTypeDefinition().IsAssignableFrom(type);
        }

        public static string Substring(string s, int from)
        {
            return Substring(s, from, s.Length);
        }

        public static string Substring(string s, int from, int to)
        {
            return s.Substring(from, to - from);
        }

        public static string GetSimpleName(this Type t)
        {
            var name = t.Name;
            var index = name.IndexOf('`');
            return index == -1 ? name : name.Substring(0, index);
        }

        public static FieldInfo[] GetDeclaredFields(Type clazz)
        {
            return clazz.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly |
                                   BindingFlags.Instance);
        }

        public static bool HasAttribute(FieldAttributes attributes, FieldAttributes flag)
        {
            return (attributes & flag) != 0;
        }

        public static CustomAttributeData GetCustomAttribute(MemberInfo info, Type attributeType)
        {
            foreach (var a in CustomAttributeData.GetCustomAttributes(info))
                if (a.Constructor.DeclaringType == attributeType)
                    return a;

            return null;
        }

        public static float IntBitsToFloat(int readInt)
        {
            var bytes = BitConverter.GetBytes(readInt);
            return BitConverter.ToSingle(bytes, 0);
        }

        public static double LongBitsToDouble(long l)
        {
            return BitConverter.Int64BitsToDouble(l);
        }

        public static int FloatToIntBits(in float f)
        {
            return BitConverter.SingleToInt32Bits(f);
        }

        public static int FloatToRawIntBits(in float value)
        {
            var bytes = BitConverter.GetBytes(value);
            return BitConverter.ToInt32(bytes, 0);
        }

        public static long DoubleToRawLongBits(in double value)
        {
            return BitConverter.DoubleToInt64Bits(value);
        }

        public static List<Type> GetParameterTypes(this ConstructorInfo info)
        {
            return info.GetParameters().Select(c => c.ParameterType).ToList();
        }

        public static List<Type> GetParameterTypes(this MethodInfo info)
        {
            return info.GetParameters().Select(c => c.ParameterType).ToList();
        }

        public static int NumberOfTrailingZeros(in long word)
        {
            var mask = 1;
            for (var i = 0; i < 64; i++, mask <<= 1)
                if ((word & mask) != 0)
                    return i;

            return 64;
        }
    }

    public class IdentityHashMap<TK, TV> : Dictionary<TK, TV>
    {
        public IdentityHashMap() : base(new IdentityEqualityComparer<TK>())
        {
        }
    }

    public class IdentityEqualityComparer<T> : IEqualityComparer<T>
    {
        public bool Equals(T x, T y)
        {
            return ReferenceEquals(x, y);
        }

        public int GetHashCode(T obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }
    }

    public class Uuid : IEquatable<Uuid>, IComparable<Uuid>
    {
        private readonly long _leastSigBits;
        private readonly long _mostSigBits;

        public Uuid(long mostSigBits, long leastSigBits)
        {
            _mostSigBits = mostSigBits;
            _leastSigBits = leastSigBits;
        }

        public int CompareTo(Uuid other)
        {
            return _mostSigBits < other._mostSigBits
                ? -1
                : _mostSigBits > other._mostSigBits
                    ? 1
                    : _leastSigBits < other._leastSigBits
                        ? -1
                        : _leastSigBits > other._leastSigBits
                            ? 1
                            : 0;
        }

        public bool Equals(Uuid other)
        {
            return this == other;
        }

        public long GetMostSignificantBits()
        {
            return _mostSigBits;
        }

        public long GetLeastSignificantBits()
        {
            return _leastSigBits;
        }

        public override string ToString()
        {
            return ((_mostSigBits >> 32) & 0xFFFFFFFF).ToString("x8") + "-" +
                   ((_mostSigBits >> 16) & 0xFFFF).ToString("x4") + "-" +
                   (_mostSigBits & 0xFFFF).ToString("x4") + "-" +
                   ((_leastSigBits >> 48) & 0xFFFF).ToString("x4") + "-" +
                   (_leastSigBits & 0xFFFFFFFFFFFF).ToString("x12");
        }

        public override bool Equals(object obj)
        {
            return this == obj as Uuid;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hilo = _mostSigBits ^ _leastSigBits;
                return (int) (hilo >> 32) ^ (int) hilo;
            }
        }

        public static bool operator ==(Uuid first, Uuid second)
        {
            return ReferenceEquals(first, second) || !ReferenceEquals(first, null) && !ReferenceEquals(second, null) &&
                   first._mostSigBits == second._mostSigBits && first._leastSigBits == second._leastSigBits;
        }

        public static bool operator !=(Uuid first, Uuid second)
        {
            return !(first == second);
        }
    }


    public static class Lists
    {
        public static void Add<T>(this IList<T> list, int index, T value)
        {
            list.Insert(index, value);
        }

        public static T RemoveAtReturningValue<T>(this IList<T> list, int index)
        {
            var value = list[index];
            list.RemoveAt(index);
            return value;
        }
    }

    public static class Maps
    {
        public static TV GetOrDefault<TK, TV>(this IDictionary<TK, TV> map, TK key, TV defaultValue)
        {
            TV result;
            return map.TryGetValue(key, out result) ? result : defaultValue;
        }

        public static TV GetOrNull<TK, TV>(this IDictionary<TK, TV> map, TK key) where TV : class
        {
            TV result;
            return map.TryGetValue(key, out result) ? result : null;
        }

        public static TV? GetOrNullable<TK, TV>(this IDictionary<TK, TV> map, TK key) where TV : struct
        {
            TV result;
            return map.TryGetValue(key, out result) ? result : new TV?();
        }
    }
}