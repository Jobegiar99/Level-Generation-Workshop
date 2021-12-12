package main

import (
	"flag"
	"math"
	"math/rand"
	"os"
	"strconv"
	"strings"
)

type Point struct {
	row    int
	column int
}

type LevelGenerator struct {
	wanderer Point
	seeker   Point
	rows     int
	columns  int

	terrain [][]int

	generalVisitedCells []Point
	seekerVisitedCells  []Point
}

func newPoint(row string, column string) Point {
	//Creates a new point with the given values
	var point Point

	point.row, _ = strconv.Atoi(row)
	point.column, _ = strconv.Atoi(column)

	return point
}

func getTerrainMatrix(rows int, columns int) [][]int {
	//Initializes the empty matrix that will be used as
	//the terrain for the level
	terrain := make([][]int, rows)
	for i := range terrain {
		terrain[i] = make([]int, columns)
	}
	return terrain
}

func getLevelGeneratorInformation(generatorInformation []string) LevelGenerator {
	//initializes a new LevelGenerator struct with the given information
	var wanderer = newPoint(
		generatorInformation[0],
		generatorInformation[1])

	var seeker = newPoint(
		generatorInformation[2],
		generatorInformation[3])

	var rows, _ = strconv.Atoi(generatorInformation[4])
	var columns, _ = strconv.Atoi(generatorInformation[5])

	var levelInformation LevelGenerator

	levelInformation.generalVisitedCells = make([]Point, 0)
	levelInformation.seekerVisitedCells = make([]Point, 0)
	levelInformation.terrain = getTerrainMatrix(rows, columns)

	levelInformation.wanderer = wanderer
	levelInformation.seeker = seeker
	levelInformation.rows = rows
	levelInformation.columns = columns

	return levelInformation
}

func manhattanDistance(pointA Point, pointB Point) int {
	//calculates the Manhattan distance between two points as
	//it is the heuristic used for this problem
	rowA := float64(pointA.row)
	rowB := float64(pointB.row)
	colA := float64(pointA.column)
	colB := float64(pointB.column)

	var rowValue = math.Abs(rowA - rowB)
	var colValue = math.Abs(colA - colB)
	distance := int(rowValue + colValue)

	return distance
}

func withinLevel(point Point, level [][]int) bool {
	//checks if a point is between the given matrix
	return ((0 <= point.row) &&
		(point.row < len(level)) &&
		(0 <= point.column) &&
		(point.column < len(level[0])))
}

func containsElement(point Point, list []Point) bool {
	//checks if a point exists in the generalVisitedCells list
	for i := range list {
		if list[i] == point {
			return true
		}
	}
	return false
}

func validPoint(point Point, levelInformation LevelGenerator) bool {
	//checks if a point is a valid candidate for the next move of Wanderer or Seeker
	return (withinLevel(point, levelInformation.terrain) &&
		!containsElement(point, levelInformation.generalVisitedCells) &&
		levelInformation.terrain[point.row][point.column] == 0)
}

func validGroundPoint(point Point, levelInformation LevelGenerator) bool {
	//checks if the point is a valid one for the ground spots
	return (withinLevel(point, levelInformation.terrain) &&
		levelInformation.terrain[point.row][point.column] == 0)
}

func generateLevel(levelInformation *LevelGenerator) {
	//the main function that generates the terrain for the level
	for !containsElement(levelInformation.seeker, levelInformation.generalVisitedCells) {
		seeker := levelInformation.seeker
		wanderer := levelInformation.wanderer
		levelInformation.terrain[seeker.row][seeker.column] = 1
		levelInformation.terrain[wanderer.row][wanderer.column] = 1

		levelInformation.generalVisitedCells = append(levelInformation.generalVisitedCells, wanderer)
		levelInformation.seekerVisitedCells = append(levelInformation.seekerVisitedCells, seeker)

		getSeekerNextMove(levelInformation)
		getWandererNextMove(levelInformation)
	}
}

func getSeekerNextMove(levelInformation *LevelGenerator) {
	//calculates seeker's next move
	moves := getMoves(levelInformation.seeker)
	validMoves := make([]Point, 0)
	originalDistance := manhattanDistance(levelInformation.seeker, levelInformation.wanderer)

	for index := range moves {
		if manhattanDistance(moves[index], levelInformation.wanderer) < originalDistance {
			validMoves = append(validMoves, moves[index])
		}
	}
	if len(validMoves) > 0 {
		levelInformation.seeker = validMoves[rand.Intn(len(validMoves))]
	} else {
		if containsElement(levelInformation.seeker, levelInformation.generalVisitedCells) {
			levelInformation.generalVisitedCells = append(levelInformation.generalVisitedCells, levelInformation.seekerVisitedCells...)
		}
		levelInformation.seekerVisitedCells = make([]Point, 0)
		row := rand.Intn(levelInformation.rows)
		col := rand.Intn(levelInformation.columns)
		levelInformation.seeker = newPoint(strconv.Itoa(row), strconv.Itoa(col))
	}
}

