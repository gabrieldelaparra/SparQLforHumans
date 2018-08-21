﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Lucene.Net.Index;
using SparqlForHumans.Core.Properties;
using SparqlForHumans.Core.Services;
using SparqlForHumans.Core.Utilities;
using Xunit;

namespace SparqlForHumans.UnitTests
{
    public class QueryServiceTests
    {
        [Fact]
        public void TestQuerySingleInstanceByLabel()
        {
            const string outputPath = "Resources/IndexSingle";
            var luceneIndexDirectory = outputPath.GetLuceneDirectory();

            var entity = QueryService.QueryEntityByLabel("Northern Ireland", luceneIndexDirectory);
            Assert.NotNull(entity);
            Assert.Equal("Q26", entity.Id);

            entity = QueryService.QueryEntityByLabel("Ireland", luceneIndexDirectory);
            Assert.NotNull(entity);
            Assert.Equal("Q26", entity.Id);

            entity = QueryService.QueryEntityByLabel("Northern", luceneIndexDirectory);
            Assert.NotNull(entity);
            Assert.Equal("Q26", entity.Id);

            entity = QueryService.QueryEntityByLabel("north", luceneIndexDirectory);
            Assert.NotNull(entity);
            Assert.Equal("Q26", entity.Id);
        }

        [Fact]
        public void TestQuerySingleInstanceById()
        {
            const string outputPath = "Resources/IndexSingle";
            var luceneIndexDirectory = outputPath.GetLuceneDirectory();

            var entity = QueryService.QueryEntityById("Q26", luceneIndexDirectory);
            Assert.NotNull(entity);
            Assert.Equal("Q26", entity.Id);
        }

        [Fact]
        public void TestQueryByMultipleIds()
        {
            var ids = new List<string>() { "Q26", "Q27", "Q29" };
            const string outputPath = "Resources/IndexMultiple";
            Assert.True(Directory.Exists(outputPath));

            var luceneIndexDirectory = outputPath.GetLuceneDirectory();
            var entities = QueryService.QueryEntityByIds(ids, luceneIndexDirectory);

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
        public void TestQueryAddProperties()
        {
            const string outputPath = "Resources/PropertyIndex";

            Assert.True(Directory.Exists(outputPath));
            var luceneIndexDirectory = outputPath.GetLuceneDirectory();

            var entity = QueryService.QueryEntityById("Q26", luceneIndexDirectory);

            Assert.NotNull(entity);
            Assert.Equal("Q26", entity.Id);

            //Properties
            Assert.Equal(4, entity.Properties.Count());

            Assert.Equal("P17", entity.Properties.ElementAt(0).Id);
            Assert.Equal("Q145", entity.Properties.ElementAt(0).Value);
            Assert.Equal(string.Empty, entity.Properties.ElementAt(0).Label);

            Assert.Equal("P47", entity.Properties.ElementAt(1).Id);
            Assert.Equal("Q27", entity.Properties.ElementAt(1).Value);
            Assert.Equal(string.Empty, entity.Properties.ElementAt(1).Label);

            Assert.Equal("P30", entity.Properties.ElementAt(2).Id);
            Assert.Equal("Q46", entity.Properties.ElementAt(2).Value);
            Assert.Equal(string.Empty, entity.Properties.ElementAt(2).Label);

            Assert.Equal("P131", entity.Properties.ElementAt(3).Id);
            Assert.Equal("Q145", entity.Properties.ElementAt(3).Value);
            Assert.Equal(string.Empty, entity.Properties.ElementAt(3).Label);

            entity.AddProperties(luceneIndexDirectory);

            Assert.Equal("P17", entity.Properties.ElementAt(0).Id);
            Assert.Equal("Q145", entity.Properties.ElementAt(0).Value);
            Assert.Equal("country", entity.Properties.ElementAt(0).Label);

            Assert.Equal("P47", entity.Properties.ElementAt(1).Id);
            Assert.Equal("Q27", entity.Properties.ElementAt(1).Value);
            Assert.Equal("shares border with", entity.Properties.ElementAt(1).Label);

            Assert.Equal("P30", entity.Properties.ElementAt(2).Id);
            Assert.Equal("Q46", entity.Properties.ElementAt(2).Value);
            Assert.Equal("continent", entity.Properties.ElementAt(2).Label);

            Assert.Equal("P131", entity.Properties.ElementAt(3).Id);
            Assert.Equal("Q145", entity.Properties.ElementAt(3).Value);
            Assert.Equal("located in the administrative territorial entity", entity.Properties.ElementAt(3).Label);
        }
    }
}
