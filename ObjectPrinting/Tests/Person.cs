using System;

namespace ObjectPrinting.Tests
{
    public class Person
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Height { get; set; }
        public int Age { get; set; }
        public TestClass TestClass { get; set; }
    }

    public class TestClass
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Person Person { get; set; }
    }
}