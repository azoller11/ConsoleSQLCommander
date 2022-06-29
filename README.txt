SQLCommander Release Version 1.0
Written by Alex Zoller 2022-June-29

Summary and Use case
	The purpose of SQLCommander (SQLC) is to enable faster development for those 
	in the C# (application) and SQL (database) communication enviorment. The framework
	or library contains methods to connect to a remote database and preform CRUD
	opporations between the application and database. The goal was to make it 
	completely dynamic for any application. The main leveragable methods in SQLC
	include creating and managing SQL tables within the database on the fly in 
	conjunction to your application. This way, if new objects are made and other objects
	are edited, they will be updated, added, or removed from within the database. Also as
	for getting objects, with simple criteria to articulate a SQL command string, one can
	pull objects from the database and then return them as new objects in a list or individually.
	This is the dynamic piece to the equation. A developer can implement SQLC into their application
	library components and follow best practices such as dependancy injection to fully utilize
	the framework from within the applicaion. 
	
Contact:
Alex Zoller
Email: Azoller11@gmail.com	
Github link: https://github.com/azoller11/ConsoleSQLCommander.git


Release 1.0 (2022-June-29)
	- First official release
	- Implementation of the following methods
		public bool updateObject<T>(T obj, List<string> whatFields, List<string> newValues);
        public bool updateObject<T>(T obj, List<Object> whatFields, List<string> newValues);
        public bool deleteObject<T>(T obj, Type field, string value);
        public bool deleteObject<T>(T obj, string field, string value);
        public bool insertObject<T>(T obj);
        public bool createObjectTable<T>(T type);
        public bool modifyObjectTable<T>(T obj);
        public bool deleteObjectTable<T>(T obj);
        public bool checkObjForID<T>(T type);
        public SqlConnection databaseConnection(string connectionString);
        public string findSQLField<T>(T field);
        public System.Data.SqlDbType findSQLType<T>(T field);
        public void open();
        public void close();
        public bool insert(string command);
        public bool update(string command);
        public bool insert(SqlCommand myCommand);
        public SqlDataReader read(string command);
        public bool testConnection();
        public Object getItem<T>(T obj, List<string> whatFields, string where, string wherevalue);
        public List<T> getItems<T>(T obj, List<string> whatFields, string where, string wherevalue, int limit);
        public List<T> getLikeItems<T>(T obj, List<string> whatFields, string where, string wherevalue, int limit);
        public bool clearItems<T>(T obj);
        public SqlConnection getConnection();
        public void log(string item);
        public string getLog();
        public string generateConnectionString(string ServerName, string Username, string Password, string DatabaseName, bool encrypt)
