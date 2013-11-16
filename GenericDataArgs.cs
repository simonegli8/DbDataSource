using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace System.Web.UI.WebControls 
{
    public class GenericDataArgs : EventArgs
    {
        public int RowsAffected { get; set; }
        public IDictionary Values { get; set; }

        /// <summary>
        /// Reference to the DataItem object, used for one of the Create/Update/Delete actions.
        /// This is automatically setup when using <seealso cref="GetDataItem()"/> or <seealso cref="FillDataItem(object)"/>.
        /// </summary>
        public object DataItem { get; set; }

        /// <summary>
        /// The DataItem to be used for the data source operation.
        /// </summary>

        public GenericDataArgs(IDictionary values)
        {
            this.Values = values;
        }

        /// <summary>
        /// Returns a new DataItem <typeparamref name="T"/> filled with the modified input values.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>A new DataItem <typeparamref name="T"/>.</returns>
        public virtual T GetDataItem<T>()
            where T:class, new()
        {
            return this.FillDataItem<T>(new T());
        }

        /// <summary>
        /// Refreshes the DataItem <typeparamref name="T"/> with the modified input values.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataItem"></param>
        /// <returns>The DataItem <typeparamref name="T"/>.</returns>
        public virtual T FillDataItem<T>(T dataItem)
            where T : class
        {
            this.DataItem = dataItem;
            this.Values.BuildObject<T>(dataItem);
            return dataItem;
        }
    }
}
