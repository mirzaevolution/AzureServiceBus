using System;

namespace Rewind.AzFunc.In.Profile.ServiceBusTrigger
{
    public class Profile
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public string Job { get; set; }
        public override string ToString()
        {
            return $"Id: {Id}, Name: {Name}, Job: {Job}";
        }
    }
}
