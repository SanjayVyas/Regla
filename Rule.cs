using System.Data;
/**
 *-----------------------------------------------------------------------------
 * File:      Rule.cs
 * Project:   Regla
 * Author:    Sanjay Vyas
 *
 * Rule represents the callback method
 *      RuleAttributes -> Options to control the rul
 *      Rule -> holds callback method and attributes
 *-----------------------------------------------------------------------------
 * Revision History
 *   [SV] 2019-Dec-20 11.39: Made JsonSerializer call generic
 *   [SV] 2019-Dec-20 11.39: Added generics
 *   [SV] 2019-Dec-19 11.12: Added IRule interface and Rule(IRule...) ctor
 *   [SV] 2019-Dec-19 4.57: Added ctor Rule(Rule rule, ...)
 *   [SV] 2019-Dec-19 4.52: Fixed setNameMethod for RuleClass objects
 *   [SV] 2019-Dec-19 2.06: Added OrRule class
 *   [SV] 2019-Dec-19 2.06: Added AndRule class
 *   [SV] 2019-Dec-19 1.33: Created
 *-----------------------------------------------------------------------------
 */

using System.Diagnostics;
using System.Reflection;
using System;

namespace Regla
{

    public interface IRule<COMPONENT, OUTPUT>
        where COMPONENT : class
        where OUTPUT : class
    {
        bool RuleMethod(COMPONENT component, OUTPUT output);
    }

    /**
     * Contains
     *      Name -> User provided (or generated) name of the Rule
     *      Group -> Multiple rules can have a common group (for group execution)
     *      Enabled -> Rules can be disabled between runs
     *      StopOnException -> Can override Engine Attribute
     *      StopOnRuleFailure -> Can overrid Engine Attribute
     */
    public class RuleAttributes : ReglaAttributes
    {
        public string Name { set; get; }
        public string Group { set; get; } = "default";
        public bool Enabled { set; get; } = true;
        public bool StopOnException { set; get; } = false;
        public bool StopOnRuleFailure { set; get; }

        public RuleAttributes(string name = null, string group = "default", bool enabled = true, bool stopOnException = true, bool stopOnRuleFailure = false)
        {
            this.Name = name;
            this.Group = group;
            this.Enabled = enabled;
            this.StopOnException = stopOnException;
            this.StopOnRuleFailure = stopOnRuleFailure;
        }

        public override string ToString()
        {
            return "\"RuleAttributes\":" + ReglaHelper.ToJson<None, None>(this);
        }
    }

