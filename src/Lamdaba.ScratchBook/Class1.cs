using static Lambdaba.Prelude;
using static Lambdaba.Types;

namespace Lamdaba.ScratchBook;

public readonly record struct Reward(double Value)
{
    // implicit conversion to double
    public static implicit operator double(Reward reward) => reward.Value;
    // implicit conversion from double
    public static implicit operator Reward(double value) => new(value);
}

public record Transition<S>(S Origin, S Destination);

public readonly record struct Probability(double Value)
{
    // implicit conversion to double
    public static implicit operator double(Probability probability) => probability.Value;
    // implicit conversion from double
    public static implicit operator Probability(double value) => new(value);
}

public readonly record struct DiscountFactor(double Value)
{
    // implicit conversion to double
    public static implicit operator double(DiscountFactor discountFactor) => discountFactor.Value;
    // implicit conversion from double
    public static implicit operator DiscountFactor(double value) => new(value);
}

public interface Environment<T, S, A>
    where T : Environment<T, S, A>
    where S : Ord<S>
    where A : Ord<A>
{
    public List<A> Actions {get; init;}
    public List<S> States {get; init;}

    /// <summary>
    /// A call to the dynamics function executes the model of the environment once, where the outcome is a state transition with the probability
    /// that transition actually occurs. Its a specialized version of the more general term "experiment", where transitions are the sample space
    /// and the transition probability the probability measure of the experiment. Episodes are equivalent to trials.
    /// </summary>
    /// <typeparam name="S"></typeparam>
    /// <typeparam name="A"></typeparam>
    /// <param name="state"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static abstract (Transition<S> transition, Probability transitionProbability) Dynamics(S state, A action);

    public static abstract Reward Reward(S state, A action);

    public DiscountFactor γ {get; init;}
}

public interface Agent<T, S, A>
{
    public static abstract A Policy(S state);
}

