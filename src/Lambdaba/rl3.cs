// using Lambdaba.Collections;
// using Lambdaba.Control;
// using Lambdaba.Data;
// using static Lambdaba.Prelude;
// using Lambdaba.Distribution;
// using System;

// namespace DynamicNeuroEvoRL3
// {
//     // Define available activation functions
//     public enum ActivationFunction
//     {
//         Sigmoid,
//         Tanh,
//         ReLU,
//         Identity
//     }

//     // Neuron data class representing individual neurons with an activation function
//     public readonly record struct Neuron(ActivationFunction Activation, List<Connection> IncomingConnections, double Output, double Gradient);

//     // Connection data class representing connections between neurons with weights
//     public readonly record struct Connection(Neuron Source, Neuron Target, double Weight);

//     // Neural Network data class
//     public readonly record struct NeuralNetwork(List<Neuron> Neurons, double LearningRate);

//     // Policy delegate
//     public delegate Distribution<A> Policy<S, A>(S state);

//     // EncodeState delegate
//     public delegate double[] EncodeState<S>(S state);

//     // EncodeAction delegate
//     public delegate double EncodeAction<A>(A action);

//     // Abstract Agent class
//     public abstract record Agent<S, A>(double CumulativeReward, double[] Behavior)
//     {
//         public abstract Policy<S, A> Policy { get; }
//         public abstract EncodeState<S> StateToInputs { get; }
//         public abstract EncodeAction<A> ActionToDoubleConverter { get; }

//         public abstract Agent<S, A> Learn(S state, double reward);
//     }

//     // DynamicNeuroEvoRLAgent class using a neural network for its policy
//     public record DynamicNeuroEvoRLAgent<S, A>(NeuralNetwork NeuralNetwork, double CumulativeReward, double[] Behavior, EncodeState<S> StateToInputs, EncodeAction<A> ActionToDoubleConverter)
//         : Agent<S, A>(CumulativeReward, Behavior)
//     {
//         public override Policy<S, A> Policy => state =>
//         {
//             var inputs = StateToInputs(state); // Encode the state to create an embedding
//             var (_, logits) = Predict(NeuralNetwork, inputs);
//             var probabilities = Softmax(logits);
//             return new Distribution<A>(probabilities.ZipWithIndex().Map(t => new OutcomeWithProbability<A>((Int)t.Item2, t.Item1)).ToList());
//         };

//         public override Agent<S, A> Learn(S state, double reward)
//         {
//             var network = Backpropagate(NeuralNetwork, StateToInputs(state), reward); // Encode the state to create an embedding
//             return new DynamicNeuroEvoRLAgent<S, A>(network, CumulativeReward + reward, Behavior, StateToInputs, ActionToDoubleConverter);
//         }

//         private (NeuralNetwork, double[]) Predict(NeuralNetwork network, double[] inputs)
//         {
//             var neurons = network.Neurons.ZipWithIndex().Map(t =>
//             {
//                 var (neuron, index) = t;
//                 if (index < inputs.Length)
//                 {
//                     return new Neuron(neuron.Activation, neuron.IncomingConnections, Activate(neuron.Activation, inputs[index]), neuron.Gradient);
//                 }
//                 else
//                 {
//                     double inputSum = neuron.IncomingConnections.FoldLeft(0.0, (sum, conn) => sum + conn.Source.Output * conn.Weight);
//                     return new Neuron(neuron.Activation, neuron.IncomingConnections, Activate(neuron.Activation, inputSum), neuron.Gradient);
//                 }
//             }).ToList();

//             return (new NeuralNetwork(neurons, network.LearningRate), neurons.Map(n => n.Output).ToArray());
//         }

//         private NeuralNetwork Backpropagate(NeuralNetwork network, double[] inputs, double target)
//         {
//             var (predictedNetwork, logits) = Predict(network, inputs);
//             var probabilities = Softmax(logits);
//             var targetIndex = (int)target;

//             var neurons = predictedNetwork.Neurons.ZipWithIndex().Map(t =>
//             {
//                 var (neuron, index) = t;
//                 if (index == targetIndex)
//                 {
//                     double outputError = 1.0 - probabilities[index]; // Target is 1 for the correct action
//                     double gradient = outputError * ActivationDerivative(neuron.Activation, neuron.Output);
//                     return new Neuron(neuron.Activation, neuron.IncomingConnections, neuron.Output, gradient);
//                 }
//                 else if (index >= inputs.Length)
//                 {
//                     double gradient = neuron.IncomingConnections.FoldLeft(0.0, (sum, conn) => sum + conn.Target.Gradient * conn.Weight) * ActivationDerivative(neuron.Activation, neuron.Output);
//                     var updatedConnections = neuron.IncomingConnections.Map(conn =>
//                         new Connection(conn.Source, conn.Target, conn.Weight + network.LearningRate * gradient * conn.Source.Output)).ToList();
//                     return new Neuron(neuron.Activation, updatedConnections, neuron.Output, gradient);
//                 }
//                 else
//                 {
//                     return neuron;
//                 }
//             }).ToList();

//             return new NeuralNetwork(neurons, network.LearningRate);
//         }

//         private double[] Softmax(double[] logits)
//         {
//             var exp = logits.Map(Math.Exp).ToArray();
//             var sumExp = exp.Sum();
//             return exp.Map(e => e / sumExp).ToArray();
//         }

