﻿using Access2Justice.Shared.Extensions;
using Access2Justice.Shared.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Access2Justice.Api.BusinessLogic
{
    public class A2JAuthorParserBusinessLogic : IA2JAuthorParserBusinessLogic
    {
        public Dictionary<string, string> Parse(string logic)
        {
            var IFstatements = logic.SplitAndReturnFullSentencesOn("END IF");
            var varsInScopeForPersonalizedPlan = new Dictionary<string, string>();

            foreach (var IFstatement in IFstatements)
            {
                // READING THE VARS AND CONDITIONS
                var leftCondition = IFstatement.GetStringBetween("IF", "SET");

                var ANDvariables = leftCondition.GetANDvariables();
                var ORvariables = leftCondition.GetORvariables();

                // SETTING THE BOOLIAN RESULT IN THE SET VAR
                var rightOf = IFstatement.GetStringBetween("SET", "END IF");
                var SETvariables = rightOf.SetValueTOVar();

                // COMPUTE RESULT
                var computedLogicVars = ComputeLogicText(ANDvariables, ORvariables, SETvariables);
                if (computedLogicVars != null)
                {
                    foreach (var computedLogicVar in computedLogicVars)
                    {
                        varsInScopeForPersonalizedPlan.Add(computedLogicVar.Key, computedLogicVar.Value);
                    }
                }
            }

            return varsInScopeForPersonalizedPlan;
        }

        public Dictionary<string, string> ComputeLogicText(Dictionary<string, bool> ANDvariables, Dictionary<string, bool> ORvariables,
            Dictionary<string, string> SETvariables)
        {
            if (!ANDvariables.Where(x => x.Value == false).Any() && !ORvariables.Where(x => x.Value == true).Any())
            {
                return SETvariables;
            }

            return null;
        }
    }
}
