using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Linq;

namespace URMStores
{
	/// <summary>
	/// Welcome to XMLManager.
	/// XMLManager is used for reading files in an XML format with the .xml extension.
	/// URM Stores, Inc. 4/24/2017 - Configuration Management
	/// 
	/// This class uses XmlDocument rather than LINQ to XML XDocument. While XDocument is the recommended option
	/// for reading and writing XML data, XPath was simpler in this case. XDocument may be added at a later date.
	/// 
	/// Use the ErrorMsg variable of this class to access the friendly message of any errors encountered. 
	/// If a method's return is not 0, true, or the intended value, ErrorMsg can be used to display the cause and help troubleshoot the issue.
	/// </summary>
	class XMLManager
	{
		private const string CLASS_VERSION = "1.0";

		//***Declarations***\\
		private XmlDocument Xml		 = new XmlDocument(); //Loads and holds the XML document.
		private string		XmlName  = String.Empty;	  //Holds the name/path of the XML document.
		private int			ValErrs	 = 0;				  //Holds the total number of validation errors found.

		/// <summary>
		/// Holds the friendly error message for any error encountered.
		/// </summary>
		public  string		ErrorMsg = String.Empty;

		//***Constructor***\\
		/// <summary>
		/// XMLManager is used for reading XML documents.
		/// </summary>
		/// <param name="name">The name and path to the XML document.</param>
		public XMLManager(string name)
		{
			//Sets the name/path of the XML document
			//
			this.XmlName = name;
		}

		//***Methods***\\
		/// <summary>
		/// Loads the XML document into memory.
		/// </summary>
		/// <returns></returns>
		public bool LoadXMLDoc()
		{
			//Load the named XML document into memory.
			//
			try
			{
				this.Xml.Load(this.XmlName);
				return (true);
			}
			catch (Exception err)
			{
				ErrorMsg = String.Format("Error loading XML document '{0}' into memory: {1}", this.XmlName, err.Message);
				return (false);
			}
		}

		/// <summary>
		/// Validates the XML document against an in line XSD schema. Also determines if XML is "Well Formed." XML file is not loaded into memory.
		/// </summary>
		/// <returns></returns>
		public bool ValidateXml()
		{
			XmlReaderSettings xmlConfig = null; //Holds the settings for the XSD validation.
			XmlReader		  reader	= null; //Object that parses the XML document.
			

			//Due to the requirment of a call back method with the possibility of multiple errors, ErrorMsg and ValErrs are cleared before validation.
			//
			this.ErrorMsg = String.Empty;
			this.ValErrs = 0;

			//Initialize settings and XML document.
			//
			try
			{
				xmlConfig = new XmlReaderSettings();

				//Validation is set to schema.
				//
				xmlConfig.ValidationType = ValidationType.Schema;

				//Validation warnings are set to be reported.
				//
				xmlConfig.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;

				//Process schemas found inside the XML document.
				//
				xmlConfig.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;

				//Processes xsi:schemaLocation or xsi:noNamespaceSchemaLocation.
				//
				xmlConfig.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;

				//Sets event handler for validation errors to instantiated call back method.
				//
				xmlConfig.ValidationEventHandler += new ValidationEventHandler(ValidateXmlCallback);

				//Creates the XML document reader with specified settings.
				//
				reader = XmlReader.Create(this.XmlName, xmlConfig);
			}
			catch (Exception err)
			{
				ErrorMsg = String.Format("Error initializing XML validation settings: {0}", err.Message);
				return (false);
			}

			//Errors caught here will determine of the XML document is 'Well Formed.'
			//
			try
			{
				//Parse through the XML document.
				//
				while (reader.Read()) ;

				//Check if any validation errors were found.
				//
				if (ValErrs > 0)
				{
					return (false);
				}

				return (true);
			}
			catch (Exception err)
			{
				ErrorMsg = String.Format("Error parsing XML document '{0}': {1}", this.XmlName, err.Message);
				return (false);
			}

		}

		/// <summary>
		/// Returns the text content of an XML element.
		/// </summary>
		/// <param name="xpath">An xpath to the XML node.</param>
		/// <returns></returns>
		public string GetElement(string xpath)
		{
			XmlNode element = null;			//Holds the element from the xpath.
			string	text	= String.Empty; //Holds the value from the element.

			try
			{
				//Initialize element based on xpath.
				//
				element = this.Xml.SelectSingleNode(xpath);

			}
			catch (Exception err)
			{
				ErrorMsg = String.Format("Error, could not initialize element '{0}': {1}", xpath, err.Message);
				return (null);
			}

			try
			{
				//Return its text content.
				//
				text = element.InnerText;

				return (text);

			}
			catch (Exception err)
			{
				ErrorMsg = String.Format("Error, could not get inner text of element '{0}': {1}", xpath, err.Message);
				return (null);
			}
		}

