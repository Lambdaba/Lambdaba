using System;
using System.Collections.Generic;
using System.Linq;
using Lambdaba;
using static Lambdaba.Base;

namespace Lambdaba.Distribution
{
    /// <summary>
    /// Represents a probability value between 0 and 1.
    /// </summary>
    public readonly record struct Probability
    {
        public Probability(float value)
        {
            if (value < 0 || value > 1)
                throw new ArgumentOutOfRangeException(nameof(value), "Probability must be between 0 and 1.");
            Value = value;
        }

        public float Value { get; }

        public static explicit operator Probability(float value) => new Probability(value);
        public static explicit operator float(Probability probability) => probability.Value;

        public override string ToString() => $"{Value * 100:F2}%";
    }

    /// <summary>
    /// Represents an outcome of a probabilistic event, paired with its associated probability.
    /// </summary>
    public readonly record struct OutcomeWithProbability<A>(A Value, Probability Probability);

    /// <summary>
    /// Abstract base class for the Distribution type, representing probability distributions.
    /// Implements Monad and MonadPlus to allow chaining and combining distributions.
    /// </summary>
    public abstract record Distribution :
        Data<Distribution>,
        Monad<Distribution>,
        MonadPlus<Distribution>,
        Alternative<Distribution>
    {
        /// <summary>
        /// Creates a probability distribution containing a single outcome with certainty (probability of 1).
        /// </summary>
        public static Data<Distribution, A> Certainly<A>(A value) => Pure(value);

        /// <summary>
        /// Monad Bind: Chains distributions by applying a function to each value in the distribution.
        /// This models the concept of dependent random variables in probability theory.
        /// </summary>
        public static Data<Distribution, B> Bind<A, B>(Data<Distribution, A> m, Func<A, Data<Distribution, B>> f)
        {
            var dist = (Distribution<A>)m;
            var newValues = dist.Outcomes.SelectMany(
                x =>
                {
                    var innerDist = (Distribution<B>)f(x.Value);
                    return innerDist.Outcomes.Select(y => new OutcomeWithProbability<B>(y.Value, new Probability(x.Probability.Value * y.Probability.Value)));
                }
            ).ToList();
            return new Distribution<B>(newValues);
        }

        /// <summary>
        /// Functor FMap: Applies a function to each value in the distribution.
        /// This corresponds to mapping a function over a random variable.
        /// </summary>
        public static Data<Distribution, B> FMap<A, B>(Func<A, B> func, Data<Distribution, A> m)
            => Bind(m, x => Pure(func(x)));

        /// <summary>
        /// MonadPlus MZero: Represents an empty distribution (no values).
        /// </summary>
        public static Data<Distribution, A> MZero<A>() => new Distribution<A>(new List<OutcomeWithProbability<A>>());

        /// <summary>
        /// MonadPlus MPlus: Combines two distributions into one.
        /// This models the combination of independent random events.
        /// </summary>
        public static Data<Distribution, A> MPlus<A>(Data<Distribution, A> a, Data<Distribution, A> b)
        {
            var distA = (Distribution<A>)a;
            var distB = (Distribution<A>)b;
            var combinedValues = distA.Outcomes.Concat(distB.Outcomes).ToList();
            return new Distribution<A>(combinedValues);
        }

        /// <summary>
        /// Monad Return: Creates a distribution containing a single value with probability 1.
        /// </summary>
        public static Data<Distribution, A> Pure<A>(A value) => new Distribution<A>(new List<OutcomeWithProbability<A>> { new OutcomeWithProbability<A>(value, new Probability(1.0f)) });

        /// <summary>
        /// Alternative Empty: Represents an empty distribution (no values).
        /// </summary>
        public static Data<Distribution, A> Empty<A>() => MZero<A>();

        /// <summary>
        /// Alternative Or: Combines two distributions into one.
        /// This models the union of events in probability theory.
        /// </summary>
        public static Data<Distribution, A> Or<A>(Data<Distribution, A> a, Data<Distribution, A> b) => MPlus(a, b);

        /// <summary>
        /// Applicative Apply: Applies a distribution of functions to a distribution of values.
        /// This models the combination of independent random variables.
        /// </summary>
        public static Data<Distribution, B> Apply<A, B>(Data<Distribution, Func<A, B>> f, Data<Distribution, A> t)
        {
            var distF = (Distribution<Func<A, B>>)f;
            var distA = (Distribution<A>)t;

            var newValues = distF.Outcomes.SelectMany(
                df => distA.Outcomes.Select(
                    da => new OutcomeWithProbability<B>(df.Value(da.Value), new Probability(df.Probability.Value * da.Probability.Value))
                )
            ).ToList();

            return new Distribution<B>(newValues);
        }
    }

