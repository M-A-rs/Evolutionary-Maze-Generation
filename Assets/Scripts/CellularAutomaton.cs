using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System.Collections.Generic;
using System;

public class CellularAutomaton
{
    public int[,] Cells { get; private set; }
    public int[] Chromosome { get; private set; }

    private const int mooreNeighboorhood = 9;
    private const int width = 32;
    private const int height = 32;
    private const int rulesIterations = 50;
    private int[,] tempCells;
    private int[,] distances;

    private Tuple<int, int> startCell;
    private Tuple<int, int> endCell;

    // Fitness values
    private FitnessType fitness;
    private int shortestPathLength = 0;
    private int totalDeadEnds = 0;
    private int sumOfShortestPathAndDeadEnds = 0;

    public CellularAutomaton(FitnessType fitness, int[] chromosome)
    {
        Assert.AreEqual(chromosome.Length, 2 * mooreNeighboorhood);
        this.fitness = fitness;
        this.Chromosome = chromosome;
        Initialize();
    }

    public CellularAutomaton(FitnessType fitness): this(fitness, CreateRandomChromosome(2 * mooreNeighboorhood))
    {}

    public int FitnessFunction()
    {
        if (fitness == FitnessType.ShortestSolutionPath)
        {
            return shortestPathLength;
        }
        else if (fitness == FitnessType.TotalDeadEnds)
        {
            return totalDeadEnds;
        }
        else if (fitness == FitnessType.SumOfShortestAndDeadEnds)
        {
            return sumOfShortestPathAndDeadEnds;
        }

        return -1;
    }

    public void Mutation(float mutationRate)
    {
        for (int i = 0; i < 2*mooreNeighboorhood; ++i)
        {
            // Return a random float number between min [inclusive] and max [inclusive] (
            float random = UnityEngine.Random.Range(0.0f , 1.0f); 
            
            if (random <= mutationRate)
            {
                if (Chromosome[i] == 0)
                {
                    Chromosome[i] = 1;
                }
                else
                {
                    Chromosome[i] = 0;
                }
            }
        }
    }
    public void Restart()
    {
        this.Chromosome = CreateRandomChromosome(2 * mooreNeighboorhood);
        Initialize();
    }

    public void Initialize()
    {
        startCell = new Tuple<int, int>(1, 1);
        endCell = new Tuple<int, int>(width - 2, height - 2);
        Cells = new int[width, height];
        distances = new int[width, height];

        //InitializeChromosome();
        BlankStateInitialization();
        tempCells = Cells.Clone() as int[,];

        Update();
    }

    static int[] CreateRandomChromosome(int length)
    {
        int[] chromosome = new int[length];

        for (int i = 0; i < chromosome.Length; ++i)
        {
            chromosome[i] = UnityEngine.Random.Range(0, 2); // Note: Range is exclusive i.e. [a; b[
        }

        return chromosome;
    }

