using System;
using System.Text.Json;
using Regla;

namespace ReglaUse
{
    /**
     * Sample Component for RulesEngine to work on
     */
    class Customer
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public DateTime Birthdate { get; set; }
        public DateTime FirstPurchase { get; set; }
    }

    /**
     * Sample "Output" object, which rules can use for writing data
     */
    class CustomerDiscount
    {
        public decimal Discount { get; set; } = 5;
    }

    /**
     * Rules can be written as methods, lambdas or even classes
     * Class rules allow us to pass constructor params, which can be used later
     * e.g.
     *      engine.AddRule(new LoyaltyDiscount(1, 0.05));
     *      engine.AddRule(new LoyaltyDiscount(5, 0.08);
     *      engine.AddRule(new LoyaltyDiscount(10, 0.10));
     */
    public class LoyaltyDiscount : Rule
    {
        public int Years { get; private set; }
        public decimal Discount { get; private set; }

        // We can set RuleAttributes by calling base Rule constructor
        public LoyaltyDiscount(int years, decimal discount)
        {
            // We can also set the RulesAttributes inside the constructor
            base.RuleMethod = CalculateDiscount;
            Years = years;
            Discount = discount;
        }

        /**
         * The actual rule can be called anything, but must have matching signature
         */
        public bool CalculateDiscount(object input, object output)
        {
            var customer = (Customer)input;
            var customerDiscount = (CustomerDiscount)output;
            if ((DateTime.Today - customer.FirstPurchase).Days / 365 < Years)
                customerDiscount.Discount = Math.Max(customerDiscount.Discount, 50);
            else
                throw new Exception("Not too old");
            return true;
        }
    }
    class Program
    {
        /**
         * One of the rules created as simple method
         * We can also use Lambda directly
         */
        static bool seniorCitizenDiscount(object input, object output)
        {
            var customer = (Customer)input;
            var discount = (CustomerDiscount)output;
            var span = (DateTime.Today - customer.Birthdate);
            int age = span.Days / 365;
            if (age > 60)
                discount.Discount = Math.Max(discount.Discount, 20);
            return true;
        }

        public static void Main()
        {
            /**
             * Create the component to initialize the RulesEngine
             * We don't HAVE to have a component, the rules can still run
             */
            var bigB = new Customer
            {
                ID = 1,
                Name = "Amitabh",
                Birthdate = new DateTime(1942, 10, 11),
                FirstPurchase = new DateTime(2005, 1, 2),
            };

            /**
             * Create an output object for Rules Engine
             * We don't HAVE to have an output object
             */
            var discount = new CustomerDiscount();

            // Create Engine Attributes
            var engineAttributes = new EngineAttributes { Name = "MyEngine", Component = bigB, Output = discount };

            // We can add individual rules or an array of rules
            var rulesArray = new Rule[] { new Rule(seniorCitizenDiscount, ruleName: "Senior") };

            // Start the engine with attributes and initial rules list
            var engine = new RulesEngine(engineAttributes, rulesArray);

            // Add more rules dynamically
            engine.AddRule(new LoyaltyDiscount(1, 5));

            // Rules can be class objects, methods or even lambdas
            engine.AddRule(new Rule((input, output) => { return false; }, ruleName: "Lambda", ruleGroupName: "L1", stopOnRuleFailure: true));

            // We can override the stop condition on a per rule bases
            engine.AddRule(new Rule((input, output) => { throw new Exception("lambda failed"); }, ruleGroupName: "L1", stopOnRuleFailure: false));


            // We can run all rules or named/group rules
            var result = engine.RunAllRules();

            // Result object has an object graph, but we can also print it as json
            Console.WriteLine("Result of RunAllRules()");
            Console.WriteLine(result);

            // Run Grouped Rules
            result = engine.RunGroupRules("L1");
            Console.WriteLine("Result of RunGroupRules()");
            Console.WriteLine(result);

            // Run specific named rules
            result = engine.RunNamedRules(new string[] { "Senior", "Lambda" });
            Console.WriteLine("Result of RunGroupRules()");
            Console.WriteLine(result);




        }
    }
}
