using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Web;
using System.Web.UI.WebControls;
using System.Collections;
using System.Threading.Tasks;
//using System.Web.DynamicData;
using System.Web.UI;
using System.Linq.Expressions;
using System.Reflection;
using Silversite;

namespace System.Web.UI.WebControls {

	/// <summary>
	/// A DataSource base class for custom data sources specific to a entity type.
	/// </summary>
	/// <typeparam name="TContext">The DbContext type</typeparam>
	/// <typeparam name="TElement">The entity type.</typeparam>
	public class DbDataSource<TContext, TElement>: GenericDataSource, IDisposable where TContext: DbContext where TElement: class, new() {

		public static readonly string ViewName = "DbView";

		public DbDataSource(): base() {
			EnableDelete = EnableInsert = EnableUpdate = true; Database = null;
			ExecuteSelect += new EventHandler<GenericSelectArgs>(OnSelect);
			ExecuteInsert += new EventHandler<GenericDataArgs>(OnInsert);
			ExecuteUpdate += new EventHandler<GenericUpdateArgs>(OnUpdate);
			ExecuteDelete += new EventHandler<GenericKeyDataArgs>(OnDelete);
			Where = set => set.OrderBy(e => 1);
			CreateContext = null;
			CreateObjectContext = null;
		}

		bool changed = false;

		private Func<IQueryable<TElement>, IQueryable<TElement>> where = null;
		/// <summary>
		/// You set this property to specify a where expression:
		/// &lt;% mydatasourceID.Where = set => set.Where(e => My where expression); %&gt;
		/// </summary>
		public Func<IQueryable<TElement>, IQueryable<TElement>> Where { get { return where; } set { where = value; changed = true; } }
		private Func<IQueryable<TElement>, IQueryable> select = null;
		/// <summary>
		/// You set this property to specify a select expression:<br/>
		/// &lt;% mydatasourceID.Select = set => set.Select(e => My select expression); %&gt;<br/>
		/// Note that when you use a select expression, you cannot insert, update and delete on the datasource anymore.
		/// </summary>
		public new Func<IQueryable<TElement>, IQueryable> Select { get { return select ?? (set => set); } set { select = value; changed = true; } }
		private Func<IEnumerable, IQueryable> DoSelect { get { return (set) => Select(set.OfType<TElement>().AsQueryable<TElement>()); } }
		public Func<IQueryable<TElement>> Data { get; set; }

		public bool Projection { get { return Select != null; } }
		string database;
		/// <summary>
		/// The name of the ConnectionString.
		/// </summary>
		public string Database { get { return database; } set { database = value; changed = true; } }
		
		public bool  EnableDelete { get; set; } 
		public bool  EnableInsert { get; set; }
		public bool  EnableUpdate { get; set; } 
		//public event EventHandler<DynamicValidatorEventArgs>  Exception; // TODO

		TContext db = null;

		public Func<string, DbContext> CreateContext { get; set; }
		public Func<string, System.Data.Entity.Core.Objects.ObjectContext> CreateObjectContext { get; set; }

		public new virtual TContext Context {
			get {
				if (db == null) {
					if (CreateContext != null) db = (TContext)CreateContext(Database);
					else if (CreateObjectContext != null) db = (TContext)new DbContext(CreateObjectContext(Database), true);
					else db = !string.IsNullOrEmpty(Database) ? New.Object<TContext>(Database) ?? New.Object<TContext>() : New.Object<TContext>();
				}
				return db;
			}
		}

		protected override void OnDataBinding(EventArgs e) {
			base.OnDataBinding(e);
			if (changed) {
				RaiseDataSourceChangedEvent(EventArgs.Empty);
				changed = false;
			}
		}

		protected void OnSelect(object sender, GenericSelectArgs args) {
			IQueryable res;
			if (Data == null) {
				var set = Context.Set<TElement>();
				if (Where != null) res = Where(set);
				else res = set;
			} else {
				if (Where != null) res = Where(Data());
				else res = Data();
			}
			
			args.SetData(res, DoSelect);
		}

		protected void OnDelete(object sender, GenericKeyDataArgs arg) {
			if (EnableDelete && Data == null) {
				var set = Context.Set<TElement>();
				var x = arg.GetDataItem<TElement>();
				set.Remove(x);
				Context.SaveChanges();
				changed = false;
				RaiseDataSourceChangedEvent(EventArgs.Empty);
			}
		}

		protected void OnInsert(object sender, GenericDataArgs arg) {
			if (EnableInsert && Data == null) {
				var set = Context.Set<TElement>();
				set.Add(arg.GetDataItem<TElement>());
				Context.SaveChanges();
				changed = false;
				RaiseDataSourceChangedEvent(EventArgs.Empty);
			}
		}

		protected void OnUpdate(object sender, GenericUpdateArgs arg) {
			if (EnableUpdate && Data == null) {
				var set = Context.Set<TElement>();
				var keynames = arg.Keys.Keys.OfType<string>().ToList();
				TElement x;
				if (arg.Keys.Count == 1) {
					x = set.Find(arg.Keys[0]);
				} else {
					var keyindexes = keynames.Select(key => typeof(TElement).GetProperties().Select(p => p.Name).ToList().IndexOf(key)).ToList();
					x = set.Find(keynames.OrderBy(name => keyindexes));
				}
				arg.FillDataItem<TElement>(x);
				Context.SaveChanges();
				changed = false;
				RaiseDataSourceChangedEvent(EventArgs.Empty);
			}
		}

