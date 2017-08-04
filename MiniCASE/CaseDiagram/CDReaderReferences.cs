using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniCASE
{
    public class CDReaderReferences: List<CDLazyReference>
    {
        public void Add(CDLazyType ltype, Guid findObjectGUID, object targetReference)
        {
            this.Add(new CDLazyReference()
            {
                lazyType = ltype,
                findObjectGuid = findObjectGUID,
                targetReference = targetReference,
                targetKey = null
            });
        }
        public void Add(CDLazyType ltype, Guid findObjectGUID, object targetReference, string targetKey)
        {
            this.Add(new CDLazyReference()
            {
                lazyType = ltype,
                findObjectGuid = findObjectGUID,
                targetReference = targetReference,
                targetKey = targetKey
            });
        }
    }

    public class CDLazyReference
    {
        public CDLazyType lazyType;
        public Guid findObjectGuid;
        public object targetReference;
        public string targetKey;
    }

    public enum CDLazyType
    {
        None = 0,
        FindDefinition,
        FindDefinitionForDictionaryValue,
        FindDefinitionForDefaultValue,
        FindStartShape,
        FindEndShape,
        FindDecompositionObject
    }

}
