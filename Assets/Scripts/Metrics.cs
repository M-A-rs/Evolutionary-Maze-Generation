using System.Collections;
using System.Collections.Generic;
using System.IO;

public class Metrics
{
    public string Filename { get; set; }

    private string fileHeader;
    private class Data
    {
        public int generation;
        public float averageFitness;
        public int maxFitness;

        public Data(int generation, float averageFitness, int maxFitness)
        {
            this.generation = generation;
            this.averageFitness = averageFitness;
            this.maxFitness = maxFitness;
        }

        public override string ToString()
        {
            return generation.ToString() + " " + averageFitness.ToString() + " " + maxFitness.ToString();
        }
    }
    private List<Data> dataToWrite;

    public Metrics(string filename, string header)
    {
        this.Filename = filename;
        this.fileHeader = header;
        this.dataToWrite = new List<Data>();
    }

    public void StoreData(int generation, float averageFitness, int maxFitness)
    {
        this.dataToWrite.Add(new Data(generation, averageFitness, maxFitness));
    }

    public void Write()
    {
        using (StreamWriter streamWriter = new StreamWriter(Filename, true))
        {
            streamWriter.WriteLine(fileHeader);

            foreach (Data data in dataToWrite)
            {
                streamWriter.WriteLine(data.ToString());
            }
        }

        dataToWrite.Clear();
    }
}
