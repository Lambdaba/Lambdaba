
using System;
using Lambdaba;
using Lambdaba.Distribution;
using ProbabilityTheory;
using static Lambdaba.Base;
using static Lambdaba.Data.List;
using static Lambdaba.Types;

namespace ProbabilityTheory
{
    // Represents a state in the MDP/POMDP
    public readonly record struct State<S>(S Value)
    {
        public static explicit operator State<S>(S value) => new State<S>(value);
        public static explicit operator S(State<S> state) => state.Value;
    }

    // Represents an action in the MDP/POMDP
    public readonly record struct Action<A>(A Value)
    {
        public static explicit operator Action<A>(A value) => new Action<A>(value);
        public static explicit operator A(Action<A> action) => action.Value;
    }

    // Represents a reward in the MDP/POMDP
    public readonly record struct Reward(double Value)
    {
        public static explicit operator Reward(double value) => new Reward(value);
        public static explicit operator double(Reward reward) => reward.Value;
    }

    // Represents an observation in the POMDP
    public readonly record struct Observation<O>(O Value)
    {
        public static explicit operator Observation<O>(O value) => new Observation<O>(value);
        public static explicit operator O(Observation<O> observation) => observation.Value;
    }

    // Represents a probability value between 0 and 1
    public readonly record struct Probability
    {
        public float Value { get; }
        public Probability(float value)
        {
            if (value < 0 || value > 1)
                throw new System.ArgumentOutOfRangeException(nameof(value), "Probability must be between 0 and 1.");
            Value = value;
        }

        public static explicit operator Probability(float value) => new Probability(value);
        public static explicit operator float(Probability probability) => probability.Value;

        public override string ToString() => $"{Value * 100:F2}%";
    }

    // Represents a transition in the MDP
    public readonly record struct Transition<S, A>(State<S> From, Action<A> Action, State<S> To, Probability Probability, Reward Reward);

    // Represents an MDP
    public record MDP<S, A>(
        List<State<S>> States,
        List<Action<A>> Actions,
        List<Transition<S, A>> Transitions,
        Func<State<S>, bool> IsTerminal);

    // Represents a transition in the POMDP
    public readonly record struct POMDPTransition<S, A, O>(State<S> From, Action<A> Action, State<S> To, Probability Probability, Reward Reward, Observation<O> Observation);

    // Represents a POMDP
    public record POMDP<S, A, O>(
        List<State<S>> States,
        List<Action<A>> Actions,
        List<Observation<O>> Observations,
        List<POMDPTransition<S, A, O>> Transitions,
        Func<State<S>, bool> IsTerminal);

    public static class MDPFunctions
    {

        // Gets possible transitions from a given state and action
        public static List<Transition<S, A>> GetTransitions<S, A>(MDP<S, A> mdp, State<S> state, Action<A> action) =>
            Filter<Transition<S, A>>(t => t.From.Equals(state) && t.Action.Equals(action), mdp.Transitions);

        // Gets the reward for a given transition
        public static Reward GetReward<S, A>(Transition<S, A> transition) => transition.Reward;

        // Checks if a state is terminal
        public static bool IsTerminal<S, A>(MDP<S, A> mdp, State<S> state) => mdp.IsTerminal(state);
    }

    public static class POMDPFunctions
    {
        // Gets possible transitions from a given state and action
        public static List<POMDPTransition<S, A, O>> GetTransitions<S, A, O>(POMDP<S, A, O> pomdp, State<S> state, Action<A> action) =>
            Filter(t => t.From.Equals(state) && t.Action.Equals(action), pomdp.Transitions);

        // Gets the reward for a given transition
        public static Reward GetReward<S, A, O>(POMDPTransition<S, A, O> transition) => transition.Reward;

        // Checks if a state is terminal
        public static bool IsTerminal<S, A, O>(POMDP<S, A, O> pomdp, State<S> state) => pomdp.IsTerminal(state);
    }
}
