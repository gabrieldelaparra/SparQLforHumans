﻿using System.Collections.Generic;
using System.Linq;

namespace SparqlForHumans.Core.Utilities
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<IEnumerable<string>> GroupBySubject(this IEnumerable<string> lines)
        {
            var list = new List<string>();
            var last = string.Empty;

            foreach (var line in lines)
            {
                var entity = line.Split(" ").FirstOrDefault();

                //Base case: first value:
                if (last == string.Empty)
                {
                    list = new List<string>();
                    last = entity;
                }

                //Switch of entity:
                // - Return list,
                // - Create new list,
                // - Assign last to current
                else if (last != entity)
                {
                    yield return list;
                    list = new List<string>();
                    last = entity;
                }

                list.Add(line);
            }
            yield return list;
        }
    }
}