using LogicLibrary.DTO.Models;

namespace LogicLibrary.Logics;

public class FoodService
{
    // ── State (owned here, not in Razor) ────────────────────────────────────
    public List<Food> Foods { get; private set; } = new();
    public List<string> Categories { get; private set; } = new();
    public string? SelectedCategory { get; private set; }
    public int TotalCalories { get; private set; }
    public int SelectedCount { get; private set; }
    public bool IsLoading { get; private set; } = true;
    public string? ErrorMessage { get; private set; }

    public event Action? StateHasChanged;

    public IEnumerable<Food> FilteredFoods => SelectedCategory is null
        ? Foods
        : Foods.Where(f => f.Category == SelectedCategory);

    // ── Initialisation ───────────────────────────────────────────────────────
    public async Task LoadFoodsAsync(HttpClient http)
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var csv = await http.GetStringAsync("data/foods.csv");
            Foods = ParseFoods(csv);
            Categories = Foods.Select(f => f.Category).Distinct().ToList();
            UpdateTotals();
            NotifyStateChanged(); // Add this
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Could not load food data: {ex.Message}";
            NotifyStateChanged(); // Add this
        }
        finally
        {
            IsLoading = false;
            NotifyStateChanged(); // Add this
        }
    }

    // ── Category filter ──────────────────────────────────────────────────────

    public void SelectCategory(string? category)
    {
        SelectedCategory = category;
        NotifyStateChanged(); // Add this
    }

    // ── Counter actions ──────────────────────────────────────────────────────

    public void Increment(Food food)
    {
        food.Count++;
        UpdateTotals();
        NotifyStateChanged(); // Add this
    }

    public void Decrement(Food food)
    {
        if (food.Count > 0)
        {
            food.Count--;
            UpdateTotals();
            NotifyStateChanged(); // Add this
        }
    }

    public void ResetAll()
    {
        foreach (var food in Foods)
            food.Count = 0;

        UpdateTotals();
        NotifyStateChanged(); // Add this
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private void UpdateTotals()
    {
        TotalCalories = Foods.Sum(f => f.TotalCalories);
        SelectedCount = Foods.Sum(f => f.Count);
    }

    private void NotifyStateChanged() // Add this helper method
    {
        StateHasChanged?.Invoke();
    }

    private static List<Food> ParseFoods(string csvContent)
    {
        var foods = new List<Food>();

        var lines = csvContent
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim())
            .ToList();

        int startIndex = 0;
        if (lines.Count > 0 && lines[0].StartsWith("Name", StringComparison.OrdinalIgnoreCase))
            startIndex = 1;

        for (int i = startIndex; i < lines.Count; i++)
        {
            var parts = lines[i].Split(',');
            if (parts.Length < 2) continue;

            foods.Add(new Food
            {
                Name = parts[0].Trim(),
                CaloriesPerServing = int.TryParse(parts[1].Trim(), out var cal) ? cal : 0,
                Category = parts.Length >= 3 ? parts[2].Trim() : "General",
                ImageUrl = parts.Length >= 4 ? parts[3].Trim()
                                     : "https://placehold.co/300x200?text=No+Image"
            });
        }

        return foods;
    }
}