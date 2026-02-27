# 🧠 C# WPF Sudoku Solver

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![WPF](https://img.shields.io/badge/WPF-Windows_Presentation_Foundation-blue?style=for-the-badge)
![Visual Studio](https://img.shields.io/badge/Visual_Studio-2022-5C2D91?style=for-the-badge&logo=visual-studio&logoColor=white)

A desktop Sudoku Solver application built with **C#, .NET 8.0, and Windows Presentation Foundation (WPF)**. 

Unlike brute-force solvers, this application visually mimics human deductive reasoning. It calculates possibilities, applies advanced Sudoku logic rules step-by-step, and only falls back to backtracking/estimation when logical deductions run out.

> **🌐 Looking for the Web Version?**
> I have ported this logic engine into a purely frontend **Cyberpunk-Themed Web App** with Image Scanning (OCR) capabilities! Check it out live [here](https://zhengheetong.github.io/Sudoku-App/).

---

## ✨ Features

* **Interactive 9x9 Grid**: A clean, colorful WPF interface where users can input starting numbers.
* **Possibility Visualization**: Empty cells automatically display their remaining possible valid numbers as tiny red text, exactly like penciling in notes on a real Sudoku board.
* **Step-by-Step "Solve"**: Watch the AI execute logic rules in real-time. Solved numbers appear in **Blue**.
* **Visual Estimation Fallback**: If logic gets stuck, the engine makes an educated guess. Guesses are visualized in **Purple**, allowing you to watch the backtracking algorithm self-correct.
* **Instant "Fast Solve"**: Bypasses the visual delays to solve the board instantly in the background.
* **State Locking**: Lock your initial inputs so they become read-only, preventing accidental modifications during the solve cycle.

## 🧮 How the Logic Engine Works

The core of this solver is built on human-like strategies found in `MainWindow.xaml.cs`. The application runs through these phases:

1. **Naked & Hidden Singles**: Scans rows, columns, and 3x3 blocks to find instances where a number has only one valid cell remaining.
2. **Possibility Elimination 1 (Naked Pairs)**: Finds two cells in the same group that share the exact same two possibilities, and eliminates those numbers from the rest of the group.
3. **Possibility Elimination 2 & 3 (Intersections)**: Identifies scenarios where a possibility is confined to a single row/column within a block, eliminating it from the rest of that row/column.
4. **Estimation Solving (Backtracking)**: When logical algorithms can no longer progress, the program selects the cell with the fewest possibilities and makes a guess. If the guess leads to a dead end, it reverts state and tries the next path.

## 🚀 Getting Started

To run this project on your local machine:

1. Ensure you have [Visual Studio 2022](https://visualstudio.microsoft.com/) installed with the **.NET desktop development** workload.
2. Clone the repository:

        git clone https://github.com/zhengheetong/SudokuSolver.git

3. Open `WPFTest3.sln` in Visual Studio.
4. Hit **F5** or click **Start** to build and run the application.

## 🎮 How to Use

1. Click any green/white cell and type a number (1-9).
2. Click **Lock** to lock the grid and reveal the possibilities (tiny red numbers) for all empty cells.
3. Click **Solve** to watch the algorithm deduce the answers visually.
4. Click **Reset** to clear the board and start a new puzzle.

## 🤝 Contributing
Contributions, bug reports, and optimizations to the logic engine are welcome! Feel free to open an issue or submit a pull request.

## 📝 License
This project is open-source and available under the MIT License.
