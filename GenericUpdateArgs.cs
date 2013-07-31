using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace Silversite.Web.UI
{
    public class GenericUpdateArgs : GenericKeyDataArgs
    {
        public IDictionary OldValues { get; set; }

        public GenericUpdateArgs(IDictionary keys, IDictionary values, IDictionary oldValues)
            : base(keys, values)
        {
            this.OldValues = oldValues;
        }

        /// <summary>
        /// For optimistic concurrency: returns a new DataItem <typeparamref name="T"/> filled with the original input values and keys.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>A new DataItem <typeparamref name="T"/>.</returns>
        public virtual T GetOldDataItem<T>()
            where T:class, new()
        {
            return this.FillOldDataItem<T>(new T());
        }

        /// <summary>
        /// For optimistic concurrency: refreshes the DataItem <typeparamref name="T"/> with the original input values and keys.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataItem"></param>
        /// <returns>The DataItem <typeparamref name="T"/>.</returns>
        private T FillOldDataItem<T>(T dataItem)
        {
            this.Keys.BuildObject<T>(dataItem);
            this.OldValues.BuildObject<T>(dataItem);
            return dataItem;
        }
    }
}
