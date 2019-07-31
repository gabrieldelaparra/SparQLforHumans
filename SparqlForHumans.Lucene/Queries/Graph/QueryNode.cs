﻿using System.Collections.Generic;
using System.Linq;
using SparqlForHumans.Models;
using SparqlForHumans.Models.Query;
using SparqlForHumans.Utilities;
namespace SparqlForHumans.Lucene.Queries.Graph
{
    public class QueryNode : Node
    {
        public bool Traversed { get; set; } = false;
        public QueryNode(Node node)
        {
            id = node.id;
            name = node.name;
            uris = node.uris;
        }
        public QueryType QueryType { get; set; } = QueryType.Unkwown;
        public List<Entity> Results { get; set; } = new List<Entity>();
        public List<string> Types { get; set; } = new List<string>();
        public bool IsKnownType { get; set; } = false;
        public bool IsDirectedToKnownType { get; set; } = false;
        public bool IsInferredType { get; set; } = false;
        public List<string> InferredTypes { get; set; } = new List<string>();

        public override string ToString()
        {
            return $"{id}:{name} {(Types.Any() ? string.Join(";", Types.Select(x => x.GetUriIdentifier())) : string.Empty)}";
        }
    }


}
