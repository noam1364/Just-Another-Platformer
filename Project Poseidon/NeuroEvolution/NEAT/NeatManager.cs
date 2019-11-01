using System.Collections.Generic;
using System;
using static Global;
using static Genome;
using static CurrentLevel;
using Project_Poseidon;
using static BossLevelBotTrainer;

public class NeatManager 
{
    public const string bestPlayerUrl = "C:/Workspaces/C# workspace/Project Poseidon/Project Poseidon/NeatData/NeatBestLastPlayer";
    private const string genomeUrl = "C:/Workspaces/C# workspace/Project Poseidon/Project Poseidon/NeatData/NeatLastGenome";
    private const string statsUrl = "C:/Workspaces/C# workspace/Project Poseidon/Project Poseidon/NeatData/NeatLastGenomeStats";
    private const string netListUrl = "C:/Workspaces/C# workspace/Project Poseidon/Project Poseidon/NeatData/NetList";
    private const bool readGenomeFromFile = true;
    
    public List<BossBot> playerList;
    private List<Genome> genomes;
    private List<Network> nets;
    private Dictionary<Genome, Network> networkMap;
    private Dictionary<Genome, Species> speciesMap;
    private List<Species> speciesList;
    public int currentPlayer;
    public int generation;
    private bool savedGenBest;  ///true if the the best network of the current generation was already save to the file
    public static Random random = new Random(69);
    public const int population = 250;
    public const int inputNodes = 6;
    public const int outputNodes = 3;
    public const float C1 = 1f;
    public const float C2 = 1f;
    public const float C3 = 0.3f;
    public const float compatiblityThreshold = 3f;
    public const float survivalChance = 0.05f;  //0.1
    public const float weightMutationChance = 0.8f; 
    public const float randomWeightChance = 0.1f; 
    public const float addNodeChance = 0.02f; //0.03
    public const float addConnectionChance = 0.1f;  //0.05 

    /// <summary>
    /// Initilizes a new training session.
    /// </summary>
    public void start ()
    {
        currentPlayer = 0;
        speciesList = new List<Species>();
        playerList = new List<BossBot>();
        System.Random r = new System.Random();

        genomes = DataHandler.ReadFromBinaryFile<List<Genome>>(genomeUrl);
        generation = DataHandler.ReadFromBinaryFile<int>(statsUrl);

        if(genomes == null||!readGenomeFromFile) ///file is empty
        {
            generation = 1;
            genomes = new List<Genome>();
            for(int i = 0;i < population;i++)
            {
                Genome genome = new Genome(inputNodes, outputNodes, r);
                genomes.Add(genome);
            }
        }
        //added:
        assignSpecies();
        makePlayers();
        startTraining();
    }
	
    /// <summary>
    /// Updates the current trainig session, must be called one time each game cycle.
    /// </summary>
	public void update ()
    {
        if(trainingComplete())
        {
            playerList.Clear();
            sortNets();
            saveData();
            createNextGen();
            assignSpecies();
            makePlayers();
            startTraining();
            savedGenBest = false;
        }
        else
        {
            if(playerList.Count==1&&!savedGenBest)
            {
                DataHandler.WriteToBinaryFile(bestPlayerUrl, playerList[0].brain);
                Genome best = ((Network)playerList[0].brain).GetGenome();
                DataHandler.WriteToBinaryFile(genomeUrl, best);
                DataHandler.WriteToBinaryFile<int>(statsUrl, generation);
                savedGenBest = true;
            }
        }
    }

    private void saveData()
    {
        DataHandler.WriteToBinaryFile(bestPlayerUrl, nets[0]);
        DataHandler.WriteToBinaryFile(genomeUrl, genomes[0]);
        DataHandler.WriteToBinaryFile<int>(statsUrl, generation);
    }

