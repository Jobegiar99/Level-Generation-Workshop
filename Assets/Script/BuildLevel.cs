using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using System.Diagnostics;
using System.IO;

public class BuildLevel : MonoBehaviour
{

    [SerializeField] Tilemap grassTilemap;
    [SerializeField] Tilemap groundTilemap;
    [SerializeField] Tilemap grassDecoratorTilemap;
    [SerializeField] TileBase grassTile;
    [SerializeField] TileBase groundTile;
    [SerializeField] List<Tile> grassDecorators;
    [SerializeField] List<GameObject> trees;
    [SerializeField] List<GameObject> houses;

    private List<GameObject> createdTrees;
    private List<GameObject> createdHouses;

    System.Func<Point, List<List<byte>>, bool> withinLevel = (point, level) => // check next line
          0 <= point.row && 0 <= point.column && point.row < level.Count && point.column < level[0].Count;

    // Start is called before the first frame update
    void Start()
    {
        
        
        createdTrees = new List<GameObject>();
        createdHouses = new List<GameObject>();
        BuildLevelFromMatrix( ReadLevel() );
        
        
    }

    /// <summary>
    /// Calls the terrainGenerator.exe to generate the level and save it into files, then it reads it and converts the strings into a matrix.
    /// </summary>
    /// <returns>The level read from the files now transformed into a matrix</returns>
    private List<List<byte>> ReadLevel()
    {
        int size = Random.Range(10, 20);
        int multiplySize = Random.Range(2, 5);
        Point bottomA = new Point(size - 1, Random.Range(0, size - 1));
        Point topA = new Point(0, Random.Range(0, size - 1));

        Point bottomB = new Point(size - 1, topA.column);
        Point rightB = new Point(Random.Range(0, size - 1), size - 1);
        Point leftC = new Point(rightB.row, 0);
        Point bottomC = new Point(size - 1, Random.Range(0, size - 1));
        Point topD = new Point(0, bottomC.column);
        Point bottomD = new Point(size - 1, Random.Range(0, size - 1));

        string[] files = new string[] {
            createStringForGoExe(bottomA, topA, size, "A"),
            createStringForGoExe(bottomB, rightB, size, "B"),
            createStringForGoExe(leftC, bottomC, size, "C"),
            createStringForGoExe(topD, bottomD, size, "D")};

        string path = "C:\\Users\\berna\\Documents\\Unity Course\\VFS\\TallerTec\\Assets\\Script";

        for (int i = 0; i < 4; i++)
        {

            string command = "-cmd " + files[i];
            ProcessStartInfo processInfo = new ProcessStartInfo();
            processInfo.FileName = "terrainGenerator.exe";
            processInfo.WorkingDirectory = path;
            processInfo.Arguments = command;

            Process proc = Process.Start(processInfo);
            proc.WaitForExit();
        }

        string[] linesA = File.ReadAllLines(path + "\\A.txt");
        string[] linesB = File.ReadAllLines(path + "\\B.txt");
        string[] linesC = File.ReadAllLines(path + "\\C.txt");
        string[] linesD = File.ReadAllLines(path + "\\D.txt");
        List<List<byte>> level = FillLevelFile(linesA, linesB, linesC, linesD, size);
        return ResizeMatrix(level, multiplySize, multiplySize * size, multiplySize * size);
    }

    /// <summary>
    /// Fills the matrix wit the information read from each file
    /// </summary>
    /// <param name="A">First part of the level</param>
    /// <param name="B">Second part of the level</param>
    /// <param name="C">Third part of the level</param>
    /// <param name="D">Fourth Part of the level</param>
    /// <param name="size">The size of the level divided by two</param>
    /// <returns>The level read from the files now transformed into a matrix</returns>
    private List<List<byte>> FillLevelFile(string[ ] A , string[] B , string[] C, string[] D, int size)
    {
        int bigSize = size * 2;
        int normalSize = size;
        List<List<byte>> level = CreateMatrixHolder(1, bigSize, bigSize);
        
        FillLevelFileHelper(ref level, B, 0, 0);
        FillLevelFileHelper(ref level, C, 0,  normalSize);
        FillLevelFileHelper(ref level, A, normalSize, 0);
        FillLevelFileHelper(ref level, D, normalSize, normalSize);
        return level;
    }

