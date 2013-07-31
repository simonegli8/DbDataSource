using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.ComponentModel;

namespace Silversite.Web.UI
{
    public class GenericArgsDataItem
    {
        public IDictionary[] Values { get; set; }

        public GenericArgsDataItem(params IDictionary[] values)
        {
            this.Values = values;
        }


        /// <summary>
        /// Gets the DataItem, after filling it with the available values.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Value<T>()
            where T : class, new()
        {
            return this.Fill<T>(new T());
        }

        /// <summary>
        /// Fills the DataItem with the modified values.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataItem"></param>
        /// <returns></returns>
        public T Fill<T>(T dataItem)
            where T:class
        {
            foreach (IDictionary value in this.Values)
            {
                value.BuildObject<T>(dataItem);
            }

            return dataItem;
        }
    }
}