    /// <summary>
    /// Represents a probability distribution over values of type A.
    /// </summary>
    public record Distribution<A>(List<OutcomeWithProbability<A>> Outcomes) : Distribution, Data<Distribution, A>
    {
        // Additional methods specific to Distribution<A> can be added here if needed
    }

    /// <summary>
    /// Provides extension methods for working with Distributions.
    /// </summary>
    public static class DistributionExtensions
    {
        /// <summary>
        /// Creates a uniform distribution over the given values.
        /// Each value has an equal probability of occurring.
        /// </summary>
        public static Data<Distribution, A> Uniform<A>(IEnumerable<A> values)
        {
            var count = values.Count();
            var probability = new Probability(1.0f / count);
            var distValues = values.Select(v => new OutcomeWithProbability<A>(v, probability)).ToList();
            return new Distribution<A>(distValues);
        }

        /// <summary>
        /// Creates a distribution from a list of probabilities and corresponding values.
        /// This allows for custom probability assignments to each value.
        /// </summary>
        public static Data<Distribution, A> Enum<A>(IEnumerable<float> probabilities, IEnumerable<A> values)
        {
            var distValues = probabilities.Zip(values, (p, v) => new OutcomeWithProbability<A>(v, new Probability(p))).ToList();
            return new Distribution<A>(distValues);
        }

        /// <summary>
        /// Creates a distribution representing a binary choice between two values with given probability.
        /// Useful for modeling Bernoulli trials.
        /// </summary>
        public static Data<Distribution, A> Choose<A>(float p, A x, A y)
        {
            return Enum(new[] { p, 1 - p }, new[] { x, y });
        }

        /// <summary>
        /// Applies a function to each value in the distribution.
        /// This models the transformation of random variables.
        /// </summary>
        public static Data<Distribution, B> MapD<A, B>(Func<A, B> func, Data<Distribution, A> dist)
        {
            return Distribution.FMap(func, dist);
        }
    }

    /// <summary>
    /// Provides methods for selecting values from distributions.
    /// </summary>
    public static class DistributionSelection
    {
        /// <summary>
        /// Selects a value from the distribution based on a random probability.
        /// This simulates sampling from the distribution.
        /// </summary>
        public static A SelectP<A>(Data<Distribution, A> distData, float p)
        {
            var dist = (Distribution<A>)distData;
            float cumulative = 0;
            foreach (var dv in dist.Outcomes)
            {
                cumulative += dv.Probability.Value;
                if (p <= cumulative)
                {
                    return dv.Value;
                }
            }
            // If p is not less than cumulative (due to rounding errors), return the last value
            return dist.Outcomes.Last().Value;
        }

        /// <summary>
        /// Picks a random value from the distribution using the IO monad to represent randomness.
        /// </summary>
        public static IO<A> Pick<A>(Data<Distribution, A> distData)
        {
            return new IO<A>(state =>
            {
                var random = new Random();
                float p = (float)random.NextDouble();
                var value = SelectP(distData, p);
                return (state, value);
            });
        }
    }

    /// <summary>
    /// Provides statistical functions for distributions.
    /// </summary>
    public static class Statistics
    {
        /// <summary>
        /// Calculates the expected value (mean) of the distribution.
        /// In statistics, the expected value is the probability-weighted average of all possible values.
        /// </summary>
        public static float Expected<A>(Data<Distribution, A> distData, Func<A, float> toFloat)
        {
            var dist = (Distribution<A>)distData;
            return dist.Outcomes.Sum(dv => toFloat(dv.Value) * dv.Probability.Value);
        }

