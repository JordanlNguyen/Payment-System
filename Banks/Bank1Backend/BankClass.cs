using Npgsql;

namespace Banks
{
    public class Bank
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string RoutingNumber { get; set; }
        public string ConnectionString { get; set; }

        public Bank(string name, string address, string phoneNumber, string routingNumber, string connectionString)
        {
            Name = name;
            Address = address;
            PhoneNumber = phoneNumber;
            RoutingNumber = routingNumber;
            ConnectionString = connectionString;
        }
    }
}
