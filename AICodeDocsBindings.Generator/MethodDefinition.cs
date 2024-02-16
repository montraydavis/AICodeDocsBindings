namespace AICodeDocsBindings.Generator
{
    public class MethodDefinition
    {
        public string Name { get; }
        public PropertyDefinition[] Parameters { get; }


        public MethodDefinition(string name, PropertyDefinition[] parameters)
        {
            Name = name;
            Parameters = parameters;
        }
    }
}
