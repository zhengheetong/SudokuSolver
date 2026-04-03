using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace SudokuSolver
{
    public class SudokuEngine
    {
        public Cell[,] Position;
        public Dictionary<int, int> currentEstimation = new Dictionary<int, int>();
        public bool pElimination = true;
        public bool IsUnsolvable = false;

        public SudokuEngine(Cell[,] position)
        {
            Position = position;
            currentEstimation.Add(0, 0);
        }

        // ==========================================
        // CORE SOLVING LOOP
        // ==========================================

        public void GetAllPossibilities()
        {
            foreach (var cell in Position)
            {
                if (cell.Value == 0) cell.PossibleValueReset();
                else cell.PossibleValues.Clear();
            }

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (Position[i, j].Value != 0)
                    {
                        RemovePossibility(Position[i, j].Row, Position[i, j].Column, Position[i, j].Block, Position[i, j].Value);
                    }
                }
            }
        }

        public void SolvePossibility()
        {
            // If any empty cell has zero possibilities, the current guess is invalid — backtrack
            var checkEmpty = Position.Cast<Cell>().Where(p => p.Value == 0 && p.PossibleValues.Count == 0);

            if (checkEmpty.Any())
            {
                ClearEstimation(currentEstimation.Last().Key);
                currentEstimation[currentEstimation.Last().Key]++;
                EstimationSolving(currentEstimation.Last().Key);
                return;
            }

            // Hidden Singles: if a value appears in only one cell in a row, place it
            for (int i = 0; i < 9; i++)
            {
                Dictionary<int, int> possibilityCheck = new Dictionary<int, int>();
                for (int j = 0; j < 9; j++)
                {
                    foreach (int s in Position[i, j].PossibleValues)
                    {
                        if (possibilityCheck.ContainsKey(s)) possibilityCheck[s]++;
                        else possibilityCheck[s] = 1;
                    }
                }
                foreach (KeyValuePair<int, int> kvp in possibilityCheck)
                {
                    if (kvp.Value == 1)
                    {
                        for (int j = 0; j < 9; j++)
                        {
                            if (Position[i, j].PossibleValues.Contains(kvp.Key))
                            {
                                MarkCellSolved(Position[i, j], kvp.Key);
                                return;
                            }
                        }
                    }
                }
            }

            // Hidden Singles: if a value appears in only one cell in a column, place it
            for (int i = 0; i < 9; i++)
            {
                Dictionary<int, int> possibilityCheck = new Dictionary<int, int>();
                for (int j = 0; j < 9; j++)
                {
                    foreach (int s in Position[j, i].PossibleValues)
                    {
                        if (possibilityCheck.ContainsKey(s)) possibilityCheck[s]++;
                        else possibilityCheck[s] = 1;
                    }
                }
                foreach (KeyValuePair<int, int> kvp in possibilityCheck)
                {
                    if (kvp.Value == 1)
                    {
                        for (int j = 0; j < 9; j++)
                        {
                            if (Position[j, i].PossibleValues.Contains(kvp.Key))
                            {
                                MarkCellSolved(Position[j, i], kvp.Key);
                                return;
                            }
                        }
                    }
                }
            }

            // Hidden Singles: if a value appears in only one cell in a 3x3 box, place it
            for (int i = 0; i < 9; i++)
            {
                Dictionary<int, int> possibilityCheck = new Dictionary<int, int>();
                foreach (Cell c in Position)
                {
                    if (c.Block == i + 1)
                    {
                        foreach (int s in c.PossibleValues)
                        {
                            if (possibilityCheck.ContainsKey(s)) possibilityCheck[s]++;
                            else possibilityCheck[s] = 1;
                        }
                    }
                }
                foreach (KeyValuePair<int, int> kvp in possibilityCheck)
                {
                    if (kvp.Value == 1)
                    {
                        for (int j = 0; j < 9; j++)
                        {
                            int row = (i / 3) * 3 + j / 3;
                            int column = (i % 3) * 3 + j % 3;
                            if (Position[row, column].PossibleValues.Contains(kvp.Key))
                            {
                                MarkCellSolved(Position[row, column], kvp.Key);
                                return;
                            }
                        }
                    }
                }
            }

            // Naked Singles: if a cell has only one possible value, place it
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (Position[i, j].Value == 0 && Position[i, j].PossibleValues.Count == 1)
                    {
                        MarkCellSolved(Position[i, j], Position[i, j].PossibleValues[0]);
                        return;
                    }
                }
            }

            // Run advanced elimination techniques before falling back to guessing
            if (pElimination)
            {
                ApplyNakedPairs();
                ApplyHiddenPairs();
                ApplyPointingPairs();
                pElimination = false;
                return;
            }

            currentEstimation.Add(currentEstimation.Count, 0);
            EstimationSolving(currentEstimation.Last().Key);
        }

        // Marks a cell as solved, clears its candidates, and propagates the elimination
        private void MarkCellSolved(Cell cell, int value, bool isEstimation = false)
        {
            cell.Value = value;
            cell.Status = isEstimation ? CellStatus.Estimated : CellStatus.Solved;
            cell.PossibleValues.Clear();
            RemovePossibility(cell.Row, cell.Column, cell.Block, cell.Value);
            pElimination = true;
        }

        // ==========================================
        // ELIMINATION TECHNIQUES
        // ==========================================

        // Naked Pairs: if two cells in a unit share exactly the same two candidates,
        // those values can be eliminated from all other cells in that unit.
        private void ApplyNakedPairs()
        {
            var unknown = Position.Cast<Cell>().Where(p => p.Value == 0).ToList();

            for (int i = 0; i < unknown.Count; i++)
            {
                for (int j = i + 1; j < unknown.Count; j++)
                {
                    var p1 = unknown[i];
                    var p2 = unknown[j];

                    if (p1.PossibleValues.Count == 2 && p1.PossibleValues.SequenceEqual(p2.PossibleValues))
                    {
                        int targetRow = -1, targetCol = -1, targetBlock = -1;

                        if (p1.Row == p2.Row) targetRow = p1.Row;
                        else if (p1.Column == p2.Column) targetCol = p1.Column;
                        if (p1.Block == p2.Block) targetBlock = p1.Block;

                        if (targetRow == -1 && targetCol == -1 && targetBlock == -1) continue;

                        int val1 = p1.PossibleValues[0];
                        int val2 = p1.PossibleValues[1];

                        RemovePossibility(targetRow, targetCol, targetBlock, val1);
                        RemovePossibility(targetRow, targetCol, targetBlock, val2);

                        p1.PossibleValues = new List<int> { val1, val2 };
                        p2.PossibleValues = new List<int> { val1, val2 };

                        p1.UpdateUIProperties();
                        p2.UpdateUIProperties();
                    }
                }
            }
        }

        // Hidden Pairs: if two values only appear in the same two cells across a row,
        // column, or box, all other candidates can be removed from those two cells.
        private void ApplyHiddenPairs()
        {
            var unknown = Position.Cast<Cell>().Where(p => p.Value == 0).ToList();

            for (int row = 0; row < 9; row++)
            {
                var cellsInRow = unknown.Where(p => p.Row == row).ToList();
                var possibilityCount = new Dictionary<int, int>();

                foreach (var p in cellsInRow)
                    foreach (int s in p.PossibleValues)
                    {
                        if (possibilityCount.ContainsKey(s)) possibilityCount[s]++;
                        else possibilityCount[s] = 1;
                    }

                var pairs = possibilityCount.Where(kvp => kvp.Value == 2).Select(kvp => kvp.Key).ToList();
                for (int i = 0; i < pairs.Count; i++)
                {
                    for (int j = i + 1; j < pairs.Count; j++)
                    {
                        var exist = cellsInRow.Where(u => u.PossibleValues.Contains(pairs[i]) && u.PossibleValues.Contains(pairs[j])).ToList();
                        if (exist.Count == 2)
                        {
                            int targetBlock = exist[0].Block == exist[1].Block ? exist[0].Block : -1;
                            var p1 = exist[0];
                            var p2 = exist[1];

                            RemovePossibility(row, -1, targetBlock, pairs[i]);
                            RemovePossibility(row, -1, targetBlock, pairs[j]);

                            p1.PossibleValues = new List<int> { pairs[i], pairs[j] };
                            p2.PossibleValues = new List<int> { pairs[i], pairs[j] };

                            p1.UpdateUIProperties();
                            p2.UpdateUIProperties();
                        }
                    }
                }
            }

            for (int col = 0; col < 9; col++)
            {
                var cellsInCol = unknown.Where(p => p.Column == col).ToList();
                var possibilityCount = new Dictionary<int, int>();

                foreach (var p in cellsInCol)
                    foreach (int s in p.PossibleValues)
                    {
                        if (possibilityCount.ContainsKey(s)) possibilityCount[s]++;
                        else possibilityCount[s] = 1;
                    }

                var pairs = possibilityCount.Where(kvp => kvp.Value == 2).Select(kvp => kvp.Key).ToList();
                for (int i = 0; i < pairs.Count; i++)
                {
                    for (int j = i + 1; j < pairs.Count; j++)
                    {
                        var exist = cellsInCol.Where(u => u.PossibleValues.Contains(pairs[i]) && u.PossibleValues.Contains(pairs[j])).ToList();
                        if (exist.Count == 2)
                        {
                            int targetBlock = exist[0].Block == exist[1].Block ? exist[0].Block : -1;
                            var p1 = exist[0];
                            var p2 = exist[1];

                            RemovePossibility(-1, col, targetBlock, pairs[i]);
                            RemovePossibility(-1, col, targetBlock, pairs[j]);

                            p1.PossibleValues = new List<int> { pairs[i], pairs[j] };
                            p2.PossibleValues = new List<int> { pairs[i], pairs[j] };

                            p1.UpdateUIProperties();
                            p2.UpdateUIProperties();
                        }
                    }
                }
            }

            for (int block = 1; block <= 9; block++)
            {
                var cellsInBlock = unknown.Where(p => p.Block == block).ToList();
                var possibilityCount = new Dictionary<int, int>();

                foreach (var p in cellsInBlock)
                    foreach (int s in p.PossibleValues)
                    {
                        if (possibilityCount.ContainsKey(s)) possibilityCount[s]++;
                        else possibilityCount[s] = 1;
                    }

                var pairs = possibilityCount.Where(kvp => kvp.Value == 2).Select(kvp => kvp.Key).ToList();
                for (int i = 0; i < pairs.Count; i++)
                {
                    for (int j = i + 1; j < pairs.Count; j++)
                    {
                        var exist = cellsInBlock.Where(u => u.PossibleValues.Contains(pairs[i]) && u.PossibleValues.Contains(pairs[j])).ToList();
                        if (exist.Count == 2)
                        {
                            var p1 = exist[0];
                            var p2 = exist[1];

                            RemovePossibility(-1, -1, block, pairs[i]);
                            RemovePossibility(-1, -1, block, pairs[j]);

                            p1.PossibleValues = new List<int> { pairs[i], pairs[j] };
                            p2.PossibleValues = new List<int> { pairs[i], pairs[j] };

                            p1.UpdateUIProperties();
                            p2.UpdateUIProperties();
                        }
                    }
                }
            }
        }

        // Pointing Pairs: if a candidate within a box is confined to a single row or
        // column, it can be eliminated from the rest of that row or column outside the box.
        private void ApplyPointingPairs()
        {
            var unknown = Position.Cast<Cell>().Where(p => p.Value == 0).ToList();

            // Check row-aligned pointing pairs
            for (int i = 1; i <= 9; i++)
            {
                Dictionary<int, string> blockRowMap = new Dictionary<int, string>();
                var temp = unknown.Where(p => p.PossibleValues.Contains(i));

                foreach (var p in temp)
                {
                    if (blockRowMap.ContainsKey(p.Block) && !blockRowMap[p.Block].Contains(p.Row.ToString()))
                        blockRowMap[p.Block] += p.Row.ToString();
                    else if (!blockRowMap.ContainsKey(p.Block))
                        blockRowMap[p.Block] = p.Row.ToString();
                }

                for (int j = 1; j <= 9; j += 3)
                {
                    if (!(blockRowMap.ContainsKey(j) && blockRowMap.ContainsKey(j + 1) && blockRowMap.ContainsKey(j + 2))) continue;

                    string blockX = String.Concat(blockRowMap[j].OrderBy(c => c));
                    string blockY = String.Concat(blockRowMap[j + 1].OrderBy(c => c));
                    string blockZ = String.Concat(blockRowMap[j + 2].OrderBy(c => c));

                    int blockToChange = 0;
                    string excluding = "";

                    if (blockX == blockY && blockX.Length == 2) { excluding = blockX; blockToChange = j + 2; }
                    else if (blockX == blockZ && blockX.Length == 2) { excluding = blockX; blockToChange = j + 1; }
                    else if (blockY == blockZ && blockY.Length == 2) { excluding = blockY; blockToChange = j; }
                    else continue;

                    var exist = unknown
                        .Where(u => u.Block == blockToChange)
                        .Where(u => u.PossibleValues.Contains(i))
                        .Where(u => excluding.Contains(u.Row.ToString()));

                    foreach (var e in exist)
                    {
                        e.PossibleValues.Remove(i);
                        e.UpdateUIProperties();
                    }
                }
            }

            // Check column-aligned pointing pairs
            for (int i = 1; i <= 9; i++)
            {
                Dictionary<int, string> blockColMap = new Dictionary<int, string>();
                var temp = unknown.Where(p => p.PossibleValues.Contains(i));

                foreach (var p in temp)
                {
                    if (blockColMap.ContainsKey(p.Block) && !blockColMap[p.Block].Contains(p.Column.ToString()))
                        blockColMap[p.Block] += p.Column.ToString();
                    else if (!blockColMap.ContainsKey(p.Block))
                        blockColMap[p.Block] = p.Column.ToString();
                }

                for (int j = 1; j <= 3; j++)
                {
                    if (!(blockColMap.ContainsKey(j) && blockColMap.ContainsKey(j + 3) && blockColMap.ContainsKey(j + 6))) continue;

                    string blockX = String.Concat(blockColMap[j].OrderBy(c => c));
                    string blockY = String.Concat(blockColMap[j + 3].OrderBy(c => c));
                    string blockZ = String.Concat(blockColMap[j + 6].OrderBy(c => c));

                    int blockToChange = 0;
                    string excluding = "";

                    if (blockX == blockY && blockX.Length == 2) { excluding = blockX; blockToChange = j + 6; }
                    else if (blockX == blockZ && blockX.Length == 2) { excluding = blockX; blockToChange = j + 3; }
                    else if (blockY == blockZ && blockY.Length == 2) { excluding = blockY; blockToChange = j; }
                    else continue;

                    var exist = unknown
                        .Where(u => u.Block == blockToChange)
                        .Where(u => u.PossibleValues.Contains(i))
                        .Where(u => excluding.Contains(u.Column.ToString()));

                    foreach (var e in exist)
                    {
                        e.PossibleValues.Remove(i);
                        e.UpdateUIProperties();
                    }
                }
            }
        }

        // ==========================================
        // BACKTRACKING (ESTIMATION)
        // ==========================================

        private void RemovePossibility(int row, int column, int block, int value)
        {
            var unknown = Position.Cast<Cell>().Where(p => p.Value == 0);

            foreach (var p in unknown)
            {
                if (p.Row == row || p.Column == column || p.Block == block)
                {
                    p.PossibleValues.Remove(value);
                    p.UpdateUIProperties();
                }
            }
        }

        private void ClearEstimation(int key)
        {
            var reset = Position.Cast<Cell>().Where(p => p.TempSolving >= key);
            foreach (var r in reset)
            {
                r.Value = 0;
                r.Status = CellStatus.Empty;
                r.TempSolving = key;
                r.PossibleValueReset();
            }
            GetAllPossibilities();
        }

        private void EstimationSolving(int key)
        {
            // If we backtrack past depth 0, the puzzle has no solution
            if (key < 0)
            {
                IsUnsolvable = true;
                MessageBox.Show("This Sudoku puzzle is unsolvable!");
                return;
            }

            var unknown = Position.Cast<Cell>().Where(p => p.Value == 0).ToList();

            // No empty cells means the puzzle is solved
            if (!unknown.Any()) return;

            // Target the cell with the fewest remaining candidates to minimize guesses
            int minPossibilities = unknown.Min(p => p.PossibleValues.Count);
            var firstShortest = unknown.First(p => p.PossibleValues.Count == minPossibilities);

            // If all guesses for this depth are exhausted, step back one level
            if (currentEstimation[key] >= minPossibilities)
            {
                currentEstimation.Remove(key);
                key--;

                if (key < 0)
                {
                    MessageBox.Show("This Sudoku puzzle is unsolvable!");
                    return;
                }

                currentEstimation[key]++;
                ClearEstimation(key);
                EstimationSolving(key);
                return;
            }

            // Tag all unknown cells with the current depth so they can be rolled back later
            foreach (var p in unknown) p.TempSolving = key;

            // Make the guess
            MarkCellSolved(firstShortest, firstShortest.PossibleValues[currentEstimation[key]], true);
        }
    }
}