    private void assignSpecies()
    {
        speciesMap = new Dictionary<Genome, Species>();
        foreach (Genome gen in genomes)
        {
            bool found = false;
            ///fins a species that this genome will fit in
            foreach (Species species in speciesList)
            {
                float distance = GenomeUtils.CompatiblityDistance(gen, species.GetMascot(), C1, C2, C3);
                if (distance < compatiblityThreshold)
                {
                    species.AddMember(gen);
                    speciesMap.Add(gen, species);
                    found = true;
                    break;
                }
            }
            ///if the genome doesnt fit in any species, create a new one
            if(!found)
            {
                Species species = new Species(gen);
                speciesList.Add(species);
                speciesMap.Add(gen, species);
            }
        }

        System.Random r = new System.Random();

        for(int i = speciesList.Count-1; i>=0; i--)
        {
            ///remove all empty specieses
            if(speciesList[i].GetCount()==0)
            {
                speciesList.RemoveAt(i);
            }
            else ///find a new mascot for the species
            {
                speciesList[i].RandomizeMascot(r);
            }
        }
    }

    /// <summary>
    /// Creates a new playerList with neural networks based on the Genomes in 'genomes'
    /// </summary>
    private void makePlayers()
    {
        nets = new List<Network>();
        networkMap = new Dictionary<Genome, Network>();

        foreach (Genome genome in genomes)
        {
            Network net = new Network(genome);
            nets.Add(net);
            networkMap.Add(genome, net);
        }

        foreach (Network net in nets)
        {
            ///new Player
            BossBot player = new BossBot(net,Global.getBossLevel().ground);
            playerList.Add(player);
        }
    }

    private void startTraining()
    {
        foreach (BossBot player in playerList)
        {
            player.init(); ///make 'update' active,not nessesary
        }
        currentPlayer = 0;
        Global.getGame().bossLevel.restart();
    }

    private bool trainingComplete()
    {
        bool flag = true;
        foreach (BossBot player in playerList)
        {
            if(player.isActive)   
            {
                flag = false;
                break;
            }
        }
        return flag;
    }

    private void sortNets()
    {
        ///sort all nets by fitness
        foreach (Network net in nets)
        {
            net.SetFitness(net.GetFitness()/speciesMap[net.GetGenome()].GetCount());
            speciesMap[net.GetGenome()].AddFitness(net.GetFitness());
        }

        nets.Sort();
        speciesList.Sort();
    }

    private void createNextGen()
    {
        generation++;
        float totalFitness = 0;
        float leftPopulation = population * (1 - survivalChance);
        List<Genome> nextGenomes = new List<Genome>();

        ///get the sum of the fitness of all the networks in the session
        foreach (Species species in speciesList)
            totalFitness += species.GetFitness();

        ///add to the next generation the best networks (survivalChance) is the presentage
        for (int i=0; i<(int)(population*survivalChance); i++)
            nextGenomes.Add(nets[i].GetGenome());

        ///fill the rest of the next generations genome list with the offsprings of each species
        foreach (Species species in speciesList)
        {
            ///Each species gets to pass a number of offsprings that matches the relative fitness of the species 
            for (int i=0; i< (int)((species.GetFitness() / totalFitness) * leftPopulation); i++)
            {
                Genome child = getChild(species);
                nextGenomes.Add(child);
            }
        }
        ///if for some reason the list is not full, fill it with offsprings of the best species
        while(nextGenomes.Count<population)
        {
            Genome child = getChild(speciesList[0]);
            nextGenomes.Add(child);
        }
        foreach(Genome genome in nextGenomes)   ///mutate all genomes of the next generation
            mutateGenome(genome);
        foreach (Species species in speciesList)
            species.Reset();
        genomes = nextGenomes;
    }
    /// <summary>
    /// Rteurns a child of 2 random parents from the given species
    /// </summary>
    /// <param name="species"></param>
    /// <returns></returns>
    private Genome getChild(Species species)
    {
        Genome parent1 = species.GetRandomGenome(random);
        Genome parent2 = species.GetRandomGenome(random);
        Genome child = new Genome();

        if(networkMap[parent1].GetFitness() > networkMap[parent2].GetFitness())
        {
            child = GenomeUtils.Crossover(parent1, parent2, random);
        }
        else
        {
            child = GenomeUtils.Crossover(parent2, parent1, random);
        }
        return child;
    }

    public void mutateGenome(Genome g)
    {
        double probability = random.NextDouble();

        if(probability < weightMutationChance)
        {
            g.mutateConnections(randomWeightChance, random);
        }
        if(probability < addNodeChance)
        {
            g.AddNodeMutation(random);
        }
        if(probability < addConnectionChance)
        {
            g.AddConnectionMutation(random);
        }
    }
}
