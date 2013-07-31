using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Collections.ObjectModel;
using System.Collections;
using System.Linq.Dynamic;

namespace Silversite.Web.UI {
	public class GenericDataSource: DataSourceControl {
		/// <summary>
		/// Occurs when the GenericDataSource control performs Select operation. This event must be handled to support the data Retrieval (Select) operation.
		/// </summary>
		public virtual event EventHandler<GenericSelectArgs> ExecuteSelect;

		/// <summary>
		/// Occurs when the GenericDataSource control performs Insert operation. This event must be handled to support the data Insert operation.
		/// </summary>
		public virtual event EventHandler<GenericDataArgs> ExecuteInsert;

		/// <summary>
		/// Occurs when the GenericDataSource control performs Update operation. This event must be handled to support the data Update operation.
		/// </summary>
		public virtual event EventHandler<GenericUpdateArgs> ExecuteUpdate;

		/// <summary>
		/// Occurs when the GenericDataSource control performs Delete operation. This event must be handled to support the data Delete operation.
		/// </summary>
		public virtual event EventHandler<GenericKeyDataArgs> ExecuteDelete;

		/// <summary>
		/// The DataSource resulted from the Select operation, automatically sorted and paged, if the case.
		/// </summary>
		public virtual IEnumerable DataSource { get; private set; }

		/// <summary>
		/// Set this to FALSE, for data insert scenarios.
		/// Once used by a data bound control, the GenericDataSource control will trigger the ExecuteSelect event to handle data Retrieval.
		/// </summary>
		public virtual bool DisableSelect { get; set; }

		/// <summary>
		/// When AutoPage is <value>True</value>, the data source set using <see cref="GenericSelectArgs.SetData()"/> handler will be automatically paged.
		/// </summary>
		public virtual bool AutoPage { get; set; }

		/// <summary>
		/// When AutoSort is <value>True</value>, the data source set using <see cref="GenericSelectArgs.SetData()"/> handler will be automatically sorted, if possible.
		/// </summary>
		public virtual bool AutoSort { get; set; }


		private Collection<string> _viewNames = new Collection<string>() { "Default GenericDataSourceView" };

		public GenericDataSource() {
			this.DisableSelect = false;
			//AutoSort is TRUE by default
			this.AutoSort = true;
			//AutoPage is TRUE by default
			this.AutoPage = true;
		}

		protected override DataSourceView GetView(string viewName) {
			return new GenericDataSourceView(this, viewName);
		}

		protected override ICollection GetViewNames() {
			return this._viewNames;
		}

		/// <summary>
		/// Gets the default (Generic)DataSourceView that handles the Select, Insert, Update and Delete operations.
		/// </summary>
		public DataSourceView DefaultView {
			get {
				return this.GetView(this._viewNames[0]);
			}
		}

		/// <summary>
		/// Triggers the data source Insert operation.
		/// </summary>
		/// <param name="args">The GenericInsertArgs arguments.</param>
		/// <returns>The number of affected rows.</returns>
		public virtual int Insert(GenericDataArgs args) {
			if (this.ExecuteInsert != null) ExecuteInsert(this, args);
			else OnExecuteInsert(args);
			return args.RowsAffected;
		}

		public virtual void OnExecuteSelect(GenericSelectArgs a) {
			if (ExecuteSelect != null) ExecuteSelect(this, a);
			else if (!this.DisableSelect) throw new NotImplementedException(string.Format(@"ExecuteSelect Handler for the GenericDataSource '{0}' is not implemented. Set DisableSelect=True -for Insert only scenarios-, else implement the Select Handler", this.ID));
		}
		public virtual void OnExecuteInsert(GenericDataArgs a) {
			if (ExecuteInsert != null) ExecuteInsert(this, a);
			else throw new NotImplementedException(string.Format("ExecuteInsert Handler for the GenericDataSource '{0}' is not implemented.", this.ID));
		}
		public virtual void OnExecuteUpdate(GenericUpdateArgs a) {
			if (ExecuteUpdate != null) ExecuteUpdate(this, a);
			else throw new NotImplementedException(string.Format("ExecuteUpdate Handler for the GenericDataSource '{0}' is not implemented.", this.ID));
		}
		public virtual void OnExecuteDelete(GenericKeyDataArgs a) {
			if (ExecuteDelete != null) ExecuteDelete(this, a);
			else throw new NotImplementedException(string.Format("ExecuteDelete Handler for the GenericDataSource '{0}' is not implemented.", this.ID));
		}

		/// <summary>
		/// Triggers the data source Update operation.
		/// It ofers support for Optimistic concurrency, by storing the original and updated parameter values.
		/// </summary>
		/// <param name="args">The GenericUpdateArgs arguments.</param>
		/// <returns>The number of affected rows.</returns>
		public virtual int Update(GenericUpdateArgs args) {
			if (this.ExecuteUpdate != null) ExecuteUpdate(this, args);
			else OnExecuteUpdate(args);
			return args.RowsAffected;
		}

		/// <summary>
		/// Triggers the data source Update operation.
		/// It ofers support for Optimistic concurrency, by storing the original and updated parameter values.
		/// </summary>
		/// <param name="args">The GenericDeleteArgs arguments.</param>
		/// <returns>The number of affected rows.</returns>
		public virtual int Delete(GenericKeyDataArgs args) {
			if (this.ExecuteDelete != null) ExecuteDelete(this, args);
			else OnExecuteDelete(args);
			return args.RowsAffected;
		}

		public virtual IEnumerable Select(GenericSelectArgs args) {
			args.AutoPage = this.AutoPage;
			args.AutoSort = this.AutoSort;
			if (this.ExecuteSelect != null) ExecuteSelect(this, args);
			else OnExecuteSelect(args);
			return DataSource = args.DataSource;
		}

		protected override void LoadViewState(object savedState) {
			base.LoadViewState(savedState);
			this.DisableSelect = (bool)(this.ViewState["DisableSelect"] ?? false);
			//AutoPage is TRUE, by default
			this.AutoPage = (bool)(this.ViewState["AutoPage"] ?? true);
			//AutoSort is TRUE by default
			this.AutoSort = (bool)(this.ViewState["AutoSort"] ?? true);
		}

		protected override object SaveViewState() {
			ViewState["DisableSelect"] = this.DisableSelect;
			if (this.AutoPage == false) {
				//Since AutoPage is TRUE by default, only save it to ViewState when is False
				ViewState["AutoPage"] = this.AutoPage;
			}
			if (this.AutoSort == false) {
				//Since AutoSort is TRUE by default, only save it to ViewState when is False
				ViewState["AutoSort"] = this.AutoSort;
			}

			return base.SaveViewState();
		}

	}
}
