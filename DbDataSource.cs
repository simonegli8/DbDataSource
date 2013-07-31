using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Web;
using System.Web.UI.WebControls;
using System.Collections;
using System.Threading.Tasks;
using System.Web.DynamicData;
using System.Web.UI;
using System.Linq.Expressions;
using System.Reflection;
//using Silversite.Reflection;

/// <summary> The namespace for the DbDataSource classes </summary>
namespace Silversite.Web.UI {

	/// <summary>
	/// A generic data source for code first. This class is intended to be derived in a specific subclass for a specific DbSet of the DbContext. If there are for example Entities of type LogEntry, you could create
	/// a class LogDataSource that derives of type DbDataSource&lt;MyContext, LogEntry&gt;.
	/// </summary>
	/// <typeparam name="TContext">The underlying DbContext</typeparam>
	/// <typeparam name="TElement">The entity type</typeparam>
	public class DbDataSource<TContext, TElement>: GenericDataSource, IDisposable where TContext: DbContext where TElement: class, new() {

		public static readonly string ViewName = "DbView";

		public DbDataSource(): base() {
			EnableDelete = EnableInsert = EnableUpdate = true; //Database = Silversite.Data.Database.Default;
			ExecuteSelect += new EventHandler<GenericSelectArgs>(OnSelect);
			ExecuteInsert += new EventHandler<GenericDataArgs>(OnInsert);
			ExecuteUpdate += new EventHandler<GenericUpdateArgs>(OnUpdate);
			ExecuteDelete += new EventHandler<GenericKeyDataArgs>(OnDelete);
			//Where = set => set.OrderBy(e => 1);
			CreateContext = null;
			CreateObjectContext = null;
		}

		bool changed = false;

		private Func<IQueryable<TElement>, IQueryable<TElement>> where = null;
		/// <summary>
		/// A Where expression. You can set this expression in the aspx markup for a derived data source like so:
		/// &lt;asp:MyDbDataSource runat="server" Where='&lt;%# (Func&lt;IQueryable&lt;MyEntityType&gt;, IQueryable&lt;MyEntityType&gt;&gt;)(set => set.Where(set2 => my expression goes here) %&gt;' /&gt;
		/// </summary>
		public Func<IQueryable<TElement>, IQueryable<TElement>> Where { get { return where; } set { where = value; changed = true; } }
		private Func<IQueryable<TElement>, IQueryable> select = null;
		/// <summary>
		/// A Select expression. You can set this expression in the aspx markup for a derived data source  like so:
		/// &lt;asp:MyDbDataSource runat="server" Select='&lt;%# (Func&lt;IQueryable&lt;MyEntityType&gt;, IQueryable&gt;)(set => set.Select(set2 => my expression goes here) %&gt;' /&gt;
		/// </summary>
		public Func<IQueryable<TElement>, IQueryable> Select { get { return select ?? (set => set); } set { select = value; changed = true; } }
		private Func<IEnumerable, IQueryable> DoSelect { get { return (set) => Select(set.OfType<TElement>().AsQueryable<TElement>()); } }
		/// <summary>
		/// An expression that gets the data. You can set this expression in the aspx markup for a derived data source like so:
		/// &lt;asp:MyDbDataSource runat="server" Source="MyNamespace.MyDbContext.MyDbSet" Data='&lt;%# (Func&lt;IQueryable&lt;MyEntityType&gt;&gt;)(() => return a DbSet or an IQueryable here) %&gt;' /&gt;
		/// </summary>
		public Func<IQueryable<TElement>> Data { get; set; }

		public bool Projection { get { return Select != null; } }
		//Data.Database database;
		//public Data.Database Database { get { return database; } set { database = value; changed = true; } }
		
		public bool  EnableDelete { get; set; } 
		public bool  EnableInsert { get; set; }
		public bool  EnableUpdate { get; set; } 
		public event EventHandler<DynamicValidatorEventArgs>  Exception; // TODO

		DbContext db = null;

		/// <summary>
		/// A Func to create the DbContext
		/// </summary>
		public Func<DbContext> CreateContext { get; set; }
		public Func<System.Data.Objects.ObjectContext> CreateObjectContext { get; set; }

