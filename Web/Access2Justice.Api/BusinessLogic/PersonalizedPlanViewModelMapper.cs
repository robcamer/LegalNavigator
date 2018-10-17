﻿using Access2Justice.Api.Interfaces;
using Access2Justice.Api.ViewModels;
using Access2Justice.Shared.Models;
using System;
using System.Collections.Generic;

namespace Access2Justice.Api.BusinessLogic
{
    public class PersonalizedPlanViewModelMapper : IPersonalizedPlanViewModelMapper
    {
        public PersonalizedPlanViewModel MapViewModel(UnprocessedPersonalizedPlan personalizedPlanStepsInScope)
        {
            // https://github.com/Microsoft/Access2Justice/issues/567

            var actionPlan = new PersonalizedPlanViewModel();

            foreach (var topic in personalizedPlanStepsInScope.UnprocessedTopics)
            {
                actionPlan.Topics.Add(new PlanTopic
                {
                    TopicName = topic.Name
                });
            }

            
            actionPlan.PersonalizedPlanId = Guid.NewGuid();
            actionPlan.IsShared = false;

            var steps = new List<PlanStep>();
            var stepOrder = 1;
            //foreach (var step in personalizedPlanStepsInScope)
            //{
            //    foreach (var childrenRoot in step.GetValueAsArray<JArray>("children"))
            //    {
            //        foreach (var child in childrenRoot.GetValueAsArray<JObject>("rootNode").GetValueAsArray<JArray>("children"))
            //        {
            //            var state = child.GetArrayValue("state").FirstOrDefault();
            //            var title = state.GetValue("title");
            //            var userContent = state.GetValue("userContent");


            //            steps.Add(new PlanStep
            //            {
            //                StepId = Guid.NewGuid(),
            //                Title = title,
            //                Description = userContent,
            //                Order = stepOrder++,
            //                IsComplete = false,
            //            });

            //            var breakpoint = string.Empty; // Todo:@Alaa - remove this temp code
            //        }                   
            //    }
            //}

            //actionPlan.Topics.Add(new PlanTopic
            //{
            //    TopicId = Guid.NewGuid(),
            //    TopicName = "New Personalized Plan Test",
            //    Steps = steps
            //});

            return actionPlan;
        }
    }
}