using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Collections;
using System.Linq;
using System.Linq.Dynamic;

namespace Silversite.Web.UI
{
    /// <summary>
    /// The DataSourceView for the GenericDataSourceControl. This DataSourceView handles the "Select", "Insert", "Update" and "Delete" events of the DataSource.
    /// </summary>
    public class GenericDataSourceView : DataSourceView
    {
        private GenericDataSource _owner;

        public GenericDataSourceView(GenericDataSource owner, string viewName)
            : base(owner, viewName)
        {
            this._owner = owner;
        }

        private GenericDataSource Owner
        {
            get { return this._owner; }
        }

        protected override IEnumerable ExecuteSelect(DataSourceSelectArguments arguments)
        {
            GenericSelectArgs args = new GenericSelectArgs(arguments)
            {
            };
            
            var theResult = this.Owner.Select(args);

            return theResult;
        }

        protected override int ExecuteInsert(IDictionary values)
        {
            GenericDataArgs args = new GenericDataArgs(values);
            int rowsReturned = this.Owner.Insert(args);
            return rowsReturned;
        }

        protected override int ExecuteUpdate(IDictionary keys, IDictionary values, IDictionary oldValues)
        {
            GenericUpdateArgs args = new GenericUpdateArgs(keys, values, oldValues);
            int rowsReturned = this.Owner.Update(args);
            return rowsReturned;
        }

        protected override int ExecuteDelete(IDictionary keys, IDictionary oldValues)
        {
            GenericKeyDataArgs args = new GenericKeyDataArgs(keys, oldValues);
            int rowsReturned = this.Owner.Delete(args);
            return rowsReturned;
        }

        public override bool CanPage
        {
            get
            {
                return true;
            }
        }

        public override bool CanRetrieveTotalRowCount
        {
            get
            {
                return true;
            }
        }
    }
}
