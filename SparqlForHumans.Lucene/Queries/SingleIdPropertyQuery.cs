﻿using SparqlForHumans.Lucene.Queries.Base;

namespace SparqlForHumans.Lucene.Queries
{
    public class SingleIdPropertyQuery : BasePropertyQuery
    {
        public SingleIdPropertyQuery(string luceneIndexPath, string searchString) : base(luceneIndexPath, searchString, 1) { }

        internal override IQueryParser QueryParser => new IdQueryParser();

        internal override bool IsInvalidSearchString(string inputString) => string.IsNullOrEmpty(inputString);
    }
}
