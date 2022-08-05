# UdonXML with Async Loading

This prefab package is a fork of the original [UdonXML by Foorack](https://github.com/Foorack/UdonXML).

UdonXML is an XML parser written in Udon for VRChat.
The purpose of this project is for it to be used as an API in other bigger projects.
This work is something the average VRChatter never will notice, but something the author hopes might be beneficial and make the life easier for world/game creators in VRChat.

An example use of this library is allowing the player to paste the entire contents of an XML file (e.g. game save file) into an Input field in VRC, and allowing the world to then parse the submitted data.

This version is designed for handling larger datasets that can't be loaded in one frame without causing lag such as small databases.

## 🛠️ Setup

### Requirements

* Unity 2019.4.31f1
* VRCSDK3
* Latest [UdonSharp](https://github.com/Merlin-san/UdonSharp/blob/master/README.md)

### Installation

1. Go to [Releases](https://github.com/Nidonocu/UdonXML/releases) and download the latest release. The file should end with `unitypackage`. If you can't find it, ask for help on Discord.
2. Import the package into Unity with Assets > Import Package > Custom Package. This will import the files to `Assets > UdonXML`.
3. Drag the `UdonXML` Prefab (blue box icon) in to the Scene.

## ⭐ Example Scene

Inside the `UdonXML` folder is a folder called `Demo`.

Double-click the `Udon Demo Scene` file to view an example using a sample of the *Northwind* Example Database. You can view the full database string that is being loaded in the `Northwind.xml` file located in the Demo folder. 

❗ **Note** - You can not load directly from an XML file, you must copy the data into the Unity Scene as shown on the *DemoController* object.

Use *CyanEmu*/*ClientEmu* or run a 'Build and Test' to view the demo, pressing the `Start` button will begin loading the XML data. Once complete, the number of records for each data type will be displayed.

⚠ **Important Note** - If testing in editor using *CyanEmu*/*ClientEmu*, do not have the *DemoController* object selected when pressing Play. The Unity editor will encounter serious lag as the large XML string will make the Inspector update very slowly.

## ✅ Getting started

### Single frame load

This code will parse and load an XML string in one execution frame. For large files this will freeze the game and if it takes longer than 10 seconds, will be halted by the client because it will be considered crashed.

1. Declare a `public UdonXML UdonXml;` field in your Udon Sharp program.
2. Assign it the value of the UdonXML GameObject in your scene. 
3. Parse your XML data with LoadXml `UdonXml.LoadXml(inputData);`. It will return an object.
4. The object returned represents the root of the xml tree, use it when executing other nodes such as `GetNodeName` or `HasChildNodes`.

### Example demo

```csharp
using UnityEngine;
using UdonSharp;

public class UdonXMLTest : UdonSharpBehaviour
{
    public UdonXML UdonXml;

    private string EXAMPLE_DATA = @"<?xml version=""1.0"" encoding=""utf-8""?>  
<books xmlns=""http://www.contoso.com/books"">  
    <book genre=""novel"" ISBN=""1-861001-57-8"" publicationdate=""1823-01-28"">  
        <title>Pride And Prejudice</title>  
        <price>24.95</price>  
    </book>
    <book genre=""novel"" ISBN=""1-861002-30-1"" publicationdate=""1985-01-01"">  
        <title>The Handmaid's Tale</title>  
        <price>29.95</price>  
    </book>  
    <book genre=""novel"" ISBN=""1-861001-45-3"" publicationdate=""1811-01-01"">  
        <title>Sense and Sensibility</title>  
        <price>19.95</price>  
    </book>  
</books>";

    public void Start()
    {
        // Parse and store the root node
        var root = UdonXml.LoadXml(EXAMPLE_DATA);

        // Fetch the first <books> node by index
        var books = UdonXml.GetChildNode(root, 1); // Index 0 will be <?xml> tag

        // Loop over all children in the <books> node
        for (var bookNum = 0; bookNum != UdonXml.GetChildNodesCount(books); bookNum++)
        {
            var book = UdonXml.GetChildNode(books, bookNum);
            // Fetch <title> and <price> nodes by tag name.
            var title = UdonXml.GetChildNodeByName(book, "title");
            var price = UdonXml.GetChildNodeByName(book, "price");

            Debug.Log("title: " + UdonXml.GetNodeValue(title) + " price: " + UdonXml.GetNodeValue(price));
        }
    }
}
```

![console output](https://i.imgur.com/g0e3ooO.png)

### Async Loading

This method will spread the loading across multiple frames, reading just a selection of characters at a time before letting the game loop continue.

1. Declare a `public UdonXML UdonXml;` field in your Udon Sharp program.
2. Declare two callback methods in your Udon Sharp program, one for progress updates and one for when the load is complete.
3. Assign the UdonXML GameObject to the `UdonXml` field in your scene.
4. Optionally, adjust the `Parse Limit` property on the UdonXml object to set how many characters are read each frame. More characters will result in faster loads but reduced framerate.
5. Optionally, adjust the `Progress Milestone Size` property on the *Async Runner* object that is a child of the UdonXml object. This will set how much progress (in percent) must be made before a status update is sent. Smaller (more frequent) updates may slightly slow down the loading process.
6. Parse your XML data with LoadXml `UdonXml.LoadXml(inputData);`. It will return an object.

```csharp
using UnityEngine;
using UdonSharp;

public class UdonXMLTest : UdonSharpBehaviour
{
    public UdonXML UdonXml;

    private string EXAMPLE_DATA = @"<?xml version=""1.0"" encoding=""utf-8""?>  
<books xmlns=""http://www.contoso.com/books"">  
    <book genre=""novel"" ISBN=""1-861001-57-8"" publicationdate=""1823-01-28"">  
        <title>Pride And Prejudice</title>  
        <price>24.95</price>  
    </book>
    <book genre=""novel"" ISBN=""1-861002-30-1"" publicationdate=""1985-01-01"">  
        <title>The Handmaid's Tale</title>  
        <price>29.95</price>  
    </book>  
    <book genre=""novel"" ISBN=""1-861001-45-3"" publicationdate=""1811-01-01"">  
        <title>Sense and Sensibility</title>  
        <price>19.95</price>  
    </book>  
</books>";

    public void Start()
    {
        // Begin the loading process
        var check = UdonXML._LoadXmlAsync(
            EXAMPLE_DATA, 
            this, 
            "_ReviewData", 
            "_PrintProgress");
        // Only one Async can run at once, if false, we couldn't start a second one
        if (!check)
        {
            Debug.LogError("Could not load data - XML operation already in progress");
            return;
        }
    }

    public void _PrintProgress()
    {
        // Update a progress bar or other UI here
        Debug.Log(UdonXML.FetchAsyncProgress().ToString("N1") + "% complete");
    }

    public void _ReviewData()
    {
        // Fetch the now loaded root node
        var root = UdonXML.FetchAsyncResult();

        // Fetch the first <books> node by index
        var books = UdonXml.GetChildNode(root, 1); // Index 0 will be <?xml> tag

        // Loop over all children in the <books> node
        for (var bookNum = 0; bookNum != UdonXml.GetChildNodesCount(books); bookNum++)
        {
            var book = UdonXml.GetChildNode(books, bookNum);
            // Fetch <title> and <price> nodes by tag name.
            var title = UdonXml.GetChildNodeByName(book, "title");
            var price = UdonXml.GetChildNodeByName(book, "price");

            Debug.Log("title: " + UdonXml.GetNodeValue(title) + " price: " + UdonXml.GetNodeValue(price));
        }
    }
}
```

## 📄 Documentation

### Loading data

#### 🔴 object LoadXml(string input)
Loads an XML structure into memory by parsing the provided input.

Returns null in case of parse failure.

#### 🔵 bool _LoadXmlAsync(string input, UdonSharpBehaviour callbackBehaviour, string callbackFunctionName, string progressCallbackFunctionName)
Queues the loading of an XML structure in the asynchronously.

`input` is the provided XML data.

The `callbackBehaviour` is the Udon Sharp program that contains the functions that should be called.

`callbackFunctionName` is the name of the function that will be called when loading is complete. This function must be `public`. 

`progressCallbackFunctionName` is the name of the function that will be called at various progression points (default is every 5%) as loading is made. This function must be `public`. 

This function will return true if loading has been queued successfully and false on failure.

#### 🎈 float FetchAsyncProgress()
Returns the progress percentage of the current loading operation.

#### 🔴 object[] FetchAsyncResult()
Fetches the completed XML data object.

Call only from the callback function once data has finished loading.

### Saving data

#### 💬 string SaveXml(object data)
Saves the stored XML structure in memory to an XML document.

Uses default indent of 4 spaces. Use `SaveXmlWithIdent` to override.

#### 💬 string SaveXmlWithIdent(object data, string indent)
Saves the stored XML structure in memory to an XML document with given indentation.


### Reading data

#### 🔵 bool HasChildNodes(object data)
Returns true if the node has child nodes.

#### 7️⃣ int GetChildNodesCount(object data)
Returns the number of children the current node has.

#### 🔴 object GetChildNode(object data, int index)
Returns the child node by the given index.

#### 🔴 object GetChildNodeByName(object data, string nodeName)
Returns the child node by the given name.

If multiple nodes exists with the same type-name then the first one will be returned.

#### 💬 string GetNodeName(object data)
Returns the type-name of the node.

#### 💬 string GetNodeValue(object data)
Returns the value of the node.

#### 🔵 bool HasAttribute(object data, string attrName)
Returns whether the node has a given attribute or not.

#### 💬 string GetAttribute(object data, string attrName)
Returns the value of the attribute by given name.


### Writing data

#### 🔴 object CreateChildNode(object data, string nodeName)
Creates a new child node under the current given node.

Returns the newly created node.

#### 🔴 object RemoveChildNode(object data, int index)
Removes a child node from the current node by given index.

Returns the deleted node.

#### 🔴 object RemoveChildNode(object data, string nodeName)
Removes a child node from the current node by given name.
If multiple nodes exist with the same name then only the first one is deleted.

Returns the deleted node, or null if not found.

#### ⚫ void SetNodeName(object data, string newName)
Sets the name of the node.

#### ⚫ void SetNodeValue(object data, string newValue)
Sets the value of the node.

#### ⚫ void SetAttribute(object data, string attrName, string newValue)
Sets the value of an attribute on given node.

Returns false if the attribute already existed, returns true if attribute was created.

#### 🔵 bool RemoveAttribute(object data, string attrName)
Removes an attribute by name in the given node.

Returns true if attribute was deleted, false if it did not exist.
