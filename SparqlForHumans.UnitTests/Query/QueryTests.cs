﻿using Lucene.Net.Store;
using SparqlForHumans.Lucene.Extensions;
using SparqlForHumans.Lucene.Index;
using SparqlForHumans.Lucene.Queries;
using SparqlForHumans.Utilities;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Directory = System.IO.Directory;

namespace SparqlForHumans.UnitTests.Query
{
    public class QueryTests
    {
        [Fact]
        public void TestFullQueryResults()
        {
            const string filename = "Resources/QueryWildcardOnePerLetter.nt";
            const string outputPath = "OneLetterWildcardQueriesFullWord";

            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();
            var actual = new MultiLabelQuery(outputPath, "Obama").GetDocuments().ToEntities().FirstOrDefault();
            Assert.Equal("Q76000000", actual.Id);

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestMultiQueryBarackObamaShouldShowFirst()
        {
            const string filename = "Resources/QueryMulti.nt";
            const string outputPath = "QueryMultiIndexBarack";

            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();
            var actual = new MultiLabelQuery(outputPath, "Obama").GetDocuments().ToEntities().FirstOrDefault();
            Assert.Equal("Q76", actual.Id);

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestMultiQueryMichelleObamaShouldShowFirst()
        {
            const string filename = "Resources/QueryMulti.nt";
            const string outputPath = "QueryMultiIndexMichelle";

            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();
            var entity = new MultiLabelQuery(outputPath, "Michelle Obama").GetDocuments().ToEntities().FirstOrDefault();

            Assert.Equal("Q13133", entity.Id);

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestNoEndWildcardQueryResults()
        {
            const string filename = "Resources/QueryWildcardOnePerLetter.nt";
            const string outputPath = "OneLetterWildcardHalfWord";

            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();
            var actual = new MultiLabelQuery(outputPath, "Oba").GetDocuments().ToEntities().FirstOrDefault();

            Assert.Equal("Q76000000", actual.Id);

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestQueryAddProperties()
        {
            const string outputPath = "Resources/PropertyIndex";
            Assert.True(Directory.Exists(outputPath));

            var entity = new SingleIdQuery(outputPath, "Q26").GetDocuments().ToEntities().FirstOrDefault();

            Assert.NotNull(entity);
            Assert.Equal("Q26", entity.Id);

            //Properties
            Assert.Equal(4, entity.Properties.Count());

            Assert.Equal("P17", entity.Properties.ElementAt(0).Id);
            Assert.Equal(string.Empty, entity.Properties.ElementAt(0).Label);

            Assert.Equal("P47", entity.Properties.ElementAt(1).Id);
            Assert.Equal(string.Empty, entity.Properties.ElementAt(1).Label);

            Assert.Equal("P30", entity.Properties.ElementAt(2).Id);
            Assert.Equal(string.Empty, entity.Properties.ElementAt(2).Label);

            Assert.Equal("P131", entity.Properties.ElementAt(3).Id);
            Assert.Equal(string.Empty, entity.Properties.ElementAt(3).Label);

            entity.AddProperties(outputPath);

            Assert.Equal("P17", entity.Properties.ElementAt(0).Id);
            Assert.Equal("country", entity.Properties.ElementAt(0).Label);

            Assert.Equal("P47", entity.Properties.ElementAt(1).Id);
            Assert.Equal("shares border with", entity.Properties.ElementAt(1).Label);

            Assert.Equal("P30", entity.Properties.ElementAt(2).Id);
            Assert.Equal("continent", entity.Properties.ElementAt(2).Label);

            Assert.Equal("P131", entity.Properties.ElementAt(3).Id);
            Assert.Equal("located in the administrative territorial entity", entity.Properties.ElementAt(3).Label);
        }

        [Fact]
        public void TestQueryByMultipleIds()
        {
            var ids = new List<string> { "Q26", "Q27", "Q29" };
            const string indexPath = "Resources/IndexMultiple";
            Assert.True(Directory.Exists(indexPath));

            var entities = new BatchIdQuery(indexPath, ids).GetDocuments().ToEntities();

            Assert.Equal(3, entities.Count());

            //Q26, Q27, Q29
            var doc = entities.ElementAt(0);
            Assert.NotNull(doc);
            Assert.Equal("Q26", doc.Id);
            Assert.Equal("Northern Ireland", doc.Label);

            doc = entities.ElementAt(1);
            Assert.NotNull(doc);
            Assert.Equal("Q27", doc.Id);
            Assert.Equal("Ireland", doc.Label);

            doc = entities.ElementAt(2);
            Assert.NotNull(doc);
            Assert.Equal("Q29", doc.Id);
            Assert.Equal("Spain", doc.Label);
        }

        [Fact]
        public static void TestQueryIsTypeFields()
        {
            Assert.False(true);
            const string filename = "Resources/TypeProperties.nt";
            const string outputPath = "CreateIndexIsTypeFields";

            outputPath.DeleteIfExists();
            Assert.False(Directory.Exists(outputPath));

            new EntitiesIndexer(filename, outputPath).Index();
            //using (var luceneIndexDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                var query = "chile";
                //var types = MultiDocumentQueries.QueryEntitiesByLabel(query, luceneIndexDirectory, true);
                var all = new MultiLabelQuery(outputPath, query).GetDocuments().ToEntities();
                //var all = MultiDocumentQueries.QueryEntitiesByLabel(query, luceneIndexDirectory, false);

                //Assert.Empty(types);
                Assert.Single(all);

                query = "country";
                //types = MultiDocumentQueries.QueryEntitiesByLabel(query, luceneIndexDirectory, true);
                all = new MultiLabelQuery(outputPath, query).GetDocuments().ToEntities();
                //all = MultiDocumentQueries.QueryEntitiesByLabel(query, luceneIndexDirectory, false);

                //Assert.Single(types);
                Assert.Single(all);
            }

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestQueryNonExistingEntityById()
        {
            const string indexPath = "Resources/IndexSingle";
            var actual = new SingleIdQuery(indexPath, "Q666").GetDocuments().ToEntities();

            Assert.Empty(actual);
        }

        [Fact]
        public void TestQueryNonExistingEntityByLabel()
        {
            const string indexPath = "Resources/IndexSingle";
            var actual = new SingleLabelQuery(indexPath, "Non-Existing").GetDocuments().ToEntities();

            Assert.Empty(actual);
        }

        [Fact]
        public void TestQueryNonExistingPropertyById()
        {
            const string indexPath = "Resources/IndexSingle";
            var actual = new SingleIdQuery(indexPath, "P666").GetDocuments().ToProperties();

            Assert.Empty(actual);
        }

        [Fact]
        public void TestQueryNonExistingPropertyByLabel()
        {
            const string indexPath = "Resources/IndexSingle";
            var actual = new SingleLabelQuery(indexPath, "Non-Existing").GetDocuments().ToProperties();

            Assert.Empty(actual);
        }

        [Fact]
        public void TestQuerySingleInstanceById()
        {
            const string indexPath = "Resources/IndexSingle";
            var actual = new SingleIdQuery(indexPath, "Q26").GetDocuments().ToEntities().FirstOrDefault();

            Assert.NotNull(actual);
            Assert.Equal("Q26", actual.Id);
        }

        [Fact]
        public void TestQuerySingleInstanceByLabel()
        {
            const string indexPath = "Resources/IndexSingle";

            var entity = new SingleLabelQuery(indexPath, "Northern Ireland").GetDocuments().ToEntities().FirstOrDefault();
            Assert.NotNull(entity);
            Assert.Equal("Q26", entity.Id);

            entity = new SingleLabelQuery(indexPath, "Ireland").GetDocuments().ToEntities().FirstOrDefault();
            Assert.NotNull(entity);
            Assert.Equal("Q26", entity.Id);

            entity = new SingleLabelQuery(indexPath, "Northern").GetDocuments().ToEntities().FirstOrDefault();
            Assert.NotNull(entity);
            Assert.Equal("Q26", entity.Id);

            entity = new SingleLabelQuery(indexPath, "north").GetDocuments().ToEntities().FirstOrDefault();
            Assert.NotNull(entity);
            Assert.Equal("Q26", entity.Id);
        }

        [Fact]
        public void TestRankMultiQuery_ShouldBeSortedByRank_AllWithAltLabels_Q1More()
        {
            const string filename = "Resources/QueryRanksAllWithAltLabelsQ1More.nt";
            const string outputPath = "QueryRanksAllWithAltLabelsQ1SortedByPageRank";

            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();

            var entities = new MultiLabelQuery(outputPath, "EntityQ").GetDocuments().ToEntities().ToArray();

            Assert.Equal("Q6", entities[0].Id); //0.222
            Assert.Equal("Q4", entities[1].Id); //0.180
            Assert.Equal("Q7", entities[2].Id); //0.180
            Assert.Equal("Q1", entities[3].Id); //0.138
            Assert.Equal("Q5", entities[4].Id); //0.128
            Assert.Equal("Q2", entities[5].Id); //0.087
            Assert.Equal("Q3", entities[6].Id); //0.061

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestRankMultiQuery_ShouldBeSortedByRank_AllWithSameAltLabels()
        {
            const string filename = "Resources/QueryRanksAllWithAltLabels.nt";
            const string outputPath = "QueryRanksAllSameAltLabelsSortedByPageRank";

            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();
            var entities = new MultiLabelQuery(outputPath, "EntityQ").GetDocuments().ToEntities().ToArray();

            Assert.Equal("Q6", entities[0].Id); //0.222
            Assert.Equal("Q4", entities[1].Id); //0.180
            Assert.Equal("Q7", entities[2].Id); //0.180
            Assert.Equal("Q1", entities[3].Id); //0.138
            Assert.Equal("Q5", entities[4].Id); //0.128
            Assert.Equal("Q2", entities[5].Id); //0.087
            Assert.Equal("Q3", entities[6].Id); //0.061

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestRankMultiQuery_ShouldBeSortedByRank_OneWithAltLabels()
        {
            const string filename = "Resources/QueryRanksOneWithAltLabels.nt";
            const string outputPath = "QueryRanksOneAltLabelsSortedByPageRank";

            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();
            var entities = new MultiLabelQuery(outputPath, "EntityQ").GetDocuments().ToEntities().ToArray();

            // Had to fix these tests to take PageRank and Boost altogether to pass.
            Assert.Equal("Q1", entities[0].Id); //0.138
            Assert.Equal("Q6", entities[1].Id); //0.222
            Assert.Equal("Q4", entities[2].Id); //0.180
            Assert.Equal("Q7", entities[3].Id); //0.180
            Assert.Equal("Q5", entities[4].Id); //0.128
            Assert.Equal("Q2", entities[5].Id); //0.087
            Assert.Equal("Q3", entities[6].Id); //0.061

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestRankMultiQuery_ShouldBeSortedByRank_OnlyAltLabels()
        {
            const string filename = "Resources/QueryRanksOnlyAltLabels.nt";
            const string outputPath = "QueryRanksSortedByPageRankOnlyAltLabels";

            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();
            var entities = new MultiLabelQuery(outputPath, "EntityQ").GetDocuments().ToEntities().ToArray();

            Assert.Equal("Q6", entities[0].Id); //0.222
            Assert.Equal("Q4", entities[1].Id); //0.180
            Assert.Equal("Q7", entities[2].Id); //0.180
            Assert.Equal("Q1", entities[3].Id); //0.138
            Assert.Equal("Q5", entities[4].Id); //0.128
            Assert.Equal("Q2", entities[5].Id); //0.087
            Assert.Equal("Q3", entities[6].Id); //0.061

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestRankMultiQuery_ShouldBeSortedByRank_OnlyLabels()
        {
            const string filename = "Resources/QueryRanksOnlyLabels.nt";
            const string outputPath = "QueryRanksSortedByPageRankOnlyLabels";

            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();
            var entities = new MultiLabelQuery(outputPath, "EntityQ").GetDocuments().ToEntities().ToArray();

            Assert.Equal("Q6", entities[0].Id); //0.222
            Assert.Equal("Q4", entities[1].Id); //0.180
            Assert.Equal("Q7", entities[2].Id); //0.180
            Assert.Equal("Q1", entities[3].Id); //0.138
            Assert.Equal("Q5", entities[4].Id); //0.128
            Assert.Equal("Q2", entities[5].Id); //0.087
            Assert.Equal("Q3", entities[6].Id); //0.061

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestSingleQuery_BarackObama_ShouldShowFirst()
        {
            const string filename = "Resources/QuerySingle.nt";
            const string outputPath = "QuerySingleIndexBarack";

            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();
            var entity = new SingleLabelQuery(outputPath, "Obama").GetDocuments().ToEntities().FirstOrDefault();

            Assert.Equal("Q76", entity.Id);

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestSingleQuery_MichelleObama_ShouldShowFirst()
        {
            const string filename = "Resources/QuerySingle.nt";
            const string outputPath = "QuerySingleIndexMichelle";

            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();
            var entity = new SingleLabelQuery(outputPath, "Michelle Obama").GetDocuments().ToEntities().FirstOrDefault();

            Assert.Equal("Q13133", entity.Id);

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestTopQueryEntitiesResults()
        {
            const string filename = "Resources/QueryEntityWildcardAllResults.nt";
            const string outputPath = "AllEntitiesResultsWildcardQueriesFullWord";

            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();
            var actual = new MultiLabelQuery(outputPath, "*").GetDocuments().ToEntities().ToArray();

            Assert.NotEmpty(actual);
            Assert.Equal("Q6", actual[0].Id); //0.222
            Assert.Equal("Q4", actual[1].Id); //0.180
            Assert.Equal("Q7", actual[2].Id); //0.180
            Assert.Equal("Q1", actual[3].Id); //0.138
            Assert.Equal("Q5", actual[4].Id); //0.128
            Assert.Equal("Q2", actual[5].Id); //0.087
            Assert.Equal("Q3", actual[6].Id); //0.061

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestTopQueryPropertiesResults()
        {
            const string filename = @"Resources/QueryPropertyWildcardAllResults.nt";
            const string outputPath = "AllPropertiesResultsWildcardQueriesFullWord";

            outputPath.DeleteIfExists();

            new PropertiesIndexer(filename, outputPath).Index();
            var actual = new MultiLabelQuery(outputPath, "*").GetDocuments().ToProperties().ToArray();

            Assert.NotEmpty(actual);
            Assert.Equal("P530", actual[0].Id); //50
            Assert.Equal("P47", actual[1].Id); //5
            Assert.Equal("P17", actual[2].Id); //3
            Assert.Equal("P30", actual[3].Id); //3

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestWithEndWildcardQueryResults()
        {
            const string filename = "Resources/QueryWildcardOnePerLetter.nt";
            const string outputPath = "OneLetterWildcardWithAsterisk";

            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();
            var actual = new MultiLabelQuery(outputPath, "Oba*").GetDocuments().ToEntities().FirstOrDefault();

            Assert.Equal("Q76000000", actual.Id);

            outputPath.DeleteIfExists();
        }
    }
}