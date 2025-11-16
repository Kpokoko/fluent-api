using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void DemonstrationTest()
        {
            var person = new Person { Name = "Alex", Surname = "Smith", Age = 19, Height = 180.5};
            person.Phone = new Phone {Name = "Смартфон Vivo", Owner = person};

            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .ExcludePropertyOfType<int>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .WithTypeSerializtionType<Phone>(new PhoneSerializer())
                //3. Для числовых типов указать культуру
                .SetTypeCulture<double>(CultureInfo.InvariantCulture)
                //4. Настроить сериализацию конкретного свойства
                .GetProperty(x => x.Name).SetSerializationType(new NameSerializer())
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .GetProperty(x => x.Surname).TrimStringToLength(3)
                //6. Исключить из сериализации конкретное свойства
                .GetProperty(x => x.Id).Exclude();
            
            string s1 = printer.PrintToString(person);
            Console.WriteLine(s1);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
            var s2 = person.PrintToString();
            Console.WriteLine("\n\n\n" + s2);
            //8. ...с конфигурированием
            var s3 = person.PrintToString(x => x.ExcludePropertyOfType<string>());
            Console.WriteLine("\n\n\n" + s3);
        }

        [Test]
        public void ExcludeTypeTest()
        {
            var expected = "Person\r\n\tName = Alex\r\n\tSurname = Smith\r\n";
            var person = new Person {Name = "Alex", Surname = "Smith", Age = 19, Height = 180.5};
            var printer = ObjectPrinter
                .For<Person>()
                .ExcludePropertyOfType<Guid>()
                .ExcludePropertyOfType<double>()
                .ExcludePropertyOfType<int>()
                .ExcludePropertyOfType<Phone>();
            var ans = printer.PrintToString(person);
            ans.Should().Be(expected);
        }

        [Test]
        public void UniqueTypeSerializationTest()
        {
            var expected = "Person\r\n\tId = 999999999\r\n\tName = Alex\r\n\tSurname = Smith\r\n\tHeight = 180,5\r\n\tAge = 19\r\n\tPhone = my phone\r\n";
            var person = new Person {Name = "Alex", Surname = "Smith", Age = 19, Height = 180.5};
            person.Phone = new Phone {Name = "Смартфон Vivo"};
            var printer = ObjectPrinter
                .For<Person>()
                .WithTypeSerializtionType<Guid>(new GuidSerializer())
                .WithTypeSerializtionType<Phone>(new PhoneSerializer());
            var ans = printer.PrintToString(person);
            ans.Should().Be(expected);
        }

        [Test]
        public void UniqueTypeCultureTest()
        {
            var expected = "Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tSurname = Smith\r\n\tHeight = 180,5\r\n\tAge = 19\r\n\tPhone = null\r\n";
            var person = new Person {Name = "Alex", Surname = "Smith", Age = 19, Height = 180.5};
            var printer = ObjectPrinter
                .For<Person>()
                .SetTypeCulture<double>(CultureInfo.CurrentCulture);
            var ans = printer.PrintToString(person);
            ans.Should().Be(expected);
        }

        [Test]
        public void GetPropertyTest()
        {
            var expected = "Id";
            var person = new Person {Name = "Alex", Surname = "Smith", Age = 19, Height = 180.5};
            var printer = ObjectPrinter
                .For<Person>()
                .GetProperty(x => x.Id);
            var ans = printer.PropertyName;
            ans.Should().Be(expected);
        }

        [Test]
        public void SetPropertySerializationType()
        {
            var expected = "Person\r\n\tId = Guid\r\n\tName = my name\r\n\tSurname = Smith\r\n\tHeight = 180,5\r\n\tAge = 19\r\n\tPhone = null\r\n";
            var person = new Person {Name = "Alex", Surname = "Smith", Age = 19, Height = 180.5};
            var printer = ObjectPrinter
                .For<Person>()
                .GetProperty(x => x.Name).SetSerializationType(new NameSerializer());
            var ans = printer.PrintToString(person);
            ans.Should().Be(expected);
        }

        [Test]
        public void SetPropertyTrimTest()
        {
            var expected = "Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tSurname = Very \r\n\tHeight = 180,5\r\n\tAge = 19\r\n\tPhone = null\r\n";
            var person = new Person {Name = "Alex", Surname = "Very long surname", Age = 19, Height = 180.5};
            var printer = ObjectPrinter
                .For<Person>()
                .GetProperty(x => x.Surname).TrimStringToLength(5);
            var ans = printer.PrintToString(person);
            ans.Should().Be(expected);
        }

        [Test]
        public void ExcludePropertyTest()
        {
            var expected = "Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tSurname = Smith\r\n\tHeight = 180,5\r\n\tAge = 19\r\n";
            var person = new Person {Name = "Alex", Surname = "Smith", Age = 19, Height = 180.5};
            var printer = ObjectPrinter
                .For<Person>()
                .GetProperty(x => x.Phone).Exclude();
            var ans = printer.PrintToString(person);
            ans.Should().Be(expected);
        }

        [Test]
        public void LoopReferencesTest()
        {
            var expected = "Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tSurname = Smith\r\n\tHeight = 180,5" +
                           "\r\n\tAge = 19\r\n\tPhone = Phone\r\n\t\tId = Guid\r\n\t\t" +
                           "Name = My phone\r\n\t\tOwner = Already serialized!\r\n";
            var person = new Person {Name = "Alex", Surname = "Smith", Age = 19, Height = 180.5};
            var phone = new Phone {Name = "My phone", Owner = person};
            person.Phone = phone;
            var printer = ObjectPrinter
                .For<Person>();
            var ans = printer.PrintToString(person);
            ans.Should().Be(expected);
        }

        [Test]
        public void ArraySerializationTest()
        {
            var array = new int[]{1, 2, 3};
            var expected = array.ToString();
            var printer = ObjectPrinter
                .For<int[]>();
            var ans = printer.PrintToString(array);
            ans.Should().Be(expected);
        }
    }
}