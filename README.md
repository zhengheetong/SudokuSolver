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
* **Step-by-Step "Solve"**: Watch the AI execute logic rules in real-time. Solved numbers appear in **Blue**.
* **Visual Estimation Fallback**: If logic gets stuck, the engine makes an educated guess. Guesses are visualized in **Purple**, allowing you to watch the backtracking algorithm self-correct.
* **Instant "Fast Solve"**: Bypasses the visual delays to solve the board instantly in the background.
* **State Locking**: Lock your initial inputs so they become read-only, preventing accidental modifications during the solve cycle.
* **Crash-Proof Validation**: Automatically detects completely unsolvable/broken puzzles and prevents infinite loops or application crashes.

## 🏗️ Architecture & Technical Highlights

This project was engineered to follow modern C# desktop development standards:
* **MVVM Pattern & Data Binding:** UI elements are fully decoupled from the logic. The grid automatically updates itself via `INotifyPropertyChanged` and XAML Data Bindings, eliminating UI thread freezing.
* **Separation of Concerns:** Strict division between the Data Model (`Cell.cs`), the View (`MainWindow.xaml.cs`), and the Math Engine (`SudokuEngine.cs`).

## 🧮 How the Logic Engine Works

The core of this solver is built on human-like strategies found in `SudokuEngine.cs`. The application runs through these phases:

1. **Naked & Hidden Singles**: Scans rows, columns, and 3x3 blocks to find instances where a number has only one valid cell remaining.
2. **Possibility Elimination 1 (Naked Pairs)**: Finds two cells in the same group that share the exact same two possibilities, and eliminates those numbers from the rest of the group.
3. **Possibility Elimination 2 & 3 (Intersections & X-Wings)**: Identifies scenarios where a possibility is confined to a single row/column within a block, eliminating it from the rest of that row/column.
4. **Dynamic DFS Backtracking (Estimation Solving)**: When logical algorithms can no longer progress, the program dynamically targets the cell with the *absolute fewest* possibilities left. It makes a guess and tags the board state. If the guess leads to a dead end, it utilizes a custom Depth-First Search rollback to instantly revert the state and try the next path.

## 🚀 Getting Started

To run this project on your local machine:

1. Ensure you have the **.NET 8.0 SDK** installed.
2. You can use either **[Visual Studio Code](https://code.visualstudio.com/)** (with the C# Dev Kit extension) or **[Visual Studio 2022](https://visualstudio.microsoft.com/)** (with the .NET desktop development workload).
3. Clone the repository:

        git clone https://github.com/zhengheetong/SudokuSolver.git

4. Open the `SudokuSolver.sln` file or the project folder in your IDE.
5. Hit **F5** or click **Start/Run** to build and launch the application.

## 🎮 How to Use

1. Click any green/white cell (or use Arrow Keys) and type a number (1-9).
2. Click **Lock** to lock the grid and reveal the possibilities (tiny red numbers) for all empty cells.
3. Click **Solve** to watch the algorithm deduce the answers visually.
4. Click **Reset** to clear the board and start a new puzzle.

## 🏆 Credits

* **Original Concept, Core Logic & Development:** Tong Zheng Hee
* **AI Assistance & Architectural Upgrades:** Refactored in collaboration with **Google Gemini**, featuring:
  * Migration to an enterprise-grade MVVM architecture (Data Bindings & `INotifyPropertyChanged`).
  * Separation of UI Views and Mathematical Logic into dedicated classes.
  * Upgrading the estimation logic into a dynamic Depth-First Search (DFS) backtracking algorithm.
  * UX Polish (Arrow key navigation, thread-freeze prevention, and UI sync fixes) completed using Visual Studio Code.

## 🤝 Contributing
Contributions, bug reports, and optimizations to the logic engine are welcome! Feel free to open an issue or submit a pull request.

## 📝 License
This project is open-source and available under the MIT License.
