using System;
using System.Collections.Generic;

public static class GenomeUtils
{
    /// <summary>
    /// Computes the distance between 2 genomes
    /// </summary>
    /// <param name="g1">The first genome</param>
    /// <param name="g2">The second genome</param>
    /// <param name="c1">Weight of excess genes</param>
    /// <param name="c2">Weight of disjoint genes</param>
    /// <param name="c3">Weight of the avarge connection weight difference</param>
    /// <returns></returns>
    public static float CompatiblityDistance(Genome g1, Genome g2, float c1, float c2, float c3)
    {
        float distance;
        int[] disjointExcess = DisjointAndExcess(g1.GetConnections(), g2.GetConnections(), g1.GetMaxInnovation(), g2.GetMaxInnovation());
        int disjointGenes = disjointExcess[0];
        int excessGenes = disjointExcess[1];
        float avgWeightDifference = AverageWeightDifference(g1.GetConnections(), g2.GetConnections());

        distance = c1 * excessGenes + c2 * disjointGenes + c3 * avgWeightDifference;

        return distance;
    }

    public static int[] DisjointAndExcess(Dictionary<int, Genome.ConnectionGene> d1, Dictionary<int, Genome.ConnectionGene> d2, int maxInno1, int maxInno2)
    {
        int disjointGenes = 0;
        int excessGenes = 0;
        
        int checkUpto = (maxInno1 > maxInno2 ? maxInno2 : maxInno1);
        foreach(Genome.ConnectionGene con in d1.Values)
        {
            if(con.GetInnovation()<=checkUpto)
            {
                disjointGenes += d2.ContainsKey(con.GetInnovation()) ? 0 : 1;
            }
            else
            {
                excessGenes += d2.ContainsKey(con.GetInnovation()) ? 0 : 1;
            }
        }

        foreach (Genome.ConnectionGene con in d2.Values)
        {
            if (con.GetInnovation() <= checkUpto)
            {
                disjointGenes += d1.ContainsKey(con.GetInnovation()) ? 0 : 1;
            }

            else
            {
                excessGenes += d1.ContainsKey(con.GetInnovation()) ? 0 : 1;
            }
        }

        int[] number = { disjointGenes, excessGenes };
        return number;
    }

    public static float AverageWeightDifference(Dictionary<int, Genome.ConnectionGene> d1, Dictionary<int, Genome.ConnectionGene> d2)
    {
        int matchingGenes = 0;
        float weightDifference = 0f;
        foreach (Genome.ConnectionGene con1 in d1.Values)
        {
            if(d2.ContainsKey(con1.GetInnovation()))
            {
                matchingGenes++;
                weightDifference += Math.Abs(con1.GetWeight() - d2[con1.GetInnovation()].GetWeight());
            }
        }
        float avgWeightDifference = weightDifference / matchingGenes;
        return avgWeightDifference;
    }

    /// <summary>
    /// Creates a child genome from two parent genomes. Parent 1 has higher fitness.
    /// </summary>
    /// <param name="parent1">The parent with the higher fitness</param>
    /// <param name="parent2">The parent with the lower fitness</param>
    /// <param name="r"></param>
    /// <returns></returns>
    public static Genome Crossover(Genome parent1, Genome parent2, Random r)      
    {
        Genome child = new Genome();

        List<Genome.NodeGene> parent1nodes = parent1.GetNodes();
        Dictionary<int, Genome.ConnectionGene> parent1connections = parent1.GetConnections();
        Dictionary<int, Genome.ConnectionGene> parent2connections = parent2.GetConnections();

        foreach (Genome.NodeGene p1node in parent1nodes)
        {
            child.AddNode(p1node);
        }

        foreach (Genome.ConnectionGene p1con in parent1connections.Values)
        {
            if (parent2connections.ContainsKey(p1con.GetInnovation()))
            {
                child.AddConnection(r.Next(100) < 50 ? p1con : parent2connections[p1con.GetInnovation()]);
            }
            else
            {
                child.AddConnection(p1con);
            }
        }

        return child;
    }

}
