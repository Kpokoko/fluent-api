using System;

namespace ObjectPrinting.Tests
{
    public class Person
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public double Height { get; set; }
        public int Age { get; set; }
        public Phone Phone { get; set; }
    }

    public class Phone
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Person Owner { get; set; }
    }
}