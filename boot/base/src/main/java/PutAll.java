
/*
Making it easier to maintain both Java and C# from the same code base.
*/

/*
C# does not have a putAll on its dictionaries.
*/
public static void PutAll<T, S>(this Dictionary<T, S> source, Dictionary<T, S> collection) {
        if (collection == null) {
            throw new ArgumentNullException("Collection is null");
        }
        foreach (var item in collection) {
            if(!source.ContainsKey(item.Key)){ 
               source.Add(item.Key, item.Value);
            }
            else {
               // handle duplicate key issue here
            }  
        } 
 }
 */