namespace ExpressionTrees.Task2.ExpressionMapping.Tests.Models
{
    public class Bar
    {
        // Mapped by convention: same name and type as Foo.Id.
        public int Id { get; set; }

        // Mapped by convention: same name and type as Foo.Name.
        public string Name { get; set; }

        // Mapped by convention with an automatic widening conversion from Foo.Age (int -> long).
        public long Age { get; set; }

        // Custom mapping: different name and data type (from Foo.Salary, double -> string).
        public string SalaryText { get; set; }

        // Custom mapping: different name and data type (computed from Foo.BirthDate).
        public int BirthYear { get; set; }

        // No matching source member -> stays at its default value.
        public string Unmapped { get; set; }
    }
}
