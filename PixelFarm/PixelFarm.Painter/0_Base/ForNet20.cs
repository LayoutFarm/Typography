
//for .NET 2.0 
namespace System
{
    //public delegate R Func<R>();
    //public delegate R Func<T, R>(T t1);
    //public delegate R Func<T1, T2, R>(T1 t1, T2 t2);
    //public delegate R Func<T1, T2, T3, R>(T1 t1, T2 t2, T3 t3);
    public delegate void Action<in T1, in T2>(T1 arg1, T2 arg2);
    public delegate void Action<in T1, in T2, in T3>(T1 arg1, T2 arg2, T3 arg3);
    public delegate void Action<in T1, in T2, in T3, in T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
    public delegate void Action<in T1, in T2, in T3, in T4, in T5>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
    public delegate void Action<in T1, in T2, in T3, in T4, in T5, in T6>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);
    public delegate void Action<in T1, in T2, in T3, in T4, in T5, in T6, in T7>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);
    public delegate void Action<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8);

#if !NETSTANDARD
    public delegate TResult Func<out TResult>();
    public delegate TResult Func<in T, out TResult>(T arg);
    public delegate TResult Func<in T1, in T2, out TResult>(T1 arg1, T2 arg2);
    public delegate TResult Func<in T1, in T2, in T3, out TResult>(T1 arg1, T2 arg2, T3 arg3);
    public delegate TResult Func<in T1, in T2, in T3, in T4, out TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
    public delegate TResult Func<in T1, in T2, in T3, in T4, in T5, out TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
    public delegate TResult Func<in T1, in T2, in T3, in T4, in T5, in T6, out TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);
    public delegate TResult Func<in T1, in T2, in T3, in T4, in T5, in T6, in T7, out TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);
    public delegate TResult Func<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, out TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8);
#endif 
}
namespace System.Runtime.InteropServices
{
    public partial class TargetedPatchingOptOutAttribute : Attribute
    {
        public TargetedPatchingOptOutAttribute(string msg) { }
    }
}

namespace System.Runtime.CompilerServices
{
    public partial class ExtensionAttribute : Attribute { }
}
//namespace System.Linq
//{
//    public class dummy { }
//}

