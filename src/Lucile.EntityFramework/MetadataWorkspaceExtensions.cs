using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data
{
    public static class MetadataWorkspaceExtensions
    {
        internal static Type GetClrTypeFromCSpaceType(this MetadataWorkspace workspace, EdmType conceptualType)
        {
            var itemCollection = (ObjectItemCollection)workspace.GetItemCollection(DataSpace.OSpace);

            if (conceptualType is StructuralType)
            {
                var objectSpaceType = workspace.GetObjectSpaceType((StructuralType)conceptualType);
                return itemCollection.GetClrType(objectSpaceType);
            }
            else if (conceptualType is EnumType)
            {
                var objectSpaceType = workspace.GetObjectSpaceType((EnumType)conceptualType);
                return itemCollection.GetClrType(objectSpaceType);
            }
            else if (conceptualType is PrimitiveType)
            {
                return ((PrimitiveType)conceptualType).ClrEquivalentType;
            }
            else if (conceptualType is CollectionType)
            {
                return workspace.GetClrTypeFromCSpaceType(((CollectionType)conceptualType).TypeUsage.EdmType);
            }
            else if (conceptualType is RefType)
            {
                return workspace.GetClrTypeFromCSpaceType(((RefType)conceptualType).ElementType);
            }
            else if (conceptualType is EdmFunction)
            {
                return workspace.GetClrTypeFromCSpaceType(((EdmFunction)conceptualType).ReturnParameter.TypeUsage.EdmType);
            }

            return null;
        }
    }
}