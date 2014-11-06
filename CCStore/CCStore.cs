using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
// Added Load and Save methods
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace CC.Common.Utils
{
  // Might be considered lame, but I think it would be easier to use than
  // the KeyValuePair<object, object> object
  public class CCStoreItem
  {
    public String Name { get; set; }
    public Object Item { get; set; }
    public String Comment { get; set; }
    
    public CCStoreItem(string name, object item, string comment)
    {
      Name = name;
      Item = item;
      Comment = comment;
    }
  }

  //[Serializable]
  //internal class CCStoreSerializationHelper : IObjectReference
  //{
  //  public object GetRealObject(StreamingContext context)
  //  {
  //    return CCStore.Instance();
  //  }
  //}

  /// <summary>
  /// I hate "global" variables. They are usually used to keep track of state. Elsewhere in the program you need
  /// or would like to check on that state. You either then change the global to public and pass the owner object
  /// around and each underlying class needs to know about that object (breaking OO encapuslations), or you pass 
  /// the global variable around and make sure to update it if the value changed for some reason.
  /// Why do this?
  /// CCStore - a singleton object that can be used everywhere
  /// No more passing objects around - this is static
  /// No more methods with tons of parameters, you won't need any anymore
  /// No keeping track of state: Read the value, update it if needed
  /// <code>
  /// int i = CCStore.Instance().Get("MyVariable", 0);
  /// // use i
  /// // increment it
  /// CCStore.Instance().Store("MyVariable", ++i);
  /// </code>
  /// If it's in storage, return that value if not return 0
  /// Simple
  /// 
  /// I'm not sure how thread-safe it is, there is a little check, but not tested
  /// </summary>
  [Serializable]
  public sealed class CCStore: IEnumerable, ISerializable
  {
    //private static CCStore instance = null;
    private static CCStore instance = new CCStore();
    private static Dictionary<string, object> _items;
    private static Dictionary<string, string> _comments;

    private readonly object sync = new object();

    private CCStore()
    {
      _items = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
      _comments = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    private CCStore(SerializationInfo info, StreamingContext context)
    {
      //info.GetType(typeof(Dictionary<string, object>));
      //info.GetType(typeof(Dictionary<string, string>));
      //instance = new CCStore();
      _items = (Dictionary<string, object>)info.GetValue("_items", typeof(Dictionary<string, object>));
      _comments = (Dictionary<string, string>)info.GetValue("_comments", typeof(Dictionary<string, string>));
    }

    //public static CCStore Instance()
    //{
    //  if (instance == null)
    //  {
    //    instance = new CCStore();
    //  }
    //  return instance;
    //}
    // Accessor and not a method - preference?
    public static CCStore Instance
    {
      get { return instance; }
    }

    // TODO - make comment an object so it can be used like so
    // store.Comment["Key"] = "Value"
    // comment = store.Comment["Key"]
    public String this[string name]
    {
      get { return _comments[name]; }
      set { _comments[name] = value; }
    }

    #region Internal Wrappers
    private void _store(string name, object item)
    {
      lock (sync)
      {
        _items[name] = item;
        _comments[name] = "";
      }
    }
    private object _get(string name, object defaultValue)
    {
      lock (sync)
      {
        if (_items.ContainsKey(name))
          return _items[name];
        else
          return defaultValue;
      }
    }
    #endregion
    
    #region Objects
    public void Store(string name, object item)
    {
      _store(name, item);
    }
    public object Get(string name, object defaultValue)
    {
      return _get(name, defaultValue);
    }
    #endregion

    #region Integers
    public void Store(string name, int item)
    {
      //_items[name] = item;
      _store(name, item);
    }
    public int Get(string name, int defaultValue)
    {
      int ret = defaultValue;
      try
      {
        ret = (int)_get(name, defaultValue);
      }
      catch { }
      return ret;
    }
    #endregion

    #region Strings
    public void Store(string name, string item)
    {
      //_items[name] = item;
      _store(name, item);
    }
    public string Get(string name, string defaultValue)
    {
      string ret = defaultValue;
      try
      {
        ret = (string)_get(name, defaultValue).ToString(); // <<<---- May want to remove this
      }
      catch { }
      return ret;
    }
    #endregion

    #region Floats
    public void Store(string name, double item)
    {
      //_items[name] = item;
      _store(name, item);
    }
    public double Get(string name, double defaultValue)
    {
      double ret = defaultValue;
      try
      {
        ret = (double)_get(name, defaultValue);
      }
      catch { }
      return ret;
    }
    #endregion

    #region DateTimes
    public void Store(string name, DateTime item)
    {
      //_items[name] = item;
      _store(name, item);
    }
    public DateTime Get(string name, DateTime defaultValue)
    {
      DateTime ret = defaultValue;
      try
      {
        ret = (DateTime)_get(name, defaultValue);
      }
      catch { }
      return ret;
    }
    #endregion

    #region Booleans
    public void Store(string name, Boolean item)
    {
      _store(name, item);
    }
    public Boolean Get(string name, Boolean defaultValue)
    {
      Boolean ret = defaultValue;
      try
      {
        ret = (Boolean)_get(name, defaultValue);
      }
      catch { }
      return ret;
    }
    #endregion

    #region Methods
    public void Clear()
    {
      _items.Clear();
    }

    public Boolean Delete(string name)
    {
      return _items.Remove(name);
    }

    public IEnumerator GetEnumerator()
    {
      foreach (KeyValuePair<string, object> kvp in _items)
      {
        yield return new CCStoreItem(kvp.Key, kvp.Value, _comments[kvp.Key]);
      }
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("_items", _items, typeof(Dictionary<string, object>));
      info.AddValue("_comments", _comments, typeof(Dictionary<string, string>));
      //info.SetType(typeof(CCStoreSerializationHelper));
    }

    public void Save(string filename)
    {
      FileStream stream = null;
      try
      {
        IFormatter formatter = new BinaryFormatter();
        //IFormatter formatter = new XmlFormatter(typeof(CCStore));
        stream = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
        //formatter.Serialize(stream, CCStore.Instance());
        formatter.Serialize(stream, CCStore.Instance);
        
        // For compression...
        //stream.Position = 0; // reset?
        //FileStream outFile = new FileStream(filename + ".gz", FileMode.Create, FileAccess.Write, FileShare.None);
        //GZipStream compress = new GZipStream(outFile, CompressionMode.Compress);
        //byte[] buffer = new byte[4096];
        //int numRead;
        //while ((numRead = stream.Read(buffer, 0, buffer.Length)) != 0)
        //{
        //  compress.Write(buffer, 0, numRead);
        //}
        //compress.Close();
      }
      catch
      {
        // Do Nothing
      }
      finally
      {
        if (null != stream)
          stream.Close();
      }
    }

    public void Save(Stream stream)
    {
      
      IFormatter formatter = new BinaryFormatter();
      formatter.Serialize(stream, CCStore.Instance);
    }

    public Boolean Load(string filename)
    {
      Boolean ret = false;
      if (File.Exists(filename))
      {
        FileStream stream = null;
        try
        {
          IFormatter formatter = new BinaryFormatter();
          stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.None);
          //instance = null; // Force an update?
          instance = (CCStore)formatter.Deserialize(stream);
        }
        catch
        {
          // Do Nothing? toss the exception up - maybe?
        }
        finally
        {
          if (null != stream)
            stream.Close();
        }
        ret = true;
      }

      return ret;
    }
    
    public Boolean Load(Stream stream)
    {
      Boolean ret;
      try
      {
        IFormatter formatter = new BinaryFormatter();
        instance = (CCStore)formatter.Deserialize(stream);
        ret = true;
      }
      catch
      {
        ret = false;
      }
      return ret;
    }
    #endregion
  }
}
