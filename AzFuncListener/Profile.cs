using System;
using System.Collections.Generic;
using System.Text;

namespace AzFuncListener
{
    public class Profile
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public string Job { get; set; }
        public string Email { get; set; }
        public string Department { get; set; }
        public override string ToString()
        {
            return $"Id: {Id}, Name: {Name}, Job: {Job}, Department: {Department}, Email: {Email}";
        }
    }
}
