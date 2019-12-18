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
 *   [SV] 2019-Dec-19 1.33: Created
 *-----------------------------------------------------------------------------
 */

using System.Diagnostics;
using System.Reflection;
using System;

namespace Regla
{
    // "#define" (or typedef) for funny Func<....>
    using RuleMethod = Func<object, object, bool>;

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
            return "\"RuleAttributes\":" + ReglaHelper.ToJson(this);
        }
    }

    /**
     * The heart of RuleEngine
     * Contain
     *      RuleMethod -> A delegate to the method to be called
     *      RuleAttributes -> Various options for the rule
     */
    public class Rule
    {
        public RuleMethod RuleMethod { protected set; get; }
        public RuleAttributes RuleAttributes { set; get; }

        /**
         * To make it simple for the user, we can figure out the rule name
         * from the calling method or class
         */
        private string setMethodName(RuleMethod method, string ruleName)
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
                    return new StackFrame(1).GetMethod().DeclaringType.Name;

            }
            return ruleName;
        }

        /**
         * Unable to get implicit converter working
         * Regular types implicit conversion works
         * Somehow, it doesnt seem to convert a method into RuleClass... HELP!
         */
        public static implicit operator Rule(RuleMethod method)
        {
            return new Rule(method);
        }

        /**
         * Core constructor which takes method and attributes
         * Other constructors will call this
         */
        public Rule(RuleMethod method, RuleAttributes ruleAttributes)
        {
            RuleMethod = method;
            ruleAttributes.Name = setMethodName(method, ruleAttributes.Name);
            RuleAttributes = ruleAttributes;
        }

        /**
         * Spread parametrize constructor, making it easy for caller
         * so that they don't have to create an object of RuleAttributes just to create a rule
         */
        public Rule(RuleMethod method = null, string ruleName = null, string ruleGroupName = "default", bool ruleEnabled = true, bool stopOnException = true, bool stopOnRuleFailure = false)
            : this(method, new RuleAttributes(ruleName, ruleGroupName, ruleEnabled, stopOnException, stopOnRuleFailure))
        {
        }

        public override string ToString()
        {
            return "\"Rule\": " + ReglaHelper.ToJson(this); 
        }
    }
}