namespace Test.Shared
{
    /// <summary>
    /// Projection result type carrying denormalized department information.
    /// </summary>
    public class DepartmentInfo
    {
        /// <summary>Gets or sets the department name.</summary>
        public string Department { get; set; } = string.Empty;

        /// <summary>Gets or sets the salary.</summary>
        public decimal Salary { get; set; }

        /// <summary>Gets or sets the employee display name.</summary>
        public string EmployeeName { get; set; } = string.Empty;
    }
}
