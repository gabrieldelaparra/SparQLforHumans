﻿using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Documents;
using SparqlForHumans.Lucene.Index.Base;
using SparqlForHumans.Models.LuceneIndex;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.RDF.Models;
using VDS.RDF;

namespace SparqlForHumans.Lucene.Index.Fields
{
    public class DescriptionIndexer : BaseFieldIndexer<TextField>
    {
        public override string FieldName => Labels.Description.ToString();

        public override bool FilterValidTriples(Triple triple)
        {
            return triple.Predicate.GetPredicateType().Equals(PredicateType.Description);
        }

        public override IEnumerable<TextField> GetField(SubjectGroup tripleGroup)
        {
            var values = SelectTripleValue(tripleGroup.FirstOrDefault(FilterValidTriples));
            return !string.IsNullOrWhiteSpace(values)
                ? new[] {new TextField(FieldName, values, Field.Store.YES)}
                : new TextField[] { };
        }

        public override string SelectTripleValue(Triple triple)
        {
            return triple?.Object.GetLiteralValue();
        }
    }
}