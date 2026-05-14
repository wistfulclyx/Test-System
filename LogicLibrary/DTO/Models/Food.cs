namespace LogicLibrary.DTO.Models;

public class Food
{
    public string Name { get; set; } = string.Empty;
    public int CaloriesPerServing { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int Count { get; set; } = 0;
    public int TotalCalories => Count * CaloriesPerServing;
}
