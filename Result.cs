/**
 *-----------------------------------------------------------------------------
 * File:      Result.cs
 * Project:   Regla
 * Author:    Sanjay Vyas
 *
 * This module contains classes related to Rules execution Results
 * There are 3 major classes
 * Result -> contains the overall result of the execution
 *     RunResultAttributes -> Result of the run of set of rules
 *     RuleResultAttributes -> An array containing results of individual rules
 *-----------------------------------------------------------------------------
 * Revision History
 *   [SV] 2019-Dec-20 11.38: Made JsonSerializer call generics
 *   [SV] 2019-Dec-20 11.38: Added generics
 *   [SV] 2019-Dec-19 1.22: Created
 *-----------------------------------------------------------------------------
 */

using System;
namespace Regla
{
    /**
     * Contains
     *      Rule -> Which was executed
     *      ReturnValue -> If there was no exception, then what was the result
     *      Exception -> If there was an exception, the exact exception object
     */
    public class RuleResultAttributes<COMPONENT, OUTPUT> : ReglaAttributes
        where COMPONENT : class
        where OUTPUT : class
    {
        public Rule<COMPONENT, OUTPUT> Rule { private set; get; }
        public bool ReturnValue { private set; get; }
        public Exception Exception { private set; get; }

        public RuleResultAttributes(Rule<COMPONENT, OUTPUT> rule, bool returnValue, Exception exception)
        {
            this.Rule = rule;
            this.ReturnValue = returnValue;
            this.Exception = exception;
        }

        public override string ToString()
        {
            return "\"RuleResultAttributes\": " + ReglaHelper.ToJson<COMPONENT, OUTPUT>(this);
        }
    }

    /**
     * Contain
     *      TotalRulesCount -> The number of rules in the original list 
     *      RulesExecutedCount -> Number of rules executed, up to point of failure (exception or false return)
     *      ExecutionType -> All, NamedRules, GroupRules
     *      FailedRuleName -> If a rule failed (Exception or false return), name of the rul
     *      FailureReason -> Whether it failed due to exception or false return
     */
    public class RunResultAttributes : ReglaAttributes
    {
        public int TotalRulesCount { private set; get; }
        public int RulesExecutedCount { private set; get; }
        public string ExecutionType { private set; get; }
        public string FailedRuleName { private set; get; }
        public string FailureReason { private set; get; }

        public RunResultAttributes(int totalRulesCount, int rulesExecutedCount, string executionType, string failedRuleName, string failureReason)
        {
            this.TotalRulesCount = totalRulesCount;
            this.RulesExecutedCount = rulesExecutedCount;
            this.ExecutionType = executionType;
            this.FailedRuleName = failedRuleName;
            this.FailureReason = failureReason;
        }

        public override string ToString()
        {
            return "\"RunResultAttributes\": " + ReglaHelper.ToJson<None, None>(this);
        }
    }

    /**
     * Contains
     *      EngineAttributes -> Various option set when creating the rule engine
     *      RunResultAttribute -> Overall statistics about the execution
     *      RulesResultAttributes -> Array of results of individual rules
     */
    public class Result<COMPONENT, OUTPUT>
        where COMPONENT : class
        where OUTPUT : class
    {
        public EngineAttributes<COMPONENT, OUTPUT> EngineAttributes { private set; get; }
        public RunResultAttributes RunResultAttributes { private set; get; }
        public RuleResultAttributes<COMPONENT, OUTPUT>[] RuleResultAttributes { private set; get; }

        public Result(EngineAttributes<COMPONENT, OUTPUT> engineAttributes, RunResultAttributes runResultAttributes, RuleResultAttributes<COMPONENT, OUTPUT>[] ruleResultAttributes)
        {
            this.EngineAttributes = engineAttributes;
            this.RunResultAttributes = runResultAttributes;
            this.RuleResultAttributes = ruleResultAttributes;
        }

        public override string ToString()
        {
            return "\"Result\": " + ReglaHelper.ToJson<COMPONENT, OUTPUT>(this);
        }
    }

    public class RuleResultAttributes : RuleResultAttributes<object, object>
    {
        public RuleResultAttributes(Rule<object, object> rule, bool ReturnValue, Exception exception)
            : base(rule, ReturnValue, exception)
        { }
    }


    public class Result : Result<object, object>
    {
        public Result(EngineAttributes<object, object> engineAttributes, RunResultAttributes runResultAttributes, RuleResultAttributes<object, object>[] ruleResultAttributes)
            : base(engineAttributes, runResultAttributes, ruleResultAttributes)
        {

        }
    }
}
