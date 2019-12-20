# Regla
A simple rules engine written in C#

## Architecture
Regla has 3 core Module
1. Rule Module
2. Engine Module
3. Result Module

### Rule
Rule encapsulates a method to call and its attributes
* Rules can be given names to identify them
* Rules can be part of a group
* Rules can specify whether to stop on failure or exception
* Rules can be composed of AndRules or OrRules for short circuiting

RuleMethod can be provided in 3 forms
1. A delegate function
2. A lambda expression
3. A class implementing IRule
3. A class inherited from Rule class

Class Name | Description
-----------|----------------------------------------
RuleMethod | Encapsulation of a delegate or Func<>
RuleAttributes | Various options for the rule
Rule | Class which hold RuleMethod and RuleAttributes
AndRule | Rule short-circuiting until a rule fails
OrRule | Rule short-circuiting until a rule succeed

### Engine
Class Name | Description
-----------|----------------------------------------
EngineAttributes| Various options for the Rules Engine
RulesEngine | Holds EngineAttributes and Rules List

### Result
Class Name | Description
-----------|----------------------------------------
RunResultAttribute|Result of Rules execution
RuleResultAttribute|Result of individual rule execution
Result|Holds RunResult and RuleResult Attribute classes

## Basic flow
#### 1. Create an instance of RulesEngine
Pass EngineAttributes as constructor parameter or set properties after instantiating

#### 2. Add Rules to engine
Either create Rules before instantiating the engine and pass them as constructor parameter or use AddRule() functions to add them after creating the engine

#### 3. Run the engine
The Engine can run in different modes e.g. RunAllRules, RunNamedRules, RunGroupRules.

## Sample code

### Simplest RuleEngine


1. Create an empty RuleEngine
2. Add a Rule (we are using Lambda here)
3. Run the rules
4. Inspect Result graph
```C#
class RulesEngineDemo
{
    static void BasicExample()
    {
        static void BasicExample()
        {
            // Create an empty RulesEngine with no Component or output object
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


    }
    public static void Main()
    {
        BasicExample();
    }
}
/* The output
Hello, world
"Result": {
  "EngineAttributes": {
    "Name": "Engine_1",
    "StopOnException": true,
    "StopOnRuleFailure": false,
    "Component": null,
    "Output": null
  },
  "RunResultAttributes": {
    "TotalRulesCount": 1,
    "RulesExecutedCount": 1,
    "ExecutionType": "All",
    "FailedRuleName": null,
    "FailureReason": null
  },
  "RuleResultAttributes": [
    {
      "Rule": {
        "RuleMethod": "<BasicExample>b__3_0",
        "RuleAttributes": {
          "Name": "<BasicExample>b__3_0",
          "Group": "default",
          "Enabled": true,
          "StopOnException": true,
          "StopOnRuleFailure": false
        }
      },
      "ReturnValue": true,
      "Exception": null
    }
  ]
}*/
```
While the output looks very detailed, we don't have to worry about it yet. The Result object graph can be used to analyze and debug our RulesEngine

### Different ways of specifying rule
Rules can be delegates, Func<>, lambda or even classes

```C#
class RulesEngineDemo
{
    // Define a rule delegate method with empty class
    // as we don't want to operator on any object or output
    static bool ruleDelegate(None component, None output)
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
    public static void Main()
    {
        DelegateExample();
    }
}
```

As we have seen, we can easily pass a method or lambda to the Rules Engine, but to do complex processing, we may require a class with parameters and multiple functions

```C#
class RulesEngineDemo
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
        public decimal Discount { get; set; };
    }

     /**
     * Rules can be written as methods, lambdas or even classes
     * Class rules allow us to pass constructor params, which can be used later
     * e.g.
     *      engine.AddRule(new LoyaltyDiscount(1, 0.05)), "L1");
     *      engine.AddRule(new LoyaltyDiscount(5, 0.08), "L2");
     *      engine.AddRule(new LoyaltyDiscount(10, 0.10), "L3");
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
         * Due to generics, we have better type safety in parameters
         */
        public bool CalculateDiscount(Customer customer, CustomerDiscount discount)
        {
            if ((DateTime.Today - customer.FirstPurchase).Days / 365 > Years)
                discount.Discount = Math.Max(discount.Discount, 50);
            return true;
        }
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


    public static void Main()
    {
        RuleClassExample();
    }
}

```

### Engine options
EngineAttributes has many options to control the engine

Attribute|Description
---------|-----------
Name|User defined name for the engine
Component|Object on which the rules will execute
Output|Object will can be used for rules to store values
StopOnException|Stop rules processing if execution is throw (default: true)
StopOnRuleFailure|Stop rules processing if a the current rule returns false (default: false)

### Rules
Rule encapsulates the user provide method (or class object) and holds its attributes
A rule can have a user defined name and/or a group, which can be used to run named rules or run group rules. These names can also be used for removing rules dynamically

```C#
:
:
// Define a new rule using a delegate method
var seniorRule = new Rule<Customer, CustomerDiscount>(seniorCitizenDiscount, name:"senior", group:"discount", stopOnException:true, stopOnRuleFailure:true);

// Add another rule for birthday
var birthdayRule = new Rule<Customer, CustomerDiscount>(birthdayDiscount, group:"discount");

// Add a rule for seasonal discount (not part of group:discount)
var seasonalDiscount = new Rule<Customer, CustomerDiscount>(new SeasonDiscount(), group:"seasonal"));

// Instantiate the engine with component and output
var engine=new RulesEngine<Customer, CustomerDiscount>(component:customer, output:discount, name:"CustomerRule");

// Run only those rules which are part of "discount" group
var groupResult = engine.RunGroupRules("discount");

// Check the discount here
Console.WriteLine("Customer: {0} got {1} discount", customer.Name, discount);

// We can also run named rules
var namedResult = engine.RunNamedRules(new string[] {"senior", "seasonal"});

// We can remove individual rules or grouped rules
engine.RemoveRule("senior");            // By name
engine.RemoveRule(birthdayDiscount);    // By delegate
engine.RemoveRule(seasonalDiscount);    // By Rule reference
engine.RemoveGroup("discount");         // By Group name

// Or we can clear all rules
engine.clearRules();
```

### Remarks
Regla is  WIP (Work In Progress) and the architecture and implementation will change with feedback. You may use this code to understand the architecture and implementation but are strongly advised against using this library for any production work.







    