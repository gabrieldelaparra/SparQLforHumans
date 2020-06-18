﻿using Lucene.Net.Documents;
using SparqlForHumans.Lucene.Index.Base;
using SparqlForHumans.Models.LuceneIndex;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.RDF.Models;
using SparqlForHumans.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace SparqlForHumans.Lucene.Index.Relations
{
    public class EntityIsTypeIndexer : BaseHashSetMapper<int>, IFieldIndexer<StringField>
    {
        public EntityIsTypeIndexer(IEnumerable<SubjectGroup> subjectGroup) : base(subjectGroup)
        {
        }

        public double Boost { get; set; }

        public string FieldName => Labels.IsTypeEntity.ToString();

        public override string NotifyMessage { get; } = "Building <InstanceOfIndex> HashSet";

        internal override void ParseTripleGroup(HashSet<int> hashSet, SubjectGroup subjectGroup)
        {
            var entityTypes = subjectGroup
                .Where(x => x.Predicate.IsInstanceOf())
                .Select(x => x.Object.GetIntId()).ToArray();

            foreach (var entityType in entityTypes)
            {
                hashSet.Add(entityType);
            }
        }

        public IEnumerable<StringField> GetField(SubjectGroup subjectGroup)
        {
            return RelationIndex.Contains(subjectGroup.Id.ToInt())
                ? new List<StringField> { new StringField(FieldName, true.ToString(), Field.Store.YES) }
                : new List<StringField>();
        }
    }
}