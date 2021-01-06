using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
                ElitistSelection();
                Crossover();
                Mutation();
                EvaluatePopulation();
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
                        Gizmos.color = (population[i].cells[x, y] == 1) ? Color.black : Color.white;
                        Vector3 pos = new Vector3(-width / 2 + x + .5f + (width * i), 0, -height / 2 + y + .5f);
                        Gizmos.DrawCube(pos, Vector3.one);
                    }
                }
            }
        }
    }
}