namespace System.Collections.Generic
{
    public class HashSet<T> : IEnumerable<T>, ICollection<T>
    {
        //for .NET 2.0
        Dictionary<int, T> _dic = new Dictionary<int, T>();
        public HashSet() { }
        public HashSet(IEnumerable<T> org)
        {
            foreach (T t in org)
            {
                Add(t);
            }
        }
        public bool Add(T data)
        {
            int hashCode = data.GetHashCode();
            if (_dic.ContainsKey(hashCode))
            {
                return false;
            }
            _dic[hashCode] = data;
            return true;
        }
        public bool Remove(T data)
        {
            return _dic.Remove(data.GetHashCode());
        }
        public bool Contains(T data)
        {
            return _dic.ContainsKey(data.GetHashCode());
        }
        public void Clear()
        {
            _dic.Clear();
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (T t in _dic.Values)
            {
                yield return t;
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (T t in _dic.Values)
            {
                yield return t;
            }
        }

        void ICollection<T>.Add(T item)
        {
            Add(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {

            int ii = arrayIndex;
            foreach (T t in _dic.Values)
            {
                array[ii] = t;
                ++ii;
            }
        }
        public int Count => _dic.Count;

        public bool IsReadOnly => false;


        public void UnionWith(HashSet<T> another)
        {
            foreach (T a in another)
            {
                Add(a);
            }
        }
    }

    public static class MyLinq
    {
        public static System.Collections.Generic.IEnumerable<TResult> Cast<TResult>(this System.Collections.IEnumerable source)
        {
            foreach (object o in source)
            {
                yield return (TResult)o;
            }
        }
        public static IEnumerable<T> Distinct<T>(this IEnumerable<T> source)
        {
            Dictionary<T, bool> tmp = new Dictionary<T, bool>();
            foreach (var t in source)
            {
                if (!tmp.ContainsKey(t))
                {
                    tmp.Add(t, true);
                    yield return t;
                }
            }
        }
        public static IEnumerable<T> OrderBy<T>(this IEnumerable<T> list, Comparison<T> comparison)
        {
            System.Collections.Generic.List<T> tmp = new List<T>(list);
            tmp.Sort(comparison);
            return tmp;
        }
        public static IEnumerable<T> OrderBy<T>(this IEnumerable<T> list, Func<T, IComparable> getKey)
        {
            System.Collections.Generic.List<T> tmp = new List<T>(list);
            tmp.Sort((a, b) => getKey(a).CompareTo(getKey(b)));
            return tmp;
        }
        public static System.Collections.Generic.Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement>(this System.Collections.Generic.IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
        {
            System.Collections.Generic.Dictionary<TKey, TElement> newdic = new Dictionary<TKey, TElement>();
            foreach (var s in source)
            {
                newdic.Add(keySelector(s), elementSelector(s));
            }
            return newdic;
        }
        public static bool Contains<T>(this T[] arr, T elem)
        {
            for (int i = 0; i < arr.Length; ++i)
            {
                if (arr[i].Equals(elem))
                {
                    return true;
                }
            }
            return false;
        }
      
        public static bool Contains<T>(this IEnumerable<T> list, T value)
        {
            foreach (T elem in list)
            {
                if (elem.Equals(value))
                {
                    return true;
                }
            }
            return false;
        }
        public static IEnumerable<U> OfType<U>(this IEnumerable list)
        {
            foreach (object elem in list)
            {
                if (elem is U u)
                {
                    yield return u;
                }
            }
        }
        public static double Min<T>(this IEnumerable<T> list, Func<T, double> evalFunc)
        {
            double min = double.MaxValue;
            bool hasSomeElem = false;
            foreach (T elem in list)
            {
                hasSomeElem = true;
                double v = evalFunc(elem);
                if (v < min)
                {
                    min = v;
                }
            }

            if (!hasSomeElem)
            {
                throw new NotSupportedException();
            }
            return min;
        }
        public static int Min<T>(this IEnumerable<T> list, Func<T, int> evalFunc)
        {
            int min = int.MaxValue;
            bool hasSomeElem = false;
            foreach (T elem in list)
            {
                hasSomeElem = true;
                int v = evalFunc(elem);
                if (v < min)
                {
                    min = v;
                }
            }

            if (!hasSomeElem)
            {
                throw new NotSupportedException();
            }
            return min;
        }
        public static double Max<T>(this IEnumerable<T> list, Func<T, double> evalFunc)
        {
            double max = double.MinValue;
            bool hasSomeElem = false;
            foreach (T elem in list)
            {
                hasSomeElem = true;
                double v = evalFunc(elem);
                if (v > max)
                {
                    max = v;
                }
            }

            if (!hasSomeElem)
            {
                throw new NotSupportedException();
            }
            return max;
        }
        public static int Max<T>(this IEnumerable<T> list, Func<T, int> evalFunc)
        {
            int max = int.MinValue;
            bool hasSomeElem = false;
            foreach (T elem in list)
            {
                hasSomeElem = true;
                int v = evalFunc(elem);
                if (v > max)
                {
                    max = v;
                }
            }

            if (!hasSomeElem)
            {
                throw new NotSupportedException();
            }
            return max;
        }
        public static T ElementAt<T>(this IEnumerable<T> list, int index)
        {
            if (list is T[] arr)
            {
                if (arr.Length > 0)
                {
                    return arr[index];
                }
                else
                {
                    return default(T);
                }

            }
            else if (list is List<T> list2)
            {
                if (list2.Count > 0)
                {
                    return list2[index];
                }
                else
                {
                    return default(T);
                }
            }
            else
            {
                int count = 0;
                foreach (T t in list)
                {
                    if (count == index)
                    {
                        return t;
                    }
                    count++;
                }
                return default(T);
            }
        }
        public static System.Collections.Generic.IEnumerable<Output> Select<TSource, Output>(this System.Collections.Generic.IEnumerable<TSource> source, Func<TSource, Output> func)
        {
            foreach (TSource t in source)
            {
                yield return func(t);
            }
        }
        public static System.Collections.Generic.IEnumerable<TSource> Skip<TSource>(this System.Collections.Generic.IEnumerable<TSource> source, int count)
        {
            int c = 0;
            foreach (TSource t in source)
            {
                c++;
                if (c < count)
                {
                    continue;
                }
                yield return t;
            }
        }
        public static System.Collections.Generic.IEnumerable<TSource> Concat<TSource>(this System.Collections.Generic.IEnumerable<TSource> first,
            System.Collections.Generic.IEnumerable<TSource> second)
        {
            foreach (TSource t in first)
            {
                yield return t;
            }
            foreach (TSource t in second)
            {
                yield return t;
            }
        }
        public static int Count<T>(this IEnumerable<T> list)
        {
            int count = 0;
            foreach (T t in list)
            {
                count++;
            }
            return count;
        }
        public static List<T> ToList<T>(this IEnumerable<T> list)
        {
            return new List<T>(list);
        }
        public static double Sum<T>(this IEnumerable<T> list, Func<T, double> getValue)
        {
            double total = 0;
            foreach (T t in list)
            {
                total += getValue(t);
            }
            return total;
        }
        public static int Sum<T>(this IEnumerable<T> list, Func<T, int> getValue)
        {
            int total = 0;
            foreach (T t in list)
            {
                total += getValue(t);
            }
            return total;
        }
        public static bool Any<T>(this IEnumerable<T> list)
        {
            foreach (T t in list)
            {
                return true;
            }
            return false;
        }

        public static bool Any<T>(this IEnumerable<T> list, Func<T, bool> predicate)
        {
            foreach (T t in list)
            {
                if (predicate(t))
                {
                    return true;
                }
            }
            return false;
        }
        public static T FirstOrDefault<T>(this IEnumerable<T> list, Func<T, bool> predicate)
        {
            foreach (T t in list)
            {
                if (predicate(t))
                {
                    return t;
                }
            }
            return default(T);
        }
        public static T First<T>(this T[] arr, Func<T, bool> predicate)
        {
            for (int i = 0; i < arr.Length; ++i)
            {
                if (predicate(arr[i]))
                {
                    return arr[i];
                }
            }
            return default(T);
        }
        public static T Last<T>(this T[] arr)
        {
            return arr[arr.Length - 1];
        }
        public static T First<T>(this IEnumerable<T> list, Func<T, bool> predicate)
        {
            foreach (T t in list)
            {
                if (predicate(t))
                {
                    return t;
                }
            }
            return default(T);
        }
        public static T First<T>(this IEnumerable<T> list)
        {

            if (list is T[] arr)
            {
                if (arr.Length > 0)
                {
                    return arr[0];
                }
                else
                {
                    return default(T);
                }

            }
            else if (list is List<T> list2)
            {
                if (list2.Count > 0)
                {
                    return list2[0];
                }
                else
                {
                    return default(T);
                }
            }
            else if (list is LinkedList<T> linkedlist)
            {
                if (linkedlist.Count > 0)
                {
                    return linkedlist.First.Value;
                }
                else
                {
                    return default(T);
                }
            }
            else
            {
                T lastOne = default(T);
                foreach (T t in list)
                {
                    return t;
                }
                return lastOne;
            }

        }
        public static T Last<T>(this IEnumerable<T> list)
        {
            if (list is T[] arr)
            {
                if (arr.Length > 0)
                {
                    return arr[arr.Length - 1];
                }
                else
                {
                    return default(T);
                }

            }
            else if (list is List<T> list2)
            {
                if (list2.Count > 0)
                {
                    return list2[list2.Count - 1];
                }
                else
                {
                    return default(T);
                }
            }
            else if (list is LinkedList<T> linkedlist)
            {
                if (linkedlist.Count > 0)
                {
                    return linkedlist.Last.Value;
                }
                else
                {
                    return default(T);
                }
            }
            else
            {
                T lastOne = default(T);
                foreach (T t in list)
                {
                    lastOne = t;
                }
                return lastOne;
            }
        }
        public static T LastOrDefault<T>(this IEnumerable<T> list)
        {
            if (list is T[] arr)
            {
                if (arr.Length > 0)
                {
                    return arr[arr.Length - 1];
                }
                else
                {
                    return default(T);
                }

            }
            else if (list is List<T> list2)
            {
                if (list2.Count > 0)
                {
                    return list2[list2.Count - 1];
                }
                else
                {
                    return default(T);
                }
            }
            else if (list is LinkedList<T> linkedlist)
            {
                if (linkedlist.Count > 0)
                {
                    return linkedlist.Last.Value;
                }
                else
                {
                    return default(T);
                }
            }
            else
            {
                T lastOne = default(T);
                foreach (T t in list)
                {
                    lastOne = t;
                }
                return lastOne;
            }
        }
        public static IEnumerable<T> Reverse<T>(this IEnumerable<T> list)
        {
            if (list is T[] arr)
            {
                for (int i = arr.Length - 1; i >= 0; --i)
                {
                    yield return arr[i];
                }

            }
            else if (list is List<T> list2)
            {
                for (int i = list2.Count - 1; i >= 0; --i)
                {
                    yield return list2[i];
                }
            }
            else if (list is LinkedList<T> linkedlist)
            {
                var node = linkedlist.Last;
                while (node != null)
                {
                    yield return node.Value;
                    node = node.Previous;
                }
            }
            else
            {
                List<T> tmp = new List<T>(list);
                for (int i = tmp.Count - 1; i >= 0; --i)
                {
                    yield return tmp[i];
                }
            }
        }
        public static IEnumerable<T> Where<T>(this IEnumerable<T> list, Func<T, bool> predicate)
        {
            foreach (T t in list)
            {
                if (predicate(t))
                    yield return t;
            }
        }

        public static T[] ToArray<T>(this IEnumerable<T> list)
        {
            List<T> list2 = new List<T>();
            foreach (T t in list)
            {
                list2.Add(t);
            }
            return list2.ToArray();
        }

    }

    public static class EnumerableEmpty<T>
    {
        static readonly T[] s_empty = new T[0];
        public static IEnumerable<T> Empty() => s_empty;
    }

    public static class Enumerable
    {
        public static double Sum<T>(IEnumerable<T> list, Func<T, double> getValue)
        {
            double sum = 0;
            foreach (T t in list)
            {
                sum += getValue(t);
            }
            return sum;
        }
        public static bool Any<T>(IEnumerable<T> list, Func<T, bool> predicate)
        {
            foreach (T t in list)
            {
                if (predicate(t))
                {
                    return true;
                }
            }
            return false;
        }
        public static IEnumerable<T> Empty<T>()
        {
            return EnumerableEmpty<T>.Empty();
        }
        public static int Count<T>(IEnumerable<T> list)
        {
            return list.Count();
        }
        public static int Count<T>(IEnumerable<T> list, Func<T, bool> predicate)
        {
            int count = 0;
            foreach (T t in list)
            {
                if (predicate(t))
                {
                    count++;
                }
            }
            return count;
        }
        public static T[] ToArray<T>(IEnumerable<T> list)
        {
            return list.ToArray();
        }
        public static T ElementAt<T>(IEnumerable<T> list, int index)
        {
            return list.ElementAt(index);
        }
        public static bool All<T>(IEnumerable<T> list, Func<T, bool> validateFunc)
        {
            foreach (T t in list)
            {
                if (!validateFunc(t))
                {
                    return false;
                }
            }
            return true;
        }
        public static IEnumerable<T> Where<T>(IEnumerable<T> list, Func<T, bool> validateFunc)
        {
            foreach (T t in list)
            {
                if (!validateFunc(t))
                {
                    yield return t;
                }
            }
        }
    }


}

