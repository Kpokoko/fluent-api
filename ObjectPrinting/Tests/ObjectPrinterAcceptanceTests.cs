using System;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void Demo()
        {
            var person = new Person { Name = "Alex", Age = 19 };

            var printer = ObjectPrinter.For<Person>()
                .ExcludePropertyOfType<TestClass>();
                //.WithTypeSerializtionType<Person>(new PersonSerializer())
                // .SetDigitsCulture<culture_info>()
                // .WtihPropertySerializationType<serialization_type>(property_name)
                // .TrimString<trim_type>()
                // .ExcludeProperty(property_name);
                //1. Исключить из сериализации свойства определенного типа
                //2. Указать альтернативный способ сериализации для определенного типа
                //3. Для числовых типов указать культуру
                //4. Настроить сериализацию конкретного свойства
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                //6. Исключить из сериализации конкретного свойства
            
            string s1 = printer.PrintToString(person);
            Console.WriteLine(s1);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
            //8. ...с конфигурированием
        }

        [Test]
        public void ClassWithNestedClassMember()
        {
            var person = new Person { Name = "Alex", Age = 19 };
            var test = new TestClass { Id = Guid.NewGuid(), Name = "AlexTest" };
            person.TestClass = test;
            var person2 = new Person { Name = "Alexxxxxxxxxxxx", Age = 21 };
            test.Person = person2;
            var printer = ObjectPrinter.For<Person>()
                .ExcludePropertyOfType<TestClass>();
                //.WithTypeSerializtionType<TestClass>(new MyItem());
            string s1 = printer.PrintToString(person);
            Console.WriteLine(s1);
        }
    }
}