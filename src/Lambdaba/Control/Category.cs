namespace Lambdaba.Control;

public interface Category<TypeConstructor>
{
    static abstract Data<TypeConstructor, A, C> Compose<A, B, C>(Data<TypeConstructor, A, B> catAB, Data<TypeConstructor, B, C> catBC);

    static abstract Data<TypeConstructor, A, A> Id<A>();
}


