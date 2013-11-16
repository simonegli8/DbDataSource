using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Collections;
using System.Linq;
using System.Linq.Dynamic;
//using System.Data.Linq;
using System.Web.UI.WebControls;

namespace System.Web.UI.WebControls 
{
	 public class GenericSelectArgs : EventArgs
	 {
		  public GenericSelectArgs()
		  {
				this.AutoPage = true;
				this.AutoSort = true;
		  }

		  public GenericSelectArgs(DataSourceSelectArguments args) : this()
		  {
				this.SelectArguments = args;
		  }

		  public DataSourceSelectArguments SelectArguments { get; set; }

		  /// <summary>
		  /// The Data Source will be sorted automatically.
		  /// </summary>
		  public bool AutoSort { get; set; }

		  /// <summary>
		  /// The Data Source will be paged automatically.
		  /// </summary>
		  public bool AutoPage { get; set; }

		  /// <summary>
		  /// Gets the DataSource Items to be displayed by the UI Control.
		  /// To set the raw DataSource Items, use the <see cref="SelectDataEventArgs.SetDataSource"/>, or <seealso cref="SelectDataEventArgs.SetPagedDataSource"/>.
		  /// </summary>
		  public IEnumerable DataSource { get; private set; }

		  /// <summary>
		  /// Sets the IQueryable data source. The data source will sorted and paged according to the <see cref="AutoSort"/> and <see cref="AutoPage"/>.
		  /// </summary>
		  /// <param name="data">IQueryable data source</param>
		  public virtual void SetData(IQueryable dataSource)
		  {
				var theResult = default(IQueryable);
				bool autoSort = this.AutoSort;
				if (dataSource.GetType().IsGenericType)
				{
					 theResult = dataSource;
				}
				else
				{
					 theResult = dataSource.OfType<object>().AsQueryable();
					 //non-generic data sources cannot be sorted by default.
					 autoSort = false;
				}

				this.SetData(theResult, autoSort, this.AutoPage);
		  }

		/// <summary>
		/// Sets the IQueryable data source. The data source will sorted and paged according to the <see cref="AutoSort"/> and <see cref="AutoPage"/>.
		/// </summary>
		/// <param name="data">IQueryable data source</param>
		public virtual void SetData(IQueryable dataSource, Func<IEnumerable<object>, IQueryable> select) {
			var theResult = default(IQueryable);
			bool autoSort = this.AutoSort;
			if (dataSource.GetType().IsGenericType) {
				theResult = dataSource;
			} else {
				theResult = dataSource.OfType<object>().AsQueryable();
				//non-generic data sources cannot be sorted by default.
				autoSort = false;
			}

			this.SetData(theResult, select, autoSort, this.AutoPage);
		}

		  /// <summary>
		  /// Sets the IEnumerable datasource. The data source will sorted and paged according to the <see cref="AutoSort"/> and <see cref="AutoPage"/>.
		  /// </summary>
		  /// <param name="data">IEnumerable data source</param>
		  public virtual void SetData(IEnumerable dataSource)
		  {
				var theResult = default(IQueryable);
				bool autoSort = this.AutoSort;
				if (dataSource.GetType().IsGenericType)
				{
					 theResult = dataSource.AsQueryable();
				}
				else
				{
					 theResult = dataSource.OfType<object>().AsQueryable();
					 autoSort = false;
				}

				this.SetData(theResult, autoSort, this.AutoPage);
		  }

		  /// <summary>
		  /// Sets the IEnumerable data source, already sorted and paged.
		  /// </summary>
		  /// <param name="data">The custom sorted and paged IEnumerable data source.</param>
		  /// <param name="totalRowCount">The total number of data items, to be used for UI data paging. The TotalRowCount is used to determine the total number of data pages.</param>
		  public virtual void SetPagedData(IEnumerable data, int totalRowCount)
		  {
				this.DataSource = data;
				this.SelectArguments.TotalRowCount = totalRowCount;
		  }


		  /// <summary>
		  /// Sets the IQueryable data source. The data source will sorted and paged according to the <param name="autoSort"/> and <param name="autoPage"/>.
		  /// </summary>
		  /// <param name="data">IQueryable data source</param>
		  public virtual void SetData(IQueryable dataSource, bool autoSort, bool autoPage)
		  {
				IQueryable theResult = dataSource;

				//performing sorting
				if (autoSort)
				{
					 theResult = dataSource.Sort(this.SelectArguments.SortExpression);
				}
				if (autoPage)
				{
					 theResult = theResult.Page(this.SelectArguments.StartRowIndex, this.SelectArguments.MaximumRows);
				}

				var dataSourceCount = dataSource.Count();
				this.SetPagedData(theResult, dataSourceCount);
		  }

		/// <summary>
		/// Sets the IQueryable data source. The data source will sorted and paged according to the <param name="autoSort"/> and <param name="autoPage"/>.
		/// </summary>
		/// <param name="data">IQueryable data source</param>
		public virtual void SetData(IQueryable dataSource, Func<IEnumerable<object>, IQueryable> select, bool autoSort, bool autoPage) {
			IQueryable theResult = dataSource;

			//performing sorting
			if (autoSort) {
				theResult = dataSource.Sort(this.SelectArguments.SortExpression);
			}
			if (autoPage) {
				theResult = theResult.Page(this.SelectArguments.StartRowIndex, this.SelectArguments.MaximumRows);
			}

			var dataSourceCount = dataSource.Count();

			var list = new List<object>();
			foreach (var res in theResult) { list.Add(res); }

			theResult = select(list);

			this.SetPagedData(theResult, dataSourceCount);
		}
	 }
}
