namespace Nation.Core.Models
{
    /// <summary>Tax rates in percent. Changed only through commands from the economy phase onward.</summary>
    public struct TaxPolicy
    {
        public float IncomeTax { get; set; }
        public float BusinessTax { get; set; }
        public float ImportTax { get; set; }

        public TaxPolicy(float incomeTax, float businessTax, float importTax)
        {
            IncomeTax = incomeTax;
            BusinessTax = businessTax;
            ImportTax = importTax;
        }
    }
}