func getWandererNextMove(levelInformation *LevelGenerator) {
	//calculates wanderer's next move
	moves := getMoves(levelInformation.wanderer)
	validMoves := make([]Point, 0)

	for index := range moves {
		if validPoint(moves[index], *levelInformation) {
			validMoves = append(validMoves, moves[index])
		}
	}
	if len(validMoves) > 0 {
		levelInformation.wanderer = validMoves[rand.Intn(len(validMoves))]
	} else {
		levelInformation.wanderer =
			levelInformation.generalVisitedCells[rand.Intn(len(levelInformation.generalVisitedCells))]
	}
}

func getMoves(currentPoint Point) []Point {
	//obtains the four directions for a possible next move
	row := currentPoint.row
	column := currentPoint.column

	var candidates = []Point{
		newPoint(strconv.Itoa(row-1), strconv.Itoa(column)),
		newPoint(strconv.Itoa(row+1), strconv.Itoa(column)),
		newPoint(strconv.Itoa(row), strconv.Itoa(column-1)),
		newPoint(strconv.Itoa(row), strconv.Itoa(column+1)),
	}
	return candidates
}

func generateGround(levelInformation *LevelGenerator) {
	print("HERE")
	groundSpots := getGroundSpots(levelInformation)
	print(len(groundSpots), "\n SPOT")
	if len(groundSpots) > 0 {
		spot := groundSpots[rand.Intn(len(groundSpots))]
		for i := range spot {
			levelInformation.terrain[spot[i].row][spot[i].column] = 2
		}
	}
}

func getGroundSpots(levelInformation *LevelGenerator) [][]Point {
	groundSpots := make([][]Point, 0)
	visited := make([]Point, 0)
	for i := range levelInformation.terrain {
		for j := range levelInformation.terrain[i] {
			currentPosition := newPoint(strconv.Itoa(i), strconv.Itoa(j))
			if levelInformation.terrain[i][j] == 0 && !containsElement(currentPosition, visited) {

				candidate := exploreGroundCandidate(&visited, currentPosition, *levelInformation)
				var landCoveragePercentile float64 = float64(len(candidate)) / float64((levelInformation.rows * levelInformation.columns))

				if len(candidate) > 0 && landCoveragePercentile > 0.1 {

					groundSpots = append(groundSpots, candidate)

				}
			}
		}
	}
	return groundSpots
}

func exploreGroundCandidate(visited *[]Point, start Point, levelInformation LevelGenerator) []Point {

	bfs := make([]Point, 0)
	bfs = append(bfs, start)
	groundCandidate := make([]Point, 0)

	for len(bfs) > 0 {
		currentPoint := bfs[0]
		moves := getMoves(currentPoint)

		bfs = bfs[1:]

		for i := range moves {
			if !containsElement(moves[i], *visited) && validGroundPoint(moves[i], levelInformation) && isNotBorder(moves[i], levelInformation) {
				*visited = append(*visited, moves[i])
				bfs = append(bfs, moves[i])
				groundCandidate = append(groundCandidate, moves[i])
			}
		}

	}
	return groundCandidate
}

func isNotBorder(point Point, levelInformation LevelGenerator) bool {
	neighbors := 0

	i := -1

	for i <= 1 {
		j := -1
		for j <= 1 {
			tempPoint := newPoint(strconv.Itoa(point.row+i), strconv.Itoa(point.column+j))
			if withinLevel(tempPoint, levelInformation.terrain) {
				neighbors++
			}
			j++
		}
		i++
	}
	return (neighbors == 9)
}

func createFile(generatorInformation []string, levelInformation LevelGenerator) {
	//writes the generated terrain into a file
	file, _ := os.Create(generatorInformation[6] + ".txt")
	for i := range levelInformation.terrain {
		row := ""
		for j := range levelInformation.terrain[i] {
			row += strconv.Itoa(levelInformation.terrain[i][j])
		}
		row += "\n"
		file.WriteString(row)
	}
	file.Close()
}

func main() {
	cmd := flag.String("cmd", "", "")
	flag.Parse()
	var generatorInformation = strings.Split(string(*cmd), (","))
	levelInformation := getLevelGeneratorInformation(generatorInformation)
	generateLevel(&levelInformation)
	generateGround(&levelInformation)
	createFile(generatorInformation, levelInformation)
}
