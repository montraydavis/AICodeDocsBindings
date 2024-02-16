namespace AICodeDocsBindings.Generator
{
    public class PropertyDefinition
    {
        public string PropertyType { get; }
        public bool IsPublic { get; }
        public bool IsStatic { get; }
        public bool IsAbstract { get; }
        public bool IsField { get; }
        public bool IsVirtual { get; }
        public bool IsOverride { get; }
        public bool IsReturnType { get; }

        public string Name { get; }

        public PropertyDefinition(string type, string name, bool isPublic, bool isStatic, bool isAbstract, bool isField, bool isVirtual, bool isOverride, bool isReturnType)
        {
            PropertyType = type;
            Name = name;
            IsPublic = isPublic;
            IsStatic = isStatic;
            IsAbstract = isAbstract;
            IsField = isField;
            IsVirtual = isVirtual;
            IsOverride = isOverride;
            IsReturnType = isReturnType;
        }
    }
}
