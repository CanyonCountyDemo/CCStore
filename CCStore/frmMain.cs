using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CC.Common.Utils;
using System.IO;
using System.Drawing.Text;

namespace Test
{
  public partial class frmMain : Form
  {
    // Why use "globals", stick it in the store
    //private int counter = 0;

    public frmMain()
    {
      CCStore s = CCStore.Instance;
      if (!s.Load("CCStore.Data.dll"))
      {
        s.Store("Name", "Ken Wilcox");
        s["Name"] = "Someone's name";
        s.Store("Age", DateTime.Now.AddYears(-1974).Year);
        s["Age"] = "Someone's Age";
        s.Store("Date", DateTime.Now);
        s.Store("Compare", (1 - 2 > 3));
        s.Store("Counter", 0);

        ArrayList al = new ArrayList();
        al.Add("One");
        al.Add("Two");
        al.Add("Three");

        s.Store("Array", al);
        s["Array"] = "My ArrayList test";

      }
      else
      {
        this.Size = new Size(s.Get("Width", 0), s.Get("Height", 0));
        this.StartPosition = FormStartPosition.Manual;
        this.Location = new Point(s.Get("Left", Left), s.Get("Top", Top));
        //this.Width = s.Get("Width", 0);
        //this.Height = s.Get("Height", 0);
      }


      //PrivateFontCollection pfc = new PrivateFontCollection();
      //pfc.AddFontFile(@"D:\Fonts\Foo.ttf");
      //pfc.AddFontFile(@"D:\Fonts\ELECTROH.TTF");
      //pfc.AddFontFile(@"D:\Fonts\Corv2.ttf");
      //pfc.AddFontFile(@"D:\Fonts\BIRTH OF A HERO.ttf");
      //this.Font = new Font(pfc.Families[0], 12, FontStyle.Regular);

      InitializeComponent();
      //MessageBox.Show(s["Array"]);
      PrintList();
      //timer1.Enabled = true;
    }

    private void PrintList()
    {
      listBox1.Items.Clear();
      foreach (CCStoreItem item in CCStore.Instance)
      {
        if (String.IsNullOrEmpty(item.Comment))
          listBox1.Items.Add(item.Name + "=" + item.Item);
        else
          listBox1.Items.Add(item.Name + "=" + item.Item + "  // " + item.Comment);
      }
    }

    private void button1_Click(object sender, EventArgs e)
    {
      CCStore store = CCStore.Instance;
      store.Store("Name", "John Doe");
      store["Name"] = "Name changed to John Doe";
      store.Store("Date", DateTime.Now);
      store["Date"] = "Date changed to DateTime.Now";

      store.Store("Store", store); // Store a copy - weeee!
      CCStore q = (CCStore)store.Get("Store", (object)null);
      PrintList();
      MessageBox.Show(q.Get("Name", "No Name"));
      if (store.Delete("Store"))
        MessageBox.Show("Deleted");

      MessageBox.Show(q.Get("Name", 0).ToString());
      PrintList();
    }

    private void timer1_Tick(object sender, EventArgs e)
    {
      //PrintList();
    }

    private void button2_Click(object sender, EventArgs e)
    {
      //counter++;
      CCStore store = CCStore.Instance;
      int counter = store.Get("Counter", 0);
      //counter++;
      // Increment BEFORE we store it, if not we won't go anywhere
      store.Store("Counter", ++counter);
      store.Store("Name" + counter.ToString(), "Jane Doe");
      PrintList();
    }

    private void button3_Click(object sender, EventArgs e)
    {
      CCStore store = CCStore.Instance;
      int counter = store.Get("Counter", 0);
      MessageBox.Show("Name = " + store.Get("Name", "Not Found"));
      MessageBox.Show("Name"+counter.ToString()+" = " + store.Get("Name" + counter.ToString(), "Not Found"));

      //ArrayList al = (ArrayList)CCStore.Instance().Get("Array", (object)null); // force a null object and not a null string
      // Never get a null error
      ArrayList al = (ArrayList)CCStore.Instance.Get("Array", new ArrayList()); // always return a valid object
      // But you still need to check the length/count/etc
      if (al.Count > 0)
      {
        MessageBox.Show("ArrayList[0] = " + al[0].ToString());
      }
      al = null;
    }

    private void button4_Click(object sender, EventArgs e)
    {
      string name = "Name";
      string item = "Item";
      InputBoxResult res = InputBox.Show("Enter a Key Name", "Input", name);
      if (res.OK)
      {
        name = res.Text;
        res = InputBox.Show("Enter a Value", "Input", item);
        if (res.OK)
        {
          item = res.Text;

          CCStore.Instance.Store(name, item);
          PrintList();
        }
      }
    }

    private void button5_Click(object sender, EventArgs e)
    {
      InputBoxResult res = InputBox.Show("What value would you like?", "Input", "Name");
      if (res.OK)
      {
        MessageBox.Show(res.Text + " = " + CCStore.Instance.Get(res.Text, "Sorry, Not Found or Usable in this Context"));
      }
    }

    private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
    {
      CCStore.Instance.Save("CCStore.Data.dll");
      //MemoryStream ms = new MemoryStream();
      //CCStore.Instance.Save(ms);
      //FileStream fs = new FileStream("C:\\DUMMY.INFO", FileMode.Create);
      //ms.WriteTo(fs);
    }

    private void listBox1_DoubleClick(object sender, EventArgs e)
    {
      // Load/Save to stream test
      //MemoryStream stream = new MemoryStream();
      //CCStore.Instance.Save(stream);
      //stream.Position = 0;
      //CCStore.Instance.Clear();
      //PrintList();
      //MessageBox.Show("All Clear");
      //CCStore.Instance.Load(stream);
      //PrintList();
    }

    private void frmMain_Move(object sender, EventArgs e)
    {
      CCStore s = CCStore.Instance;
      s.Store("Top", Top);
      s.Store("Left", Left);
      s.Store("Width", Width);
      s.Store("Height", Height);
      PrintList();
    }
  }
}