//         private double Activate(ActivationFunction activation, double inputSum) =>
//             activation switch
//             {
//                 ActivationFunction.Sigmoid => 1.0 / (1.0 + Math.Exp(-inputSum)),
//                 ActivationFunction.Tanh => Math.Tanh(inputSum),
//                 ActivationFunction.ReLU => Math.Max(0, inputSum),
//                 ActivationFunction.Identity => inputSum,
//                 _ => inputSum
//             };

//         private double ActivationDerivative(ActivationFunction activation, double output) =>
//             activation switch
//             {
//                 ActivationFunction.Sigmoid => output * (1.0 - output), // Sigmoid derivative
//                 ActivationFunction.Tanh => 1.0 - output * output, // Tanh derivative
//                 ActivationFunction.ReLU => output > 0 ? 1.0 : 0.0, // ReLU derivative
//                 ActivationFunction.Identity => 1.0, // Identity derivative
//                 _ => 1.0
//             };
//     }

//     // Environment delegate
//     public delegate (S nextState, double reward) Environment<S, A>(S state, A action);

//     // Functional methods for behaviors
//     public static class Behaviors
//     {
//         private static Random random = new Random();

//         // Novelty Search for selecting agents
//         public static List<Agent<S, A>> SelectMostNovelAgents<S, A>(List<Agent<S, A>> agents, int populationSize)
//         {
//             return agents.OrderByDescending(agent => CalculateNovelty(agent, agents))
//                          .Take(populationSize)
//                          .ToList();
//         }

//         private static double CalculateNovelty<S, A>(Agent<S, A> agent, List<Agent<S, A>> agents)
//         {
//             return agents.Filter(a => a != agent)
//                          .Map(other => BehaviorDistance(agent, other))
//                          .Sort()
//                          .Take(5)
//                          .Average();
//         }

//         private static double BehaviorDistance<S, A>(Agent<S, A> agent1, Agent<S, A> agent2)
//         {
//             return Math.Abs(agent1.CumulativeReward - agent2.CumulativeReward); // Behavior distance based on reward
//         }

//         // Run the multi-agent learning system
//         public static void RunMultiAgentLearning<S, A>(Environment<S, A> environment, int populationSize, int generations, Random random)
//         {
//             var agents = InitializePopulation<S, A>(populationSize, random);

//             for (int generation = 0; generation < generations; generation++)
//             {
//                 Console.WriteLine($"Generation {generation}");

//                 agents = agents.Map(agent => RunEpisode(agent, environment, random)).ToList();

//                 agents = SelectMostNovelAgents(agents, populationSize);

//                 agents = NeuroevolveAgents(agents, populationSize, random);
//             }
//         }

//         private static List<Agent<S, A>> InitializePopulation<S, A>(int populationSize, Random random)
//         {
//             return Range(0, populationSize)
//                    .Map(_ => new DynamicNeuroEvoRLAgent<S, A>(new NeuralNetwork(List<Neuron>.Empty, 0.01), 0, new double[0], state => state as double[], action => (double)(object)action)) // Minimalistic neural network
//                    .ToList();
//         }

//         private static Agent<S, A> RunEpisode<S, A>(Agent<S, A> agent, Environment<S, A> environment, Random random)
//         {
//             S state = GetInitialState<S>(random);
//             double cumulativeReward = 0;

//             for (int step = 0; step < 100; step++)
//             {
//                 var distribution = agent.Policy(state); // Get the action distribution
//                 A action = distribution.Sample(); // Sample an action from the distribution
//                 var (nextState, reward) = environment(state, action); // Environment interaction
//                 agent = agent.Learn(state, reward); // Learn from the environment
//                 state = nextState;
//                 cumulativeReward += reward;
//             }

//             return agent;
//         }

//         private static List<Agent<S, A>> NeuroevolveAgents<S, A>(List<Agent<S, A>> agents, int populationSize, Random random)
//         {
//             return Range(0, populationSize).Map(_ =>
//             {
//                 var parent1 = agents[random.Next(agents.Count)];
//                 var parent2 = agents[random.Next(agents.Count)];
//                 var offspringNetwork = DynamicNeuroEvoRLAgent<S, A>.Crossover(parent1 as DynamicNeuroEvoRLAgent<S, A>, parent2 as DynamicNeuroEvoRLAgent<S, A>, random);
//                 var mutatedNetwork = DynamicNeuroEvoRLAgent<S, A>.Mutate(offspringNetwork, 0.1, random);
//                 return new DynamicNeuroEvoRLAgent<S, A>(mutatedNetwork, 0, new double[0], parent1.StateToInputs, parent1.ActionToDoubleConverter);
//             }).ToList();
//         }

//         private static S GetInitialState<S>(Random random)
//         {
//             return (S)(object)new double[] { random.NextDouble(), random.NextDouble(), random.NextDouble(), random.NextDouble() }; // Placeholder state
//         }
//     }

//     // Main program execution
//     public class Program
//     {
//         private static Random random = new Random();

//         public static void Main(string[] args)
//         {
//             Environment<double[], double> environment = (state, action) => (new double[] { 0, 0, 0, 0 }, random.NextDouble()); // Simple environment

//             Behaviors.RunMultiAgentLearning(environment, 20, 100, random); // Run the multi-agent system
//         }
//     }
// }