    /// <summary>
    /// Helps to fill the matrix with the information read from the files
    /// </summary>
    /// <param name="level">Level container</param>
    /// <param name="part">The information that was read from the file</param>
    /// <param name="rS">the amount of rows</param>
    /// <param name="cS">The amount of columns</param>
    private void FillLevelFileHelper(ref List<List<byte>> level, string[] part, int rS, int cS)
    {
        foreach(string s in part)
        {
            int col = cS;
            List<byte> currentRow = new List<byte>();
            foreach(char c in s)
            {
                byte val;
                switch( c)
                {
                    case '0':
                        val = 0;
                        break;
                    case '1':
                        val = 2;
                        break;
                    default:
                        val = 1;
                        break;
                }
                level[rS][col] = val;
                col++;
                
            }
            rS++;
            level.Add(currentRow);
        }
    }



    /// <summary>
    /// Creates the encoded information that terrainGenerator.exe will use to create each part.
    /// </summary>
    /// <param name="wanderer">Wanderer startint position</param>
    /// <param name="seeker">Seeker starting position</param>
    /// <param name="size">The size of the level divided by two</param>
    /// <param name="fileName">The name that the output file must have</param>
    /// <returns>The information encoded as a string</returns>
    private string createStringForGoExe(Point wanderer, Point seeker, int size, string fileName)
    {
        string levelInformation = "";
        levelInformation += wanderer.row.ToString() + ",";
        levelInformation += wanderer.column.ToString() + ",";
        levelInformation += seeker.row.ToString() + ",";
        levelInformation += seeker.column.ToString() + "," + size.ToString() + "," + size.ToString() + "," + fileName;
        return levelInformation;
    }



    /// <summary>
    /// Uses the information of the input matrix to build the level where: 
    ///1 --> Grass
    ///2 --> Ground
    /// </summary>
    /// <param name="level"> The level read from the files now transformed into a matrix </param>
    private void BuildLevelFromMatrix(List<List<byte>> level)
    { 
        for (int i = 0; i < level.Count; i++)
        {
            for (int j = 0; j < level[i].Count; j++)
            {
                if (level[i][j] == 2)
                {
                    BuildLevelFromMatrixGroundHelper(i, j, level);
                }
                else if (level[i][j] == 1)
                {

                    BuildLevelFromMatrixGrassHelper(i, j, level);

                }
            }
        }
    }

    /// <summary>
    /// Places a ground sprite and tries to check if a decorator can be placed.
    /// </summary>
    /// <param name="i"> Current Row</param>
    /// <param name="j"> Current Column</param>
    /// <param name="level">TThe level read from the files now transformed into a matrix </param>
    private void BuildLevelFromMatrixGroundHelper(int i, int j, List<List<byte>> level)
    {
        Vector3Int position = new Vector3Int(i, j, -1);
        groundTilemap.SetTile(position, groundTile);
        for (int row = -1; row <= 1; row++)
        {
            for (int column = -1; column <= 1; column++)
            {
                Vector3Int grassPosition = new Vector3Int(i - row, j - column, -1);
                grassTilemap.SetTile(grassPosition, grassTile);
            }
        }
        Point currentPoint = new Point(i, j);
        if (Random.Range(0f, 1f) < 0.2 && IsNotBorder(currentPoint, level, 2) && NoHouseNearby(currentPoint))
        {
            createdHouses.Add(Instantiate(houses[Random.Range(0, houses.Count)], groundTilemap.GetCellCenterLocal(new Vector3Int(i, j - 15, 0)), Quaternion.identity));
        }
    }


    /// <summary>
    /// Places a grass sprite and tries to check if a decorator can be placed.
    /// </summary>
    /// <param name="i"> Current Row</param>
    /// <param name="j"> Current Column</param>
    /// <param name="level">The level read from the files now transformed into a matrix </param>
    private void BuildLevelFromMatrixGrassHelper(int i, int j, List<List<byte>> level)
    {
        Vector3Int position = new Vector3Int(i, j, -1);
        Point currentPosition = new Point(i, j);
        grassTilemap.SetTile(position, grassTile);

        if (Random.Range(0f, 1f) < 0.1 && IsNotBorder(currentPosition, level, 1))
        {
            grassDecoratorTilemap.SetTile(position, grassDecorators[Random.Range(0, grassDecorators.Count - 1)]);
        }
        else if (Random.Range(0f, 1f) < 0.3 && IsNotBorder(currentPosition, level, 1) && NoTreeNearby(currentPosition))
        {
            createdTrees.Add(Instantiate(trees[Random.Range(0, trees.Count)], grassTilemap.GetCellCenterLocal(new Vector3Int(i, j - 15, 0)), Quaternion.identity));
        }
    }


