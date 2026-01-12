namespace SemanticKernelTraining.Models
{
    /// <summary>
    /// Represents a customer with their membership and transaction details
    /// </summary>
    public class Customer
    {
        /// <summary>
        /// Gets or sets the customer's first name
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the customer's last name
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the customer's membership level
        /// </summary>
        public string Membership { get; set; }

        /// <summary>
        /// Gets or sets the customer's order history
        /// </summary>
        public List<Order>? Orders { get; set; }

        /// <summary>
        /// Gets or sets the customer's chat history
        /// </summary>
        public List<History>? History { get; set; }
    }

    /// <summary>
    /// Represents an order made by a customer
    /// </summary>
    public class Order
    {
        /// <summary>
        /// Gets or sets the order ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the product name
        /// </summary>
        public string Product { get; set; }

        /// <summary>
        /// Gets or sets the order total amount
        /// </summary>
        public decimal Total { get; set; }
    }

    /// <summary>
    /// Represents a chat history entry
    /// </summary>
    public class History
    {
        /// <summary>
        /// Gets or sets the role (User, System, Assistant)
        /// </summary>
        public string Role { get; set; }

        /// <summary>
        /// Gets or sets the message content
        /// </summary>
        public string Content { get; set; }
    }
}