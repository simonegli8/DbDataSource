# DbDataSource

DbDataSource is a ASP.NET WebForms DataSource control for simple use with EntityFramework CodeFirst.

You can add a datasource in your markup like so:

<ss:DbDataSource ID="source" runat="server" Source="Namespace.MyDbContext.MyDbSetProperty" /> 

You can also filter the result with LINQ in the Where property and select values with the Select property like so:

Where:

<% source.Where = set => set.OfType<MyEntityType>().Where(s => My where expression here)) %> 

Select:

<% source.Select = set => set.OfType<MyEntityType>().Select(s => My select expression here)) %> 

Note: If you set the Select property, you cannot insert, update and delete anymore on the DataSource.
