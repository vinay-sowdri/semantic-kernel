namespace SemanticKernelTraining.Models;
public class Flight{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime Date { get; set; }
    public decimal Price { get; set; }
    public string Destination { get; set; }
    public string Airline { get; set; }
    public bool IsBooked { get; set; }
}