		protected override void OnUnload(EventArgs e) {
			base.OnUnload(e);
			Dispose();
		}
		void IDisposable.Dispose() {
			if (db != null) { db.Dispose(); db = null; } 
		}
	}

	public class OCDataSource<TContext, TElement>: DbDataSource<DbContext, TElement>, IDisposable
		where TContext: System.Data.Entity.Core.Objects.ObjectContext
		where TElement: class, new() {

		public OCDataSource(): base() { }

		DbContext db;
		public override DbContext Context {
			get {
				if (db == null) {
					if (CreateContext != null) db = CreateContext(Database);
					else if (CreateObjectContext != null) db = new DbContext(CreateObjectContext(Database), true);
					else db = new DbContext(New.Object<TContext>(), true);
				}
				return db;
			}
		}
	}


	/// <summary>
	/// A DataSource for EntityFramework.<br/>
	/// You use the DbDataSource like so:<br/>
	/// <br/>
	/// &lt;asp:DbDataSource ID="source" runat="server" Source="Namespace.MyDbContext.MyDbSetProperty" /&gt;<br/>
	/// <br/>
	/// You can additionally set Where and Select LINQ expressions like so:<br/>
	/// <br/>
	/// &lt;%<br/>
	///   source.Where = set => set.OfType&lt;MyEntityType&gt;().Where(p => My where expression...);<br/>
	///   source.Select = set => set.OfType&lt;MyEntityType&gt;().Select(p => My select expression...);<br/>
	/// %&gt;
	/// </summary>
	public class DbDataSource: GenericDataSource, IDisposable {

		public DbDataSource(): base() {
			EnableDelete = EnableInsert = EnableUpdate = true; Database = null;
			ExecuteSelect += new EventHandler<GenericSelectArgs>(OnSelect);
			ExecuteInsert += new EventHandler<GenericDataArgs>(OnInsert);
			ExecuteUpdate += new EventHandler<GenericUpdateArgs>(OnUpdate);
			ExecuteDelete += new EventHandler<GenericKeyDataArgs>(OnDelete);
			CreateContext = null;
			CreateObjectContext = null;
			Where = null;
		}
		public DbDataSource(string source) : this() { Source = source; }

		bool changed;
		private Type TContext = null;
		public Type ContextType { get { return TContext; } set { TContext = value; } }

		private Func<IQueryable, IQueryable> where = null;
		/// <summary>
		/// You set this property to specify a where expression:<br/>
		/// &lt;% mydatasourceID.Where = set => set.OfType&lt;MyEntityType&gt;().Where(e => My where expression); %&gt;
		/// </summary>
		public Func<IQueryable, IQueryable> Where { get { return where; } set { where = value; changed = true; } }
		private Func<IQueryable, IQueryable> select = null;
		/// <summary>
		/// You set this property to specify a select expression:<br/>
		/// &lt;% mydatasourceID.Select = set => set.OfType&lt;MyEntityType&gt;().Select(e => My select expression); %&gt;<br/>
		/// Note that when you use a select expression, you cannot insert, update and delete on the datasource anymore.
		/// </summary>
		public new Func<IQueryable, IQueryable> Select { get { return select ?? (set => set); } set { select = value; changed = true; } }
		private Func<IEnumerable<object>, IQueryable> DoSelect { get { return (set) => Select(set.AsQueryable()); } }
		public Func<IQueryable> Data { get; set; }

		public string Set { get { return (string)ViewState["Set"]; } set { ViewState["Set"] = value; changed = true; } }
		public bool Projection { get { return Select != null; } }
		string database;
		/// <summary>
		/// The name of the ConnectionString.
		/// </summary>
		public string Database { get { return database; } set { database = value; changed = true; } }
		
		public bool EnableDelete { get; set; }
		public bool EnableInsert { get; set; }
		public bool EnableUpdate { get; set; }
		public string EntitySetName { get { return Set; } set { Set = value; changed = true; } }
		//public event EventHandler<DynamicValidatorEventArgs> Exception; // TODO
		/// <summary>
		/// The Source of the data in the format "Namespace.MyDbContext.MyDbSetProperty"
		/// </summary>
		public string Source {
			get { return (ContextType ?? typeof(DbContext)).FullName + "." + Set; }
			set {
				string assembly = null;
				var comma = value.IndexOf(',');
				var type = value;
				if (comma >= 0) {
					assembly = value.Substring(comma);
					type = value.Substring(0, comma);
				}
				string set = null;
				var dot = type.LastIndexOf('.');
				if (dot <= 0) throw new ArgumentException(string.Format("Source {0} must be the fully qualified name of a property of a DbContext.", type));
				set = type.Substring(dot + 1);
				type = type.Substring(0, dot);

				if (assembly != null) type = type + assembly;

				ContextType = New.Type(type);
				Set = set;
				changed = true;
			}
		}

