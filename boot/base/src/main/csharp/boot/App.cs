using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;

// vim: et:ts=4:sw=4

namespace boot
{

	using ClojureRuntimeShim = org.projectodd.shimdandy.ClojureRuntimeShim;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public class App
	public class App
	{
		private static File aetherfile = null;
		private static File[] podjars = null;
		private static File[] corejars = null;
		private static File[] workerjars = null;
		private static string cljversion = null;
		private static string cljname = null;
		private static string bootversion = null;
		private static string localrepo = null;
		private static string appversion = null;
		private static string channel = "RELEASE";
		private static string booturl = "http://boot-clj.com";
		private static string githuburl = "https://api.github.com/repos/boot-clj/boot/releases";
		private static bool update_always = false;
		private static ClojureRuntimeShim aethershim = null;
		private static Dictionary<string, File[]> depsCache = null;

		private static readonly File homedir = new File(System.getProperty("user.home"));
		private static readonly File workdir = new File(System.getProperty("user.dir"));
		private const string aetherjar = "aether.uber.jar";
		private static readonly AtomicLong counter = new AtomicLong(0);
		private static readonly ExecutorService ex = Executors.newCachedThreadPool();

		public static string Version
		{
			get
			{
				return appversion;
			}
		}
		public static string BootVersion
		{
			get
			{
				return bootversion;
			}
		}
		public static string ClojureName
		{
			get
			{
				return cljname;
			}
		}

		private static readonly WeakHashMap<ClojureRuntimeShim, object> pods = new WeakHashMap<ClojureRuntimeShim, object>();
		private static readonly ConcurrentDictionary<string, object> stash = new ConcurrentDictionary<string, object>();

