﻿using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using SparqlForHumans.Lucene.Queries.Base;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SparqlForHumans.Lucene.Queries
{
    public interface IQuery
    {
        string LuceneIndexPath { get; set; }

    }
    public class SingleIdQuery : BaseQuery
    {
        public SingleIdQuery(string luceneIndexPath, string searchString) : base(luceneIndexPath, searchString, 1) { }

        internal override IQueryParser QueryParser => new IdQueryParser();

        internal override bool IsInvalidSearchString(string inputString) => string.IsNullOrEmpty(inputString);
    }

    public class MultiIdQuery : BaseQuery
    {
        public MultiIdQuery(string luceneIndexPath, string searchString) : base(luceneIndexPath, searchString, 20) { }

        internal override IQueryParser QueryParser => new IdQueryParser();
        internal override bool IsInvalidSearchString(string inputString) => string.IsNullOrEmpty(inputString);
    }
    public class MultiLabelQuery : BaseQuery
    {
        public MultiLabelQuery(string luceneIndexPath, string searchString) : base(luceneIndexPath, searchString, 20) { }

        internal override IQueryParser QueryParser => new LabelsQueryParser();
        internal override bool IsInvalidSearchString(string inputString) => string.IsNullOrEmpty(Regex.Replace(inputString, @"[^a-zA-Z0-9-*]", string.Empty));
        internal override string PrepareSearchTerm(string inputString) => BaseParser.PrepareSearchTerm(inputString);
    }
    public class SingleLabelQuery : BaseQuery
    {
        public SingleLabelQuery(string luceneIndexPath, string searchString) : base(luceneIndexPath, searchString, 1) { }

        internal override IQueryParser QueryParser => new LabelsQueryParser();

        internal override bool IsInvalidSearchString(string inputString) => string.IsNullOrEmpty(Regex.Replace(inputString, @"[^a-zA-Z0-9-*]", string.Empty));
        internal override string PrepareSearchTerm(string inputString) => BaseParser.PrepareSearchTerm(inputString);
    }

    public class BatchIdQuery : BaseQuery
    {
        public BatchIdQuery(string luceneIndexPath, IEnumerable<string> searchStrings) : base(luceneIndexPath, searchStrings) { }

        internal override IQueryParser QueryParser => new IdQueryParser();

        internal override bool IsInvalidSearchString(string inputString) => string.IsNullOrEmpty(inputString);
    }

    public abstract class BaseQuery : IQuery
    {
        public BaseQuery(string luceneIndexPath, IEnumerable<string> searchStrings, int resultsLimit = 1)
        {
            LuceneIndexPath = luceneIndexPath;
            SearchStrings = searchStrings;
            ResultsLimit = resultsLimit;
            queryParser = QueryParser.GetQueryParser();
        }

        public BaseQuery(string luceneIndexPath, string searchString, int resultsLimit = 1)
        {
            LuceneIndexPath = luceneIndexPath;
            SearchStrings = new List<string> { searchString };
            ResultsLimit = resultsLimit;
            queryParser = QueryParser.GetQueryParser();
        }

        internal IEnumerable<string> SearchStrings { get; set; }
        public string LuceneIndexPath { get; set; }
        internal abstract IQueryParser QueryParser { get; }
        internal QueryParser queryParser { get; }
        internal int ResultsLimit { get; set; }
        internal virtual Filter Filter { get; set; } = null;

        internal virtual bool IsInvalidSearchString(string inputString) => false;

        internal virtual string PrepareSearchTerm(string inputString) => inputString;

        public virtual IReadOnlyList<Document> GetDocuments()
        {
            var list = new List<Document>();

            if(SearchStrings.All(IsInvalidSearchString)) 
                return list;

            using (var luceneDirectory = FSDirectory.Open(LuceneIndexPath))
            using (var luceneDirectoryReader = DirectoryReader.Open(luceneDirectory))
            {
                var searcher = new IndexSearcher(luceneDirectoryReader);

                foreach (var searchString in SearchStrings)
                {
                    if(IsInvalidSearchString(searchString)) continue;
                    var preparedSearchTerm = PrepareSearchTerm(searchString);

                    var query = BaseParser.ParseQuery(preparedSearchTerm, queryParser);
                    var hits = searcher.Search(query, Filter, ResultsLimit).ScoreDocs;
                    list.AddRange(hits.Select(hit => searcher.Doc(hit.Doc)));
                }
            }
            return list;
        }
    }
}
