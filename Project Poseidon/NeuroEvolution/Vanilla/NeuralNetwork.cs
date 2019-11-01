using System;
using System.Linq;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using System.Reflection;

namespace Project_Poseidon.NeuroEvolution
{
    [Serializable]
    public class NeuralNetwork : NeuralNetwork.INetwork
    {
        private static Random random = new Random(69);
        public List<Layer> layers;
        public NeuralNetwork()
        {   ///for serializing perpuses

        }
        public NeuralNetwork(int[] dims)
        {
            layers = new List<Layer>();
            for(int i = 0;i < dims.Length - 1;i++)
                layers.Add(new Layer(new int[] { dims[i], dims[i + 1] }));
            layers.Add(new Layer(new int[] { dims[dims.Length - 1], 0 }));///last layer has no synaptic connections,only neurons and biases
            layers[0].bias = null;///first layer has no biases
        }
        public NeuralNetwork(NeuralNetwork n) : this(n.getDims())
        {
            Layer l = null;
            for(int g = 0;g < layers.Count - 1;g++)   ///not in including the last layer,which is a dummy layer
            {
                l = layers[g];
                for(int i = 0;i < l.fullSynapse.RowCount;i++)
                {
                    if(g != 0)    ///first layer has no biases
                        l.bias[i] = n.layers[g].bias[i];
                    if(l.fullSynapse != null) ///if its a last layer,there are no synaptic connections
                    {
                        for(int j = 0;j < l.fullSynapse.ColumnCount;j++)
                        {
                            l.fullSynapse[i, j] = n.layers[g].fullSynapse[i, j];
                        }
                    }
                }
            }
        }
        public NeuralNetwork(List<NeuralNetwork> parentsLst) : this(parentsLst[0].getDims())
        {
            Layer l = null;
            Random random = new Random();
            NeuralNetwork[] parents = parentsLst.ToArray();
            NeuralNetwork curParent;
            for(int g = 0;g < layers.Count - 1;g++)   ///not in including the last layer,which is a dummy layer
            {
                curParent = selectParent(random, parents);
                l = layers[g];
                for(int i = 0;i < l.fullSynapse.RowCount;i++)
                {
                    if(g != 0)    ///first layer has no biases
                        l.bias[i] = curParent.layers[g].bias[i];
                    if(l.fullSynapse != null) ///if its a last layer,there are no synaptic connections
                    {
                        for(int j = 0;j < l.fullSynapse.ColumnCount;j++)
                        {
                            l.fullSynapse[i, j] = curParent.layers[g].fullSynapse[i, j];
                        }
                    }
                }
            }
        }
        public float[] getOutput(float[] input)
        {
            Vector<double> inputVec = Vector<double>.Build.Dense(input.Length);
            foreach(float n in input)
                inputVec.Add((double)n);
            Vector<double> output = feedNet(inputVec);
            float[] outArr = new float[output.Count];
            for(int i = 0;i < output.Count;i++)
                outArr[i] = (float)output[i];
            return outArr;
        }
        public Vector<double> feedNet(Vector<double> input)
        {
            if(input.Count != layers[0].neurons.Count) return null;///input must be same size as the input layer
            layers[0].neurons = input; ///initiate first layer with the input;
            Layer prevLayer, currentLayer = null;
            for(int i = 0;i < layers.Count - 1;i++)
            {
                prevLayer = layers[i]; currentLayer = layers[i + 1];
                ///recive the output of the previous layer | order of multiplicatiom MATTERS!
                currentLayer.neurons = prevLayer.neurons * prevLayer.getSynapse();
                currentLayer.neurons = sigmoid(currentLayer.neurons + currentLayer.bias); ///activation function
            }
            return currentLayer.neurons;
        }
        public void mutate(double mutationRate, double mutationIntencity, Random random)
        {
            Layer l = null;
            double probability = 0;
            for(int g = 0;g < layers.Count;g++)
            {
                l = layers[g];
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
                        }
                    }
                }
            }
        }
        private Vector<double> sigmoid(Vector<double> v)
        {
            Vector<double> ret = Vector<double>.Build.Dense(v.Count);
            for(int i = 0;i < v.Count;i++)
                ret[i] = 1 / (1 + Math.Pow(Math.E, -v[i]));
            return ret;
        }
        public void initialize()
        {
            Layer l = null;
            for(int g = 0;g < layers.Count;g++)   ///not including the last layer,which is a dummy layer
            {
                l = layers[g];
                for(int i = 0;i < l.neurons.Count;i++)
                {
                    if(g != 0)    ///first layer has no biases
                        l.bias[i] = -1 + 2 * random.NextDouble();
                    if(l.fullSynapse != null) ///if its a last layer,there are no synaptic connections
                    {
                        for(int j = 0;j < l.fullSynapse.ColumnCount;j++)
                        {
                            l.fullSynapse[i, j] = -1 + 2 * random.NextDouble();
                        }
                    }
                }
            }
        }
        public int[] getDims()
        {
            int[] arr = new int[this.layers.Count];
            for(int i = 0;i < arr.Length;i++)
            {
                arr[i] = layers[i].neurons.Count;
            }
            return arr;
        }
        private NeuralNetwork selectParent(Random rand, NeuralNetwork[] parents)
        {   ///random is recived as a param because if a new Random will be created each call,it will be the same every time
            double prob = rand.NextDouble();
            int parentIdx = (int)Math.Floor(prob * parents.Length);
            return parents[parentIdx];
        }
        public void SetFitness(float fit)
        {

        }
        [Serializable]
        public class Layer
        {
            /// <summary>
            /// connections synapses to the next layer   |   1st index represents the weights of all 
            ///connections of a specific neuron,and the 2nd index is the weight of the connections of this layer to each neurons in the next layer
            ///example : synape[0,1] is the weight of the connection between the 0 neuron of this layer to the 1 neuron of the next
            ///dims = synape[this.length,next.length]
            ///each row in a neuron
            /// </summary>
            public Matrix<double> fullSynapse;
            /// <summary>
            /// The double represents a bool value (1-true,0-false) | a matrix with the same dimentions as the synapse matrix,
            /// each entry states if the corresponding connection is enabled
            /// </summary>
            public Matrix<double> disabledCons;
            public Vector<double> neurons;
            public Vector<double> bias;
            public MethodInfo activation;

            public Layer(int[] dims)
            {
                if(dims[1] == 0)
                {
                    fullSynapse = null;///if this is a last layer in a network
                }
                else
                {
                    fullSynapse = Matrix<double>.Build.Dense(dims[0], dims[1]);
                    disabledCons = Matrix<double>.Build.Dense(dims[0], dims[1]);
                }
                activation = null;
                neurons = Vector<double>.Build.Dense(dims[0]);
                bias = Vector<double>.Build.Dense(dims[0]);
            }
            public Matrix<double> getSynapse()
            {
                return fullSynapse;
            }
        }
        public interface INetwork
        {
            float[] getOutput(float[] input);
            void SetFitness(float fit);
        }
    }
}