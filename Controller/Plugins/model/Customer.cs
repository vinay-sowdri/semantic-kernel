namespace SemanticKernelTraining.Models
{
public class Customer
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Membership { get; set; }
    public List<Order>? Orders { get; set; }
    public List<History>? History { get; set; }

}

public class Order
{
    public int Id { get; set; }
    public string Product { get; set; }
    public decimal Total { get; set; }
}

public class History
{
    public string Role { get; set; }
    public string Content { get; set; }
}
}