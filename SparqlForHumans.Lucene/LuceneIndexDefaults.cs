﻿using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Util;
using VDS.RDF;

namespace SparqlForHumans.Lucene
{
    public static class LuceneIndexDefaults
    {
        public static LuceneVersion IndexVersion { get; } = LuceneVersion.LUCENE_48;

        public static IndexWriterConfig CreateStandardIndexWriterConfig()
        {
            return CreateIndexWriterConfig(new StandardAnalyzer(IndexVersion));
        }

        public static IndexWriterConfig CreateKeywordIndexWriterConfig()
        {
            return CreateIndexWriterConfig(new KeywordAnalyzer());
        }

        private static IndexWriterConfig CreateIndexWriterConfig(Analyzer analyzer)
        {
            Options.InternUris = false;
            var indexConfig = new IndexWriterConfig(IndexVersion, analyzer)
            {
                OpenMode = OpenMode.CREATE_OR_APPEND
            };
            return indexConfig;
        }
    }
}