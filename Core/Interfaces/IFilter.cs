using System.Collections.Generic;

namespace Kobanan
{
    public interface IFilter 
    {
        FilterMask GetMask();
        FilterEnumerator GetEnumerator();
    }
}