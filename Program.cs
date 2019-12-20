/**
 *-----------------------------------------------------------------------------
 * File:      Program.cs
 * Project:   Regla
 * Author:    Sanjay Vyas
 *
 * This is a test program for Regla RulesEngine
 *-----------------------------------------------------------------------------
 * Revision History
 *   [SV] 2019-Dec-20 11.37: Added generics 
 *   [SV] 2019-Dec-19 5.10: Reorganized the program structure to modular 
 *   [SV] 2019-Dec-19 1.13: Created
 *-----------------------------------------------------------------------------
 */

using System;
using Regla;
#nullable enable
namespace ReglaUse
{
    /**
     * Sample Component for RulesEngine to work on
     */
    class Customer
    {
        public int ID { get; set; } = 0;
        public string? Name { get; set; } = null;
        public DateTime Birthdate { get; set; }
        public DateTime FirstPurchase { get; set; }
    }

    /**
     * Sample "Output" object, which rules can use for writing data
     */
    class CustomerDiscount
    {
        public decimal Discount { get; set; } = 0;
    }

    /**
     * Rules can be written as methods, lambdas or even classes
     * Class rules allow us to pass constructor params, which can be used later
     * e.g.
     *      engine.AddRule(new LoyaltyDiscount(1, 0.05));
     *      engine.AddRule(new LoyaltyDiscount(5, 0.08);
     *      engine.AddRule(new LoyaltyDiscount(10, 0.10));
     */
    class LoyaltyDiscount : Rule<Customer, CustomerDiscount>
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
        public bool CalculateDiscount(Customer customer, CustomerDiscount discount)
        {
            if ((DateTime.Today - customer.FirstPurchase).Days / 365 > Years)
                discount.Discount = Math.Max(discount.Discount, 50);
            return true;
        }
    }

    class RulesEngineDemo
    {
        /**
         * One of the rules created as simple method
         * We can also use Lambda directly
         */
        static bool seniorCitizenDiscount(Customer customer, CustomerDiscount discount)
        {
            var span = (DateTime.Today - customer.Birthdate);
            int age = span.Days / 365;
            if (age > 60)
                discount.Discount = Math.Max(discount.Discount, 20);
            return true;
        }

        static bool trueRule(object component, object output)
        {
            Console.WriteLine("trueRule");
            return true;
        }

        static bool falseRule(object component, object output)
        {
            Console.WriteLine("falseRule");
            return false;
        }

        static void BasicExample()
        {
            // Create an empty RulesEngine
            var myRules = new RulesEngine<None, None>();

            // Add a lambda rule to the engine
            myRules.AddRule(new Rule<None, None>((component, output) => { Console.WriteLine("Hello, world"); return true; }));

            // Run the engine, it will print Hello, world
            var result = myRules.RunAllRules();
            // Hello, world

            // Result class contains detailed object graph
            // Which can be accessed thru properties
            // It's ToString() returns a Json string
            Console.WriteLine(result);

        }

        // Define a rule delegate method
        static bool ruleDelegate(object component, object output)
        {
            Console.WriteLine("ruleDelegate called");
            return false;
        }

        static void DelegateExample()
        {
            // Create EngineAttributes with Name
            var engineOptions = new EngineAttributes<None, None> { Name = "MyEngine" };

            // Instantiate the RulesEngine with option
            var myEngine = new RulesEngine<None, None>(engineOptions);

            // Now add Lambda rule
            myEngine.AddRule(new Rule<None, None>((c, o) => { Console.WriteLine("Hello, world"); return true; }));

            // Next, add a delegate method
            myEngine.AddRule(new Rule<None, None>(ruleDelegate));

            myEngine.RunAllRules();
            // Output:
            // Hello, world
            // ruleDelegate called
        }

        static void RuleClassExample()
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
            var engineAttributes = new EngineAttributes<Customer, CustomerDiscount> { Component = bigB, Output = discount };

            // We can add individual rules or an array of rules
            var rulesArray = new Rule<Customer, CustomerDiscount>[] { new Rule<Customer, CustomerDiscount>(seniorCitizenDiscount, ruleName: "Senior") };

            // Start the engine with attributes and initial rules list
            var engine = new RulesEngine<Customer, CustomerDiscount>(engineAttributes, rulesArray);

            // We are adding multiple rules based on the same class, so we must name them explicitly
            engine.AddRule(new Rule<Customer, CustomerDiscount>(new LoyaltyDiscount(1, 5), ruleName: "L1"));
            engine.AddRule(new Rule<Customer, CustomerDiscount>(new LoyaltyDiscount(5, 8), ruleName: "L2"));
            engine.AddRule(new Rule<Customer, CustomerDiscount>(new LoyaltyDiscount(10, 10), ruleName: "L3"));

            Console.WriteLine(engine.RunAllRules());
        }

        class SeasonDiscount : IRule<Customer, CustomerDiscount>
        {
            public DateTime SeasonStart { set; get; }
            public DateTime SeasonEnd { set; get; }
            public decimal Discount { set; get; }

            public SeasonDiscount(DateTime fromDate, DateTime toDate, decimal discount)
            {
                SeasonStart = fromDate;
                SeasonEnd = toDate;
                Discount = discount;
            }

            public bool RuleMethod(Customer component, CustomerDiscount output)
            {
                CustomerDiscount discountObj = (CustomerDiscount)output;
                if (DateTime.Today >= SeasonStart && DateTime.Today <= SeasonEnd)
                    discountObj.Discount += this.Discount;
                return true;
            }
        }

        static void IRuleExample()
        {
            // Create an input object (component)
            var customer = new Customer { ID = 1, Name = "Amitabh" };

            // Create an output object (discount)
            var discount = new CustomerDiscount();

            // Create a new rule for xmas seasson
            SeasonDiscount seasonal = new SeasonDiscount(new DateTime(2019, 12, 15), new DateTime(2019, 12, 25), 5);

            // Populate the engine
            var engine = new RulesEngine<Customer, CustomerDiscount>(component: customer, output: discount);
            engine.AddRule(new Rule<Customer, CustomerDiscount>(seasonal));

            // Run the rule
            var result = engine.RunAllRules();

            // Check if we got the discount
            Console.WriteLine($"Seasonal discount {discount.Discount}");

            // Let's inspect the result
            Console.WriteLine(result);

        }

        public static void Main()
        {
            BasicExample();
            DelegateExample();
            RuleClassExample();
            IRuleExample();
        }
    }
}
