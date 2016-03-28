using System;
using System.Reflection;

namespace MacomberMapCommunications.Extensions
{
    public static class Copy
    {
        public static void CopyTo<TSource, TDest>(this TSource sourceObject, TDest destinationObject)
        {
            Type tSource = typeof(TSource);
            Type tDest = typeof(TDest);

            var flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

            // copy properties
            foreach (var sourceProp in tSource.GetProperties(flags))
            {
                if (!sourceProp.CanRead || (sourceProp.GetIndexParameters().Length > 0))
                    continue;

                var destProp = tDest.GetProperty(sourceProp.Name, flags);
                if ((destProp != null) && destProp.CanWrite)
                    destProp.SetValue(destinationObject, sourceProp.GetValue(sourceObject, null), null);
            }

            // copy fields
            foreach (var sourceField in tSource.GetFields(flags))
            {
                var destField = tDest.GetField(sourceField.Name, flags);
                if ((destField != null) && !destField.IsInitOnly)
                    destField.SetValue(destinationObject, sourceField.GetValue(sourceObject));
            }
        }

    }
}