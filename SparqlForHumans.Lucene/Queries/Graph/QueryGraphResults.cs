﻿using System;
using SparqlForHumans.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using SparqlForHumans.Models;
using SparqlForHumans.Wikidata.Services;

namespace SparqlForHumans.Lucene.Queries.Graph
{
    public static class QueryGraphResults
    {
        //private static Logger.Logger logger = Logger.Logger.Init();
        public static void GetGraphQueryResults(this QueryGraph graph, string entitiesIndexPath, string propertyIndexPath)
        {
            graph.SetIndexPaths(entitiesIndexPath, propertyIndexPath);

            InMemoryQueryEngine.Init(entitiesIndexPath, propertyIndexPath);
            graph.SetTypesDomainsAndRanges();

            graph.RunNodeQueries();
            graph.RunEdgeQueries();
        }

        private static void RunNodeQueries(this QueryGraph graph)
        {
            foreach (var node in graph.Nodes.Select(x => x.Value))
            {
                //Given Type, Do not Query
                if (node.IsGivenType)
                {
                    node.Results = new List<Entity>();
                    continue;
                }

                //Node that is not connected, return random results
                if (node.IsNotConnected(graph))
                {
                    var rnd = new Random();
                    var randomEntities = Enumerable.Repeat(1, 20).Select(_ => rnd.Next(999999)).Select(x => $"Q{x}");
                    node.Results = new BatchIdEntityQuery(graph.EntitiesIndexPath, randomEntities).Query();
                    continue;
                }

                //Just instance of, search only for that.
                if (!node.HasIncomingEdges(graph) && node.GetOutgoingEdges(graph).Count().Equals(1) &&
                    node.IsInstanceOfType)
                {
                    node.Results = new BatchIdEntityInstanceQuery(graph.EntitiesIndexPath, node.Types, 20).Query();
                    continue;
                }

                //The other complex queries. Try endpoint first, if timeout, try with the index.
                //If the user has a timeout, is because his query is still too broad.
                //Some suggestions will be proposed with the local index, until the query can be completed by the endpoint.
                var results = GraphApiQueries.RunQuery(node.ToSparql(graph).ToString())?.Select(x => x.ToEntity()).ToList();

                if (results == null)
                {
                    var intersectTypes = new List<string>();

                    //Outgoing edges candidates, take their domain
                    var domainTypes = node.GetOutgoingEdges(graph).Where(x => !x.IsInstanceOf).SelectMany(x => x.Domain).ToList();
                    intersectTypes = intersectTypes.IntersectIfAny(domainTypes).ToList();

                    //Incoming edges candidates, take their range.
                    var rangeTypes = node.GetIncomingEdges(graph).Where(x => !x.IsInstanceOf).SelectMany(x => x.Range).ToList();
                    intersectTypes = intersectTypes.IntersectIfAny(rangeTypes).ToList();

                    //Combine domain & range, take a random sample and get those results:
                    intersectTypes = intersectTypes.TakeRandom(100).ToList();
                    node.Results = new BatchIdEntityInstanceQuery(graph.EntitiesIndexPath, intersectTypes, 20).Query(100);

                    //If the instance is of a specific type, intersect a random sample of the instances with the previous results filter out the valid results:
                    if (node.IsInstanceOfType)
                    {
                        //Intersect (Not if any, we want only the results of that instance, even if there are none):
                        var instanceOfResults = new BatchIdEntityInstanceQuery(graph.EntitiesIndexPath, node.Types, 200).Query(20);
                        node.Results = node.Results.Intersect(instanceOfResults).ToList();
                    }
                }
                else
                {
                    node.Results = results;
                }
            }
        }