		/// <summary>
		/// Returns an array of the text content of multiple XML elements as they appear in the XML document.
		/// </summary>
		/// <param name="xpath">An xpath to the XML nodes.</param>
		/// <returns></returns>
		public string[] GetElementList(string xpath)
		{
			XmlNodeList elementList = null; //Holds all the elements found at the specified xpath.
			XmlNode		element		= null; //Holds a single element extracted from the elements found.
			string[]    textContent = null; //Holds all the text content found in the elements.

			//Initialize elements at xpath and array at size of elements found.
			//
			try
			{
				elementList = this.Xml.SelectNodes(xpath);
				textContent = new string[elementList.Count];
			}
			catch (Exception err)
			{
				ErrorMsg = String.Format("Error, could not initialize elements at '{0}': {1}", xpath, err.Message);
				return (null);
			}

			//Extract each element's inner text.
			//
			try
			{
				//Run loop for each element found.
				//
				for (int i = 0; i < elementList.Count; i++)
				{
					//Get the specific element.
					//
					element = elementList.Item(i);
					
					//Set the value in a parallel array.
					//
					textContent[i] = element.InnerText;

				}

				return (textContent);
			}
			catch (Exception err)
			{
				ErrorMsg = String.Format("Error, could not get inner text of elements at '{0}': {1}", xpath, err.Message);
				return (null);
			}
		}

		/// <summary>
		/// Returns the value for an attribute of an element.
		/// </summary>
		/// <param name="xpath">An xpath to the XML nodes.</param>
		/// <param name="attr">The name of the attribute.</param>
		/// <returns></returns>
		public string GetAttribute(string xpath, string attr)
		{
			XmlNode element	  = null;		  //Holds the element from the xpath.
			string  attrValue = String.Empty; //Holds the value from the attribute.

			try
			{
				//Initialize element based on xpath.
				//
				element = this.Xml.SelectSingleNode(xpath);
			}
			catch (Exception err)
			{
				ErrorMsg = String.Format("Error, could not initialize element '{0}': {1}", xpath, err.Message);
				return (null);
			}

			try
			{
				//Get attribute value.
				//
				attrValue = element.Attributes[attr].InnerText;

				return (attrValue);
			}
			catch (Exception err)
			{
				ErrorMsg = String.Format("Error, could not get attribute '{0}' of element '{1}': {2}", attr, xpath, err.Message);
				return (null);
			}
		}

		/// <summary>
		/// Returns the value for an attribute of multiple elements.
		/// </summary>
		/// <param name="xpath">An xpath to XML nodes.</param>
		/// <param name="attr">The name of the attribute.</param>
		/// <returns></returns>
		public string[] GetAttributeList(string xpath, string attr)
		{
			XmlNodeList elementList = null; //Holds all the elements found at the specified xpath.
			XmlNode		element		= null; //Holds a single element extracted from the elements found.
			string[]	attrValues  = null; //Holds all the text content found for the attributes.

			//Initialize elements at xpath and array at size of elements found.
			//
			try
			{
				elementList = this.Xml.SelectNodes(xpath);
				attrValues = new string[elementList.Count];
			}
			catch (Exception err)
			{
				ErrorMsg = String.Format("Error, could not initialize elements at '{0}': {1}", xpath, err.Message);
				return (null);
			}

			//Extract each element's attribute text.
			//
			try
			{
				//Run loop for each element found.
				//
				for (int i = 0; i < elementList.Count; i++)
				{
					//Get the specific element.
					//
					element = elementList.Item(i);

					//Set the value in a parallel array.
					//
					attrValues[i] = element.Attributes[attr].InnerText;

				}

				return (attrValues);
			}
			catch (Exception err)
			{
				ErrorMsg = String.Format("Error, could not get inner text of attributes '{0}' at '{1}': {1}", attr, xpath, err.Message);
				return (null);
			}
		}

		/// <summary>
		/// This private method is a call back of ValidateXml for XML validation.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void ValidateXmlCallback(object sender, ValidationEventArgs args)
		{
			try
			{
				//An error or warning was found.
				//
				this.ValErrs++;

				//Check if it was an error or warning.
				//
				if (args.Severity == XmlSeverityType.Warning)
				{
					ErrorMsg += String.Format("\nWarning: No schema found. Validation could not be performed. {0}", args.Message);
				}
				else
				{
					ErrorMsg += String.Format("\nError: {0}", args.Message);
				}
			}
			catch (Exception err)
			{
				ErrorMsg = String.Format("Error catching validation exception: {0}", err.Message);
			}
		}
	}
}