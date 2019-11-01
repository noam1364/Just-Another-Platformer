using System;
using System.Collections.Generic;

public class Species : IComparable<Species>
{
    private Genome mascot;
    private List<Genome> members;
    private float fitness;

    public Species(Genome firstMember)
    {
        members = new List<Genome> {firstMember};
        mascot = firstMember;
        fitness = 0;
    }

    public Genome GetRandomGenome(Random r)
    {
        return members[r.Next(members.Count)];
    }

    /// <summary>
    /// Set a new mascot randomly, from the genomes that belong to the species
    /// </summary>
    /// <param name="r"></param>
    public void RandomizeMascot(Random r)
    {
        mascot = members[r.Next(members.Count)];
    }

    public void AddMember(Genome genome)
    {
        members.Add(genome);
    }

    public void AddFitness(float fit)
    {
        fitness += fit;
    }
    /// <summary>
    /// Returns the total fitness of all the genomes in the species
    /// </summary>
    /// <returns></returns>
    public float GetFitness()
    {
        return fitness;
    }

    public int GetCount()
    {
        return members.Count;
    }
    /// <summary>
    /// Returns the mascot, which is a genome in the species that represents the whole 
    /// species when checking if another genome belongs to this species
    /// </summary>
    /// <returns></returns>
    public Genome GetMascot()
    {
        return mascot;
    }
    /// <summary>
    /// Reinitializes the species, leaves only the mascot
    /// </summary>
    public void Reset()
    {
        members.Clear();
        fitness = 0;
    }

    public int CompareTo(Species other)
    {
        return other.GetFitness().CompareTo(fitness);
    }
}
