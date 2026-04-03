# 🧠 C# WPF Sudoku Solver

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![WPF](https://img.shields.io/badge/WPF-Windows_Presentation_Foundation-blue?style=for-the-badge)
![Visual Studio](https://img.shields.io/badge/Visual_Studio-2022-5C2D91?style=for-the-badge&logo=visual-studio&logoColor=white)
![VS Code](https://img.shields.io/badge/VS_Code-0078D4?style=for-the-badge&logo=visual%20studio%20code&logoColor=white)

A desktop Sudoku Solver application built with **C#, .NET 8.0, and Windows Presentation Foundation (WPF)**.

Unlike brute-force solvers, this application visually mimics human deductive reasoning. It calculates possibilities, applies advanced Sudoku logic rules step-by-step, and only falls back to a highly optimized backtracking algorithm when logical deductions run out. Built with a clean, enterprise-grade **MVVM architecture**.

> **🌐 Looking for the Web Version?**
> I have ported this logic engine into a purely frontend **Cyberpunk-Themed Web App** with Image Scanning (OCR) capabilities! Check it out live [here](https://zhengheetong.github.io/Sudoku-App/).

---

## ✨ Features

* **Interactive 9x9 Grid**: A clean, colorful WPF interface where users can input starting numbers.
* **Seamless UX (Arrow Navigation)**: Use the Up, Down, Left, and Right arrow keys to smoothly navigate and wrap around the grid while typing.
* **Possibility Visualization**: Empty cells automatically display their remaining possible valid numbers as tiny red text, exactly like penciling in notes on a real Sudoku board.
* **Step-by-Step "Solve"**: Watch the engine execute logic rules in real-time. Solved numbers appear in **Blue**.
* **Visual Estimation Fallback**: If logic gets stuck, the engine makes an educated guess. Guesses appear in **Purple**, allowing you to watch the backtracking algorithm self-correct in real-time.
* **Instant "Fast Solve"**: Bypasses visual delays to solve the board instantly in the background.
* **State Locking**: Lock your initial inputs so they become read-only, preventing accidental modifications during the solve cycle.
* **Crash-Proof Validation**: Automatically detects unsolvable puzzles and prevents infinite loops or application crashes.

---

## 🏗️ Architecture & Technical Highlights

This project follows modern C# desktop development standards:

* **MVVM Pattern & Data Binding:** UI elements are fully decoupled from logic. The grid automatically updates via `INotifyPropertyChanged` and XAML Data Bindings, eliminating UI thread freezing.
* **Separation of Concerns:** Strict division between the Data Model (`Cell.cs`), the View (`MainWindow.xaml.cs`), and the Math Engine (`SudokuEngine.cs`).

---

## 🧮 How the Logic Engine Works

The solver in `SudokuEngine.cs` works through a sequence of human-like strategies, escalating in complexity:

### Phase 1 — Hidden Singles
Scans every row, column, and 3×3 box. If a number can only legally go in one cell within that unit, it is placed there immediately.

### Phase 2 — Naked Singles
If any individual cell has only one remaining candidate, that value is placed.

### Phase 3 — `ApplyNakedPairs()`
Finds two cells within the same unit (row, column, or box) that share exactly the same two candidates. Since those two values must occupy those two cells, they can be safely eliminated from all other cells in that unit.

### Phase 4 — `ApplyHiddenPairs()`
Identifies two values that only appear in the same two cells across a row, column, or box. All other candidates in those two cells are removed, as those two values must go there.

### Phase 5 — `ApplyPointingPairs()`
If a candidate within a box is confined to cells that all fall in a single row or column, that candidate can be eliminated from the rest of that row or column outside the box.

### Phase 6 — Dynamic DFS Backtracking (`EstimationSolving`)
When all logical techniques are exhausted, the engine dynamically targets the empty cell with the *fewest* remaining candidates and makes an educated guess. The board state is tagged at that depth. If the guess produces a contradiction, a Depth-First Search rollback instantly reverts the board and tries the next candidate.

---

## 🚀 Getting Started

1. Ensure you have the **.NET 8.0 SDK** installed.
2. Use either **[Visual Studio 2022](https://visualstudio.microsoft.com/)** (with the .NET Desktop workload) or **[VS Code](https://code.visualstudio.com/)** (with the C# Dev Kit extension).
3. Clone the repository:

        git clone https://github.com/zhengheetong/SudokuSolver.git

4. Open `SudokuSolver.sln` in your IDE.
5. Press **F5** or click **Run** to build and launch.

---

## 🎮 How to Use

1. Click any cell (or use **Arrow Keys**) and type a number from 1–9.
2. Click **Lock** to lock your inputs and reveal the pencil-mark possibilities for all empty cells.
3. Click **Solve** to step through the algorithm and watch it deduce answers visually.
4. Click **Fast Solve** to skip the visualization and solve instantly.
5. Click **Reset** to clear the board and start fresh.

---

## 🏆 Credits

* **Original Concept, Core Logic & Development:** Tong Zheng Hee
* **AI Assistance & Architectural Upgrades:** Refactored in collaboration with **Google Gemini**, featuring:
  * Migration to an enterprise-grade MVVM architecture (Data Bindings & `INotifyPropertyChanged`).
  * Separation of UI Views and Mathematical Logic into dedicated classes.
  * Upgrading the estimation logic into a dynamic Depth-First Search (DFS) backtracking algorithm.
  * Renaming elimination methods to standard Sudoku terminology (`ApplyNakedPairs`, `ApplyHiddenPairs`, `ApplyPointingPairs`).
  * UX polish (arrow key navigation, thread-freeze prevention, UI sync) completed in Visual Studio Code.

---

## 🤝 Contributing
Contributions, bug reports, and logic engine optimizations are welcome! Feel free to open an issue or submit a pull request.

## 📝 License
This project is open-source and available under the MIT License.
