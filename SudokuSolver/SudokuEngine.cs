using System;
using System.Collections.Generic;
using System.Linq;

namespace SudokuSolver
{
    public class SudokuEngine
    {
        public Cell[,] Position;
        public Dictionary<int, int> currentEstimation = new Dictionary<int, int>();
        public bool pElimination = true;

        public SudokuEngine(Cell[,] position)
        {
            Position = position;
            currentEstimation.Add(0, 0);
        }

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
            var checkEmpty = Position.Cast<Cell>().Where(p => p.Value == 0 && p.PossibleValues.Count == 0);
            
            if (checkEmpty.Any())
            {
                ClearEstimation(currentEstimation.Last().Key); 
                currentEstimation[currentEstimation.Last().Key]++; 
                EstimationSolving(currentEstimation.Last().Key); 
                return;
            }

            // Row solving
            for (int i = 0; i < 9; i++)
            {
                Dictionary<int, int> posibilitycheck = new Dictionary<int, int>();
                for (int j = 0; j < 9; j++)
                {
                    foreach (int s in Position[i, j].PossibleValues)
                    {
                        if (posibilitycheck.ContainsKey(s)) posibilitycheck[s]++;
                        else posibilitycheck[s] = 1;
                    }
                }
                foreach (KeyValuePair<int, int> kvp in posibilitycheck)
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

            // Column solving
            for (int i = 0; i < 9; i++)
            {
                Dictionary<int, int> posibilitycheck = new Dictionary<int, int>();
                for (int j = 0; j < 9; j++)
                {
                    foreach (int s in Position[j, i].PossibleValues)
                    {
                        if (posibilitycheck.ContainsKey(s)) posibilitycheck[s]++;
                        else posibilitycheck[s] = 1;
                    }
                }
                foreach (KeyValuePair<int, int> kvp in posibilitycheck)
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

            // Box solving
            for (int i = 0; i < 9; i++)
            {
                Dictionary<int, int> posibilitycheck = new Dictionary<int, int>();
                foreach (Cell c in Position)
                {
                    if (c.Block == i + 1)
                    {
                        foreach (int s in c.PossibleValues)
                        {
                            if (posibilitycheck.ContainsKey(s)) posibilitycheck[s]++;
                            else posibilitycheck[s] = 1;
                        }
                    }
                }
                foreach (KeyValuePair<int, int> kvp in posibilitycheck)
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

            // Remaining solving (Naked singles)
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

            if (pElimination)
            {
                PossibilityElimination1();
                PossibilityElimination2();
                PossibilityElimination3();
                pElimination = false;
                return;
            }

            currentEstimation.Add(currentEstimation.Count, 0);
            EstimationSolving(currentEstimation.Last().Key);
        }
        
        // Helper method to keep logic DRY
        private void MarkCellSolved(Cell cell, int value, bool isEstimation = false)
        {
            cell.Value = value;
            cell.Status = isEstimation ? CellStatus.Estimated : CellStatus.Solved;
            cell.PossibleValues.Clear();
            RemovePossibility(cell.Row, cell.Column, cell.Block, cell.Value);
            pElimination = true;
        }

        private void PossibilityElimination1()
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
                        int t_row = -1, t_column = -1, t_block = -1;

                        if (p1.Row == p2.Row) t_row = p1.Row;
                        else if (p1.Column == p2.Column) t_column = p1.Column;
                        if (p1.Block == p2.Block) t_block = p1.Block;

                        if (t_row == -1 && t_column == -1 && t_block == -1) continue;

                        int val1 = p1.PossibleValues[0];
                        int val2 = p1.PossibleValues[1];

                        RemovePossibility(t_row, t_column, t_block, val1);
                        RemovePossibility(t_row, t_column, t_block, val2);

                        p1.PossibleValues = new List<int> { val1, val2 };
                        p2.PossibleValues = new List<int> { val1, val2 };
                    }
                }
            }
        }

        private void PossibilityElimination2()
        {
            var unknown = Position.Cast<Cell>().Where(p => p.Value == 0).ToList();

            for (int row = 0; row < 9; row++)
            {
                var cellinrow = unknown.Where(p => p.Row == row).ToList();
                var keyValuePairs = new Dictionary<int, int>();
                
                foreach (var p in cellinrow)
                {
                    foreach (int s in p.PossibleValues)
                    {
                        if (keyValuePairs.ContainsKey(s)) keyValuePairs[s]++;
                        else keyValuePairs[s] = 1;
                    }
                }

                var pair = keyValuePairs.Where(kvp => kvp.Value == 2).Select(kvp => kvp.Key).ToList();
                for (int i = 0; i < pair.Count; i++)
                {
                    for (int j = i + 1; j < pair.Count; j++)
                    {
                        var exist = cellinrow.Where(u => u.PossibleValues.Contains(pair[i]) && u.PossibleValues.Contains(pair[j])).ToList();
                        if (exist.Count == 2)
                        {
                            int t_block = exist[0].Block == exist[1].Block ? exist[0].Block : -1;
                            var p1 = exist[0];
                            var p2 = exist[1];

                            RemovePossibility(row, -1, t_block, pair[i]);
                            RemovePossibility(row, -1, t_block, pair[j]);

                            p1.PossibleValues = new List<int> { pair[i], pair[j] };
                            p2.PossibleValues = new List<int> { pair[i], pair[j] };
                        }
                    }
                }
            }

            for (int col = 0; col < 9; col++)
            {
                var cellincol = unknown.Where(p => p.Column == col).ToList();
                var keyValuePairs = new Dictionary<int, int>();
                
                foreach (var p in cellincol)
                {
                    foreach (int s in p.PossibleValues)
                    {
                        if (keyValuePairs.ContainsKey(s)) keyValuePairs[s]++;
                        else keyValuePairs[s] = 1;
                    }
                }

                var pair = keyValuePairs.Where(kvp => kvp.Value == 2).Select(kvp => kvp.Key).ToList();
                for (int i = 0; i < pair.Count; i++)
                {
                    for (int j = i + 1; j < pair.Count; j++)
                    {
                        var exist = cellincol.Where(u => u.PossibleValues.Contains(pair[i]) && u.PossibleValues.Contains(pair[j])).ToList();
                        if (exist.Count == 2)
                        {
                            int t_block = exist[0].Block == exist[1].Block ? exist[0].Block : -1;
                            var p1 = exist[0];
                            var p2 = exist[1];

                            RemovePossibility(-1, col, t_block, pair[i]);
                            RemovePossibility(-1, col, t_block, pair[j]);

                            p1.PossibleValues = new List<int> { pair[i], pair[j] };
                            p2.PossibleValues = new List<int> { pair[i], pair[j] };
                        }
                    }
                }
            }

            for (int block = 1; block <= 9; block++)
            {
                var cellsinblock = unknown.Where(p => p.Block == block).ToList();
                var keyValuePairs = new Dictionary<int, int>();
                
                foreach (var p in cellsinblock)
                {
                    foreach (int s in p.PossibleValues)
                    {
                        if (keyValuePairs.ContainsKey(s)) keyValuePairs[s]++;
                        else keyValuePairs[s] = 1;
                    }
                }

                var pair = keyValuePairs.Where(kvp => kvp.Value == 2).Select(kvp => kvp.Key).ToList();
                for (int i = 0; i < pair.Count; i++)
                {
                    for (int j = i + 1; j < pair.Count; j++)
                    {
                        var exist = cellsinblock.Where(u => u.PossibleValues.Contains(pair[i]) && u.PossibleValues.Contains(pair[j])).ToList();

                        if (exist.Count == 2)
                        {
                            var p1 = exist[0];
                            var p2 = exist[1];

                            RemovePossibility(-1, -1, block, pair[i]);
                            RemovePossibility(-1, -1, block, pair[j]);

                            p1.PossibleValues = new List<int> { pair[i], pair[j] };
                            p2.PossibleValues = new List<int> { pair[i], pair[j] };
                        }
                    }
                }
            }
        }

        private void PossibilityElimination3()
        {
            var unknown = Position.Cast<Cell>().Where(p => p.Value == 0).ToList();
            
            for (int i = 1; i <= 9; i++)
            {
                Dictionary<int, string> blockrowsPair = new Dictionary<int, string>();
                var temp = unknown.Where(p => p.PossibleValues.Contains(i));
                
                foreach (var p in temp)
                {
                    if (blockrowsPair.ContainsKey(p.Block) && !blockrowsPair[p.Block].Contains(p.Row.ToString()))
                        blockrowsPair[p.Block] += p.Row.ToString();
                    else if (!blockrowsPair.ContainsKey(p.Block))
                        blockrowsPair[p.Block] = p.Row.ToString();
                }
                
                for (int j = 1; j <= 9; j += 3)
                {
                    if (!(blockrowsPair.ContainsKey(j) && blockrowsPair.ContainsKey(j + 1) && blockrowsPair.ContainsKey(j + 2))) continue;
                    
                    string blockx = String.Concat(blockrowsPair[j].OrderBy(c => c));
                    string blocky = String.Concat(blockrowsPair[j + 1].OrderBy(c => c));
                    string blockz = String.Concat(blockrowsPair[j + 2].OrderBy(c => c));

                    int blocktochange = 0;
                    string excluding = "";

                    if (blockx == blocky && blockx.Length == 2) { excluding = blockx; blocktochange = j + 2; }
                    else if (blockx == blockz && blockx.Length == 2) { excluding = blockx; blocktochange = j + 1; }
                    else if (blocky == blockz && blocky.Length == 2) { excluding = blocky; blocktochange = j; }
                    else continue;

                    var exist = unknown
                        .Where(u => u.Block == blocktochange)
                        .Where(u => u.PossibleValues.Contains(i))
                        .Where(u => excluding.Contains(u.Row.ToString()));

                    foreach (var e in exist) e.PossibleValues.Remove(i);
                }
            }
            
            for (int i = 1; i <= 9; i++)
            {
                Dictionary<int, string> blockcolumnsPair = new Dictionary<int, string>();
                var temp = unknown.Where(p => p.PossibleValues.Contains(i));
                
                foreach (var p in temp)
                {
                    if (blockcolumnsPair.ContainsKey(p.Block) && !blockcolumnsPair[p.Block].Contains(p.Column.ToString()))
                        blockcolumnsPair[p.Block] += p.Column.ToString();
                    else if (!blockcolumnsPair.ContainsKey(p.Block))
                        blockcolumnsPair[p.Block] = p.Column.ToString();
                }
                
                for (int j = 1; j <= 9; j += 3)
                {
                    if (!(blockcolumnsPair.ContainsKey(j) && blockcolumnsPair.ContainsKey(j + 3) && blockcolumnsPair.ContainsKey(j + 6))) continue;
                    
                    string blockx = String.Concat(blockcolumnsPair[j].OrderBy(c => c));
                    string blocky = String.Concat(blockcolumnsPair[j + 3].OrderBy(c => c));
                    string blockz = String.Concat(blockcolumnsPair[j + 6].OrderBy(c => c));
                    
                    int blocktochange = 0;
                    string excluding = "";
                    
                    if (blockx == blocky && blockx.Length == 2) { excluding = blockx; blocktochange = j + 6; }
                    else if (blockx == blockz && blockx.Length == 2) { excluding = blockx; blocktochange = j + 3; }
                    else if (blocky == blockz && blocky.Length == 2) { excluding = blocky; blocktochange = j; }
                    else continue;
                    
                    var exist = unknown
                        .Where(u => u.Block == blocktochange)
                        .Where(u => u.PossibleValues.Contains(i))
                        .Where(u => excluding.Contains(u.Column.ToString()));
                        
                    foreach (var e in exist) e.PossibleValues.Remove(i);
                }
            }
        }

        private void RemovePossibility(int row, int column, int block, int value)
        {
            var unknown = Position.Cast<Cell>().Where(p => p.Value == 0);

            foreach (var p in unknown)
            {
                if (p.Row == row || p.Column == column || p.Block == block)
                {
                    p.PossibleValues.Remove(value);
                    p.UpdateUIProperties(); // <--- ADD THIS LINE to alert the UI bindings
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
            if (currentEstimation[key] > 1)
            {
                currentEstimation.Remove(key);
                key--;
                currentEstimation[key]++;
                ClearEstimation(key);
                EstimationSolving(key);
                return;
            }   

            var unknown = Position.Cast<Cell>().Where(p => p.Value == 0);

            foreach (var p in unknown) p.TempSolving = key;

            var shortest = unknown.Where(p => p.PossibleValues.Count == 2);

            if (!shortest.Any())
            {
                currentEstimation.Remove(key);
                key--;
                currentEstimation[key]++;
                ClearEstimation(key);
                EstimationSolving(key);
                return;
            }

            var firstShortest = shortest.First();
            MarkCellSolved(firstShortest, firstShortest.PossibleValues[currentEstimation.Last().Value], true);
        }
    }
}