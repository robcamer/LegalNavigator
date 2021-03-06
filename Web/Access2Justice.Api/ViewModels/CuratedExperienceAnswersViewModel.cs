﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Access2Justice.Api.ViewModels
{
    [ExcludeFromCodeCoverage]
    public class CuratedExperienceAnswersViewModel
    {
        [JsonProperty(PropertyName = "curatedExperienceId")]
        public Guid CuratedExperienceId { get; set; }

        [JsonProperty(PropertyName = "answersDocId")]
        public Guid AnswersDocId { get; set; }

        [JsonProperty(PropertyName = "buttonId")]
        public Guid ButtonId { get; set; }

        [JsonProperty(PropertyName = "fields")]
        public IList<Field> Fields { get; set; }
        public CuratedExperienceAnswersViewModel()
        {
            Fields = new List<Field>();
        }
    }

    [ExcludeFromCodeCoverage]
    public class Field
    {
        [JsonProperty(PropertyName = "fieldId")]
        public string FieldId { get; set; }

        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }
    }
}