        private static void RunEdgeQueries(this QueryGraph graph)
        {
            foreach (var edge in graph.Edges.Select(x => x.Value))
            {
                if (edge.IsGivenType)
                {
                    edge.Results = new List<Property>();
                    continue;
                }

                List<string> domainPropertiesIds;
                List<string> rangePropertiesIds;
                List<string> propertiesIds;
                List<Entity> subjectNodes;
                List<Entity> objectNodes;
                List<Property> results;

                results = GraphApiQueries.RunQuery(edge.ToSparql(graph).ToString())?.Select(x => x.ToProperty()).ToList();

                if (results == null)
                {
                    var source = edge.GetSourceNode(graph);
                    var target = edge.GetTargetNode(graph);

                    switch (edge.QueryType)
                    {
                        case QueryType.GivenSubjectTypeDirectQueryOutgoingProperties:
                            subjectNodes = new BatchIdEntityQuery(graph.EntitiesIndexPath, source.Types).Query();
                            propertiesIds = subjectNodes.SelectMany(x => x.Properties).Select(x => x.Id).ToList();
                            edge.Results = new BatchIdPropertyQuery(graph.PropertiesIndexPath, propertiesIds).Query();
                            break;
                        case QueryType.GivenSubjectAndObjectTypeDirectQueryIntersectOutInProperties:
                            results = GraphApiQueries.RunQuery(edge.ToSparql(graph).ToString())?.Select(x => x.ToProperty()).ToList();
                            subjectNodes = new BatchIdEntityQuery(graph.EntitiesIndexPath, source.Types).Query();
                            var subjectProperties = subjectNodes.SelectMany(x => x.Properties).Select(x => x.Id).ToList();
                            objectNodes = new BatchIdEntityQuery(graph.EntitiesIndexPath, target.Types).Query();
                            var objectProperties = objectNodes.SelectMany(x => x.ReverseProperties).Select(x => x.Id).ToList();
                            propertiesIds = subjectProperties.Intersect(objectProperties).ToList();
                            edge.Results = new BatchIdPropertyQuery(graph.PropertiesIndexPath, propertiesIds).Query();
                            break;
                        case QueryType.GivenObjectTypeDirectQueryIncomingProperties:
                            objectNodes = new BatchIdEntityQuery(graph.EntitiesIndexPath, target.Types).Query();
                            propertiesIds = objectNodes.SelectMany(x => x.ReverseProperties).Select(x => x.Id).ToList();
                            edge.Results = new BatchIdPropertyQuery(graph.PropertiesIndexPath, propertiesIds).Query();
                            break;
                        case QueryType.KnownSubjectAndObjectTypesIntersectDomainRangeProperties:
                            domainPropertiesIds = InMemoryQueryEngine.BatchEntityIdOutgoingPropertiesQuery(source.Types).ToList();
                            rangePropertiesIds = InMemoryQueryEngine.BatchEntityIdIncomingPropertiesQuery(target.Types).ToList();
                            propertiesIds = domainPropertiesIds.Intersect(rangePropertiesIds).Distinct().Select(x => $"P{x}").ToList();
                            edge.Results = new BatchIdPropertyQuery(graph.PropertiesIndexPath, propertiesIds).Query().OrderByDescending(x => x.Rank).ToList();
                            break;
                        case QueryType.KnownSubjectTypeQueryDomainProperties:
                            var domains = InMemoryQueryEngine.BatchEntityIdOutgoingPropertiesQuery(source.Types).ToList();
                            propertiesIds = domains.Select(x => $"{x.GetUriIdentifier()}").ToList();
                            edge.Results = new BatchIdPropertyQuery(graph.PropertiesIndexPath, propertiesIds).Query().OrderByDescending(x => x.Rank).ToList();
                            break;
                        case QueryType.KnownObjectTypeQueryRangeProperties:
                            var ranges = InMemoryQueryEngine.BatchEntityIdIncomingPropertiesQuery(target.Types).ToList();
                            propertiesIds = ranges.Select(x => $"{x.GetUriIdentifier()}").ToList();
                            edge.Results = new BatchIdPropertyQuery(graph.PropertiesIndexPath, propertiesIds).Query().OrderByDescending(x => x.Rank).ToList();
                            break;
                        case QueryType.InferredDomainAndRangeTypeProperties:
                            domainPropertiesIds = InMemoryQueryEngine.BatchEntityIdOutgoingPropertiesQuery(source.Types).ToList();
                            rangePropertiesIds = InMemoryQueryEngine.BatchEntityIdIncomingPropertiesQuery(target.Types).ToList();
                            propertiesIds = domainPropertiesIds.Intersect(rangePropertiesIds).Distinct().Select(x => $"P{x}").ToList();
                            edge.Results = new BatchIdPropertyQuery(graph.PropertiesIndexPath, propertiesIds).Query();
                            break;
                        case QueryType.InferredDomainTypeProperties:
                            domainPropertiesIds = InMemoryQueryEngine.BatchEntityIdOutgoingPropertiesQuery(source.Types).ToList();
                            propertiesIds = domainPropertiesIds.Distinct().Select(x => $"P{x}").ToList();
                            edge.Results = new BatchIdPropertyQuery(graph.PropertiesIndexPath, propertiesIds).Query();
                            break;
                        case QueryType.InferredRangeTypeProperties:
                            rangePropertiesIds = InMemoryQueryEngine.BatchEntityIdIncomingPropertiesQuery(target.Types).ToList();
                            propertiesIds = rangePropertiesIds.Distinct().Select(x => $"P{x}").ToList();
                            edge.Results = new BatchIdPropertyQuery(graph.PropertiesIndexPath, propertiesIds).Query();
                            break;
                    }
                }
                else
                {
                    edge.Results = results;
                }
            }
        }
    }
}
