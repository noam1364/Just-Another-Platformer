using System;
using System.Collections.Generic;
using static NeatManager;

[Serializable]
public class Genome
{
    private List<int> connectionKeys;                           ///list of innovation numbers of connections
    private List<NodeGene> nodeList;                            ///a list of node genes
    private Dictionary<int, ConnectionGene> connectionList;     ///a map of connection genes

    
    public Genome()                                            
    {
        connectionKeys = new List<int>();
        nodeList = new List<NodeGene>();
        connectionList = new Dictionary<int, ConnectionGene>();
    }
    /// <summary>
    /// Makes a basic genome with only input and output nodes
    /// </summary>
    /// <param name="inputNodes">Number of input nodes</param>
    /// <param name="outputNodes">Number of output nodes</param>
    /// <param name="r"></param>
    public Genome(int inputNodes, int outputNodes, Random r)              
    {
        connectionKeys = new List<int>();
        nodeList = new List<NodeGene>();
        connectionList = new Dictionary<int, ConnectionGene>();
        int innovation = 1;
        for (int i = 1; i <= inputNodes; i++)
        {
            nodeList.Add(new NodeGene(i, NodeGene.TYPE.INPUT));
        }
        for (int i = inputNodes + 1; i <= inputNodes + outputNodes; i++)
        {
            nodeList.Add(new NodeGene(i, NodeGene.TYPE.OUTPUT));
            for (int j = 1; j <= inputNodes; j++)
            {
                float weight = (float)((r.NextDouble() * 2) - 1);
                connectionList.Add(innovation, new ConnectionGene(j, i, weight, true, innovation));
                connectionKeys.Add(innovation);
                innovation++;
            }
        }
        InnovationGenerator.SetInnovation(connectionKeys.Count);
    }

    public List<NodeGene> GetNodes()
    {
        return nodeList;
    }

    public Dictionary<int, ConnectionGene> GetConnections()
    {
        return connectionList;
    }

    public int GetMaxInnovation()
    {
        return connectionKeys[connectionKeys.Count-1];
    }

    public void AddNode(NodeGene node)                           ///Adds a node in the genome
    {
        nodeList.Add(new NodeGene(node));
    }

    public void AddConnection(ConnectionGene con)                 ///Adds a connection in the genome
    {
        connectionList.Add(con.GetInnovation(), new ConnectionGene(con));
        connectionKeys.Add(con.GetInnovation());
    }

    public void mutateConnections(float randomChance, Random r)
    {
        foreach (ConnectionGene con in connectionList.Values)
        {
            if (r.NextDouble() < randomChance)
            {
                con.RandomWeight(r);
            }
            else
            {
                con.ShiftWeight(r);
            }
        }
    }

    public void AddNodeMutation(Random r)                                   ///insert a node between two connected nodes
    {
        int conKey = connectionKeys[r.Next(connectionKeys.Count)];      ///get a random connection
        ConnectionGene con = connectionList[conKey];
        int node1 = con.GetInNode();
        int node2 = con.GetOutNode();

        con.Disable();  ///disable the connection that the node is placed on

        NodeGene newNode = new NodeGene(nodeList.Count + 1, NodeGene.TYPE.HIDDEN);      
        nodeList.Add(newNode);                                                         

        int innovation1 = InnovationGenerator.GetInnovation();
        int innovation2 = InnovationGenerator.GetInnovation();

        connectionKeys.Add(innovation1);
        connectionList.Add(innovation1, new ConnectionGene(node1, newNode.GetID(), 1f, true, innovation1));   

        connectionKeys.Add(innovation2);
        connectionList.Add(innovation2, new ConnectionGene(newNode.GetID(), node2, con.GetWeight(), true, innovation2));
    }

    public void AddConnectionMutation(Random r)                               ///Adds a connection between two random nodes
    {
        int node1 = r.Next(nodeList.Count);
        int node2 = r.Next(nodeList.Count);

        NodeGene.TYPE type1 = nodeList[node1].GetNodeType();
        NodeGene.TYPE type2 = nodeList[node2].GetNodeType();

        if (type1 == type2 && type1 != NodeGene.TYPE.HIDDEN)    ///invalid pair
        {
            AddConnectionMutation(r);                      ///try again with a random a pair
            return;
        }

        foreach (ConnectionGene con in connectionList.Values)
        {
            if (node1 == con.GetInNode() && node2 == con.GetOutNode() || node2 == con.GetInNode() && node1 == con.GetOutNode())  ///if connection already exists
            {
                return;
            }
        }

        if (type1 == NodeGene.TYPE.OUTPUT || type1 == NodeGene.TYPE.HIDDEN && type2 == NodeGene.TYPE.INPUT) ///Switch nodes if they are reversed
        {
            int tmp = node1;
            NodeGene.TYPE tmpType = type1;
            node1 = node2;
            type1 = type2;
            node2 = tmp;
            type2 = tmpType;
        }

        float weight = (float)((r.NextDouble() * 2) - 1);
        int innovation = InnovationGenerator.GetInnovation();
        connectionList.Add(innovation, new ConnectionGene(node1 + 1, node2 + 1, weight, true, innovation));
        connectionKeys.Add(innovation);
    }

    [Serializable]
    public class NodeGene                                       ///NodeGene Subclass
    {
        [Serializable]
        public enum TYPE
        {
            INPUT, OUTPUT, HIDDEN
        };
        private int ID;                                         ///Node ID
        private TYPE type;                                      ///Node type

        public NodeGene()
        {

        }

        public NodeGene(int id, TYPE type)                  
        {
            ID = id;
            this.type = type;
        }

        public NodeGene(NodeGene copy)
        {
            ID = copy.ID;
            type = copy.type;
        }

        public int GetID()
        {
            return ID;
        }

        public TYPE GetNodeType()
        {
            return type;
        }
    }

    [Serializable]
    public class ConnectionGene                               
    {
        private int inNode;         ///input node
        private int outNode;        ///output node 
        private float weight;       ///connection weight
        private bool expressed;     ///is the connection enabled or disabled
        private int innovation;     ///innovation number of connection

        public ConnectionGene()
        {

        }

        public ConnectionGene(int inNode, int outNode, float weight, bool expressed, int innovation)    
        {
            this.inNode = inNode;
            this.outNode = outNode;
            this.weight = weight;
            this.expressed = expressed;
            this.innovation = innovation;
        }

        public ConnectionGene(ConnectionGene copy)
        {
            inNode = copy.GetInNode();
            outNode = copy.GetOutNode();
            weight = copy.GetWeight();
            expressed = copy.IsExpressed();
            innovation = copy.GetInnovation();
        }

        public void Disable()       
        {
            expressed = false;
        }

        public void RandomWeight(Random r)
        {
            weight = (float)(r.NextDouble()*2-1);
        }

        public void ShiftWeight(Random r)
        {
            weight += (float)(r.NextDouble()-0.5)*0.5f;
        }

        public int GetInNode()
        {
            return inNode;
        }

        public int GetOutNode()
        {
            return outNode;
        }

        public float GetWeight()
        {
            return weight;
        }

        public bool IsExpressed()
        {
            return expressed;
        }

        public int GetInnovation()
        {
            return innovation;
        }
    }
}