    /// <summary>
    /// Checks if a tree can be placed if it is a  valid position.
    /// </summary>
    /// <param name="currentPosition">Current tile position</param>
    /// <returns>True if the tree can be placed, false if not.</returns>
    private bool NoTreeNearby(Point currentPosition)
    {
        foreach(GameObject tree in createdTrees)
        {
            if (Vector3.Distance(tree.transform.position, grassTilemap.GetCellCenterLocal(new Vector3Int(currentPosition.row, currentPosition.column - 15, 0))) < 4)
                return false;
        }
        return true;
    }

    /// <summary>
    /// Checks if a house can be placed if it is a  valid position.
    /// </summary>
    /// <param name="currentPosition">Current tile position</param>
    /// <returns>True if the house can be placed, false if not.</returns>
    private bool NoHouseNearby(Point currentPosition)
    {
        foreach (GameObject house in createdHouses)
        {
            if (Vector3.Distance(house.transform.position, groundTilemap.GetCellCenterLocal(new Vector3Int(currentPosition.row, currentPosition.column - 15, 0))) < 4)
                return false;
        }
        return true;
    }


    /// <summary>
    /// Checks if the current position is not a border
    /// </summary>
    /// <param name="point">Current tile position</param>
    /// <param name="level">The level read from the files now transformed into a matrix</param>
    /// <param name="targetSprite">The type of sprite that we are checking not to be a border</param>
    /// <returns>True if it's not a border, false if it's a border</returns>
    private bool IsNotBorder(Point point, List<List<byte>> level, byte targetSprite)
    {
        byte neighbors = 0;
        byte distance = 2;
        for (int i = -distance; i <= distance; i++)
        {
            for (int j = -distance; j <= distance; j++)
            {
                if (i == 0 && j == 0)
                    continue;

                if (withinLevel(new Point(point.row + i, point.column + j), level) && level[point.row + i][point.column + j] == targetSprite)
                    neighbors++;
            }
        }
        
        return neighbors == 24;

    }

    /// <summary>
    /// Allows you to generate a new Island upon click
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0)) 
            Application.LoadLevel(Application.loadedLevel);
    }


    /// <summary>
    /// Makes the matrix bigger
    /// </summary>
    /// <param name="level">The level read from the files now transformed into a matrix</param>
    /// <param name="size">The target size of the new matrix</param>
    /// <param name="rows">The amount of rows that the original matrix has</param>
    /// <param name="columns">The amount of columns that the original matrix has</param>
    /// <returns>The level after resizing it</returns>
    private List<List<byte>> ResizeMatrix(List<List<byte>> level, int size, int rows, int columns)
    {

        List<List<byte>> newMatrix = CreateMatrixHolder(size, rows , columns);
        return FillResizedMatrixHolder(newMatrix, level, size );
    }

    /// <summary>
    /// Creates an empty matrix with the new target size
    /// </summary>
    /// <param name="size">The target size of the new matrix</param>
    /// <param name="rows">The amount of rows that the original matrix has</param>
    /// <param name="columns">The amount of columns that the original matrix has</param>
    /// <returns>The empty matrix with the target size</returns>
    private List<List<byte>> CreateMatrixHolder(int size,int rows, int columns)
    {
        List<List<byte>> newMatrix = new List<List<byte>>();
        for (int i = 0; i < rows * size; i++)
        {
            List<byte> tempRow = new List<byte>();
            for (int j = 0; j < columns * size; j++)
                tempRow.Add(0);

            newMatrix.Add(tempRow);
        }
        return newMatrix;
    }

    /// <summary>
    /// Fills the matrix holder with the original level information
    /// </summary>
    /// <param name="newMatrix">The matrix holder created with CreateMatrixHolder function</param>
    /// <param name="level">The level read from the files now transformed into a matrix</param>
    /// <param name="size">The target size of the new matrix</param>
    /// <returns></returns>
    private List<List<byte>> FillResizedMatrixHolder(List<List<byte>> newMatrix, List<List<byte>> level, int size)
    {
        
        for (int i = 0; i < level.Count; i++)
        {
            for (int j = 0; j < level[i].Count; j++)
            {
                for (int newRow = i * size; newRow < ((i * size) + size ); newRow++)
                {
                   
                    for (int newColumn = j * size; newColumn < ((j * size) + size); newColumn++)
                    {
                        if (level[i][j] == 1)
                            newMatrix[newRow][newColumn] = 1;
                        else
                            newMatrix[newRow][newColumn] = level[i][j];
                        //newMatrix[newRow][newColumn] = level[i][j];
                        
                    }
                }
            }
        }
        return newMatrix;
    }
}