		/// <summary>
		/// The DbContext for this data source.
		/// </summary>
		public virtual DbContext Context {
			get {
				if (db == null) {
					if (CreateContext != null) db = (TContext)CreateContext();
					else if (CreateObjectContext != null) db = new DbContext(CreateObjectContext(), true);
					//else if (typeof(TContext).IsSubclassOf(typeof(Data.Context))) db = New.Object<TContext>() ?? New.Object<TContext>();
					else db = Activator.CreateInstance<TContext>();
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
				var set = Context.Set<TElement>()
					.OrderBy<TElement, bool>(e => true);
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


	/// <summary>
	/// A DataSource for EF CodeFirst.
	/// </summary>
	/// <example>
	/// You can add a datasource in your markup like so:<br/>
	/// <br/>
	/// &lt;ss:DbDataSource runat="server" Source="Namespace.MyDbContext.MyDbSetProperty" /&gt;<br/>
	/// <br/>
	/// You can also filter the result with LINQ in the Where property and select values with the Select property like so:<br/>
	///<br/>
	/// Where:<br/>
	///<br/>
	/// &lt;ss:DbDataSource runat="server" Source="..." Where='&lt;%# (Func&lt;IQueryable, IQueryable&gt;)(set => set.OfType&lt;MyEntityType&gt;().Where(s =&gt; My where expression here)) %&gt;' /&gt;<br/>
	///<br/>
	/// Select:<br/>
	///<br/>
	/// &lt;ss:DbDataSource runat="server" Source="..." Select='&lt;%# (Func&lt;IQueryable, IQueryable&gt;)(set =&gt; set.OfType&lt;MyEntityType&gt;().Select(s =&gt; My select expression here)) %&gt;' /&gt;<br/>
	/// </example>
	public class DbDataSource: GenericDataSource, IDisposable {

		public static readonly string ViewName = "DbView";

		public DbDataSource(): base() {
			EnableDelete = EnableInsert = EnableUpdate = true; //Database = Silversite.Data.Database.Default;
			ExecuteSelect += new EventHandler<GenericSelectArgs>(OnSelect);
			ExecuteInsert += new EventHandler<GenericDataArgs>(OnInsert);
			ExecuteUpdate += new EventHandler<GenericUpdateArgs>(OnUpdate);
			ExecuteDelete += new EventHandler<GenericKeyDataArgs>(OnDelete);
			//Where = set => set.OrderBy(e => 1);
			CreateContext = null;
			CreateObjectContext = null;
		}
		public DbDataSource(string source) : this() { Source = source; }

		bool changed;
		private Type TElement = null;
		private Type TContext = null;

		/// <summary>
		/// The Type of the used DbContext.
		/// </summary>
		public Type ContextType { get { return TContext; } set { TContext = value; } }

		private Func<IQueryable, IQueryable> where = null;
		/// <summary>
		/// A Where expression. You can set this expression in the aspx markup like so:
		/// &lt;asp:DbDataSource runat="server" Source="MyNamespace.MyDbContext.MyDbSet" Where='&lt;%# (Func&lt;IQueryable, IQueryable&gt;)(set => set.OfType&lt;MyEntityType&gt;().Where(set2 => my expression goes here) %&gt;' /&gt;
		/// </summary>
		public Func<IQueryable, IQueryable> Where { get { return where; } set { where = value; changed = true; } }
		private Func<IQueryable, IQueryable> select = null;
		/// <summary>
		/// A Select expression. You can set this expression in the aspx markup like so:
		/// &lt;asp:DbDataSource runat="server" Source="MyNamespace.MyDbContext.MyDbSet" Select='&lt;%# (Func&lt;IQueryable, IQueryable&gt;)(set => set.OfType&lt;MyEntityType&gt;().Select(set2 => my expression goes here) %&gt;' /&gt;
		/// </summary>
		public Func<IQueryable, IQueryable> Select { get { return select ?? (set => set); } set { select = value; changed = true; } }
		private Func<IEnumerable<object>, IQueryable> DoSelect { get { return (set) => Select(set.AsQueryable()); } }
		/// <summary>
		/// An expression that gets the data. You can set this expression in the aspx markup like so:
		/// &lt;asp:DbDataSource runat="server" Source="MyNamespace.MyDbContext.MyDbSet" Data='&lt;%# (Func&lt;IQueryable&gt;)(() => return a DbSet or an IQueryable here) %&gt;' /&gt;
		/// </summary>
		public Func<IQueryable> Data { get; set; }

		/// <summary>
		/// The name of the DbSet Property in the DbContext. Usually you don't set this property but you set the Source property instead.
		/// </summary>
		public string Set { get { return (string)ViewState["Set"]; } set { ViewState["Set"] = value; changed = true; } }
		public bool Projection { get { return Select != null; } }
		
		public bool EnableDelete { get; set; }
		public bool EnableInsert { get; set; }
		public bool EnableUpdate { get; set; }
		public string EntitySetName { get { return Set; } set { Set = value; changed = true; } }
		public event EventHandler<DynamicValidatorEventArgs> Exception; // TODO

		/// <summary>
		/// A string describing the DbSet to get the data from of the form "Namespace.DbContext.DbSetProperty".
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

				ContextType = Type.GetType(type);
				Set = set;
				changed = true;
			}
		}

		DbContext db = null;
		
		/// <summary>
		/// A Func to create the DbContext.
		/// </summary>
		public Func<DbContext> CreateContext { get; set; }
		public Func<System.Data.Objects.ObjectContext> CreateObjectContext { get; set; }

		/// <summary>
		/// The DbContext used.
		/// </summary>
		public virtual DbContext Context {
			get {
				if (db == null) {
					if (CreateContext != null) db = CreateContext();
					else if (CreateObjectContext != null) db = new DbContext(CreateObjectContext(), true);
					//else if (ContextType.IsSubclassOf(typeof(Data.Context))) db = (DbContext)(New.Object(ContextType) ?? New.Object(ContextType));
					else if (ContextType.IsSubclassOf(typeof(DbContext))) db = (DbContext)(Activator.CreateInstance(ContextType));
					else if (ContextType.IsSubclassOf(typeof(System.Data.Objects.ObjectContext))) db = new DbContext((System.Data.Objects.ObjectContext)Activator.CreateInstance(ContextType), true);
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
				var func = typeof(Func<,>).MakeGenericType( set.ElementType, typeof(bool));
				var ofType = typeof(Queryable).GetMethod("OfType", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(IQueryable) }, null)
					.MakeGenericMethod(set.ElementType);
				var orderBy = typeof(Queryable).GetMethods()
					.Where(method => method.Name == "OrderBy")
					.Where(method => method.GetParameters().Length == 2)
					.Single()
					.MakeGenericMethod(set.ElementType, typeof(bool));
				var lambdafunc = typeof(Expression).GetMethods()
					.Where(method => method.Name == "Lambda")
					.Where(method => method.GetParameters().Length == 2 && method.IsGenericMethod && method.GetParameters()[1].ParameterType == typeof(ParameterExpression[]))
					.First()
					.MakeGenericMethod(func);
				//Expression.Lambda<Func<UserInfo, bool>>(
			
				/*
					var lambda = Expression.Lambda<Func<Element, bool>>(
						Exression.Constant(true),
						new ParameterExpression[] { Expression.Parameter(set.ElementType, "e") }
					)
				*/
				var lambda = lambdafunc.Invoke(null,
						new object[] {
							Expression.Constant(true),
							new ParameterExpression[] { Expression.Parameter(set.ElementType, "e") }
						}
					);

				set = (IQueryable)ofType.Invoke(null, new[] { set });
				set = (IQueryable)orderBy.Invoke(null, new[] { set, lambda });

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

	/// <summary>
	/// A DataSource for an ObjectContext (For EF ModelFirst or DatabaseFirst)
	/// </summary>
	/// <typeparam name="TContext">The context type that derives from ObjectContext.</typeparam>
	/// <typeparam name="TElement">The entity type.</typeparam>
	public class OCDataSource<TContext, TElement> : DbDataSource<DbContext, TElement>, IDisposable
		where TContext : System.Data.Objects.ObjectContext
		where TElement : class, new() {

		public static readonly string ViewName = "DbView";

		public OCDataSource() : base() { }

		DbContext db;
		public virtual DbContext Context {
			get {
				if (db == null) {
					if (CreateContext != null) db = CreateContext();
					else if (CreateObjectContext != null) db = new DbContext(CreateObjectContext(), true);
					else db = new DbContext(Activator.CreateInstance<TContext>(), true);
				}
				return db;
			}
		}
	}
}