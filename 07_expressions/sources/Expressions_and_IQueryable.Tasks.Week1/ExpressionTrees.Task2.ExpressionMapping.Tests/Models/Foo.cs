using System;

namespace ExpressionTrees.Task2.ExpressionMapping.Tests.Models
{
    public class Foo
    {
        public int Id { get; set; }

        public string Name { get; set; }

        // Same name as Bar.Age but a narrower type (int -> long widening conversion).
        public int Age { get; set; }

        // Mapped to a destination member with a different name and data type.
        public double Salary { get; set; }

        // Mapped to a destination member via a computed expression.
        public DateTime BirthDate { get; set; }
    }
}
