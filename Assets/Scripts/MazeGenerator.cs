﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MazeGenerator : MonoBehaviour
{
    [SerializeField]
    private int populationSize = 64;
    [SerializeField]
    private int maxGenerations = 100;
    [SerializeField]
    [Range(0.0f, 1.0f)]
    private float mutationRate = 0.05f;
    [SerializeField]
    private FitnessType fitnessFunction;

    private const int chromosomeSize = 18;
    private const int width = 32;
    private const int height = 32;
    List<CellularAutomaton> population = new List<CellularAutomaton>();
    int currentGeneration = 0;

    //string[] chromosomes;
    // Start is called before the first frame update
    void Start()
    {
        //chromosomes = new string[populationSize];
        for (int i = 0; i < populationSize; ++i)
        {
            population.Add(new CellularAutomaton(fitnessFunction));
            //chromosomes[i] = string.Join("", population[i].Chromosome);
        }
        EvaluatePopulation();
    }

    // Update is called once per frame
    void Update()
    {
        // For testing
        /*if (Input.GetMouseButtonDown(0))
        {
            foreach (CellularAutomaton maze in population)
            {
                maze.Restart();
            }
        }*/

        // TODO: implement genetic algorithm
        
        if (currentGeneration < maxGenerations && Input.GetMouseButtonDown(0))
        {
            //Debug.Log("GENERATION: " + currentGeneration.ToString());
            ++currentGeneration;
            
            SelectionAndCrossover();
            Mutation();
            EvaluatePopulation();

            //string totalChromosomes = string.Empty;
            //for (int i = 0; i < populationSize; ++i)
            //{
                //totalChromosomes += i.ToString() + ": " + chromosomes[i] + " " + string.Join("", population[i].Chromosome) + "\n";
            //}
            //Debug.Log("CHROMOSOMES:\n" + totalChromosomes);
        }
    }

    void EvaluatePopulation()
    {
        foreach (CellularAutomaton maze in population)
        {
            maze.Update();
        }

        population.Sort((first, second) => first.FitnessFunction().CompareTo(second.FitnessFunction()));

        string scores = string.Empty;
        foreach (CellularAutomaton maze in population)
        {
            scores += maze.FitnessFunction().ToString() + " ";
        }
        Debug.Log("Generation: " + currentGeneration.ToString() + " Total Scores: " + scores);
    }

    void OnDrawGizmos()
    {
        if (population != null)
        {
            for (int i = 0; i < population.Count; ++i)
            {
                for (int x = 0; x < width; ++x)
                {
                    for (int y = 0; y < height; ++y)
                    {
                        Gizmos.color = (population[i].Cells[x, y] == 1) ? Color.black : Color.white;
                        Vector3 pos = new Vector3(-width / 2 + x + .5f + (width * (i % 8)), 0, -height / 2 + y + .5f + (height * (i / 8)));
                        Gizmos.DrawCube(pos, Vector3.one);
                    }
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        OnDrawGizmos();
    }

    // TODO: Fix hardcoded values
    void SelectionAndCrossover()
    {
        /*for (int i = 0; i < 4; ++i)
        {
            // generate a random number between 8 and 15:
            int parentOne = UnityEngine.Random.Range(8, 16); // Note: Range is exclusive i.e. [a; b[
            int parentTwo = UnityEngine.Random.Range(8, 16); // Note: Range is exclusive i.e. [a; b[

            while (parentOne == parentTwo) 
            {
                parentTwo = UnityEngine.Random.Range(8, 16); // Note: Range is exclusive i.e. [a; b[
            }
            
            // generate a random point to Single Point Crossover between 1 and 16,
            // since the chromossome has 18 positions:
            int randomSinglePoint = UnityEngine.Random.Range(1, 17); // Note: Range is exclusive i.e. [a; b[
            
            Debug.Log("PARENTS: " + parentOne + ", " + parentTwo + " CROSSOVER-POINT: " + randomSinglePoint);

            int[] chromosomeOne = new int[chromosomeSize];
            
            for (int j = 0; j < randomSinglePoint; ++j)
            {
                chromosomeOne[j] = population[parentOne].Chromosome[j];
            }

            for (int k = randomSinglePoint; k < chromosomeSize; ++k)
            {
                chromosomeOne[k] = population[parentTwo].Chromosome[k]; 
            }

            population[2*i] = new CellularAutomaton(fitnessFunction, chromosomeOne);

            int[] chromosomeTwo = new int[chromosomeSize];

            for (int j = 0; j < randomSinglePoint; ++j)
            {
                chromosomeTwo[j] = population[parentTwo].Chromosome[j];
            }

            for (int k = randomSinglePoint; k < chromosomeSize; ++k)
            {
                chromosomeTwo[k] = population[parentOne].Chromosome[k]; 
            }

            population[2*i + 1] = new CellularAutomaton(fitnessFunction, chromosomeTwo);
        }*/
        for (int i = 0; i < populationSize / 4; ++i)
        {
            // generate a random number between 8 and 15:
            int parentOne = UnityEngine.Random.Range(populationSize / 2, populationSize); // Note: Range is exclusive i.e. [a; b[
            int parentTwo = UnityEngine.Random.Range(populationSize / 2, populationSize); // Note: Range is exclusive i.e. [a; b[

            while (parentOne == parentTwo)
            {
                parentTwo = UnityEngine.Random.Range(populationSize / 2, populationSize); ; // Note: Range is exclusive i.e. [a; b[
            }

            // generate a random point to Single Point Crossover between 1 and 16,
            // since the chromossome has 18 positions:
            int randomSinglePoint = UnityEngine.Random.Range(1, 17); // Note: Range is exclusive i.e. [a; b[

            //Debug.Log("PARENTS: " + parentOne + ", " + parentTwo + " CROSSOVER-POINT: " + randomSinglePoint);

            int[] chromosomeOne = new int[chromosomeSize];

            for (int j = 0; j < randomSinglePoint; ++j)
            {
                chromosomeOne[j] = population[parentOne].Chromosome[j];
            }

            for (int k = randomSinglePoint; k < chromosomeSize; ++k)
            {
                chromosomeOne[k] = population[parentTwo].Chromosome[k];
            }

            population[2 * i] = new CellularAutomaton(fitnessFunction, chromosomeOne);

            int[] chromosomeTwo = new int[chromosomeSize]; 

            for (int j = 0; j < randomSinglePoint; ++j)
            {
                chromosomeTwo[j] = population[parentTwo].Chromosome[j];
            }

            for (int k = randomSinglePoint; k < chromosomeSize; ++k)
            {
                chromosomeTwo[k] = population[parentOne].Chromosome[k];
            }

            population[2 * i + 1] = new CellularAutomaton(fitnessFunction, chromosomeTwo);
        }
    }

    void Mutation()
    {
        for (int i = 0; i < populationSize / 2; ++i)
        {
            population[i].Mutation(mutationRate);
        }
    }
}