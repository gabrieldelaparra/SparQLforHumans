﻿using SparqlForHumans.Lucene.Queries.Base;
using SparqlForHumans.Lucene.Queries.Parsers;

namespace SparqlForHumans.Lucene.Queries
{
    public class MultiIdInstanceOfEntityQuery : BaseEntityQuery
    {
        public MultiIdInstanceOfEntityQuery(string luceneIndexPath, string searchString) : base(luceneIndexPath, searchString, 20) { }

        internal override IQueryParser QueryParser => new InstanceOfQueryParser();
        internal override bool IsInvalidSearchString(string inputString) => string.IsNullOrEmpty(inputString);
    }
}