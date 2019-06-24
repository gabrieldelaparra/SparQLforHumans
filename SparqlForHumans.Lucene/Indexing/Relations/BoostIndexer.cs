﻿using Lucene.Net.Documents;
using SparqlForHumans.Lucene.Indexing.Base;
using SparqlForHumans.Lucene.Indexing.Relations.Mappings;
using SparqlForHumans.Models.LuceneIndex;
using SparqlForHumans.RDF.Models;
using SparqlForHumans.Utilities;

namespace SparqlForHumans.Lucene.Indexing.Relations
{
    public class BoostIndexer : BoostMapper, IFieldIndexer<DoubleField>
    {
        public BoostIndexer(string inputFilename) : base(inputFilename)
        {
        }

        public double Boost { get; set; }

        public string FieldName => Labels.Rank.ToString();

        public DoubleField GetField(SubjectGroup subjectGroup)
        {
            return RelationIndex.ContainsKey(subjectGroup.Id.ToNumbers())
                ? new DoubleField(FieldName, RelationIndex[subjectGroup.Id.ToNumbers()], Field.Store.YES)
                : null;
        }
    }
}