    /**
     * The heart of RuleEngine
     * Contain
     *      RuleMethod -> A delegate to the method to be called
     *      RuleAttributes -> Various options for the rule
     */
    public class Rule<COMPONENT, OUTPUT>
        where COMPONENT : class
        where OUTPUT : class
    {
        public virtual Func<COMPONENT, OUTPUT, bool> RuleMethod { protected set; get; }
        public RuleAttributes RuleAttributes { set; get; }

        /**
         * To make it simple for the user, we can figure out the rule name
         * from the calling method or class
         */
        private string setMethodName(Func<COMPONENT, OUTPUT, bool> method, string ruleName)
        {
            /** 
             * A Rule method may not be set in constructor, so it can be null
             */
            if (method != null)
            {
                // If no rule name, set the method name itself as the rule name
                if (ruleName == null)
                    return method.GetMethodInfo().Name;
            }
            else
            {
                /**
                 * For classes implementing Rule, their ctor will call the Rule constructor
                 * In which case the method will not be null, but .ctor
                 * 
                 * In that case, find out the class name of the constructor and set it as ruleName
                 */
                if (ruleName == null)
                {
                    string name;
                    int m = 1;
                    name = new StackFrame(1).GetMethod().DeclaringType.Name;
                    while (name == "Rule")
                    {
                        name = new StackFrame(++m).GetMethod().DeclaringType.Name;
                    }
                    return name;
                }

            }
            return ruleName;
        }

        /**
         * Core constructor which takes method and attributes
         * Other constructors will call this
         */
        public Rule(Func<COMPONENT, OUTPUT, bool> method, RuleAttributes ruleAttributes)
        {
            RuleMethod = method;
            ruleAttributes.Name = setMethodName(RuleMethod, ruleAttributes.Name);
            RuleAttributes = ruleAttributes;
        }

        public Rule(IRule<COMPONENT, OUTPUT> rule, RuleAttributes ruleAttributes)
        {
            RuleMethod = rule.RuleMethod;
            RuleAttributes = (ruleAttributes == null ? new RuleAttributes() : ruleAttributes);
            ruleAttributes.Name = setMethodName(RuleMethod, ruleAttributes.Name);

        }
        /**
         * Spread parametrize constructor, making it easy for caller
         * so that they don't have to create an object of RuleAttributes just to create a rule
         */
        public Rule(Func<COMPONENT, OUTPUT, bool> ruleMethod = null, string ruleName = null, string ruleGroupName = "default", bool ruleEnabled = true, bool stopOnException = true, bool stopOnRuleFailure = false)
            : this(ruleMethod, new RuleAttributes(ruleName, ruleGroupName, ruleEnabled, stopOnException, stopOnRuleFailure))
        {
        }

        public Rule(Rule<COMPONENT, OUTPUT> rule, string ruleName = null, string ruleGroupName = "default", bool ruleEnabled = true, bool stopOnException = true, bool stopOnRuleFailure = false)
            : this(rule.RuleMethod, new RuleAttributes(ruleName, ruleGroupName, ruleEnabled, stopOnException, stopOnRuleFailure))
        {
        }

        public Rule(IRule<COMPONENT, OUTPUT> iRule, string ruleName = null, string ruleGroupName = "default", bool ruleEnabled = true, bool stopOnException = true, bool stopOnRuleFailure = false)
            : this(iRule.RuleMethod, new RuleAttributes(ruleName, ruleGroupName, ruleEnabled, stopOnException, stopOnRuleFailure))
        {
        }

        public override string ToString()
        {
            return "\"Rule\": " + ReglaHelper.ToJson<COMPONENT, OUTPUT>(this);
        }
    }

    /**
     * Class to implement adn short circuiting
     * 
     * AndRule will continue executing rules until one returns false
     * If non return false, the final outcome is true
     */
    internal class AndRule<COMPONENT, OUTPUT> : Rule<COMPONENT, OUTPUT>
        where COMPONENT : class
        where OUTPUT : class
    {
        Rule<COMPONENT, OUTPUT>[] andRules = null;

        private bool andMethod(COMPONENT component, OUTPUT output)
        {
            bool result = false;
            foreach (var rule in andRules)
            {
                result = rule.RuleMethod(component, output);
                if (result == false)
                    return false;
            }
            return true;
        }

        public AndRule(string ruleName = null, params Rule<COMPONENT, OUTPUT>[] rulesArray)
            : base((Rule<COMPONENT, OUTPUT>)null, ruleName)
        {
            RuleMethod = andMethod;
            andRules = rulesArray;
        }
    }

    /**
     * Class to implement short circuiting rule
     * 
     * OrRule will continue executing rules until one returns true
     * If none return true, the final outcome is false
     */
    internal class OrRule<COMPONENT, OUTPUT> : Rule<COMPONENT, OUTPUT>
        where COMPONENT : class
        where OUTPUT : class
    {
        public override Func<COMPONENT, OUTPUT, bool> RuleMethod { protected set; get; }
        public Rule<COMPONENT, OUTPUT>[] orRules { private set; get; } = null;

        private bool orMethod(COMPONENT component, OUTPUT output)
        {
            bool result = false;
            foreach (var rule in orRules)
            {
                result = rule.RuleMethod(component, output);
                if (result == true)
                    return true;
            }

            return false;
        }

        public OrRule(string ruleName = null, params Rule<COMPONENT, OUTPUT>[] rulesArray)
            : base((Rule<COMPONENT, OUTPUT>)null, ruleName)
        {
            RuleMethod = orMethod;
            orRules = rulesArray;
        }
    }
}