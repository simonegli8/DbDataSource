using System.Collections;

namespace Silversite.Web.UI
{
    public class GenericKeyDataArgs : GenericDataArgs
    {
        public IDictionary Keys { get; private set; }

        public GenericKeyDataArgs(IDictionary keys, IDictionary oldValues) : base(oldValues)
        {
            this.Keys = keys;
        }

        /// <summary>
        /// Refreshes the DataItem <typeparamref name="T"/> with the modified input values and keys.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataItem"></param>
        /// <returns>The DataItem <typeparamref name="T"/>.</returns>
        public override T FillDataItem<T>(T dataItem)
        {
            this.Values.BuildObject<T>(dataItem);
            //Filling also with Keys values.
            this.Keys.BuildObject<T>(dataItem);

            return dataItem;
        }
    }
}
