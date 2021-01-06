using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MazeGenerator : MonoBehaviour
{
    [SerializeField]
    private int populationSize = 16;
    [SerializeField]
    private int maxGenerations = 90;
    [SerializeField]
    [Range(0.0f, 1.0f)]
    private float mutationRate = 0.005f;
    [SerializeField]
    private FitnessType fitnessFunction;

    private const int width = 32;
    private const int height = 32;
    List<CellularAutomaton> population = new List<CellularAutomaton>();

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < populationSize; ++i)
        {
            population.Add(new CellularAutomaton(fitnessFunction));
        }
    }

    // Update is called once per frame
    void Update()
    {
        // For testing
        if (Input.GetMouseButtonDown(0))
        {
            foreach (CellularAutomaton maze in population)
            {
                maze.Initialize();
            }
        }

        // TODO: implement genetic algorithm
        /*
        if (Input.GetMouseButtonDown(0))
        {
            EvaluatePopulation();
            for (int i = 0; i < maxGenerations; ++i)
            {
                
                SelectionAndCrossover();
                Mutation();
                EvaluatePopulation();
                // apagar initialize (modificar)
            }
        }*/
    }

    void EvaluatePopulation()
    {
        foreach (CellularAutomaton maze in population)
        {
            maze.Update();
        }

        population.Sort((first, second) => first.FitnessFunction().CompareTo(second.FitnessFunction()));
    }

    void OnDrawGizmos()
    {
        if (population != null)
        {
            for (int i = 0; i < population.Count; ++i)
            {
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        Gizmos.color = (population[i].Cells[x, y] == 1) ? Color.black : Color.white;
                        Vector3 pos = new Vector3(-width / 2 + x + .5f + (width * i), 0, -height / 2 + y + .5f);
                        Gizmos.DrawCube(pos, Vector3.one);
                    }
                }
            }
        }
    }

    void SelectionAndCrossover()
    {

        for (int i = 0; i < 4; i++)
        {
            // generate a random number between 8 and 15:
            int parentOne = UnityEngine.Random.Range(8, 16); // Note: Range is exclusive i.e. [a; b[
            int parentTwo = UnityEngine.Random.Range(8, 16);

            while (parentOne == parentTwo) 
            {
                parentTwo = UnityEngine.Random.Range(8, 16);
            }
            
            // generate a random point to Single Point Crossover between 0 and 17,
            // since the chromossome has 18 positions:
            int randomSinglePoint = UnityEngine.Random.Range(0, 18);

            int[] chromossomeOne = new int[18];
            
            for (int j = 0; j < randomSinglePoint; j ++)
            {
                chromossomeOne[j] = population[parentOne].Chromosome[j];
            }

            for (int k = randomSinglePoint; k < 18; k++)
            {
                chromossomeOne[k] = population[parentTwo].Chromosome[k]; 
            }

            CellularAutomaton childOne = new CellularAutomaton(fitnessFunction, chromossomeOne);


            int[] chromossomeTwo = new int[18];

            for (int j = 0; j < randomSinglePoint; j ++)
            {
                chromossomeTwo[j] = population[parentTwo].Chromosome[j];
            }

            for (int k = randomSinglePoint; k < 18; k++)
            {
                chromossomeTwo[k] = population[parentOne].Chromosome[k]; 
            }

            CellularAutomaton childTwo = new CellularAutomaton(fitnessFunction, chromossomeTwo);

            population[2*i] = childOne;
            population[2*i + 1] = childTwo;

        }
        
    
    }
}
