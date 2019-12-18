# Serilization in C#

## Motivation 

I have encounter some code that inherits `SortedSet<T>`, for example 
```c#
public class Dius : SortedSet<int> 
{
	int a; 
	string b; 
}
```
I need to do serializations on this object. 
If you use C# *DataContract Serializer* or *XML Serializer*, 
you will find that only contents in *SortedSet\<int\>* will be serialized. 
Neither of the extra properties *a* or *b* will be serialized. 
This is because the class inherits a class from either `IEnumerable` 
or `ICollection`. 

See 
- https://stackoverflow.com/questions/666054/c-sharp-inheriting-generic-collection-and-serialization 
- https://stackoverflow.com/questions/1797947/xmlserializer-doesnt-serialize-everything-in-my-class
- https://stackoverflow.com/questions/55301839/serializing-class-inherited-from-list-using-datacontractserializer-does-not-se
- https://stackoverflow.com/questions/5069099/when-a-class-is-inherited-from-list-xmlserializer-doesnt-serialize-other-att
- https://stackoverflow.com/questions/25103608/serialize-sorted-set-members
- https://stackoverflow.com/questions/377486/xmlserialize-a-custom-collection-with-an-attribute
- https://stackoverflow.com/questions/25251798/deserializing-xml-in-object-using-datacontractserializer
- https://docs.microsoft.com/en-us/dotnet/standard/serialization/introducing-xml-serialization#items-that-can-be-serialized
- and two more below

For JSON .NET 
- https://stackoverflow.com/questions/5863496/is-there-any-way-to-json-net-serialize-a-subclass-of-listt-that-also-has-extra


Basically, in most of these answers, the alternative is to redefine the class like this
```c# 
public class Dius 
{
	int a; 
	string b; 
	SortedSet<int> collection; 
}
```
However, I CANNOT redefine the class definition in this problem. 
So I found another workaround, see 
- https://stackoverflow.com/questions/3431843/serialization-of-class-derived-from-list-using-datacontract/3432106#3432106
- https://stackoverflow.com/questions/1285018/datacontractserializer-not-serializing-member-of-class-that-inherits-iserializab

The idea is that, for *DataContract Serializer*, it will use customized `IXmlSerializable` 
interface methods to serialize the object. 

This time, we define a `Dius_Wrapper` class that looks like the second definition above, 
and let the original class `Dius` implements the interface `IXmlSerializable`. 
The interface methods and the wrapper class will look like this 
```c# 
// All the code below is within class Dius, which implements IXmlSerializable
[DataContract]
private class Dius_Wrapper
{
    [DataMember]
    public int a;
    [DataMember]
    public string b;
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
    DataContractSerializer ser = 
		new DataContractSerializer(typeof(Dius_Wrapper), knownTypes);
    reader.ReadString();
    Dius_Wrapper dw = (Dius_Wrapper)ser.ReadObject(reader, true);

    this.Clear();
    this.UnionWith(dw.collection);
    this.a = dw.a;
    this.b = dw.b;
}

public void WriteXml(XmlWriter writer)
{
    Dius_Wrapper dw = new Dius_Wrapper() 
	{ 
		collection = new SortedSet<Yem>(this), 
		a = this.a, 
		b = this.b 
	};
    List<Type> knownTypes = new List<Type>() { typeof(Dius) };
    DataContractSerializer ser = 
		new DataContractSerializer(typeof(Dius_Wrapper), knownTypes);
    ser.WriteObject(writer, dw);
}
```

This may not be the best solution, but it solves the problem without significant 
changes to the original code. 
You may download the code and run it yourself. 
Although I wrote this code for `SortedSet<>`, 
it should also work with other `IEnumerable`, such as `List<>`, hopefully. 

## License 

MIT. Use at your own risk. 
