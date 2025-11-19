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
                .WithTypeSerializationStyle<Phone>(new Serializer((obj, nestingLevel, deepnessLevel) => "MyPhone" + Environment.NewLine))
                //3. Для числовых типов указать культуру
                .SetTypeCulture<double>(CultureInfo.InvariantCulture)
                //4. Настроить сериализацию конкретного свойства
                .GetPropertyByName(x => x.Name)
                    .SetSerializationStyle(new Serializer((obj, nestingLevel, deepnessLevel) => "MyName" + Environment.NewLine))
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .GetPropertyByName(x => x.Surname).TrimStringToLength(3)
                //6. Исключить из сериализации конкретное свойства
                .GetPropertyByName(x => x.Id).Exclude();
            
            string s1 = printer.PrintToString(person);
            Console.WriteLine(s1);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
            var s2 = person.PrintToString();
            Console.WriteLine("\n\n\n" + s2);
            //8. ...с конфигурированием
            var s3 = person.PrintToString(x => x.GetPropertyByName(p => p.Name).TrimStringToLength(3));
            Console.WriteLine("\n\n\n" + s3);
        }

        [Test]
        public void ExcludeType_ShouldRemove_PropertiesWithThisType()
        {
            var person = new Person {Name = "Alex", Surname = "Smith", Age = 19, Height = 180.5};
            var printer = ObjectPrinter
                .For<Person>()
                .ExcludePropertyOfType<Guid>()
                .ExcludePropertyOfType<double>()
                .ExcludePropertyOfType<int>()
                .ExcludePropertyOfType<Phone>();

            var expected =
                "Person\r\n" +
                "\tName = Alex\r\n" +
                "\tSurname = Smith\r\n";
            
            var ans = printer.PrintToString(person);
            ans.Should().Be(expected);
        }

        [Test]
        public void WithTypeSerializationStyle_ShouldSetUniqueSerializationStyle_ToPropertiesWithThisType()
        {
            var person = new Person {Name = "Alex", Surname = "Smith", Age = 19, Height = 180.5};
            person.Phone = new Phone {Name = "Смартфон Vivo"};
            var printer = ObjectPrinter
                .For<Person>()
                .WithTypeSerializationStyle<Guid>(new Serializer((obj, nestingLevel, deepnessLevel) => "999999999" + Environment.NewLine))
                .WithTypeSerializationStyle<Phone>(new Serializer((obj, nestingLevel, deepnessLevel) => "My phone" + Environment.NewLine));
            var ans = printer.PrintToString(person);
            
            var expected =
                "Person\r\n\t" +
                "Id = 999999999\r\n\t" +
                "Name = Alex\r\n\t" +
                "Surname = Smith\r\n\t" +
                "Height = 180,5\r\n\t" +
                "Age = 19\r\n\t" +
                "Phone = My phone\r\n";
            
            ans.Should().Be(expected);
        }

        [Test]
        public void SetTypeCulture_ShouldSetTypeCulture_ToPropertiesWithThisType()
        {
            var person = new Person {Name = "Alex", Surname = "Smith", Age = 19, Height = 180.5};
            var printer = ObjectPrinter
                .For<Person>()
                .SetTypeCulture<double>(CultureInfo.CurrentCulture);
            var ans = printer.PrintToString(person);
            
            var baseGuid = Guid.Empty.ToString();
            var expected =
                "Person\r\n\t" +
                $"Id = {baseGuid}\r\n\t" +
                "Name = Alex\r\n\t" +
                "Surname = Smith\r\n\t" +
                "Height = 180,5\r\n\t" +
                "Age = 19\r\n\t" +
                "Phone = null\r\n";
            
            ans.Should().Be(expected);
        }

        [Test]
        public void GetPropertyByName_ShouldReturn_PropertyInfoByName()
        {
            var person = new Person {Name = "Alex", Surname = "Smith", Age = 19, Height = 180.5};
            var printer = ObjectPrinter
                .For<Person>()
                .GetPropertyByName(x => x.Id);
            var ans = printer.PropertyName;
            
            var expected = "Id";
            
            ans.Should().Be(expected);
        }

        [Test]
        public void SetSerializationStyle_ShouldSetUniqueSerializationStyle_ToGotProperty()
        {
            var person = new Person {Name = "Alex", Surname = "Smith", Age = 19, Height = 180.5};
            var printer = ObjectPrinter
                .For<Person>()
                .GetPropertyByName(x => x.Name).SetSerializationStyle(new Serializer((obj, nestingLevel, deepnessLevel) => "My name" + Environment.NewLine));
            var ans = printer.PrintToString(person);
            
            var baseGuid = Guid.Empty.ToString();
            var expected =
                "Person\r\n\t" +
                $"Id = {baseGuid}\r\n\t" +
                "Name = My name\r\n\t" +
                "Surname = Smith\r\n\t" +
                "Height = 180,5\r\n\t" +
                "Age = 19\r\n\t" +
                "Phone = null\r\n";
            
            ans.Should().Be(expected);
        }

        [Test]
        public void TrimStringToLength_ShouldTrimPropertyLength_ToGotProperty()
        {
            var person = new Person {Name = "Alex", Surname = "Very long surname", Age = 19, Height = 180.5};
            var printer = ObjectPrinter
                .For<Person>()
                .GetPropertyByName(x => x.Surname).TrimStringToLength(5);
            var ans = printer.PrintToString(person);
            
            var baseGuid = Guid.Empty.ToString();
            var expected =
                "Person\r\n\t" +
                $"Id = {baseGuid}\r\n\t" +
                "Name = Alex\r\n\t" +
                "Surname = Very \r\n\t" +
                "Height = 180,5\r\n\t" +
                "Age = 19\r\n\t" +
                "Phone = null\r\n";
            
            ans.Should().Be(expected);
        }

        [Test]
        public void Exclude_ShouldRemove_GotProperty()
        {
            var person = new Person {Name = "Alex", Surname = "Smith", Age = 19, Height = 180.5};
            var printer = ObjectPrinter
                .For<Person>()
                .GetPropertyByName(x => x.Phone).Exclude();
            var ans = printer.PrintToString(person);
            
            var baseGuid = Guid.Empty.ToString();
            var expected =
                "Person\r\n\t" +
                $"Id = {baseGuid}\r\n\t" +
                "Name = Alex\r\n\t" +
                "Surname = Smith\r\n\t" +
                "Height = 180,5\r\n\t" +
                "Age = 19\r\n";
            
            ans.Should().Be(expected);
        }

        [Test]
        public void LoopReferencesTest()
        {
            var baseGuid = Guid.Empty.ToString();
            var expected = $"Person\r\n\tId = {baseGuid}\r\n\tName = Alex\r\n\tSurname = Smith\r\n\tHeight = 180,5" +
                           $"\r\n\tAge = 19\r\n\tPhone = Phone\r\n\t\tId = {baseGuid}\r\n\t\t" +
                           "Name = My phone\r\n\t\tOwner = Deepness exceeded!\r\n";
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