		public static WeakHashMap<ClojureRuntimeShim, object> Pods
		{
			get
			{
				return pods;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static Object getStash(String key) throws Exception
		public static object getStash(string key)
		{
			return stash.Remove(key);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static String setStash(Object value) throws Exception
		public static string setStash(object value)
		{
			string key = UUID.randomUUID().ToString();
			stash[key] = value;
			return key;
		}

		public class Exit : Exception
		{
			public Exit(string m) : base(m)
			{
			}
			public Exit(string m, Exception c) : base(m, c)
			{
			}
		}

		private static long nextId()
		{
			return counter.addAndGet(1);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static File getBootDir() throws Exception
		public static File BootDir
		{
			get
			{
				return bootdir();
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static boolean isWindows() throws Exception
		public static bool Windows
		{
			get
			{
				return (System.getProperty("os.name").ToLower().IndexOf("win", StringComparison.Ordinal) >= 0);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static File mkFile(File parent, String... kids) throws Exception
		public static File mkFile(File parent, params string[] kids)
		{
			File ret = parent;
			foreach (string k in kids)
			{
				ret = new File(ret, k);
			}
			return ret;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void mkParents(File f) throws Exception
		public static void mkParents(File f)
		{
			File ff = f.CanonicalFile.ParentFile;
			if (!ff.exists())
			{
				ff.mkdirs();
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static ClassLoader tccl() throws Exception
		public static ClassLoader tccl()
		{
			return Thread.CurrentThread.ContextClassLoader;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static InputStream resource(String path) throws Exception
		public static System.IO.Stream resource(string path)
		{
			return tccl().getResourceAsStream(path);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.util.Properties propertiesResource(String path) throws Exception
		public static Properties propertiesResource(string path)
		{
			Properties p = new Properties();
			using (System.IO.Stream @is = resource(path))
			{
				p.load(@is);
			}
			return p;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static File bootdir() throws Exception
		public static File bootdir()
		{
			File h = new File(System.getProperty("user.home"));
			string a = System.getProperty("BOOT_HOME");
			string b = System.getenv("BOOT_HOME");
			string c = (new File(h, ".boot")).CanonicalPath;
			return new File((!string.ReferenceEquals(a, null)) ? a : ((!string.ReferenceEquals(b, null)) ? b : c));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static String md5hash(String data) throws Exception
		public static string md5hash(string data)
		{
			java.security.MessageDigest algo = java.security.MessageDigest.getInstance("MD5");
			return javax.xml.bind.DatatypeConverter.printHexBinary(algo.digest(data.GetBytes()));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static File projectDir() throws Exception
		public static File projectDir()
		{
			for (File f = workdir; f != null; f = f.ParentFile)
			{
				File tmp = new File(f, ".git");
				if (tmp.exists() && tmp.Directory)
				{
					return f;
				}
			}
			return null;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.util.HashMap<String, String> properties2map(java.util.Properties p) throws Exception
		public static Dictionary<string, string> properties2map(Properties p)
		{
			Dictionary<string, string> m = new Dictionary<string, string>();
			foreach (KeyValuePair<object, object> e in p.entrySet())
			{
				m[(string) e.Key] = (string) e.Value;
			}
			return m;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.util.Properties map2properties(java.util.HashMap<String, String> m) throws Exception
		public static Properties map2properties(Dictionary<string, string> m)
		{
			Properties p = new Properties();
			foreach (KeyValuePair<string, string> e in m.SetOfKeyValuePairs())
			{
				p.setProperty(e.Key, e.Value);
			}
			return p;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.util.HashMap<String, File> propertiesFiles() throws Exception
		public static Dictionary<string, File> propertiesFiles()
		{
			Dictionary<string, File> ret = new Dictionary<string, File>();
			string[] names = new string[]{"boot", "project", "cwd"};
			File[] dirs = new File[]{bootdir(), projectDir(), workdir};
			for (int i = 0; i < dirs.Length; i++)
			{
				ret[names[i]] = new File(dirs[i], "boot.properties");
			}
			return ret;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.util.Properties mergeProperties() throws Exception
		public static Properties mergeProperties()
		{
			Properties p = new Properties();
			Dictionary<string, File> fs = propertiesFiles();
			foreach (string k in new string[]{"boot", "project", "cwd"})
			{
				try
				{
						using (System.IO.FileStream @is = new System.IO.FileStream(fs[k], System.IO.FileMode.Open, System.IO.FileAccess.Read))
						{
						p.load(@is);
						}
				}
				catch (FileNotFoundException)
				{
				}
			}
			return p;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void setDefaultProperty(java.util.Properties p, String k, String dfl) throws Exception
		public static void setDefaultProperty(Properties p, string k, string dfl)
		{
			if (p.getProperty(k) == null)
			{
				p.setProperty(k, dfl);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.util.HashMap<String, String> config() throws Exception
		public static Dictionary<string, string> config()
		{
			Dictionary<string, string> ret = new Dictionary<string, string>();

//JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'putAll' method:
			ret.putAll(properties2map(mergeProperties()));
			ret.Remove("BOOT_HOME");
//JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'putAll' method:
			ret.putAll(System.getenv());
//JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'putAll' method:
			ret.putAll(properties2map(System.Properties));

			IEnumerator<string> i = ret.Keys.GetEnumerator();
			while (i.MoveNext())
			{
				string k = i.Current;
				if (!k.StartsWith("BOOT_", StringComparison.Ordinal))
				{
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
					i.remove();
				}
			}

			return ret;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static String config(String k) throws Exception
		public static string config(string k)
		{
			return config()[k];
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static String config(String k, String dfl) throws Exception
		public static string config(string k, string dfl)
		{
			string v = config(k);
			if (!string.ReferenceEquals(v, null))
			{
				return v;
			}
			else
			{
				System.setProperty(k, dfl);
				return dfl;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static String jarVersion(File f) throws Exception
		private static string jarVersion(File f)
		{
			string ret = null;
			JarEntry e = null;
			string pat = "META-INF/maven/boot/boot/pom.properties";
			using (JarFile jar = new JarFile(f))
			{
				if ((e = jar.getJarEntry(pat)) != null)
				{
					using (System.IO.Stream @is = jar.getInputStream(e))
					{
						Properties p = new Properties();
						p.load(@is);
						ret = p.getProperty("version");
					}
				}
			}
			return ret;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static java.util.Properties writeProps(File f) throws Exception
		private static Properties writeProps(File f)
		{
			mkParents(f);
			ClojureRuntimeShim a = aetherShim();
			Properties p = new Properties();
			string c = cljversion;
			string n = cljname;
			string t = null;

			try
			{
					using (System.IO.FileStream @is = new System.IO.FileStream(f, System.IO.FileMode.Open, System.IO.FileAccess.Read))
					{
					p.load(@is);
					}
			}
			catch (FileNotFoundException)
			{
			}

			if (string.ReferenceEquals(bootversion, null))
			{
				foreach (File x in resolveDepJars(a, "boot", channel, n, c))
				{
					if (null != (t = jarVersion(x)))
					{
						bootversion = t;
					}
				}
			}

			p.setProperty("BOOT_VERSION", bootversion);
			setDefaultProperty(p, "BOOT_CLOJURE_NAME", n);
			setDefaultProperty(p, "BOOT_CLOJURE_VERSION", c);

			using (System.IO.FileStream os = new System.IO.FileStream(f, System.IO.FileMode.Create, System.IO.FileAccess.Write))
			{
					p.store(os, booturl);
			}

			return p;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static java.util.Properties readProps(File f, boolean create) throws Exception
		private static Properties readProps(File f, bool create)
		{
			mkParents(f);
			FileLock @lock = null;
			Properties p = new Properties();

			if (!Windows && f.exists())
			{
				@lock = (new RandomAccessFile(f, "rw")).Channel.@lock();
			}

			try
			{
					using (System.IO.FileStream @is = new System.IO.FileStream(f, System.IO.FileMode.Open, System.IO.FileAccess.Read))
					{
					p.load(@is);
					if (p.getProperty("BOOT_CLOJURE_VERSION") == null || p.getProperty("BOOT_VERSION") == null)
					{
						throw new Exception("missing info");
					}
					return p;
					}
			}
			catch (Exception)
			{
				if (!create)
				{
					return null;
				}
				else
				{
					return writeProps(f);
				}
			}
			finally
			{
				if (@lock != null)
				{
					@lock.release();
				}
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static java.util.HashMap<String, File[]> seedCache() throws Exception
		private static Dictionary<string, File[]> seedCache()
		{
			if (depsCache != null)
			{
				return depsCache;
			}
			else
			{
				ClojureRuntimeShim a = aetherShim();

				Dictionary<string, File[]> cache = new Dictionary<string, File[]>();

				cache["boot/pod"] = resolveDepJars(a, "boot/pod");
				cache["boot/core"] = resolveDepJars(a, "boot/core");
				cache["boot/worker"] = resolveDepJars(a, "boot/worker");

				return depsCache = cache;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static Object validateCache(File f, Object cache) throws Exception
		private static object validateCache(File f, object cache)
		{
			foreach (File[] fs in ((Dictionary<string, File[]>) cache).Values)
			{
				foreach (File d in fs)
				{
					if (!d.exists() || f.lastModified() < d.lastModified())
					{
						throw new Exception("dep jar doesn't exist");
					}
				}
			}
			return cache;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static Object writeCache(File f, Object m) throws Exception
		private static object writeCache(File f, object m)
		{
			mkParents(f);
			using (System.IO.FileStream os = new System.IO.FileStream(f, System.IO.FileMode.Create, System.IO.FileAccess.Write), ObjectOutputStream oos = new ObjectOutputStream(os))
			{
				oos.writeObject(m);
			}
			return m;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static Object readCache(File f) throws Exception
		private static object readCache(File f)
		{
			mkParents(f);
			FileLock @lock = null;
			if (!Windows)
			{
				@lock = (new RandomAccessFile(f, "rw")).Channel.@lock();
			}
			try
			{
				long max = 18 * 60 * 60 * 1000;
				long age = DateTimeHelperClass.CurrentUnixTimeMillis() - f.lastModified();
				if (age > max)
				{
					throw new Exception("cache age exceeds TTL");
				}
				using (System.IO.FileStream @is = new System.IO.FileStream(f, System.IO.FileMode.Open, System.IO.FileAccess.Read), ObjectInputStream ois = new ObjectInputStream(@is))
				{
					return validateCache(f, ois.readObject());
				}
			}
			catch (Exception)
			{
				return writeCache(f, seedCache());
			}
			finally
			{
				if (@lock != null)
				{
					@lock.release();
				}
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static org.projectodd.shimdandy.ClojureRuntimeShim newShim(String name, Object data, File[] jarFiles) throws Exception
		public static ClojureRuntimeShim newShim(string name, object data, File[] jarFiles)
		{
			URL[] urls = new URL[jarFiles.Length];

			for (int i = 0; i < jarFiles.Length; i++)
			{
				urls[i] = jarFiles[i].toURI().toURL();
			}

			ClassLoader cl = new URLClassLoader(urls, typeof(App).ClassLoader);
			ClojureRuntimeShim rt = ClojureRuntimeShim.newRuntime(cl);

			rt.Name = !string.ReferenceEquals(name, null) ? name : "anonymous";

			File[] hooks = new File[]
			{
				new File(bootdir(), "boot-shim.clj"),
				new File("boot-shim.clj")
			};

			foreach (File hook in hooks)
			{
			  if (hook.exists())
			  {
				rt.invoke("clojure.core/load-file", hook.Path);
			  }
			}

			rt.require("boot.pod");
			rt.invoke("boot.pod/seal-app-classloader");
			rt.invoke("boot.pod/set-data!", data);
			rt.invoke("boot.pod/set-pods!", pods);
			rt.invoke("boot.pod/set-this-pod!", new WeakReference<ClojureRuntimeShim>(rt));

			pods.put(rt, new object());
			return rt;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static org.projectodd.shimdandy.ClojureRuntimeShim newPod(String name, Object data) throws Exception
		public static ClojureRuntimeShim newPod(string name, object data)
		{
			return newShim(name, data, podjars);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static org.projectodd.shimdandy.ClojureRuntimeShim newPod(String name, Object data, File[] jarFiles) throws Exception
		public static ClojureRuntimeShim newPod(string name, object data, File[] jarFiles)
		{
			File[] files = new File[jarFiles.Length + podjars.Length];

			for (int i = 0; i < podjars.Length; i++)
			{
				files[i] = podjars[i];
			}
			for (int i = 0; i < jarFiles.Length; i++)
			{
				files[i + podjars.Length] = jarFiles[i];
			}

			return newShim(name, data, files);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static org.projectodd.shimdandy.ClojureRuntimeShim aetherShim() throws Exception
		private static ClojureRuntimeShim aetherShim()
		{
			if (aethershim == null)
			{
				ensureResourceFile(aetherjar, aetherfile);
				aethershim = newShim("aether", null, new File[] {aetherfile});
			}
			return aethershim;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void extractResource(String resource, File outfile) throws Exception
		public static void extractResource(string resource, File outfile)
		{
			mkParents(outfile);
			int n = 0;
			sbyte[] buf = new sbyte[4096];
			using (System.IO.Stream @in = resource(resource), System.IO.Stream @out = new System.IO.FileStream(outfile, System.IO.FileMode.Create, System.IO.FileAccess.Write))
			{
				while ((n = @in.read(buf)) > 0)
				{
					@out.write(buf, 0, n);
				}
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void ensureResourceFile(String r, File f) throws Exception
		public static void ensureResourceFile(string r, File f)
		{
			if (!f.exists())
			{
				extractResource(r, f);
			}
		}

		public static File[] resolveDepJars(ClojureRuntimeShim shim, string sym)
		{
			return resolveDepJars(shim, sym, bootversion, cljname, cljversion);
		}

		public static File[] resolveDepJars(ClojureRuntimeShim shim, string sym, string bootversion, string cljname, string cljversion)
		{
			shim.require("boot.aether");
			if (!string.ReferenceEquals(localrepo, null))
			{
				shim.invoke("boot.aether/set-local-repo!", localrepo);
			}
			if (update_always)
			{
				shim.invoke("boot.aether/update-always!");
			}
			return (File[]) shim.invoke("boot.aether/resolve-dependency-jars", sym, bootversion, cljname, cljversion);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.util.concurrent.Future<org.projectodd.shimdandy.ClojureRuntimeShim> newShimFuture(final String name, final Object data, final File[] jars) throws Exception
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
		public static Future<ClojureRuntimeShim> newShimFuture(string name, object data, File[] jars)
		{
			return ex.submit(new CallableAnonymousInnerClass(name, data, jars));
		}

		private class CallableAnonymousInnerClass : Callable
		{
			private string name;
			private object data;
			private File[] jars;

			public CallableAnonymousInnerClass(string name, object data, File[] jars)
			{
				this.name = name;
				this.data = data;
				this.jars = jars;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.projectodd.shimdandy.ClojureRuntimeShim call() throws Exception
			public virtual ClojureRuntimeShim call()
			{
				return newShim(name, data, jars);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.util.concurrent.Future<org.projectodd.shimdandy.ClojureRuntimeShim> newCore(Object data) throws Exception
		public static Future<ClojureRuntimeShim> newCore(object data)
		{
			return newShimFuture("core", data, corejars);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.util.concurrent.Future<org.projectodd.shimdandy.ClojureRuntimeShim> newWorker() throws Exception
		public static Future<ClojureRuntimeShim> newWorker()
		{
			return newShimFuture("worker", null, workerjars);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static int runBoot(java.util.concurrent.Future<org.projectodd.shimdandy.ClojureRuntimeShim> core, java.util.concurrent.Future<org.projectodd.shimdandy.ClojureRuntimeShim> worker, String[] args) throws Exception
		public static int runBoot(Future<ClojureRuntimeShim> core, Future<ClojureRuntimeShim> worker, string[] args)
		{
			ConcurrentLinkedQueue<Runnable> hooks = new ConcurrentLinkedQueue<Runnable>();
			try
			{
				if (!string.ReferenceEquals(localrepo, null))
				{
					worker.get().require("boot.aether");
					worker.get().invoke("boot.aether/set-local-repo!", localrepo);
				}
				core.get().require("boot.main");
				core.get().invoke("boot.main/-main", nextId(), worker.get(), hooks, args);
				return -1;
			}
			catch (Exception t)
			{
				return (t is Exit) ? int.Parse(t.Message) : -2;
			}
			finally
			{
				foreach (Runnable h in hooks)
				{
					h.run();
				}
				try
				{
					core.get().close();
				}
				catch (InterruptedException)
				{
				}
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static String readVersion() throws Exception
		public static string readVersion()
		{
			Properties p = new Properties();
			using (System.IO.Stream @in = resource("boot/base/version.properties"))
			{
				p.load(@in);
			}
			return p.getProperty("version");
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void printVersion() throws Exception
		public static void printVersion()
		{
			Properties p = new Properties();
			p.setProperty("BOOT_VERSION", config("BOOT_VERSION"));
			p.setProperty("BOOT_CLOJURE_NAME", config("BOOT_CLOJURE_NAME"));
			p.setProperty("BOOT_CLOJURE_VERSION", config("BOOT_CLOJURE_VERSION"));
			p.store(System.out, booturl);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void updateBoot(File bootprops, String version, String chan) throws Exception
		public static void updateBoot(File bootprops, string version, string chan)
		{
			update_always = true;
			bootversion = version;
			channel = chan;
			Properties p = writeProps(bootprops);
			p.store(System.out, booturl);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void main(String[] args) throws Exception
		public static void Main(string[] args)
		{
			if (System.getProperty("user.name").Equals("root") && !config("BOOT_AS_ROOT", "no").Equals("yes"))
			{
				throw new Exception("refusing to run as root (set BOOT_AS_ROOT=yes to force)");
			}

			// BOOT_VERSION is decided by the loader; it will respect the
			// boot.properties files, env vars, system properties, etc.
			// or it will use the latest installed version.
			//
			// Since 2.4.0 we can assume that bootversion and appversion
			// are the same (or boot.main will throw an exception).
			bootversion = appversion = readVersion();

			File cachehome = mkFile(bootdir(), "cache");
			File bootprops = mkFile(bootdir(), "boot.properties");
			File jardir = mkFile(cachehome, "lib", appversion);
			File bootcache = mkFile(cachehome, "cache", "boot");

			localrepo = config("BOOT_LOCAL_REPO");
			cljversion = config("BOOT_CLOJURE_VERSION", "1.8.0");
			cljname = config("BOOT_CLOJURE_NAME", "org.clojure/clojure");
			aetherfile = mkFile(cachehome, "lib", appversion, aetherjar);

			readProps(bootprops, true);

			if (args.Length > 0 && ((args[0]).Equals("-u") || (args[0]).Equals("--update")))
			{
				updateBoot(bootprops, (args.Length > 1) ? args[1] : null, "RELEASE");
				Environment.Exit(0);
			}

			if (args.Length > 0 && ((args[0]).Equals("-U") || (args[0]).Equals("--update-snapshot")))
			{
				updateBoot(bootprops, null, "(0,)");
				Environment.Exit(0);
			}

			if (args.Length > 0 && ((args[0]).Equals("-V") || (args[0]).Equals("--version")))
			{
				printVersion();
				Environment.Exit(0);
			}

			string repo = (string.ReferenceEquals(localrepo, null)) ? "default" : md5hash((new File(localrepo)).CanonicalFile.Path);

			File cachefile = mkFile(bootcache, repo, cljversion, bootversion, "deps.cache");
			Dictionary<string, File[]> cache = (Dictionary<string, File[]>) readCache(cachefile);

			podjars = cache["boot/pod"];
			corejars = cache["boot/core"];
			workerjars = cache["boot/worker"];

			Thread shutdown = new ThreadAnonymousInnerClass();
			Runtime.Runtime.addShutdownHook(shutdown);
			Environment.Exit(runBoot(newCore(null), newWorker(), args));
		}

		private class ThreadAnonymousInnerClass : System.Threading.Thread
		{
			public ThreadAnonymousInnerClass()
			{
			}

			public virtual void run()
			{
				ex.shutdown();
			}
		}
	}

}