		DbContext db = null;
		
		public Func<string, DbContext> CreateContext { get; set; }
		public Func<string, System.Data.Entity.Core.Objects.ObjectContext> CreateObjectContext { get; set; }

		public new virtual DbContext Context {
			get {
				if (db == null) {
					if (CreateContext != null) db = CreateContext(Database);
					else if (CreateObjectContext != null) db = new DbContext(CreateObjectContext(Database), true);
					else if (ContextType.Is<DbContext>()) db = (DbContext)(!string.IsNullOrEmpty(Database) ? New.Object(ContextType, Database) ?? New.Object(ContextType) : New.Object(ContextType));
					else if (ContextType.Is<System.Data.Entity.Core.Objects.ObjectContext>()) db = new DbContext((System.Data.Entity.Core.Objects.ObjectContext)New.Object(ContextType), true);
					else throw new NotSupportedException(string.Format("Unsupported Context type {0}.", ContextType.FullName));
				}
				return db;
			}
		}

		protected override void OnDataBinding(EventArgs e) {
			base.OnDataBinding(e);
			if (changed) {
				RaiseDataSourceChangedEvent(EventArgs.Empty);
				changed = false;
			}
		}

		private IQueryable SetAsQuery { get { var c = Context; var t = c.GetType(); var p = t.GetProperty(Set); return (IQueryable)p.GetValue(c); } }
		private DbSet SetAsDbSet { get { var c = Context; var t = c.GetType(); var p = t.GetProperty(Set); return c.Set(p.PropertyType.GetGenericArguments().FirstOrDefault()); } }


		protected void OnSelect(object sender, GenericSelectArgs args) {
			IQueryable res;
			if (Data == null) {
				var set = SetAsQuery;
				//set.OfType<bool>()
				// .OrderBy(e => 1);
				var func = typeof(Func<,>).MakeGenericType(set.ElementType, typeof(bool));
				var ofType = typeof(Queryable).GetMethod("OfType", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(IQueryable) }, null)
					.MakeGenericMethod(set.ElementType);
				var orderBy = typeof(Queryable).GetMethods()
					.Where(method => method.Name == "OrderBy")
					.Where(method => method.GetParameters().Length == 2)
					.Single()
					.MakeGenericMethod(set.ElementType, typeof(bool));

				/*
					var lambda = Expression.Lambda<Func<Element, bool>>(
						Exression.Constant(true),
						new ParameterExpression[] { Expression.Parameter(set.ElementType, "e") }
					)
				*/			
				var lambdafunc = typeof(Expression).GetMethods()
					.Where(method => method.Name == "Lambda")
					.Where(method => method.GetParameters().Length == 2 && method.IsGenericMethod && method.GetParameters()[1].ParameterType == typeof(ParameterExpression[]))
					.First()
					.MakeGenericMethod(func);

				var lambda = lambdafunc.Invoke(null,
						new object[2] {
							Expression.Constant(true),
							new ParameterExpression[] { Expression.Parameter(set.ElementType, "e") }
						}
					);

				ofType.Invoke(null, new[] { set });
				orderBy.Invoke(null, new[] { set, lambda });

				if (Where != null) res = Where(set);
				else res = set;
			} else {
				if (Where != null) res = Where(Data());
				else res = Data();
			}

			args.SetData(res, DoSelect);
		}

		protected void OnDelete(object sender, GenericKeyDataArgs arg) {
			if (EnableDelete && Data == null) {
				var set = SetAsDbSet;
				var x = arg.DataItem;
				set.Remove(x);
				Context.SaveChanges();
				changed = false;
				RaiseDataSourceChangedEvent(EventArgs.Empty);
			}
		}

		protected void OnInsert(object sender, GenericDataArgs arg) {
			if (EnableInsert && Data == null) {
				var set = SetAsDbSet;
				set.Add(arg.DataItem); ;
				Context.SaveChanges();
				changed = false;
				RaiseDataSourceChangedEvent(EventArgs.Empty);
			}
		}

		protected void OnUpdate(object sender, GenericUpdateArgs arg) {
			if (EnableUpdate && Data == null) {
				var set = SetAsDbSet;
				var keynames = arg.Keys.Keys.OfType<string>().ToList();
				object x;
				if (arg.Keys.Count == 1) {
					x = set.Find(arg.Keys[0]);
				} else {
					var keyindexes = keynames.Select(key => set.ElementType.GetProperties().Select(p => p.Name).ToList().IndexOf(key)).ToList();
					x = set.Find(keynames.OrderBy(name => keyindexes));
				}
				foreach (var prop in set.ElementType.GetProperties()) {
					prop.SetValue(x, arg.Values[prop.Name]);
				}
				Context.SaveChanges();
				changed = false;
				RaiseDataSourceChangedEvent(EventArgs.Empty);
			}
		}

		protected override void OnUnload(EventArgs e) {
			base.OnUnload(e);
			Dispose();
		}

		void IDisposable.Dispose() {
			if (db != null) { db.Dispose(); db = null; }
		}
	}
}