    /*
     * Initializes all inner cells to an empty state (0), and the outer cells to a filled state (1) because they're borders
     */
    void BlankStateInitialization()
    {
        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                {
                    Cells[x, y] = 1;
                }
                else
                {
                    Cells[x, y] = 0;
                }
            }
        }
    }
    
    public void Update()
    {
        for (int i = 0; i < rulesIterations; ++i)
        {
            UpdateCells();
        }

        // Forcibly clear the start and end cells after cellular automata update
        Cells[startCell.Item1, startCell.Item2] = 0;
        Cells[endCell.Item1, endCell.Item2] = 0;
        MergeRegions();
        EvaluateFitness();
    }

    /*
     * Apply cellular automata rules to update cells. 
     * Note that the outer cells are not updated.
     */
    void UpdateCells()
    {
        for (int x = 1; x < width - 1; ++x)
        {
            for (int y = 1; y < height - 1; ++y)
            {
                int filledCells = CountFilledNeighbors(x, y);

                if (Cells[x, y] == 0)
                {
                    if (Chromosome[filledCells] == 1)
                    {
                        tempCells[x, y] = 1;
                    }
                }
                else
                {
                    if (Chromosome[filledCells + mooreNeighboorhood] == 0)
                    {
                        tempCells[x, y] = 0;
                    }
                }
            }
        }

        Cells = tempCells.Clone() as int[,];
    }

    int CountFilledNeighbors(int gridX, int gridY)
    {
        int filledCells = 0;
        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; ++neighbourX)
        {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; ++neighbourY)
            {
                if (!IsInMapRange(neighbourX, neighbourY))
                {
                    Debug.Log("Unexpected error: cell out of bounds at (x, y) = " + neighbourX + ", " + neighbourY);
                }

                if (neighbourX == gridX && neighbourY == gridY)
                {
                    continue;
                }

                filledCells += Cells[neighbourX, neighbourY];
            }
        }

        return filledCells;
    }

    bool IsInMapRange(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    /*
     * Merge disconnected rooms iteratively until there's only one
     * room left on the maze.
     */
    void MergeRegions()
    {
        List<List<Coord>> roomRegions = GetRegions(0);
        while (roomRegions.Count > 1)
        {
            List<Room> survivingRooms = new List<Room>();

            foreach (List<Coord> roomRegion in roomRegions)
            {
                survivingRooms.Add(new Room(roomRegion, Cells));
            }

            ConnectClosestRooms(survivingRooms);
            roomRegions = GetRegions(0);
        }
    }

    /*
     * Evaluate fitness functions. This is the shortest solution path length (F1), 
     * the total dead count (F2) and the unweighted sum of these quantities (F3).
     */
    void EvaluateFitness()
    {
        ComputeDistancesFromStart();
        shortestPathLength = distances[endCell.Item1, endCell.Item2];

        // If the fitness is ShortestSolutionPath, then it's unnecessary to count the dead ends
        if (fitness != FitnessType.ShortestSolutionPath)
        {
            totalDeadEnds = CountDeadEnds();
            sumOfShortestPathAndDeadEnds = shortestPathLength + totalDeadEnds;
        }
    }

    void ComputeDistancesFromStart()
    {
        for (int i = 0; i < Cells.GetLength(0); ++i)
        {
            for (int j = 0; j < Cells.GetLength(1); ++j)
            {
                distances[i, j] = (Cells[i, j] == 1) ? -1 : 0;
            }
        }

        // Auxiliary arrays to visit 4-neighborhood in NESW order
        var rowDisplacement = new int[] { -1, 0, 1, 0 };
        var columnDisplacement = new int[] { 0, 1, 0, -1 };

        Queue<Tuple<int, int>> vertices = new Queue<Tuple<int, int>>();
        vertices.Enqueue(startCell);

        while (vertices.Count > 0)
        {
            var nextCell = vertices.Peek();
            vertices.Dequeue();

            for (int i = 0; i < rowDisplacement.Length; ++i)
            {
                var neighbor = new Tuple<int, int>(nextCell.Item1 + rowDisplacement[i], nextCell.Item2 + columnDisplacement[i]);
                int neighborCell = distances[neighbor.Item1, neighbor.Item2];

                if (neighborCell == -1) // Wall detected
                {
                    continue;
                }
                else if (neighborCell != 0) // Already visited
                {
                    continue;
                }

                distances[neighbor.Item1, neighbor.Item2] = distances[nextCell.Item1, nextCell.Item2] + 1;
                vertices.Enqueue(neighbor);
            }
        }
    }

    int CountDeadEnds()
    {
        int deadEnds = 0;
        // Auxiliary arrays to visit 4-neighborhood in NESW order
        var rowDisplacement = new int[] { -1, 0, 1, 0 };
        var columnDisplacement = new int[] { 0, 1, 0, -1 };

        for (int i = 0; i < distances.GetLength(0); ++i)
        {
            for (int j = 0; j < distances.GetLength(1); ++j)
            {
                if (distances[i, j] == -1) // Wall detected
                {
                    continue;
                }
                else if (i == startCell.Item1 && j == startCell.Item2) // Start cell is not a dead end
                {
                    continue;
                }

                if (MaxDistanceOnNeighborhood(i, j) && !IsHall(i, j))
                {
                    ++deadEnds;
                }
            }
        }

        return deadEnds;
    }

    bool MaxDistanceOnNeighborhood(int row, int column)
    {
        // Auxiliary arrays to visit 4-neighborhood in NESW order
        var rowDisplacement = new int[] { -1, 0, 1, 0 };
        var columnDisplacement = new int[] { 0, 1, 0, -1 };

        int maxDistance = -1;
        for (int i = 0; i < rowDisplacement.Length; ++i)
        {
            var neighbor = new Tuple<int, int>(row + rowDisplacement[i], column + columnDisplacement[i]);

            maxDistance = Math.Max(maxDistance, distances[neighbor.Item1, neighbor.Item2]);
        }

        return distances[row, column] >= maxDistance;
    }

    bool IsHall(int row, int column)
    {
        if (distances[row - 1, column] != -1 && distances[row + 1, column] != -1)
        {
            return true;
        }
        else if (distances[row, column - 1] != -1 && distances[row, column + 1] != -1)
        {
            return true;
        }

        return false;
    }

    void ConnectClosestRooms(List<Room> allRooms)
    {
        int bestDistance = 0;
        Coord bestTileA = new Coord();
        Coord bestTileB = new Coord();
        Room bestRoomA = new Room();
        Room bestRoomB = new Room();
        bool possibleConnectionFound = false;

        foreach (Room roomA in allRooms)
        {
            possibleConnectionFound = false;

            foreach (Room roomB in allRooms)
            {
                if (roomA == roomB)
                {
                    continue;
                }

                if (roomA.IsConnected(roomB))
                {
                    possibleConnectionFound = false;
                    break;
                }

                for (int tileIndexA = 0; tileIndexA < roomA.edgeTiles.Count; tileIndexA++)
                {
                    for (int tileIndexB = 0; tileIndexB < roomB.edgeTiles.Count; tileIndexB++)
                    {
                        Coord tileA = roomA.edgeTiles[tileIndexA];
                        Coord tileB = roomB.edgeTiles[tileIndexB];
                        int distanceBetweenRooms = (int)(Mathf.Pow(tileA.tileX - tileB.tileX, 2) + Mathf.Pow(tileA.tileY - tileB.tileY, 2));

                        if (distanceBetweenRooms < bestDistance || !possibleConnectionFound)
                        {
                            bestDistance = distanceBetweenRooms;
                            possibleConnectionFound = true;
                            bestTileA = tileA;
                            bestTileB = tileB;
                            bestRoomA = roomA;
                            bestRoomB = roomB;
                        }
                    }
                }
            }

            if (possibleConnectionFound)
            {
                CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            }
        }
    }

    void CreatePassage(Room roomA, Room roomB, Coord tileA, Coord tileB)
    {
        Room.ConnectRooms(roomA, roomB);
        //Debug.DrawLine(CoordToWorldPoint(tileA), CoordToWorldPoint(tileB), Color.green, 100);

        List<Coord> line = GetLine(tileA, tileB);
        foreach (Coord c in line)
        {
            DrawCircle(c, 5);
        }
    }

    void DrawCircle(Coord c, int r)
    {
        for (int x = -r; x <= r; x++)
        {
            for (int y = -r; y <= r; y++)
            {
                if (x * x + y * y <= r * r)
                {
                    int drawX = c.tileX + x;
                    int drawY = c.tileY + y;
                    if (drawX == 0 || drawX == width - 1 || drawY == 0 || drawY == height - 1)
                    {
                        continue;
                    }

                    if (IsInMapRange(drawX, drawY))
                    {
                        Cells[drawX, drawY] = 0;
                    }
                }
            }
        }
    }

    List<Coord> GetLine(Coord from, Coord to)
    {
        List<Coord> line = new List<Coord>();

        int x = from.tileX;
        int y = from.tileY;

        int dx = to.tileX - from.tileX;
        int dy = to.tileY - from.tileY;

        bool inverted = false;
        int step = Math.Sign(dx);
        int gradientStep = Math.Sign(dy);

        int longest = Mathf.Abs(dx);
        int shortest = Mathf.Abs(dy);

        if (longest < shortest)
        {
            inverted = true;
            longest = Mathf.Abs(dy);
            shortest = Mathf.Abs(dx);

            step = Math.Sign(dy);
            gradientStep = Math.Sign(dx);
        }

        int gradientAccumulation = longest / 2;
        for (int i = 0; i < longest; i++)
        {
            line.Add(new Coord(x, y));

            if (inverted)
            {
                y += step;
            }
            else
            {
                x += step;
            }

            gradientAccumulation += shortest;
            if (gradientAccumulation >= longest)
            {
                if (inverted)
                {
                    x += gradientStep;
                }
                else
                {
                    y += gradientStep;
                }
                gradientAccumulation -= longest;
            }
        }

        return line;
    }

    Vector3 CoordToWorldPoint(Coord tile)
    {
        return new Vector3(-width / 2 + .5f + tile.tileX, 2, -height / 2 + .5f + tile.tileY);
    }

    List<List<Coord>> GetRegions(int tileType)
    {
        List<List<Coord>> regions = new List<List<Coord>>();
        int[,] mapFlags = new int[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (mapFlags[x, y] == 0 && Cells[x, y] == tileType)
                {
                    List<Coord> newRegion = GetRegionTiles(x, y);
                    regions.Add(newRegion);

                    foreach (Coord tile in newRegion)
                    {
                        mapFlags[tile.tileX, tile.tileY] = 1;
                    }
                }
            }
        }

        return regions;
    }

    List<Coord> GetRegionTiles(int startX, int startY)
    {
        List<Coord> tiles = new List<Coord>();
        int[,] mapFlags = new int[width, height];
        int tileType = Cells[startX, startY];

        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(new Coord(startX, startY));
        mapFlags[startX, startY] = 1;

        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue();
            tiles.Add(tile);

            for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
            {
                for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                {
                    if (IsInMapRange(x, y) && (y == tile.tileY || x == tile.tileX))
                    {
                        if (mapFlags[x, y] == 0 && Cells[x, y] == tileType)
                        {
                            mapFlags[x, y] = 1;
                            queue.Enqueue(new Coord(x, y));
                        }
                    }
                }
            }
        }

        return tiles;
    }

    struct Coord
    {
        public int tileX;
        public int tileY;

        public Coord(int x, int y)
        {
            tileX = x;
            tileY = y;
        }
    }


    class Room : IComparable<Room>
    {
        public List<Coord> tiles;
        public List<Coord> edgeTiles;
        public List<Room> connectedRooms;
        public int roomSize;
        public bool isAccessibleFromMainRoom;
        public bool isMainRoom;

        public Room()
        {
        }

        public Room(List<Coord> roomTiles, int[,] map)
        {
            tiles = roomTiles;
            roomSize = tiles.Count;
            connectedRooms = new List<Room>();

            edgeTiles = new List<Coord>();
            foreach (Coord tile in tiles)
            {
                for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
                {
                    for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                    {
                        if (x < 0 || x >= map.GetLength(0) || y < 0 || y >= map.GetLength(1))
                        {
                            continue;
                        }

                        if (x == tile.tileX || y == tile.tileY)
                        {
                            if (map[x, y] == 1)
                            {
                                edgeTiles.Add(tile);
                            }
                        }
                    }
                }
            }
        }

        public void SetAccessibleFromMainRoom()
        {
            if (!isAccessibleFromMainRoom)
            {
                isAccessibleFromMainRoom = true;
                foreach (Room connectedRoom in connectedRooms)
                {
                    connectedRoom.SetAccessibleFromMainRoom();
                }
            }
        }

        public static void ConnectRooms(Room roomA, Room roomB)
        {
            if (roomA.isAccessibleFromMainRoom)
            {
                roomB.SetAccessibleFromMainRoom();
            }
            else if (roomB.isAccessibleFromMainRoom)
            {
                roomA.SetAccessibleFromMainRoom();
            }
            roomA.connectedRooms.Add(roomB);
            roomB.connectedRooms.Add(roomA);
        }

        public bool IsConnected(Room otherRoom)
        {
            return connectedRooms.Contains(otherRoom);
        }

        public int CompareTo(Room otherRoom)
        {
            return otherRoom.roomSize.CompareTo(roomSize);
        }
    }

    /*void OnDrawGizmos()
    {
        if (cells != null)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Gizmos.color = (cells[x, y] == 1) ? Color.black : Color.white;
                    Vector3 pos = new Vector3(-width / 2 + x + .5f, 0, -height / 2 + y + .5f);
                    Gizmos.DrawCube(pos, Vector3.one);
                }
            }
        }
    }*/
}