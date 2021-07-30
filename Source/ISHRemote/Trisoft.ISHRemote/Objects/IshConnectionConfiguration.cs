/*
* Copyright (c) 2014 All Rights Reserved by the SDL Group.
* 
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
* 
*     http://www.apache.org/licenses/LICENSE-2.0
* 
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Trisoft.ISHRemote.Exceptions;

namespace Trisoft.ISHRemote.Objects
{
    /// <summary>
    /// The connectionconfiguration.xml file is a discovery file that teels you how the web services are configured.
    /// Since 2013/10.0.0 it indicates the token issuer for the active authentication flow. More recently we want to know the application name (used by ASMX web services).
    /// </summary>
    internal class IshConnectionConfiguration
    {
        /// <summary>
        /// The clientconfiguration.xml discovery file tells us the server-side CMS software version is ... eg. 14.0.3
        /// </summary>
        public string SoftwareVersion {get;set;}
        /// <summary>
        /// The clientconfiguration.xml discovery file tells us the ApplicationName like "InfoShareAuthor" or "InfoShareAuthorDITA" (where DITA is the projectsuffix). Historically it was the TriDKApp registry key ;-)
        /// </summary>
        public string ApplicationName { get; set; }

        /// <summary>
        /// The clientconfiguration.xml discovery file tells us the configured ISHWS Url is "https://ish.example.com/ISHWS/" while you perhaps are doing https://localhost/ or behind the configured load balancer.
        /// </summary>
        public Uri InfoShareWSUrl { get; set; }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="xmlConnectionConfiguration">An xml string holding the /ISHWS/connectionconfiguration.xml content</param>
        public IshConnectionConfiguration(string xmlConnectionConfiguration)
        {
            // A sample 14SP3/14.0.3 file looks like
            //<?xml version="1.0" encoding="utf-8"?>
            //<connectionconfiguration version="1.0.0.0">
            //  <infosharesoftwareversion>14.0.3</infosharesoftwareversion>
            //  <infoshareapplicationname>InfoShareAuthor</infoshareapplicationname>
            //  <infosharewsurl>https://ish.example.com/ISHWS/</infosharewsurl>
            //  <infoshareauthorurl>https://ish.example.com/ISHCM/</infoshareauthorurl>
            //  <infosharecsurl>https://ish.example.com/ISHCS/</infosharecsurl>
            //  <infosharewscertificatevalidationmode>None</infosharewscertificatevalidationmode>
            //  <issuer>
            //    <!-- issuerwstrustbindingtype (WindowsMixed/UserNameMixed) -->
            //    <authenticationtype>UserNameMixed</authenticationtype>
            //    <url>https://ish.example.com/ISHSTS/issue/wstrust/mixed/username</url>
            //  </issuer>
            //  <projectconfigurationurl>https://ish.example.com/ISHCM/ClientConfig/ClientConfig.xml</projectconfigurationurl>
            //</connectionconfiguration>

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xmlConnectionConfiguration);
            // Potential version check in the future: xmlDocument.SelectSingleNode("connectionconfiguration").Attributes.GetNamedItem("version").Value);
            SoftwareVersion = xmlDocument.SelectSingleNode("connectionconfiguration/infosharesoftwareversion").InnerText;
            ApplicationName = xmlDocument.SelectSingleNode("connectionconfiguration/infoshareapplicationname").InnerText;
            InfoShareWSUrl = new Uri(xmlDocument.SelectSingleNode("connectionconfiguration/infosharewsurl").InnerText);
        }
    }
}
