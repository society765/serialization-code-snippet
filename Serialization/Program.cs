using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using System.Security.Permissions;
using System.Xml.Serialization;
using System.Xml.Schema;

using Newtonsoft.Json;

namespace Serialization
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello world");

            var tbs = new Perc();
            tbs.d.init();

            var yem1 = new Yem();
            yem1.a = 777;

            var yem2 = new Yem();
            yem2.a = 666;

            tbs.d.Add(yem1);
            tbs.d.Add(yem2);

            Perc readin; 

            DataContractSerializer ser = new DataContractSerializer(typeof(Perc));
            string output = JsonConvert.SerializeObject(tbs);

            using (FileStream fs = new FileStream(@".\a.xml", FileMode.Create)){
                XmlTextWriter writer = new XmlTextWriter(fs, Encoding.ASCII);
                writer.Formatting = System.Xml.Formatting.Indented;
                ser.WriteObject(writer, tbs);
                writer.Close(); 
            }

            using (FileStream fs = new FileStream(@".\a.xml", FileMode.Open))
            {
                XmlTextReader reader = new XmlTextReader(fs); 
                readin = (Perc)ser.ReadObject(reader, true);
            }

            Console.ReadLine(); 
        }
    }

    [DataContract]
    class Perc
    {
        [DataMember]
        public string a;
        [DataMember]
        public int b;
        [DataMember]
        public double c;
        [DataMember]
        public Dius d; 

        public Perc()
        {
            a = "123";
            b = 5;
            c = 3.142;
            d = new Dius(); 
        }
    }

    public class Dius : SortedSet<Yem>, IXmlSerializable
    {
        [DataMember]
        public string a;
        [DataMember]
        public Yem d; 

        public Dius() : base() { }
        public Dius(SortedSet<Yem> that) : base(that) { }

        public void init()
        {
            a = "aaa";
            d = new Yem();
            d.a = 5551;
        }

        [DataContract]
        private class Dius_Wrapper
        {
            [DataMember]
            public string a;
            [DataMember]
            public Yem d;
            [DataMember]
            public SortedSet<Yem> collection;
        }

        public XmlSchema GetSchema()
        {
            return null; 
        }

        public void ReadXml(XmlReader reader)
        {
            List<Type> knownTypes = new List<Type>() { typeof(Dius) };
            DataContractSerializer ser = new DataContractSerializer(typeof(Dius_Wrapper), knownTypes);
            reader.ReadString();
            Dius_Wrapper dw = (Dius_Wrapper)ser.ReadObject(reader, true);

            this.Clear();
            this.UnionWith(dw.collection);
            this.a = dw.a;
            this.d = dw.d; 
        }
        
        public void WriteXml(XmlWriter writer)
        {
            Dius_Wrapper dw = new Dius_Wrapper() { collection = new SortedSet<Yem>(this), a = this.a, d = this.d };
            List<Type> knownTypes = new List<Type>() { typeof(Dius) };
            DataContractSerializer ser = new DataContractSerializer(dw.GetType(), knownTypes);
            ser.WriteObject(writer, dw);
        }
    }


    [DataContract]
    public class Yem : IComparable
    {
        [DataMember]
        public int a { get; set; }

        public int CompareTo(object obj)
        {
            Yem that = obj as Yem; 
            return this.a.CompareTo(that.a);
        }
    }
}
