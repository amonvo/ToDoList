namespace ToDoList
{
    class Program
    {
        // Globální seznam úkolů a cesta k souboru pro uložení seznamu úkolů
        static List<TodoItem> todoList = new List<TodoItem>();
        const string filePath = "todolist.txt";


        static void Main(string[] args)
        {
            LoadTodoListFromFile(); // Načte uložené úkoly ze souboru
            CheckUpcomingDeadlines(); // Nová funkce pro upozornění na blížící se termíny

            bool running = true;
            while (running)
            {
                // Zobrazuje hlavní menu aplikace
                Console.Clear();
                Console.WriteLine("To-Do List Application");
                Console.WriteLine("======================");
                Console.WriteLine("1. Zobrazit seznam úkolů");
                Console.WriteLine("2. Přidat nový úkol");
                Console.WriteLine("3. Odstranit úkol");
                Console.WriteLine("4. Označit úkol jako splněný");
                Console.WriteLine("5. Nastavit úkol jako prioritní");
                Console.WriteLine("6. Filtrovat úkoly");
                Console.WriteLine("7. Uložit a ukončit aplikaci");
                Console.WriteLine("8. Zobrazit statistiky");
                Console.WriteLine("9. Pouze uložit změny");  // Změněný název pro bod 9
                Console.WriteLine("10. Vyhledat úkol s našeptávačem");
                Console.Write("\nVyber akci: ");

                string choice = Console.ReadLine();
                // Výběr akce na základě uživatelského vstupu
                switch (choice)
                {
                    case "1":
                        DisplayTodoList(); // Zobrazí seznam úkolů
                        break;
                    case "2":
                        AddTodo(); // Přidá nový úkol
                        break;
                    case "3":
                        RemoveTodo(); // Odstraní úkol
                        break;
                    case "4":
                        MarkTodoAsCompleted(); // Označí úkol jako splněný
                        break;
                    case "5":
                        MarkTodoAsPriority(); // Nastaví úkol jako prioritní
                        break;
                    case "6":
                        FilterTodoList(); // Filtrovat úkoly podle kategorie
                        break;
                    case "7":
                        SaveTodoListToFile();
                        running = false;  // Uloží seznam úkolů a ukončí aplikaci
                        break;
                    case "8":
                        ShowStatistics();  // Zobrazí statistiky úkolů
                        break;
                    case "9":
                        SaveTodoListToFile();  // Pouze uloží změny a nechá aplikaci běžet
                        Console.WriteLine("Změny byly uloženy.");
                        Console.WriteLine("\nStiskněte libovolnou klávesu pro návrat do menu.");
                        Console.ReadKey();
                        break;
                    case "10":  
                        SearchTodoWithSuggestions();  // Vyhledá úkoly s našeptávačem
                        break;
                    default:
                        Console.WriteLine("Neplatná volba. Zkuste to znovu.");
                        break;
                }
            }
        }

        // Funkce pro upozornění na blížící se termíny úkolů
        static void CheckUpcomingDeadlines()
        {
            Console.Clear();
            foreach (var todo in todoList)
            {
                // Zkontroluje, jestli má úkol nastavený termín a není splněn
                if (todo.Deadline.HasValue && !todo.IsCompleted)
                {
                    TimeSpan timeLeft = todo.Deadline.Value - DateTime.Now;
                    if (timeLeft.TotalHours <= 24 && timeLeft.TotalHours > 0)
                    {
                        // Upozornění, že úkol má termín do 24 hodin
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"Upozornění: Úkol '{todo.Description}' má termín do 24 hodin!");
                        Console.ResetColor();
                    }
                    else if (timeLeft.TotalHours < 0)
                    {
                        // Varování, že úkol má prošlý termín
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Varování: Úkol '{todo.Description}' má prošlý termín!");
                        Console.ResetColor();
                    }
                }

                // Pokud je úkol opakující se a dokončen, vytvoří nový úkol
                if (todo.IsCompleted && !string.IsNullOrEmpty(todo.RepeatFrequency))
                {
                    GenerateNextRecurringTask(todo); // Vytvoří nový opakující se úkol
                }
            }
            Console.WriteLine("\nStiskněte libovolnou klávesu pro pokračování.");
            Console.ReadKey();
        }

        // Vytvoří nový úkol na základě opakujícího se úkolu
        static void GenerateNextRecurringTask(TodoItem completedTask)
        {
            DateTime? newDeadline = null;
            if (completedTask.Deadline.HasValue)
            {
                // Nastaví nový termín splnění podle frekvence opakování (denní, týdenní, měsíční)
                switch (completedTask.RepeatFrequency)
                {
                    case "denně":
                        newDeadline = completedTask.Deadline.Value.AddDays(1);
                        break;
                    case "týdně":
                        newDeadline = completedTask.Deadline.Value.AddDays(7);
                        break;
                    case "měsíčně":
                        newDeadline = completedTask.Deadline.Value.AddMonths(1);
                        break;
                }
            }

            // Přidá nový opakující se úkol do seznamu
            todoList.Add(new TodoItem
            {
                Description = completedTask.Description,
                IsCompleted = false,
                IsPriority = completedTask.IsPriority,
                Deadline = newDeadline,
                Category = completedTask.Category,
                RepeatFrequency = completedTask.RepeatFrequency
            });

            Console.WriteLine($"Opakující se úkol '{completedTask.Description}' byl vytvořen s novým termínem.");
        }

        // Funkce pro vyhledávání úkolu s klíčovým slovem
        static void SearchTodo()
        {
            Console.Clear();
            Console.Write("Zadejte klíčové slovo pro vyhledávání: ");
            string keyword = Console.ReadLine().ToLower();

            // Najde všechny úkoly, které obsahují zadané klíčové slovo
            List<TodoItem> foundItems = todoList.FindAll(todo => todo.Description.ToLower().Contains(keyword));

            // Zobrazí nalezené úkoly
            if (foundItems.Count == 0)
            {
                Console.WriteLine("Žádné úkoly neodpovídají klíčovému slovu.");
            }
            else
            {
                Console.WriteLine($"Nalezené úkoly s klíčovým slovem '{keyword}':");
                foreach (var todo in foundItems)
                {
                    string status = todo.IsCompleted ? " (Splněno)" : " (Nesplněno)";
                    string priority = todo.IsPriority ? " [Prioritní]" : "";
                    string deadline = todo.Deadline.HasValue ? $" (Termín: {todo.Deadline.Value.ToShortDateString()})" : "";
                    Console.WriteLine($"{todo.Description}{status}{priority}{deadline}");
                }
            }
            Console.WriteLine("\nStiskněte libovolnou klávesu pro návrat do menu.");
            Console.ReadKey();
        }

        // Funkce pro zobrazení statistik úkolů
        static void ShowStatistics()
        {
            int totalTasks = todoList.Count; // Celkový počet úkolů
            int completedTasks = todoList.FindAll(todo => todo.IsCompleted).Count; // Počet splněných úkolů
            int pendingTasks = totalTasks - completedTasks; // Počet nesplněných úkolů

            Console.Clear();
            Console.WriteLine("Statistiky úkolů:");
            Console.WriteLine("=================");
            Console.WriteLine($"Celkový počet úkolů: {totalTasks}");
            Console.WriteLine($"Splněné úkoly: {completedTasks}");
            Console.WriteLine($"Nesplněné úkoly: {pendingTasks}");

            // Zobrazí další statistiky (např. úkoly s termínem)
            var tasksWithDeadline = todoList.FindAll(todo => todo.Deadline.HasValue);
            if (tasksWithDeadline.Count > 0)
            {
                Console.WriteLine($"Úkoly s termínem: {tasksWithDeadline.Count}");
            }

            var overdueTasks = tasksWithDeadline.FindAll(todo => todo.Deadline.Value < DateTime.Now && !todo.IsCompleted);
            if (overdueTasks.Count > 0)
            {
                Console.WriteLine($"Prošlé úkoly: {overdueTasks.Count}");
            }

            Console.WriteLine("\nStiskněte libovolnou klávesu pro návrat do menu.");
            Console.ReadKey();
        }


        // Funkce pro zobrazení a třídění seznamu úkolů
        static void DisplayTodoList()
        {
            Console.Clear();
            if (todoList.Count == 0)
            {
                Console.WriteLine("Seznam úkolů je prázdný.");
            }
            else
            {
                // Seřadí úkoly podle priority a termínu splnění
                todoList.Sort((x, y) =>
                {
                    int priorityComparison = y.IsPriority.CompareTo(x.IsPriority);
                    if (priorityComparison == 0)
                    {
                        if (x.Deadline.HasValue && y.Deadline.HasValue)
                        {
                            return x.Deadline.Value.CompareTo(y.Deadline.Value);
                        }
                        return 0;
                    }
                    return priorityComparison;
                });

                // Vypíše úkoly
                Console.WriteLine("Seznam úkolů:");
                for (int i = 0; i < todoList.Count; i++)
                {
                    string status = todoList[i].IsCompleted ? " (Splněno)" : " (Nesplněno)";
                    string priority = todoList[i].IsPriority ? " [Prioritní]" : "";
                    string deadline = todoList[i].Deadline.HasValue ? $" (Termín: {todoList[i].Deadline.Value.ToShortDateString()})" : "";
                    string category = string.IsNullOrWhiteSpace(todoList[i].Category) ? "" : $" [{todoList[i].Category}]";
                    Console.WriteLine($"{i + 1}. {todoList[i].Description}{status}{priority}{deadline}{category}");
                }
            }
            Console.WriteLine("\nStiskněte libovolnou klávesu pro návrat do menu.");
            Console.ReadKey();
        }



        // Přidání nového úkolu s volitelným termínem, kategorií a frekvencí opakování
        static void AddTodo()
        {
            Console.Clear();
            Console.Write("Zadejte nový úkol: ");
            string newTodo = Console.ReadLine();

            // Nabídne možnost nastavit termín splnění
            Console.Write("Chcete nastavit termín splnění? (y/n): ");
            bool setDeadline = Console.ReadLine().ToLower() == "y";
            DateTime? deadline = null;
            if (setDeadline)
            {
                Console.Write("Zadejte termín (yyyy-mm-dd): ");
                if (DateTime.TryParse(Console.ReadLine(), out DateTime date))
                {
                    deadline = date;
                }
            }

            // Nabídne možnost přidat kategorii
            Console.Write("Zadejte kategorii úkolu (např. Práce, Osobní, Důležité): ");
            string category = Console.ReadLine();

            // Nabídne možnost, zda má být úkol opakující se
            Console.Write("Chcete, aby byl úkol opakující se? (y/n): ");
            bool isRepeating = Console.ReadLine().ToLower() == "y";
            string repeatFrequency = null;
            if (isRepeating)
            {
                Console.Write("Zadejte frekvenci opakování (denně, týdně, měsíčně): ");
                repeatFrequency = Console.ReadLine();
            }

            // Přidá nový úkol do seznamu
            todoList.Add(new TodoItem
            {
                Description = newTodo,
                IsCompleted = false,
                IsPriority = false,
                Deadline = deadline,
                Category = category,
                RepeatFrequency = repeatFrequency
            });
            Console.WriteLine("Úkol přidán!");
            SaveTodoListToFile();
            Console.WriteLine("\nStiskněte libovolnou klávesu pro návrat do menu.");
            Console.ReadKey();
        }

        // Funkce pro vyhledávání úkolu s našeptávačem
        static void SearchTodoWithSuggestions()
        {
            Console.Clear();
            Console.Write("Zadejte klíčové slovo pro vyhledávání: ");
            string keyword = "";
            ConsoleKeyInfo keyInfo;

            do
            {
                keyInfo = Console.ReadKey(true); // Získá stisknutou klávesu, ale nevypíše ji
                if (keyInfo.Key == ConsoleKey.Backspace && keyword.Length > 0)
                {
                    // Smaže poslední znak
                    keyword = keyword.Substring(0, keyword.Length - 1);
                    Console.Clear();
                    Console.Write($"Zadejte klíčové slovo pro vyhledávání: {keyword}");
                }
                else if (keyInfo.Key != ConsoleKey.Enter && keyInfo.Key != ConsoleKey.Backspace)
                {
                    // Přidá stisknutý znak do klíčového slova
                    keyword += keyInfo.KeyChar;
                    Console.Write(keyInfo.KeyChar); // Vypíše stisknutý znak
                }

                // Zobrazí návrhy během psaní
                ShowSuggestions(keyword);

            } while (keyInfo.Key != ConsoleKey.Enter);

            Console.WriteLine(); // Přesun na nový řádek po stisknutí Enteru

            // Zobrazení finálních výsledků po stisknutí Enter
            List<TodoItem> foundItems = todoList.FindAll(todo => todo.Description.ToLower().Contains(keyword.ToLower()));

            if (foundItems.Count == 0)
            {
                Console.WriteLine("Žádné úkoly neodpovídají klíčovému slovu.");
            }
            else
            {
                Console.WriteLine($"Nalezené úkoly s klíčovým slovem '{keyword}':");
                foreach (var todo in foundItems)
                {
                    string status = todo.IsCompleted ? " (Splněno)" : " (Nesplněno)";
                    string priority = todo.IsPriority ? " [Prioritní]" : "";
                    string deadline = todo.Deadline.HasValue ? $" (Termín: {todo.Deadline.Value.ToShortDateString()})" : "";
                    Console.WriteLine($"{todo.Description}{status}{priority}{deadline}");
                }
            }

            Console.WriteLine("\nStiskněte libovolnou klávesu pro návrat do menu.");
            Console.ReadKey();
        }

        // Zobrazí návrhy na základě zadávaného klíčového slova
        static void ShowSuggestions(string keyword)
        {
            Console.WriteLine(); // Vypíše návrhy úkolů odpovídajících zadání
            List<TodoItem> suggestions = todoList.FindAll(todo => todo.Description.ToLower().Contains(keyword.ToLower()));

            Console.WriteLine("\nNávrhy:");
            if (suggestions.Count == 0)
            {
                Console.WriteLine("Žádné návrhy.");
            }
            else
            {
                foreach (var todo in suggestions)
                {
                    Console.WriteLine(todo.Description);
                }
            }
        }


        // Přidání funkce filtrování úkolů podle kategorie
        static void FilterTodoList()
        {
            Console.Clear();
            Console.Write("Zadejte kategorii pro filtrování (např. Práce, Osobní, Důležité): ");
            string category = Console.ReadLine();

            List<TodoItem> filteredList = todoList.FindAll(todo => todo.Category == category);

            Console.Clear();
            if (filteredList.Count == 0)
            {
                Console.WriteLine($"Žádné úkoly v kategorii '{category}' nebyly nalezeny.");
            }
            else
            {
                Console.WriteLine($"Úkoly v kategorii '{category}':");
                foreach (var todo in filteredList)
                {
                    string status = todo.IsCompleted ? " (Splněno)" : " (Nesplněno)";
                    string priority = todo.IsPriority ? " [Prioritní]" : "";
                    string deadline = todo.Deadline.HasValue ? $" (Termín: {todo.Deadline.Value.ToShortDateString()})" : "";
                    Console.WriteLine($"{todo.Description}{status}{priority}{deadline}");
                }
            }
            Console.WriteLine("\nStiskněte libovolnou klávesu pro návrat do menu.");
            Console.ReadKey();
        }

        // Odstraní úkol ze seznamu
        static void RemoveTodo()
        {
            Console.Clear();
            if (todoList.Count == 0)
            {
                Console.WriteLine("Seznam úkolů je prázdný.");
            }
            else
            {
                DisplayTodoList();
                Console.Write("\nZadejte číslo úkolu, který chcete odstranit: ");
                if (int.TryParse(Console.ReadLine(), out int index) && index > 0 && index <= todoList.Count)
                {
                    todoList.RemoveAt(index - 1);
                    Console.WriteLine("Úkol odstraněn!");
                }
                else
                {
                    Console.WriteLine("Neplatné číslo úkolu.");
                }
            }
            SaveTodoListToFile();
            Console.WriteLine("\nStiskněte libovolnou klávesu pro návrat do menu.");
            Console.ReadKey();
        }

        // Označí úkol jako splněný
        static void MarkTodoAsCompleted()
        {
            Console.Clear();
            if (todoList.Count == 0)
            {
                Console.WriteLine("Seznam úkolů je prázdný.");
            }
            else
            {
                DisplayTodoList();
                Console.Write("\nZadejte číslo úkolu, který chcete označit jako splněný: ");
                if (int.TryParse(Console.ReadLine(), out int index) && index > 0 && index <= todoList.Count)
                {
                    todoList[index - 1].IsCompleted = true; // Označí úkol jako splněný
                    Console.WriteLine("Úkol označen jako splněný!");
                }
                else
                {
                    Console.WriteLine("Neplatné číslo úkolu.");
                }
            }
            SaveTodoListToFile(); // Uloží změny po označení úkolu
            Console.WriteLine("\nStiskněte libovolnou klávesu pro návrat do menu.");
            Console.ReadKey();
        }

        // Označí úkol jako prioritní
        static void MarkTodoAsPriority()
        {
            Console.Clear();
            if (todoList.Count == 0)
            {
                Console.WriteLine("Seznam úkolů je prázdný.");
            }
            else
            {
                DisplayTodoList();
                Console.Write("\nZadejte číslo úkolu, který chcete označit jako prioritní: ");
                if (int.TryParse(Console.ReadLine(), out int index) && index > 0 && index <= todoList.Count)
                {
                    todoList[index - 1].IsPriority = true; // Nastaví úkol jako prioritní
                    Console.WriteLine("Úkol označen jako prioritní!");
                }
                else
                {
                    Console.WriteLine("Neplatné číslo úkolu.");
                }
            }
            SaveTodoListToFile(); // Uloží změny po označení úkolu jako prioritního
            Console.WriteLine("\nStiskněte libovolnou klávesu pro návrat do menu.");
            Console.ReadKey();
        }


        // Uloží seznam úkolů do textového souboru
        static void SaveTodoListToFile() 
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                foreach (var todo in todoList)
                {
                    string deadline = todo.Deadline.HasValue ? todo.Deadline.Value.ToString("yyyy-MM-dd") : "";
                    writer.WriteLine($"{todo.Description}|{todo.IsCompleted}|{todo.IsPriority}|{deadline}");
                }
            }
            Console.WriteLine("Seznam úkolů byl uložen.");
        }

        // Načte seznam úkolů ze souboru při spuštění aplikace
        static void LoadTodoListFromFile()
        {
            if (File.Exists(filePath))
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var parts = line.Split('|');
                        todoList.Add(new TodoItem
                        {
                            Description = parts[0],
                            IsCompleted = bool.Parse(parts[1]),
                            IsPriority = bool.Parse(parts[2]),
                            Deadline = !string.IsNullOrWhiteSpace(parts[3]) ? DateTime.Parse(parts[3]) : (DateTime?)null
                        });
                    }
                }
            }
        }
    }



    // Třída reprezentující jednotlivý úkol
    class TodoItem
    {
        public string Description { get; set; } // Popis úkolu
        public bool IsCompleted { get; set; } // Označuje, zda je úkol splněn
        public bool IsPriority { get; set; } // Označuje, zda je úkol prioritní
        public DateTime? Deadline { get; set; } // Termín splnění úkolu (pokud je zadán)
        public string Category { get; set; } // Kategorie úkolu (např. Práce, Osobní)
        public string RepeatFrequency { get; set; } // Frekvence opakování úkolu (denně, týdně, měsíčně)
        public DateTime? CompletionDate { get; set; } // Datum, kdy byl úkol dokončen
    }
}
