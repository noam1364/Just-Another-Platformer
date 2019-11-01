using System;
using System.Collections.Generic;
using Project_Poseidon;
using static Global;
using Project_Poseidon.NeuroEvolution;
using static BossLevelBotTrainer;
using static Project_Poseidon.NeuroEvolution.NeuralNetwork;
public static class NeuroEvolutionManager
{
    public static readonly int[] structure = new int[] {6,20,20,2 };
    public static List<NeuralNetwork> population = null;
    public static List<BossBot> deadPopulation = null;
    public static int currentGen=1,currentSpecies=1;
    public const int populationSize = 100;
    const double mutationRate = 0.1, mutationIntensity = 0.05,flikConRate = 0.001;
    ///flikConRate - the probability that a connection will be disabled,or that a disabled will be reEnabled
    private static string fileUrl = "C:/Workspaces/C# workspace/Project Poseidon/Project Poseidon/AI";
    public static void Initiate()
    {
        population = new List<NeuralNetwork>();
        deadPopulation = new List<BossBot>();
    }
    public static void CreateFirstGen()
    {
        NeuralNetwork n = DataHandler.ReadFromBinaryFile<NeuralNetwork>(fileUrl),temp = null;
        population = new List<NeuralNetwork>(NeuroEvolutionManager.populationSize);
        for(int i = 0;i < NeuroEvolutionManager.populationSize;i++)
        {
            if(n == null||true)
            {
                temp = new NeuralNetwork(structure);
                temp.initialize();
                population.Add(temp);
            }
            else
                population.Add(n);
        }
    }
    public static void CreateNextGen()
    {
        currentSpecies = 0;
        currentGen++;
        ///find the best player in the last population,and save it
        BossBot bestPlayer = deadPopulation[0];
        foreach(BossBot currentPlayer in deadPopulation)
        {
            if(currentPlayer.getFitness() > bestPlayer.getFitness())
                bestPlayer = currentPlayer;
        }
        DataHandler.WriteToBinaryFile<NeuralNetwork>(fileUrl, (NeuralNetwork)bestPlayer.brain);
        ///create a new population based on the best sample of the last gen,mutated
        population = new List<NeuralNetwork>();
        deadPopulation = new List<BossBot>();
        NeuralNetwork temp;
        for(int i=0;i<populationSize;i++)
        {
            temp = new NeuralNetwork((NeuralNetwork)bestPlayer.brain);
            temp = mutate(temp,mutationRate, mutationIntensity);
            population.Add(temp);
        }
    }
    private static NeuralNetwork mutate(NeuralNetwork n,double mutationRate, double mutationIntencity)
    {
        Layer l = null;
        double probability = 0;
        for(int g = 0;g < n.layers.Count;g++)
        {
            l = n.layers[g];
            for(int i = 0;i < l.neurons.Count;i++)
            {
                probability = random.NextDouble(); ///determins if the current bias will be mutated or not
                if(g != 0 && probability <= mutationRate)    ///first layer has no biases
                    l.bias[i] *= 1 - mutationIntencity + 2 * random.NextDouble() * mutationIntencity;///between -mI to +mI

                if(l.fullSynapse != null) ///The last layer has no synaptic connections
                {
                    for(int j = 0;j < l.fullSynapse.ColumnCount;j++)
                    {
                        probability = random.NextDouble(); ///determins if the current synaptic connection will be mutated or not
                        if(probability <= mutationRate)
                            l.fullSynapse[i, j] = -mutationIntencity + 2 * random.NextDouble() * mutationIntencity;///between -mI to +mI
                        else if(probability<=flikConRate)   ///if the current conection is to be diabeld or reEnabled
                        {
                            double currentWeight = l.fullSynapse[i, j];
                            if(currentWeight==0)    ///if this connection is disabled,reEnable by loading the previous weight
                            {
                                l.fullSynapse[i, j] = l.disabledCons[i, j];
                            }
                            else ///if the connection is enabled,disable it
                            {
                                l.disabledCons[i, j] = currentWeight;
                                l.fullSynapse[i, j] = 9;
                            }
                        }
                    }
                }
            }
        }
        return n;
    }
}
