﻿using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Util;
using SparqlForHumans.Models.LuceneIndex;

namespace SparqlForHumans.Lucene.Queries
{
    public class BaseParser
    {
        public static Query ParseQuery(string searchQuery, QueryParser parser)
        {
            Query query;
            try
            {
                query = parser.Parse(searchQuery.Trim());
            }
            catch (ParseException)
            {
                query = parser.Parse(QueryParserBase.Escape(searchQuery.Trim()));
            }

            return query;
        }

        public static string PrepareSearchTerm(string input)
        {
            //return input;
            //return $"*{input.Trim()}*";

            var terms = input.Trim()
                .Replace("-", " ")
                .Split(' ')
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(x => $"*{x.Trim()}*");

            var result = string.Join(" ", terms);
            return result;
        }

        // Pass SingleFieldQuery(Id), for searching by Id. Returns results sorted by rank.
        internal static Document QueryDocumentByIdAndRank(string searchId, IndexSearcher searcher)
        {
            var parser = GetIdParser();

            return QueryDocumentByRank(searchId, searcher, parser);
        }

        internal static QueryParser GetIdParser()
        {
            return new QueryParser(LuceneVersion.LUCENE_48, Labels.Id.ToString(), new KeywordAnalyzer());
        }

        internal static QueryParser GetMultiFieldParser()
        {
            var boostsDictionary = new Dictionary<string, float>();
            QueryParser parser = new MultiFieldQueryParser(
                LuceneVersion.LUCENE_48,
                new[]
                {
                    Labels.Label.ToString(),
                    Labels.AltLabel.ToString()
                },
                new StandardAnalyzer(LuceneVersion.LUCENE_48),
                boostsDictionary
            );

            parser.MultiTermRewriteMethod = new MultiTermQuery.TopTermsScoringBooleanQueryRewrite(int.MaxValue);
            parser.AllowLeadingWildcard = true;

            return parser;
        }

        // Pass MultiFieldQuery(Label, AltLabel), for searching Labels. Returns results sorted by rank.
        internal static Document QueryDocumentByRank(string searchText, IndexSearcher searcher, QueryParser parser,
            Filter filter = null)
        {
            //Adds Sorting
            //var sort = new Sort(SortField.FIELD_SCORE,
            //    new SortField(Labels.Rank.ToString(), SortFieldType.DOUBLE, true));

            var query = ParseQuery(searchText, parser);
            //var hit = searcher.Search(query, filter, 1, sort).ScoreDocs;
            var hit = searcher.Search(query, filter, 1).ScoreDocs;

            if (hit == null || hit.Length.Equals(0))
                return null;

            return searcher.Doc(hit.FirstOrDefault().Doc);
        }
    }
}