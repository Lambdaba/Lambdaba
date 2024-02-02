using System.Collections;
using System.Runtime.CompilerServices;
using Lambdaba.Data;
using static Lambdaba.Base;
using static Lambdaba.Types;

namespace Lambdaba;

public static class Types
{
    public abstract record List :
        Monad<List>,
        Data<List>,
        Foldable<List>,
        Alternative<List>,
        MonadPlus<List>        
    {
        public static List<A> Create<A>(ReadOnlySpan<A> items)
        {
            List<A> result = new Empty<A>();
            for (int i = items.Length - 1; i >= 0; i--)
            {
                result = new NonEmpty<A>(items[i], result);
            }
            return result;
        }

        public static Data<List, B> Apply<A, B>(Data<List, Func<A, B>> f, Data<List, A> t) => 
            f switch
            {
                Empty<Func<A, B>> _ => new Empty<B>(),
                NonEmpty<Func<A, B>>(var head, var tail) => [.. (Map(head, (List<A>)t)), ..(List<B>)Apply(tail, t)],
                _ => throw new NotSupportedException()
            };
        public static Data<List, B> Bind<A, B>(Data<List, A> a, Func<A, Data<List, B>> f) => 
            a switch
            {
                Empty<A> _ => new Empty<B>(),
                NonEmpty<A>(var head, var tail) => [..(List<B>)f(head), ..(List<B>)Bind(tail, f)],
                _ => throw new NotSupportedException()
            };
        public static Data<List, B> FMap<A, B>(Func<A, B> f, Data<List, A> a) => 
            Map(f, (List<A>) a);
        public static Data<List, A> Pure<A>(A a) => 
            (List<A>) [a];
        public static Data<List, A> Empty<A>() => 
            new Empty<A>();
        public static Data<List, A> Or<A>(Data<List, A> a, Data<List, A> b) => 
            List<A>.Combine((List<A>)a, (List<A>)b);
    }

    [CollectionBuilder(typeof(List), "Create")]
    public abstract record List<A> :
        List,
        Data<List, A>, 
        IReadOnlyList<A>,
        Monoid<List<A>>,
        Show<List<A>>
    {
        public int Count => this.Count();

        public List<A> Slice(int start, int length)
            => Create<A>(this.Skip(start).Take(length).ToArray());

        public IEnumerator<A> GetEnumerator() => this switch
        {
            Empty<A> empty => empty.GetEnumerator(),
            NonEmpty<A> nonEmpty => new NonEmptyEnumerator<A>(nonEmpty),
            _ => throw new NotSupportedException()
        };

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        static List<A> Monoid<List<A>>.Mempty() => [];

        public A this[int index] 
        {
            get
            {
                if (index < 0)
                {
                    throw new IndexOutOfRangeException();
                }

                List<A> current = this;
                for (int i = 0; i < index; i++)
                {
                    if (current is NonEmpty<A> nonEmpty)
                    {
                        current = nonEmpty.Tail;
                    }
                    else
                    {
                        throw new IndexOutOfRangeException();
                    }
                }

                if (current is NonEmpty<A> nonEmptyAtTarget)
                {
                    return nonEmptyAtTarget.Head;
                }
                else
                {
                    throw new IndexOutOfRangeException();
                }
            }
        }

        public static List<A> MConcat(List<List<A>> xss) =>
            xss switch
            {
                [] => [],
                [var head, .. var tail] => head + MConcat(tail),
                _ => throw new NotSupportedException()
            };
            

        public IEnumerable<A> Add(A i) => new NonEmpty<A>(i, this);

        public static List<A> Combine(List<A> a, List<A> b) => 
            a switch
            {
                [] => b,
                NonEmpty<A>(var head, var tail) => new NonEmpty<A>(head, Combine(tail, b)),
                _ => throw new NotSupportedException()
            };

        public static List<A> STimes(Int n, List<A> x)
        {
            return (int)n switch
            {
                0 => [],
                _ => Rep(n),
            };
            
            List<A> Rep(int n)
            {
                return n switch
                {
                    0 => [],
                    _ => x + Rep(n - 1),
                };
            }
        }

        public static List<A> operator +(List<A> a, List<A> b) => Combine(a, b);

        public static ShowS ShowPrec(Int p, List<A> x) =>
            s =>
                Show.ShowList__(Show.ShowS<A>, x)(s);
            
    }

    public sealed record NonEmpty<A>(A Head, List<A> Tail) : List<A>
    {

    }

    public sealed record Empty<A> : List<A>
    {
        public new IEnumerator<A> GetEnumerator()
        {
            yield break;
        }
    }
}

public class NonEmptyEnumerator<T> : IEnumerator<T>, IEnumerator, IDisposable
    where T : notnull
{
    private NonEmpty<T> _list;
    private bool _started = false;

    public NonEmptyEnumerator(NonEmpty<T> list)
    {
        _list = list;
    }
    public T Current =>
        GetCurrent();

    private T GetCurrent() =>
        _started
        ? _list.Head
        : throw new InvalidOperationException("Enumeration not yet started");

    object IEnumerator.Current =>
        GetCurrent();

    public void Dispose() {; }

    public bool MoveNext()
    {
        if (_started)
        {
            switch (_list.Tail)
            {
                case Empty<T>:
                    return false;
                case NonEmpty<T> ts:
                    _list = ts;
                    return true;
                default:
                    return false;

            }
        }
        else
        {
            _started = true;
            return _list is NonEmpty<T> && _list is not null;
        }
    }

    public void Reset() => throw new NotImplementedException();
}

// public record Solo<A> : Data<A>, Semigroup<Solo<A>>, Monoid<Solo<A>> 
//     where A : Semigroup<A>, Monoid<A>
// {
//     public A Value { get; }

//     public Solo(A value)
//     {
//         Value = value;
//     }

//     public static Solo<A> operator +(Solo<A> a, Solo<A> b) => 
//         new(a.Value + b.Value);

//     public static Solo<A> STimes<B>(B n, Solo<A> a) where B : Integral<B> => 
//         new(A.STimes(n, a.Value));

//     public static Solo<A> Combine(Solo<A> a, Solo<A> b) => a + b;
//     public static Solo<A> Mempty() => new(A.Mempty());
// }