        /// <summary>
        /// Calculates the variance of the distribution.
        /// Variance measures how much the values of the distribution vary from the mean.
        /// </summary>
        public static float Variance<A>(Data<Distribution, A> distData, Func<A, float> toFloat)
        {
            var expectedValue = Expected(distData, toFloat);
            var dist = (Distribution<A>)distData;
            return dist.Outcomes.Sum(dv => dv.Probability.Value * (float)Math.Pow(toFloat(dv.Value) - expectedValue, 2));
        }

        /// <summary>
        /// Calculates the standard deviation of the distribution.
        /// Standard deviation is the square root of the variance and provides a measure of dispersion.
        /// </summary>
        public static float StdDev<A>(Data<Distribution, A> distData, Func<A, float> toFloat)
        {
            return (float)Math.Sqrt(Variance(distData, toFloat));
        }
    }

    /// <summary>
    /// Represents a transition function that takes an input and returns a distribution over outputs.
    /// This models stochastic processes or Markov chains in probability theory.
    /// </summary>
    public delegate Data<Distribution, A> StochasticProcess<A>(A input);

    /// <summary>
    /// Provides methods for iterating transitions and simulating probabilistic processes.
    /// </summary>
    public static class Iterate
    {
        /// <summary>
        /// Iterates a transition function n times.
        /// This models the repeated application of a stochastic process.
        /// </summary>
        public static StochasticProcess<A> Star<A>(int n, StochasticProcess<A> t)
        {
            if (n <= 0) return a => Distribution.Pure(a);
            return a => Distribution.Bind(t(a), x => Star(n - 1, t)(x));
        }

        /// <summary>
        /// Iterates a random change function n times using the IO monad.
        /// This models the simulation of a stochastic process over time.
        /// </summary>
        public static Func<A, IO<A>> StarIO<A>(int n, Func<A, IO<A>> t)
        {
            if (n <= 0) return a => ReturnIO(a);
            return a => BindIO(t(a), x => StarIO(n - 1, t)(x));
        }
    }

    /// <summary>
    /// Provides methods for simulating probabilistic transitions.
    /// </summary>
    public static class Simulation
    {
        /// <summary>
        /// Simulates a transition function k times, starting from a given value.
        /// This can be used to approximate the distribution after k steps.
        /// </summary>
        public static IO<Data<Distribution, A>> Simulate<A>(int k, StochasticProcess<A> t, A start)
        {
            IO<Data<Distribution, A>> simulateStep(A a, int iterations)
            {
                if (iterations <= 0)
                    return ReturnIO(Distribution.Pure(a));
                else
                {
                    var dist = t(a);
                    return BindIO(DistributionSelection.Pick(dist), next =>
                        simulateStep(next, iterations - 1));
                }
            }

            return simulateStep(start, k);
        }
    }

    /// <summary>
    /// Example usage of the probability constructs.
    /// </summary>
    public static class CoinFlipExample
    {
        public static void Run()
        {
            // Define a biased coin flip transition
            // Probability of Heads is 0.7, Tails is 0.3
            StochasticProcess<string> coinFlip = a => DistributionExtensions.Choose(0.7f, "Heads", "Tails");

            // Simulate flipping the coin 3 times starting from an initial state
            var result = Iterate.Star(3, coinFlip)("Start");

            // Since "Start" is not used in the coinFlip transition, we can ignore it
            var dist = (Distribution<string>)result;

            // Calculate probabilities for each outcome
            var counts = dist.Outcomes
                .GroupBy(dv => dv.Value)
                .Select(g => new { Outcome = g.Key, Probability = g.Sum(dv => dv.Probability.Value) })
                .ToList();

            // Display the probabilities
            foreach (var item in counts)
            {
                Console.WriteLine($"Outcome: {item.Outcome}, Probability: {item.Probability * 100:F2}%");
            }

            // Calculate expected value (not meaningful for strings, but included for demonstration)
            // Assuming "Heads" = 1, "Tails" = 0
            Func<string, float> toFloat = outcome => outcome == "Heads" ? 1.0f : 0.0f;

            var expectedValue = Statistics.Expected(result, toFloat);
            Console.WriteLine($"Expected value: {expectedValue}");
        }
    }

    // Entry point to run the example
    class Program
    {
        static void Main(string[] args)
        {
            CoinFlipExample.Run();
        }
    }
}