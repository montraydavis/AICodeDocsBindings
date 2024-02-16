namespace AICodeDocsBindings.Generator
{
    using Codelyzer.Analysis.Model;
    using Microsoft.CodeAnalysis;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a class that sorts a list of class declarations by their dependencies.
    /// </summary>
    public class DependencySorter
    {
        private class Graph
        {
            public Dictionary<string, List<string>> AdjacencyList = new();

            /// <summary>
            /// Adds a vertex to the graph.
            /// </summary>
            /// <param name="vertex">The vertex to add.</param>
            public void AddVertex(string vertex)
            {
                if (!AdjacencyList.ContainsKey(vertex))
                {
                    AdjacencyList[vertex] = new List<string>();
                }
            }

            /// <summary>
            /// Adds an edge between two vertices in the graph.
            /// </summary>
            /// <param name="from">The starting vertex of the edge.</param>
            /// <param name="to">The ending vertex of the edge.</param>
            public void AddEdge(string from, string to)
            {
                if (AdjacencyList.ContainsKey(from) && !AdjacencyList[from].Contains(to))
                {
                    AdjacencyList[from].Add(to);
                }
            }
        }

        /// <summary>
        /// Sorts a list of class declarations by their dependencies.
        /// </summary>
        /// <param name="classes">The list of class declarations to sort.</param>
        /// <param name="usings">The list of using directives.</param>
        /// <returns>The sorted list of class declarations.</returns>
        public List<ClassDeclaration> SortByDependency(List<ClassDeclaration> classes, List<UsingDirective> usings)
        {
            var graph = new Graph();
            var classByName = classes.ToDictionary(c => c.Identifier, c => c);
            var classNames = classByName.Keys.ToHashSet();
            var usingNames = usings.Select(u => u.Identifier).ToHashSet();

            foreach (var cls in classes)
            {
                var className = cls.Identifier;
                graph.AddVertex(className);

                AddEdgesForBaseTypes(cls, classNames, graph);
                AddEdgesForUsings(cls, usingNames, classNames, graph);
                AddEdgesForMethodBodies(cls, classNames, graph);
            }

            var sortedClassNames = TopologicalSort(graph);
            return sortedClassNames.Select(name => classByName[name]).ToList();
        }

        /// <summary>
        /// Adds edges to the graph for the method bodies of a class.
        /// </summary>
        /// <param name="cls">The class declaration.</param>
        /// <param name="classNames">The set of class names.</param>
        /// <param name="graph">The graph to add edges to.</param>
        private void AddEdgesForMethodBodies(ClassDeclaration cls, HashSet<string> classNames, Graph graph)
        {
            // Implementation not shown
        }


        /// <summary>
        /// Adds edges to the graph for the base types of a class.
        /// </summary>
        /// <param name="cls">The class declaration.</param>
        /// <param name="classNames">The set of class names.</param>
        /// <param name="graph">The graph to add edges to.</param>
        private void AddEdgesForBaseTypes(ClassDeclaration cls, HashSet<string> classNames, Graph graph)
        {
            var className = cls.Identifier;
            if (cls.BaseList != null)
            {
                foreach (var baseType in cls.BaseList)
                {
                    var baseTypeName = baseType;
                    if (classNames.Contains(baseTypeName))
                    {
                        graph.AddEdge(className, baseTypeName);
                    }
                }
            }
        }

        /// <summary>
        /// Adds edges to the graph for the using directives of a class.
        /// </summary>
        /// <param name="cls">The class declaration.</param>
        /// <param name="usingNames">The set of using directive names.</param>
        /// <param name="classNames">The set of class names.</param>
        /// <param name="graph">The graph to add edges to.</param>
        private void AddEdgesForUsings(ClassDeclaration cls, HashSet<string> usingNames, HashSet<string> classNames, Graph graph)
        {
            var className = cls.Identifier;
            foreach (var usingName in usingNames)
            {
                if (usingName != className)
                {
                    var matchingTypes = classNames.Where(c => c.StartsWith(usingName + "."));
                    foreach (var matchingType in matchingTypes)
                    {
                        graph.AddEdge(className, matchingType);
                    }
                }
            }
        }

        /// <summary>
        /// Performs a topological sort on the graph.
        /// </summary>
        /// <param name="graph">The graph to sort.</param>
        /// <returns>The sorted list of vertices.</returns>
        private List<string> TopologicalSort(Graph graph)
        {
            var visited = new HashSet<string>();
            var stack = new Stack<string>();

            foreach (var vertex in graph.AdjacencyList.Keys)
            {
                TopologicalSortUtil(vertex, visited, stack, graph);
            }

            return stack.ToList();
        }

        /// <summary>
        /// Recursive helper method for performing a topological sort.
        /// </summary>
        /// <param name="vertex">The current vertex.</param>
        /// <param name="visited">The set of visited vertices.</param>
        /// <param name="stack">The stack used for the topological sort.</param>
        /// <param name="graph">The graph to sort.</param>
        private void TopologicalSortUtil(string vertex, HashSet<string> visited, Stack<string> stack, Graph graph)
        {
            if (!visited.Contains(vertex))
            {
                visited.Add(vertex);
                foreach (var neighbor in graph.AdjacencyList[vertex])
                {
                    TopologicalSortUtil(neighbor, visited, stack, graph);
                }
                stack.Push(vertex);
            }
        }
    }
}
